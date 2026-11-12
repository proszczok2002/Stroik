package pl.proszczok.stroik;

import android.app.*;
import android.content.*;
import android.content.pm.ServiceInfo;
import android.os.*;

/** Explicit foreground capture: started only by the visible microphone button. */
public final class TuningService extends Service {
  private PowerManager.WakeLock wake;
  private boolean started;

  @Override
  public int onStartCommand(Intent intent, int flags, int id) {
    TuningSession session = TuningSession.get(this);
    if (intent != null && "stop".equals(intent.getAction())) {
      session.stop();
      stopSelf();
      return START_NOT_STICKY;
    }
    try {
      Model m = session.model;
      NotificationManager manager = getSystemService(NotificationManager.class);
      manager.createNotificationChannel(
          new NotificationChannel(
              "tuning", m.t("Strojenie", "Tuning"), NotificationManager.IMPORTANCE_LOW));
      PendingIntent open =
          PendingIntent.getActivity(
              this,
              0,
              new Intent(this, MainActivity.class),
              PendingIntent.FLAG_IMMUTABLE | PendingIntent.FLAG_UPDATE_CURRENT);
      PendingIntent stop =
          PendingIntent.getService(
              this,
              1,
              new Intent(this, TuningService.class).setAction("stop"),
              PendingIntent.FLAG_IMMUTABLE | PendingIntent.FLAG_UPDATE_CURRENT);
      Notification note =
          new Notification.Builder(this, "tuning")
              .setSmallIcon(android.R.drawable.ic_btn_speak_now)
              .setContentTitle("Stroik")
              .setContentText(
                  m.t(
                      "Mikrofon włączony. Wyniki także przy wygaszonym ekranie.",
                      "Microphone on. Results also with the screen off."))
              .setContentIntent(open)
              .setOngoing(true)
              .addAction(
                  new Notification.Action.Builder(
                          null, m.t("Wyłącz mikrofon", "Stop microphone"), stop)
                      .build())
              .build();
      startForeground(
          104,
          note,
          ServiceInfo.FOREGROUND_SERVICE_TYPE_MICROPHONE
              | ServiceInfo.FOREGROUND_SERVICE_TYPE_MEDIA_PLAYBACK);
      if (!started) {
        wake =
            ((PowerManager) getSystemService(POWER_SERVICE))
                .newWakeLock(PowerManager.PARTIAL_WAKE_LOCK, "Stroik:capture");
        wake.acquire();
        session.audio.start();
        started = true;
      }
      session.serviceActive = true;
      session.state();
    } catch (Exception error) {
      session.failed(error.getMessage());
      stopSelf();
    }
    return START_NOT_STICKY;
  }

  @Override
  public void onDestroy() {
    TuningSession session = TuningSession.get(this);
    session.audio.stop();
    session.serviceActive = false;
    session.state();
    if (wake != null && wake.isHeld()) wake.release();
    stopForeground(STOP_FOREGROUND_REMOVE);
    super.onDestroy();
  }

  @Override
  public IBinder onBind(Intent intent) {
    return null;
  }

  @Override
  public void onTaskRemoved(Intent rootIntent) {
    TuningSession.get(this).stop();
    stopSelf();
  }
}
