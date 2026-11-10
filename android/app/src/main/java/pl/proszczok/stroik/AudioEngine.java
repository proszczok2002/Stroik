package pl.proszczok.stroik;

import android.media.*;
import android.os.*;
import java.util.concurrent.*;

public final class AudioEngine implements AutoCloseable {
  public interface Listener {
    void result(String text);

    void reading(double cents);

    void advance();

    void failed(String text);
  }

  private final Listener listener;
  private final Handler ui = new Handler(Looper.getMainLooper());
  private final ExecutorService player = Executors.newSingleThreadExecutor();
  private volatile Model config;
  private volatile boolean running, closed, paused;
  private AudioRecord recorder;
  private AudioTrack fork, monitor;
  private Thread worker;
  private volatile long muteUntil, repeatAt;
  private volatile int generation;
  private int rate = 48000;
  private final AudioManager manager;

  public AudioEngine(Model m, Listener l, AudioManager manager) {
    config = m.copy();
    listener = l;
    this.manager = manager;
  }

  public boolean running() {
    return running;
  }

  public void configure(Model m) {
    config = m.copy();
    muteUntil = 0;
    generation++;
    if (running && m.method == 2) strike();
  }

  public void setPaused(boolean p) {
    paused = p;
    if (p) stopFork();
  }

  public static boolean headphones(AudioManager a) {
    for (AudioDeviceInfo d : a.getDevices(AudioManager.GET_DEVICES_OUTPUTS)) {
      int t = d.getType();
      if (t == AudioDeviceInfo.TYPE_WIRED_HEADPHONES
          || t == AudioDeviceInfo.TYPE_WIRED_HEADSET
          || t == AudioDeviceInfo.TYPE_USB_HEADSET
          || t == AudioDeviceInfo.TYPE_BLUETOOTH_A2DP
          || t == AudioDeviceInfo.TYPE_BLE_HEADSET) return true;
    }
    return false;
  }

  public synchronized void start() {
    if (running) return;
    if (closed) throw new IllegalStateException("Closed");
    for (int candidate : new int[] {48000, 44100}) {
      rate = candidate;
      int min =
          AudioRecord.getMinBufferSize(
              rate, AudioFormat.CHANNEL_IN_MONO, AudioFormat.ENCODING_PCM_16BIT);
      if (min <= 0) continue;
      for (int source :
          new int[] {MediaRecorder.AudioSource.UNPROCESSED, MediaRecorder.AudioSource.MIC}) {
        try {
          recorder = createRecord(source, min);
        } catch (IllegalArgumentException | UnsupportedOperationException error) {
          recorder = null;
        }
        if (recorder != null && recorder.getState() == AudioRecord.STATE_INITIALIZED) break;
        if (recorder != null) recorder.release();
        recorder = null;
      }
      if (recorder != null) break;
    }
    if (recorder == null) {
      throw new IllegalStateException("Cannot open microphone / Nie można otworzyć mikrofonu");
    }
    recorder.startRecording();
    running = true;
    worker = new Thread(this::capture, "Stroik microphone");
    worker.start();
    if (config.method == 2) strike();
  }

  private AudioRecord createRecord(int source, int min) {
    return new AudioRecord.Builder()
        .setAudioSource(source)
        .setAudioFormat(
            new AudioFormat.Builder()
                .setSampleRate(rate)
                .setChannelMask(AudioFormat.CHANNEL_IN_MONO)
                .setEncoding(AudioFormat.ENCODING_PCM_16BIT)
                .build())
        .setBufferSizeInBytes(Math.max(min, 32768))
        .build();
  }

  private void post(int g, Runnable action) {
    ui.post(() -> {
      if (!closed && g == generation) action.run();
    });
  }

