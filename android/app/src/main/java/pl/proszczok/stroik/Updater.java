package pl.proszczok.stroik;

import android.app.*;
import android.content.*;
import android.content.pm.*;
import android.net.Uri;
import android.os.*;
import android.provider.Settings;
import java.io.*;
import java.net.*;
import java.security.MessageDigest;
import java.util.*;
import java.util.concurrent.*;
import org.json.JSONObject;

public final class Updater {
  static final String MANIFEST =
      "https://raw.githubusercontent.com/proszczok2002/Stroik/main/android/update.json";
  private final MainActivity activity;
  private final ExecutorService executor = Executors.newSingleThreadExecutor();
  private volatile boolean busy;
  private int offered;

  public Updater(MainActivity a) {
    activity = a;
  }

  public void close() {
    executor.shutdownNow();
  }

  static boolean assetUrl(String u) {
    try {
      URI uri = new URI(u);
      return "https".equals(uri.getScheme())
          && "github.com".equals(uri.getHost())
          && uri.getUserInfo() == null
          && uri.getPath().startsWith("/proszczok2002/Stroik/releases/download/android-v")
          && !uri.getPath().contains("..");
    } catch (Exception e) {
      return false;
    }
  }

  static byte[] fetch(String url, int max) throws Exception {
    for (int attempt = 0; ; attempt++) {
      try { return fetchOnce(url, max); }
      catch (IOException error) {
        if (attempt >= 2 || Thread.currentThread().isInterrupted()
            || error instanceof HttpError && !((HttpError) error).temporary()) throw error;
        Thread.sleep(400L << attempt);
      }
    }
  }

  private static final class HttpError extends IOException {
    final int status;
    HttpError(int value) { super("HTTP " + value); status = value; }
    boolean temporary() { return status == 408 || status == 429 || status >= 500; }
  }

  private static byte[] fetchOnce(String url, int max) throws Exception {
    long started = SystemClock.elapsedRealtime();
    URL u = new URL(url);
    for (int redirects = 0; redirects < 6; redirects++) {
      String host = u.getHost();
      if (!u.getProtocol().equals("https")
          || !(host.equals("github.com")
              || host.equals("raw.githubusercontent.com")
              || host.equals("release-assets.githubusercontent.com")
              || host.equals("objects.githubusercontent.com")))
        throw new IOException("Untrusted download host");
      HttpURLConnection c = (HttpURLConnection) u.openConnection();
      c.setConnectTimeout(20000);
      c.setReadTimeout(30000);
      c.setInstanceFollowRedirects(false);
      c.setUseCaches(false);
      c.setRequestProperty("User-Agent", "Stroik-Android/1.0");
      c.setRequestProperty("Cache-Control", "no-cache");
      try {
        int status = c.getResponseCode();
        if (status >= 300 && status < 400) {
          u = new URL(u, c.getHeaderField("Location"));
          continue;
        }
        if (status != 200) throw new HttpError(status);
        try (InputStream in = c.getInputStream();
            ByteArrayOutputStream out = new ByteArrayOutputStream()) {
          byte[] buffer = new byte[16384];
          int n;
          while ((n = in.read(buffer)) != -1) {
            if (Thread.currentThread().isInterrupted() || SystemClock.elapsedRealtime() - started > 600000)
              throw new java.io.InterruptedIOException("Download interrupted or timed out");
            if (out.size() + n > max) throw new IOException("Download too large");
            out.write(buffer, 0, n);
          }
          if (c.getContentLengthLong() >= 0 && c.getContentLengthLong() != out.size()) throw new EOFException("Incomplete download");
          return out.toByteArray();
        }
      } finally {
        c.disconnect();
      }
    }
    throw new IOException("Too many redirects");
  }

  private void log(String text, Exception error) {
    try {
      File file = new File(activity.getFilesDir(), "update.log");
      if (file.length() > 1000000) {
        File old = new File(activity.getFilesDir(), "update.previous.log");
        if (old.exists()) old.delete();
        file.renameTo(old);
      }
      try (PrintWriter writer = new PrintWriter(new OutputStreamWriter(new FileOutputStream(file, true), java.nio.charset.StandardCharsets.UTF_8))) {
        writer.println(new java.util.Date() + " " + text + (error == null ? "" : ": " + error));
      }
    } catch (IOException ignored) { }
  }

  public void check(boolean quiet) {
    if (busy
        || activity.isFinishing()
        || activity.isDestroyed()
        || (quiet && !activity.model.updates)) return;
    busy = true;
    executor.execute(() -> {
      try {
        JSONObject j =
            new JSONObject(
                new String(fetch(MANIFEST, 100000), java.nio.charset.StandardCharsets.UTF_8));
        int code = j.getInt("versionCode");
        PackageInfo installed =
            activity
                .getPackageManager()
                .getPackageInfo(activity.getPackageName(), PackageManager.GET_SIGNING_CERTIFICATES);
        if (!activity.getPackageName().equals(j.getString("packageName"))
            || !assetUrl(j.getString("apkUrl"))
            || !j.getString("sha256").matches("[0-9a-fA-F]{64}"))
          throw new IOException("Invalid update manifest");
        activity.runOnUiThread(() -> {
          busy = false;
          if (!activity.active || activity.isFinishing() || (quiet && !activity.model.updates))
            return;
          if (code <= installed.getLongVersionCode()) {
            if (!quiet)
              activity.message(
                  activity.t("Masz najnowszą wersję.", "You have the latest version."));
            return;
          }
          if (quiet && offered == code) return;
          offered = code;
          String notes =
              j.optString(
                  activity.model.english ? "notesEn" : "notesPl",
                  activity.t("Brak opisu zmian.", "Release notes unavailable."));
          activity.showText(
              activity.t("Nowa wersja ", "New version ") + j.optString("versionName"),
              notes
                  + "\n\n"
                  + activity.t(
                      "Pobrać APK i otworzyć systemowe potwierdzenie instalacji?",
                      "Download the APK and open Android's installation confirmation?"),
              () -> download(j));
        });
      } catch (Exception e) {
        log("Update check failed", e);
        activity.runOnUiThread(() -> {
          busy = false;
          if (!quiet && activity.active)
            activity.message(
                activity.t("Sprawdzenie nie powiodło się: ", "Check failed: ") + e.getMessage());
        });
      }
    });
  }

