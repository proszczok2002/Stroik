package pl.proszczok.stroik;

import android.app.*;
import android.os.Bundle;
import android.view.*;
import android.view.accessibility.AccessibilityNodeInfo;
import android.widget.*;
import java.util.Locale;
import java.util.function.DoubleConsumer;

/** One accessibility stop: native adjustable range plus an explicit numeric-entry action. */
final class NumberSlider extends SeekBar {
  private final MainActivity owner;
  private final String label;
  private final double min, max, step;
  private double value;
  private final TextView caption;
  private final DoubleConsumer change;
  private boolean doubleTapped;

  NumberSlider(
      MainActivity owner,
      String label,
      double initial,
      double min,
      double max,
      double step,
      TextView caption,
      DoubleConsumer change) {
    super(owner);
    this.owner = owner;
    this.label = label;
    this.min = min;
    this.max = max;
    this.step = step;
    this.caption = caption;
    this.change = change;
    setMax((int) Math.round((max - min) / step));
    setKeyProgressIncrement(1);
    setMinimumHeight((int) (64 * getResources().getDisplayMetrics().density + .5));
    setContentDescription(label);
    setImportantForAccessibility(IMPORTANT_FOR_ACCESSIBILITY_YES);
    setScreenReaderFocusable(true);
    assign(initial, false);
    setOnSeekBarChangeListener(
        new OnSeekBarChangeListener() {
          public void onStartTrackingTouch(SeekBar bar) {}

          public void onStopTrackingTouch(SeekBar bar) {}

          public void onProgressChanged(SeekBar bar, int progress, boolean user) {
            if (user) assign(min + progress * step, true);
          }
        });
    setOnClickListener(v -> edit());
    setOnKeyListener((v, key, event) -> {
      if ((key == KeyEvent.KEYCODE_ENTER || key == KeyEvent.KEYCODE_DPAD_CENTER)
          && event.getAction() == KeyEvent.ACTION_UP) {
        edit();
        return true;
      }
      return false;
    });
    GestureDetector taps =
        new GestureDetector(
            owner,
            new GestureDetector.SimpleOnGestureListener() {
              public boolean onDown(android.view.MotionEvent event) {
                return true;
              }

              public boolean onDoubleTap(android.view.MotionEvent event) {
                doubleTapped = true;
                edit();
                return true;
              }
            });
    setOnTouchListener((v, event) -> {
      taps.onTouchEvent(event);
      boolean consume = doubleTapped;
      if (event.getActionMasked() == MotionEvent.ACTION_UP
          || event.getActionMasked() == MotionEvent.ACTION_CANCEL) doubleTapped = false;
      return consume;
    });
  }

  private String formatted() {
    int decimals = step >= 1 ? 0 : step >= .1 ? 1 : 2;
    return String.format(
        owner.model.english ? Locale.US : new Locale("pl"), "%." + decimals + "f", value);
  }

  private void assign(double next, boolean notify) {
    next = Math.max(min, Math.min(max, Math.round((next - min) / step) * step + min));
    boolean different = Math.abs(next - value) > 1e-8;
    value = next;
    setProgress((int) Math.round((value - min) / step));
    caption.setText(label + ": " + formatted());
    setStateDescription(formatted());
    if (notify && different) change.accept(value);
  }

  @Override
  public void onInitializeAccessibilityNodeInfo(AccessibilityNodeInfo info) {
    super.onInitializeAccessibilityNodeInfo(info);
    info.setRangeInfo(
        AccessibilityNodeInfo.RangeInfo.obtain(
            AccessibilityNodeInfo.RangeInfo.RANGE_TYPE_FLOAT,
            (float) min,
            (float) max,
            (float) value));
    info.addAction(
        new AccessibilityNodeInfo.AccessibilityAction(
            AccessibilityNodeInfo.ACTION_CLICK, owner.t("Wpisz wartość", "Enter value")));
  }

  @Override
  public boolean performAccessibilityAction(int action, Bundle args) {
    if (action == AccessibilityNodeInfo.ACTION_CLICK) {
      edit();
      return true;
    }
    if (action == AccessibilityNodeInfo.AccessibilityAction.ACTION_SET_PROGRESS.getId()) {
      if (args == null || !args.containsKey(AccessibilityNodeInfo.ACTION_ARGUMENT_PROGRESS_VALUE))
        return false;
      double next = args.getFloat(AccessibilityNodeInfo.ACTION_ARGUMENT_PROGRESS_VALUE);
      if (!Double.isFinite(next) || next < min || next > max) return false;
      assign(next, true);
      return true;
    }
    if (action == AccessibilityNodeInfo.ACTION_SCROLL_FORWARD
        || action == AccessibilityNodeInfo.ACTION_SCROLL_BACKWARD) {
      assign(value + (action == AccessibilityNodeInfo.ACTION_SCROLL_FORWARD ? step : -step), true);
      return true;
    }
    return super.performAccessibilityAction(action, args);
  }

  private void edit() {
    EditText entry = new EditText(owner);
    entry.setSingleLine(true);
    entry.setTextSize(20);
    entry.setHint(label);
    entry.setInputType(
        android.text.InputType.TYPE_CLASS_NUMBER
            | android.text.InputType.TYPE_NUMBER_FLAG_DECIMAL
            | (min < 0 ? android.text.InputType.TYPE_NUMBER_FLAG_SIGNED : 0));
    entry.setText(String.valueOf(value));
    entry.selectAll();
    AlertDialog dialog =
        new AlertDialog.Builder(owner)
            .setTitle(label)
            .setView(entry)
            .setPositiveButton(owner.t("Zapisz", "Save"), null)
            .setNegativeButton(owner.t("Anuluj", "Cancel"), null)
            .create();
    dialog.setOnShowListener(ignored -> {
      entry.requestFocus();
      dialog
          .getWindow()
          .setSoftInputMode(WindowManager.LayoutParams.SOFT_INPUT_STATE_ALWAYS_VISIBLE);
      dialog
          .getButton(AlertDialog.BUTTON_POSITIVE)
          .setOnClickListener(v -> {
            try {
              double next = Double.parseDouble(entry.getText().toString().trim().replace(',', '.'));
              if (!Double.isFinite(next) || next < min || next > max)
                throw new IllegalArgumentException();
              assign(next, true);
              dialog.dismiss();
            } catch (RuntimeException error) {
              entry.setError(
                  owner.t("Wpisz wartość od ", "Enter a value from ")
                      + min
                      + owner.t(" do ", " to ")
                      + max);
            }
          });
    });
    dialog.show();
  }
}