  private void capture() {
    short[] frame = new short[4096], window = new short[8192], pair = new short[65536];
    int count = 0, pairCount = 0, g = -1;
    long lastVisual = 0, lastFork = 0, lastPair = 0;
    double storedLower = 0;
    Dsp.Session session = new Dsp.Session();
    Dsp.Taps taps = new Dsp.Taps();
    boolean armed = false;
    long armedAt = 0;
    try {
      while (running) {
        int n = recorder.read(frame, 0, frame.length, AudioRecord.READ_BLOCKING);
        if (!running) break;
        if (n <= 0) throw new IllegalStateException("AudioRecord error " + n);
        long now = SystemClock.elapsedRealtime();
        Model m = config;
        int current = generation;
        if (g != current) {
          g = current;
          count = pairCount = 0;
          storedLower = 0;
          session.reset();
          taps.reset();
          armed = false;
        }
        final int messageGeneration = g;
        if (m.method == 3 && m.monitor) {
          if (!headphones(manager))
            throw new IllegalStateException(
                m.t(
                    "Podłącz słuchawki przed odsłuchem mikrofonu.",
                    "Connect headphones before microphone monitoring."));
          writeMonitor(frame, n);
        } else stopMonitor();
        if (m.method == 2 && repeatAt > 0 && now >= repeatAt && !paused) {
          repeatAt = 0;
          strike();
        }
        if (now < muteUntil || (m.method == 2 && repeatAt > 0)) {
          count = pairCount = 0;
          session.reset();
          taps.reset();
          continue;
        }
        if ((m.method == 0 || m.method == 3)
            && m.autoFork
            && !paused
            && now - lastFork
                >= Math.max(m.forkSeconds * 1000, m.method == 0 && m.forkTap ? 6500 : 1000)) {
          lastFork = now;
          strike();
          continue;
        }
        double rms = Dsp.rms(frame, n),
            threshold = new double[] {.004, .008, .016}[m.sensitivity],
            frequency = 0;
        if (m.forkTap && m.method != 1 && taps.observe(rms, threshold, now, m.gap)) {
          muteUntil = now + 2000;
          post(g, listener::advance);
          continue;
        }
        if (m.interval != 0 && m.method != 0 && m.pairMode == 1) {
          if (rms < threshold) {
            pairCount = 0;
            continue;
          }
          int take = Math.min(n, pair.length - pairCount);
          System.arraycopy(frame, 0, pair, pairCount, take);
          pairCount += take;
          if (pairCount == pair.length) {
            pairCount = 0;
            if (now - lastPair < 3000) continue;
            lastPair = now;
            double[] v =
                m.base() + m.semitones() <= 108
                    ? Dsp.together(
                        pair,
                        rate,
                        Model.hz(m.base(), m.reference) / Math.pow(2, m.correction / 1200),
                        m.interval)
                    : null;
            if (v == null) {
              post(
                  g,
                  () ->
                      listener.result(
                          m.t(
                              "Nie wykryto dwóch składowych. Spróbuj osobno.",
                              "Two components not found. Try separately.")));
            } else {
              double correction = Math.pow(2, m.correction / 1200);
              reportInterval(
                  m, Dsp.compare(v[0] * correction, v[1] * correction, m.interval), true, g);
            }
            muteUntil = now + 4000;
            if (m.method == 2) repeatAt = now + 5000;
          }
          continue;
        }
        if (rms >= threshold) {
          int take = Math.min(n, window.length);
          if (count + take > window.length) {
            int overflow = count + take - window.length;
            System.arraycopy(window, overflow, window, 0, count - overflow);
            count -= overflow;
          }
          System.arraycopy(frame, n - take, window, count, take);
          count += take;
          if (count == window.length)
            frequency = Dsp.pitch(window, rate) * Math.pow(2, m.correction / 1200);
        } else count = 0;
        if (m.graphics && frequency > 0 && now - lastVisual > 200) {
          lastVisual = now;
          double cents =
              1200
                  * Math.log(frequency / Model.hz(m.resolved(frequency), m.reference))
                  / Math.log(2);
          post(g, () -> listener.reading(cents));
        }
        if (m.method == 0) continue;
        if (m.advance && !m.forkTap && m.interval == 0 && armed) {
          if (now - armedAt > 20000) {
            armed = false;
            taps.reset();
          } else if (taps.observe(rms, threshold, now, m.gap)
              && frequency > 0
              && Math.abs(
                      1200
                          * Math.log(frequency / Model.hz(m.resolved(frequency), m.reference))
                          / Math.log(2))
                  < 35) {
            muteUntil = now + 2000;
            armed = false;
            post(g, listener::advance);
            continue;
          }
        }
        double done = session.observe(frequency, now);
        if (done <= 0) continue;
        if (m.interval != 0) {
          int wanted = m.base() + (storedLower > 0 ? m.semitones() : 0);
          if (wanted > 108) {
            post(g, () -> listener.result(m.t("Wybierz niższą nutę.", "Choose a lower note.")));
            muteUntil = now + 2000;
            continue;
          }
          if (Math.abs(1200 * Math.log(done / Model.hz(wanted, m.reference)) / Math.log(2)) > 90) {
            post(
                g,
                () ->
                    listener.result(
                        m.t("Zagraj właściwą nutę: ", "Play the expected note: ")
                            + m.note(wanted)));
            muteUntil = now + 2000;
            continue;
          }
          if (storedLower == 0) {
            storedLower = done;
            post(
                g,
                () ->
                    listener.result(
                        m.t(
                            "Dolny zapisany. Zagraj górny i wycisz.",
                            "Lower stored. Play upper and mute.")));
            muteUntil = now + 2000;
          } else {
            reportInterval(m, Dsp.compare(storedLower, done, m.interval), false, g);
            storedLower = 0;
            muteUntil = now + 4000;
            if (m.method == 2) repeatAt = now + 5000;
          }
          continue;
        }
        int midi = m.resolved(done);
        double expected = Model.hz(midi, m.reference),
            cents = 1200 * Math.log(done / expected) / Math.log(2);
        String direction =
            Math.abs(cents) <= m.tolerance
                ? m.t("dobrze", "in tune")
                : cents < 0 ? m.t("za nisko", "too low") : m.t("za wysoko", "too high");
        String text =
            m.note(midi)
                + ", "
                + direction
                + ", "
                + String.format(
                    m.english ? java.util.Locale.US : new java.util.Locale("pl"),
                    "%+.1f Hz; %+.1f ",
                    done - expected,
                    cents)
                + m.t("centów.", "cents.");
        post(messageGeneration, () -> listener.result(text));
        muteUntil = now + 1800;
        if (m.method == 2) repeatAt = now + 5000;
        if (m.advance && Math.abs(cents) <= m.tolerance) {
          armed = true;
          armedAt = now;
          taps.reset();
        }
      }
    } catch (Exception e) {
      if (running) {
        running = false;
        post(generation, () -> listener.failed(e.getMessage()));
      }
    } finally {
      stopMonitor();
    }
  }

