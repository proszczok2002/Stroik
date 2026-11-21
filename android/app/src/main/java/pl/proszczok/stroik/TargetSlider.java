package pl.proszczok.stroik;

import android.app.AlertDialog;
import android.os.Bundle;
import android.view.MotionEvent;
import android.view.accessibility.AccessibilityNodeInfo;
import android.widget.EditText;
import android.widget.SeekBar;
import java.util.List;

final class TargetSlider extends SeekBar {
  private static final int PREVIOUS_OCTAVE = 0x01030001, NEXT_OCTAVE = 0x01030002;
  private final MainActivity owner;
  private List<Integer> notes = List.of();
  private float x, y;
  private int fingers;
  private final int gestureDistance;

  TargetSlider(MainActivity owner) {
    super(owner);
    this.owner = owner;
    gestureDistance = Math.round(24 * getResources().getDisplayMetrics().density);
    setMinimumHeight(gestureDistance * 3);
    setContentDescription(owner.t("Poprzedni lub następny dźwięk", "Previous or next note"));
    setOnClickListener(view -> edit());
    setOnSeekBarChangeListener(new OnSeekBarChangeListener() {
      public void onStartTrackingTouch(SeekBar bar) {}
      public void onStopTrackingTouch(SeekBar bar) {}
      public void onProgressChanged(SeekBar bar, int position, boolean user) {
        if (user && position < notes.size()) owner.setTarget(notes.get(position), false);
      }
    });
  }

  void sync() {
    notes = owner.model.navigableNotes();
    setMax(Math.max(0, notes.size() - 1));
    // SeekBar recalculates the keyboard increment when its range changes.
    setKeyProgressIncrement(1);
    setProgress(Math.max(0, notes.indexOf(owner.model.base())));
    setStateDescription(owner.model.spokenNote(owner.model.base()));
  }

  @Override public void onInitializeAccessibilityNodeInfo(AccessibilityNodeInfo info) {
    super.onInitializeAccessibilityNodeInfo(info);
    info.addAction(new AccessibilityNodeInfo.AccessibilityAction(AccessibilityNodeInfo.ACTION_CLICK,
        owner.t("Wpisz dźwięk", "Enter note")));
    if (!owner.model.strings()) {
      info.addAction(new AccessibilityNodeInfo.AccessibilityAction(PREVIOUS_OCTAVE,
          owner.t("Poprzednia oktawa", "Previous octave")));
      info.addAction(new AccessibilityNodeInfo.AccessibilityAction(NEXT_OCTAVE,
          owner.t("Następna oktawa", "Next octave")));
    }
  }

  @Override public boolean performAccessibilityAction(int action, Bundle arguments) {
    if (action == AccessibilityNodeInfo.ACTION_SCROLL_FORWARD
        || action == AccessibilityNodeInfo.ACTION_SCROLL_BACKWARD) {
      return isEnabled() && owner.stepTarget(action == AccessibilityNodeInfo.ACTION_SCROLL_FORWARD ? 1 : -1);
    }
    if (action == PREVIOUS_OCTAVE || action == NEXT_OCTAVE) {
      owner.changeOctave(action == PREVIOUS_OCTAVE ? -1 : 1);
      return true;
    }
    return super.performAccessibilityAction(action, arguments);
  }

  @Override public boolean onTouchEvent(MotionEvent event) {
    switch (event.getActionMasked()) {
      case MotionEvent.ACTION_DOWN:
        x = event.getX();
        y = event.getY();
        fingers = 1;
        getParent().requestDisallowInterceptTouchEvent(true);
        break;
      case MotionEvent.ACTION_POINTER_DOWN:
        fingers = Math.max(fingers, event.getPointerCount());
        break;
      case MotionEvent.ACTION_UP:
        getParent().requestDisallowInterceptTouchEvent(false);
        float dx = event.getX() - x, dy = event.getY() - y;
        float distance = Math.abs(dy) >= Math.abs(dx) ? dy : dx;
        if (Math.abs(distance) >= gestureDistance) {
          int direction = distance > 0 ? 1 : -1;
          if (fingers == 1) owner.stepTarget(direction);
          else if (fingers == 2) owner.changeOctave(direction);
        } else if (fingers == 1) performClick();
        break;
      case MotionEvent.ACTION_CANCEL:
        getParent().requestDisallowInterceptTouchEvent(false);
        break;
      default:
        break;
    }
    return true;
  }

  private void edit() {
    EditText input = new EditText(owner);
    input.setSingleLine();
    input.setHint("C4");
    AlertDialog dialog = new AlertDialog.Builder(owner)
        .setTitle(owner.t("Wpisz dźwięk", "Enter note")).setView(input)
        .setPositiveButton(owner.t("Zapisz", "Save"), null)
        .setNegativeButton(owner.t("Anuluj", "Cancel"), null).create();
    Runnable save = () -> {
      int midi = Model.parseNote(input.getText().toString());
      if (owner.setTarget(midi, true)) dialog.dismiss();
      else input.setError(owner.t("Wpisz dźwięk dostępny dla tego instrumentu.",
          "Enter a note available for this instrument."));
    };
    input.setOnEditorActionListener((view, action, event) -> { save.run(); return true; });
    dialog.setOnShowListener(event -> {
      dialog.getButton(AlertDialog.BUTTON_POSITIVE).setOnClickListener(view -> save.run());
      input.requestFocus();
    });
    dialog.getWindow().setSoftInputMode(android.view.WindowManager.LayoutParams.SOFT_INPUT_STATE_ALWAYS_VISIBLE);
    dialog.show();
  }
}