  private void download(JSONObject j) {
    if (busy) return;
    if (!activity.getPackageManager().canRequestPackageInstalls()) {
      activity.showText(
          activity.t("Zgoda Androida", "Android permission"),
          activity.t(
              "Android wymaga zgody na instalowanie aktualizacji z aplikacji Stroik. Włącz ją w"
                  + " następnym oknie, wróć i ponów sprawdzanie. Każda instalacja nadal wymaga"
                  + " potwierdzenia.",
              "Android requires permission to install updates from Stroik. Enable it in the next"
                  + " screen, return and check again. Every installation still requires"
                  + " confirmation."),
          () ->
              activity.startActivity(
                  new Intent(
                      Settings.ACTION_MANAGE_UNKNOWN_APP_SOURCES,
                      Uri.parse("package:" + activity.getPackageName()))));
      return;
    }
    busy = true;
    activity.status(
        activity.t("Pobieranie i weryfikacja aktualizacji…", "Downloading and verifying update…"));
    executor.execute(() -> {
      File file = new File(activity.getCacheDir(), "stroik-update.apk");
      try {
        byte[] data = fetch(j.getString("apkUrl"), 40000000);
        String actual = hex(MessageDigest.getInstance("SHA-256").digest(data));
        if (!actual.equalsIgnoreCase(j.getString("sha256")))
          throw new IOException("SHA-256 mismatch");
        try (FileOutputStream out = new FileOutputStream(file)) {
          out.write(data);
        }
        PackageManager pm = activity.getPackageManager();
        PackageInfo archive =
            pm.getPackageArchiveInfo(
                file.getAbsolutePath(), PackageManager.GET_SIGNING_CERTIFICATES);
        PackageInfo installed =
            pm.getPackageInfo(activity.getPackageName(), PackageManager.GET_SIGNING_CERTIFICATES);
        if (archive == null
            || !activity.getPackageName().equals(archive.packageName)
            || archive.getLongVersionCode() != j.getInt("versionCode")
            || archive.getLongVersionCode() <= installed.getLongVersionCode()
            || !sameSignatures(installed, archive))
          throw new IOException("APK package, version or certificate mismatch");
        log("APK verified: version=" + archive.getLongVersionCode() + ", bytes=" + data.length + ", SHA-256=" + actual, null);
        activity.runOnUiThread(() -> {
          busy = false;
          if (!activity.active) return;
          try {
            install(file);
          } catch (Exception e) {
            activity.message(e.getMessage());
          }
        });
      } catch (Exception e) {
        log("Download/install failed", e);
        file.delete();
        activity.runOnUiThread(() -> {
          busy = false;
          if (activity.active)
            activity.message(
                activity.t("Aktualizacja nie powiodła się: ", "Update failed: ") + e.getMessage());
        });
      }
    });
  }

  static boolean sameSignatures(PackageInfo a, PackageInfo b) {
    if (a.signingInfo == null || b.signingInfo == null) return false;
    Set<String> first = new HashSet<>(), second = new HashSet<>();
    for (Signature s : a.signingInfo.getApkContentsSigners()) first.add(hex(s.toByteArray()));
    for (Signature s : b.signingInfo.getApkContentsSigners()) second.add(hex(s.toByteArray()));
    return !first.isEmpty() && first.equals(second);
  }

  static String hex(byte[] bytes) {
    StringBuilder s = new StringBuilder();
    for (byte b : bytes) s.append(String.format(Locale.US, "%02x", b & 255));
    return s.toString();
  }

  private void install(File file) throws Exception {
    PackageInstaller installer = activity.getPackageManager().getPackageInstaller();
    PackageInstaller.SessionParams params =
        new PackageInstaller.SessionParams(PackageInstaller.SessionParams.MODE_FULL_INSTALL);
    params.setAppPackageName(activity.getPackageName());
    params.setSize(file.length());
    if (Build.VERSION.SDK_INT >= 31)
      params.setRequireUserAction(PackageInstaller.SessionParams.USER_ACTION_REQUIRED);
    if (Build.VERSION.SDK_INT >= 33)
      params.setPackageSource(PackageInstaller.PACKAGE_SOURCE_DOWNLOADED_FILE);
    int id = installer.createSession(params);
    try (PackageInstaller.Session session = installer.openSession(id)) {
      try (InputStream in = new FileInputStream(file);
          OutputStream out = session.openWrite("Stroik.apk", 0, file.length())) {
        byte[] block = new byte[16384];
        int count;
        while ((count = in.read(block)) != -1) out.write(block, 0, count);
        session.fsync(out);
      }
      Intent callback = new Intent(activity, InstallReceiver.class);
      PendingIntent pending =
          PendingIntent.getBroadcast(
              activity,
              id,
              callback,
              PendingIntent.FLAG_UPDATE_CURRENT | PendingIntent.FLAG_MUTABLE);
      session.commit(pending.getIntentSender());
    } catch (Exception error) {
      installer.abandonSession(id);
      throw error;
    }
  }
}
