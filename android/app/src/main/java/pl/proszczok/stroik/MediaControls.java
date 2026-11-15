package pl.proszczok.stroik;

import android.content.*;
import android.media.MediaMetadata;
import android.media.session.*;
import android.os.*;
import android.view.KeyEvent;

/** Standard Android media controls used by TalkBack's two-finger double tap. */
final class MediaControls {
  private final MediaSession session;
  private final TuningSession tuning;

  MediaControls(Context context, TuningSession tuning) {
    this.tuning = tuning;
    session = new MediaSession(context, "Stroik tuning");
    session.setFlags(
        MediaSession.FLAG_HANDLES_MEDIA_BUTTONS | MediaSession.FLAG_HANDLES_TRANSPORT_CONTROLS);
    session.setMetadata(
        new MediaMetadata.Builder().putString(MediaMetadata.METADATA_KEY_TITLE, "Stroik").build());
    session.setCallback(
        new MediaSession.Callback() {
          public void onPlay() {
            if (!playing()) tuning.toggleFromMedia();
          }

          public void onPause() {
            tuning.endSequence();
          }

          public void onStop() {
            tuning.endSequence();
          }

          public boolean onMediaButtonEvent(Intent intent) {
            KeyEvent key = intent.getParcelableExtra(Intent.EXTRA_KEY_EVENT);
            if (key != null
                && (key.getKeyCode() == KeyEvent.KEYCODE_MEDIA_PLAY_PAUSE
                    || key.getKeyCode() == KeyEvent.KEYCODE_HEADSETHOOK)) {
              if (key.getAction() == KeyEvent.ACTION_DOWN && key.getRepeatCount() == 0)
                tuning.toggleFromMedia();
              return true;
            }
            return super.onMediaButtonEvent(intent);
          }
        },
        new Handler(Looper.getMainLooper()));
    refresh();
  }

  private boolean playing() {
    return tuning.playing();
  }

  void setAvailable(boolean value) {
    session.setActive(value || tuning.serviceActive);
    refresh();
  }

  void refresh() {
    session.setPlaybackState(
        new PlaybackState.Builder()
            .setActions(
                PlaybackState.ACTION_PLAY
                    | PlaybackState.ACTION_PAUSE
                    | PlaybackState.ACTION_PLAY_PAUSE
                    | PlaybackState.ACTION_STOP)
            .setState(
                playing() ? PlaybackState.STATE_PLAYING : PlaybackState.STATE_PAUSED,
                PlaybackState.PLAYBACK_POSITION_UNKNOWN,
                1)
            .build());
  }

  MediaSession.Token token() {
    return session.getSessionToken();
  }
}
