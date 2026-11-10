package pl.proszczok.stroik;

import android.content.*;
import android.content.pm.PackageInstaller;
import android.widget.Toast;

public final class InstallReceiver extends BroadcastReceiver {
  public void onReceive(Context context, Intent intent) {
    int status = intent.getIntExtra(PackageInstaller.EXTRA_STATUS, PackageInstaller.STATUS_FAILURE);
    if (status == PackageInstaller.STATUS_PENDING_USER_ACTION) {
      Intent confirm = (Intent) intent.getParcelableExtra(Intent.EXTRA_INTENT);
      if (confirm != null) {
        confirm.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        context.startActivity(confirm);
      }
      return;
    }
    String text =
        status == PackageInstaller.STATUS_SUCCESS
            ? "Stroik: aktualizacja zainstalowana / Update installed"
            : "Stroik: instalacja nieukończona / Installation not completed (" + status + ")";
    context.getSharedPreferences("stroik", 0).edit().putString("installStatus", text).apply();
    if (status == PackageInstaller.STATUS_SUCCESS)
      new java.io.File(context.getCacheDir(), "stroik-update.apk").delete();
    Toast.makeText(context, text, Toast.LENGTH_LONG).show();
  }
}
