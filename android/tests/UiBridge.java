package pl.proszczok.stroik;

import android.app.*;
import android.os.*;
import android.view.accessibility.*;
import java.util.*;

// Separate test-process UI inspection. It never suppresses TalkBack or injects gestures.
public final class UiBridge extends Instrumentation {
  Bundle arguments;

  public void onCreate(Bundle args) {
    super.onCreate(args);
    arguments = args;
    start();
  }

  public void onStart() {
    Bundle output = new Bundle();
    try {
      UiAutomation ui = getUiAutomation(UiAutomation.FLAG_DONT_SUPPRESS_ACCESSIBILITY_SERVICES);
      android.accessibilityservice.AccessibilityServiceInfo info = ui.getServiceInfo();
      info.flags |=
          android.accessibilityservice.AccessibilityServiceInfo.FLAG_REPORT_VIEW_IDS
              | android.accessibilityservice.AccessibilityServiceInfo
                  .FLAG_RETRIEVE_INTERACTIVE_WINDOWS;
      ui.setServiceInfo(info);
      AccessibilityNodeInfo root = ui.getRootInActiveWindow();
      if (root == null) throw new IllegalStateException("No active accessibility window");
      List<AccessibilityNodeInfo> nodes = new ArrayList<>();
      collect(root, nodes);
      String wanted = arguments.getString("text", "");
      String id = arguments.getString("id", "");
      String expectedPackage = arguments.getString("package", "");
      if (!wanted.isEmpty() || !id.isEmpty()) {
        List<AccessibilityNodeInfo> matches = new ArrayList<>();
        for (AccessibilityNodeInfo node : nodes) {
          if (!expectedPackage.isEmpty() && !expectedPackage.equals(String.valueOf(node.getPackageName())))
            continue;
          if ((!wanted.isEmpty()
                  && (wanted.equals(String.valueOf(node.getText()))
                      || wanted.equals(String.valueOf(node.getContentDescription()))))
              || (!id.isEmpty() && id.equals(node.getViewIdResourceName()))) matches.add(node);
        }
        if (matches.size() != 1)
          throw new IllegalStateException(
              "Expected exactly one current UI match; got " + matches.size());
        AccessibilityNodeInfo node = matches.get(0);
        while (node != null && !node.isClickable()) node = node.getParent();
        if (node == null || !node.performAction(AccessibilityNodeInfo.ACTION_CLICK))
          throw new IllegalStateException("UI click refused");
        SystemClock.sleep(700);
        root = ui.getRootInActiveWindow();
        nodes.clear();
        if (root != null) collect(root, nodes);
      }
      StringBuilder text = new StringBuilder();
      for (AccessibilityNodeInfo node : nodes) {
        if (node.getText() == null && node.getContentDescription() == null && !node.isCheckable())
          continue;
        text.append(node.getPackageName())
            .append(" | ")
            .append(node.getClassName())
            .append(" | text=")
            .append(node.getText())
            .append(" | desc=")
            .append(node.getContentDescription())
            .append(" | id=")
            .append(node.getViewIdResourceName())
            .append(" | check=")
            .append(node.isCheckable())
            .append("/")
            .append(node.isChecked())
            .append(" | enabled=")
            .append(node.isEnabled())
            .append("\n");
      }
      output.putString("stream", text.toString());
      finish(Activity.RESULT_OK, output);
    } catch (Throwable error) {
      output.putString("stream", "UI ERROR: " + error);
      finish(Activity.RESULT_CANCELED, output);
    }
  }

  private void collect(AccessibilityNodeInfo node, List<AccessibilityNodeInfo> out) {
    out.add(node);
    for (int i = 0; i < node.getChildCount(); i++) {
      AccessibilityNodeInfo child = node.getChild(i);
      if (child != null) collect(child, out);
    }
  }
}