  private void reportInterval(Model m, double[] v, boolean experimental, int g) {
    String text =
        (experimental
                ? m.t("Eksperymentalny pomiar składowych. ", "Experimental component measurement. ")
                : "")
            + String.format(
                m.english ? java.util.Locale.US : new java.util.Locale("pl"),
                m.t(
                    "Odchyłka od stroju równomiernego: %+.1f centów. Szacowane dudnienia: %.2f na"
                        + " sekundę.",
                    "Deviation from equal temperament: %+.1f cents. Estimated beats: %.2f per"
                        + " second."),
                v[2],
                v[3]);
    post(g, () -> listener.result(text));
  }

  public void strike() {
    if (closed) return;
    Model m = config;
    int base = m.base();
    if (m.interval != 0 && base + m.semitones() > 108) {
      ui.post(() -> listener.failed(m.t("Górna nuta poza zakresem.", "Upper note out of range.")));
      return;
    }
    long now = SystemClock.elapsedRealtime();
    if (m.method == 0 || m.method == 2) muteUntil = now + 3400;
    int playbackGeneration = generation;
    player.execute(() -> {
      if (closed || generation != playbackGeneration) return;
      short[] data =
          Dsp.fork(
              Model.hz(base, m.reference),
              m.interval == 0 ? 0 : Model.hz(base + m.semitones(), m.reference),
              44100);
      synchronized (this) {
        if (closed || generation != playbackGeneration) return;
        stopFork();
        fork =
            new AudioTrack.Builder()
                .setAudioAttributes(
                    new AudioAttributes.Builder()
                        .setUsage(AudioAttributes.USAGE_MEDIA)
                        .setContentType(AudioAttributes.CONTENT_TYPE_MUSIC)
                        .build())
                .setAudioFormat(
                    new AudioFormat.Builder()
                        .setSampleRate(44100)
                        .setEncoding(AudioFormat.ENCODING_PCM_16BIT)
                        .setChannelMask(AudioFormat.CHANNEL_OUT_MONO)
                        .build())
                .setBufferSizeInBytes(data.length * 2)
                .setTransferMode(AudioTrack.MODE_STATIC)
                .build();
        fork.write(data, 0, data.length);
        fork.play();
      }
    });
  }

  private synchronized void writeMonitor(short[] data, int n) {
    if (monitor == null)
      monitor =
          new AudioTrack.Builder()
              .setAudioAttributes(
                  new AudioAttributes.Builder().setUsage(AudioAttributes.USAGE_MEDIA).build())
              .setAudioFormat(
                  new AudioFormat.Builder()
                      .setSampleRate(rate)
                      .setEncoding(AudioFormat.ENCODING_PCM_16BIT)
                      .setChannelMask(AudioFormat.CHANNEL_OUT_MONO)
                      .build())
              .setBufferSizeInBytes(
                  Math.max(
                      16384,
                      AudioTrack.getMinBufferSize(
                          rate, AudioFormat.CHANNEL_OUT_MONO, AudioFormat.ENCODING_PCM_16BIT)))
              .setTransferMode(AudioTrack.MODE_STREAM)
              .build();
    monitor.play();
    monitor.write(data, 0, n, AudioTrack.WRITE_NON_BLOCKING);
  }

  private synchronized void stopMonitor() {
    if (monitor != null) {
      monitor.pause();
      monitor.flush();
      monitor.release();
      monitor = null;
    }
  }

  public synchronized void stopFork() {
    if (fork != null) {
      try {
        fork.stop();
      } catch (Exception ignored) {
      }
      fork.release();
      fork = null;
    }
  }

  public void stop() {
    running = false;
    generation++;
    repeatAt = 0;
    if (recorder != null) {
      try {
        recorder.stop();
      } catch (Exception ignored) {
      }
    }
    Thread t = worker;
    if (t != null && t != Thread.currentThread())
      try {
        t.join();
      } catch (InterruptedException e) {
        Thread.currentThread().interrupt();
      }
    worker = null;
    if (recorder != null) {
      recorder.release();
      recorder = null;
    }
    stopFork();
    stopMonitor();
  }

  public void close() {
    closed = true;
    stop();
    player.shutdownNow();
    ui.removeCallbacksAndMessages(null);
  }
}
