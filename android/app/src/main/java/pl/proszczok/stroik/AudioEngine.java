package pl.proszczok.stroik;

import android.media.*;
import android.os.*;
import java.util.concurrent.*;
import java.util.concurrent.atomic.AtomicInteger;

public final class AudioEngine implements AutoCloseable {
  public interface Listener {
    void result(String text);

    void reading(double cents);

    void advance();

    default void mapped(int midi, int count, int needed, double average) {}
    default void nextMapped() {}

    void failed(String text);

    default void forkPaused(String text) {
      result(text);
    }

    default void playbackChanged() {}
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
  private volatile boolean speechPending;
  private volatile boolean strikeAfterSpeech;
  private volatile boolean resultSpeech;
  private volatile double navigationAfterSpeech;
  private volatile double navigationTolerance;
  private volatile long speakerAdvanceUntil;
  private volatile int generation;
  private final AtomicInteger playbackEpoch = new AtomicInteger();
  private int rate = 48000;
  private final AudioManager manager;
  private volatile Mapping mapping;

  private static final class Mapping {
    final int midi, needed;
    final boolean tap;
    final Capture capture = new Capture();
    int count;
    double sum;

    Mapping(int midi, int needed, boolean tap) { this.midi = midi; this.needed = needed; this.tap = tap; }
  }

  void configureMapping(int midi, int count, boolean tap) { mapping = new Mapping(midi, count, tap); }
  boolean mappingActive() { return mapping != null; }

  public AudioEngine(Model m, Listener l, AudioManager manager) {
    config = m.copy();
    listener = l;
    this.manager = manager;
  }

  public boolean running() {
    return running;
  }

  public synchronized boolean forkPlaying() {
    return fork != null
        && fork.getPlayState() == AudioTrack.PLAYSTATE_PLAYING
        && fork.getPlaybackHeadPosition() < 44100 * 3;
  }

  public void holdForSpeech() {
    holdForSpeech(false);
  }

  public void holdForSpeech(boolean result) {
    resultSpeech = result;
    speechPending = true;
    if (mapping == null && config.method >= 2 && running) {
      strikeAfterSpeech = true;
      stopFork();
    }
  }

  public void speechFinished() {
    long now = SystemClock.elapsedRealtime();
    muteUntil = now + 200;
    speechPending = false;
    if (strikeAfterSpeech) {
      strikeAfterSpeech = false;
      if (!paused && running && config.method == 2) {
        int pause = resultSpeech ? config.hybridPause : 0;
        if (resultSpeech && navigationAfterSpeech > 0 && config.forkTap) {
          speakerAdvanceUntil = now + (long)(config.gap * 1000) + 2200;
          navigationAfterSpeech = 0;
          repeatAt = Math.max(speakerAdvanceUntil, now + pause * 1000L);
        } else if (pause == 0) strike();
        else repeatAt = now + pause * 1000L;
      }
      else if (!paused && config.method != 1) ui.post(this::strike);
    }
    resultSpeech = false;
  }

  public void configure(Model m) {
    Model previous = config;
    boolean referenceChanged =
        previous.method != m.method
            || previous.base() != m.base()
            || previous.reference != m.reference
            || previous.interval != m.interval
            || !previous.instrumentMap.equals(m.instrumentMap)
            || !previous.forkCalibration.equals(m.forkCalibration);
    boolean measurementChanged =
        referenceChanged
            || previous.sensitivity != m.sensitivity
            || previous.correction != m.correction
            || previous.pairMode != m.pairMode
            || previous.forkTap != m.forkTap
            || previous.advance != m.advance
            || previous.gap != m.gap
            || previous.instrument != m.instrument
            || previous.swapped != m.swapped;
    config = m.copy();
    if (measurementChanged) { generation++; navigationAfterSpeech = 0; speakerAdvanceUntil = 0; }
    // Preserve the reference-decay guard when only appearance/voice/update options change.
    if (referenceChanged && m.method != 1 && (m.method == 0 || running)) strike();
    else if (referenceChanged && m.method == 1) stopFork();
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
    try {
      recorder.startRecording();
    } catch (RuntimeException error) {
      recorder.release();
      recorder = null;
      throw error;
    }
    running = true;
    if (mapping == null && config.method >= 2 && config.beginner) holdForSpeech();
    worker = new Thread(this::capture, "Stroik microphone");
    worker.start();
    if (mapping == null && config.method >= 2 && !config.beginner) strike();
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
    short[] frame = new short[4096];
    Capture state = new Capture();
    try {
      while (running) {
        int n = recorder.read(frame, 0, frame.length, AudioRecord.READ_BLOCKING);
        if (!running) break;
        if (n <= 0) throw new IllegalStateException("AudioRecord error " + n);
        process(frame, n, SystemClock.elapsedRealtime(), state);
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

  static final class Capture {
    final short[] window = new short[8192], pair = new short[65536];
    int count, pairCount, g = -1;
    long lastVisual, lastFork, lastPair;
    double storedLower;
    final Dsp.Session session = new Dsp.Session();
    final Dsp.Taps taps = new Dsp.Taps();
    final Dsp.ConfirmedStrikes confirmed = new Dsp.ConfirmedStrikes();
  }

  void process(short[] frame, int n, long now, Capture state) {
    Model m = config;
    Mapping currentMapping = mapping;
    if (currentMapping != null) {
      if (!speechPending && now >= muteUntil) captureMapping(currentMapping, m, frame, n, now);
      return;
    }
    int current = generation;
    if (state.g != current) {
      state.g = current;
      state.count = state.pairCount = 0;
      state.storedLower = 0;
      state.session.reset();
      state.taps.reset();
      state.confirmed.reset();
    }
    final int messageGeneration = state.g;
    if (m.method == 3 && m.monitor) {
      if (!headphones(manager))
        throw new IllegalStateException(
            m.t(
                "Podłącz słuchawki przed odsłuchem mikrofonu.",
                "Connect headphones before microphone monitoring."));
      writeMonitor(frame, n);
    } else stopMonitor();
    if (m.method == 2 && repeatAt > 0 && now >= repeatAt && !paused && !speechPending) {
      repeatAt = 0;
      strike();
    }
    if (!speechPending && speakerAdvanceUntil > 0 && now >= muteUntil && now < speakerAdvanceUntil) {
      double threshold = new double[] {.004, .008, .016}[m.sensitivity];
      if (state.taps.observe(Dsp.Taps.transientLevel(frame, n), threshold, now, m.gap)) {
        speakerAdvanceUntil = 0;
        repeatAt = 0;
        muteUntil = now + 1000;
        post(state.g, listener::advance);
      }
      return;
    }
    if (speechPending || now < muteUntil || (m.method == 2 && repeatAt > 0)) {
      state.count = state.pairCount = 0;
      state.session.reset();
      state.taps.reset();
      return;
    }
    if (m.method == 0
        && m.autoFork
        && !paused
        && now - state.lastFork
            >= Math.max(m.forkSeconds * 1000, m.method == 0 && m.forkTap ? 6500 : 1000)) {
      state.lastFork = now;
      strike();
      return;
    }
    double rms = Dsp.rms(frame, n),
        threshold = new double[] {.004, .008, .016}[m.sensitivity],
        frequency = 0;
    if (navigationAfterSpeech > 0) {
      state.confirmed.arm(navigationAfterSpeech, navigationTolerance, now);
      navigationAfterSpeech = 0;
    }
    if (m.forkTap && m.method == 0 && state.taps.observe(rms, threshold, now, m.gap)) {
      muteUntil = now + 2000;
      post(state.g, listener::advance);
      return;
    }
    if (m.interval != 0 && m.method != 0 && m.pairMode == 1) {
      if (rms < threshold) {
        state.pairCount = 0;
        return;
      }
      int take = Math.min(n, state.pair.length - state.pairCount);
      System.arraycopy(frame, 0, state.pair, state.pairCount, take);
      state.pairCount += take;
      if (state.pairCount == state.pair.length) {
        state.pairCount = 0;
        if (now - state.lastPair < 3000) return;
        state.lastPair = now;
        double[] v =
            m.base() + m.semitones() <= 108
                ? Dsp.together(
                    state.pair,
                    rate,
                    m.targetHz(m.base()) / Math.pow(2, m.correction / 1200),
                    m.interval)
                : null;
        if (v == null) {
          post(
              state.g,
              () ->
                  listener.result(
                      m.t(
                          "Nie wykryto dwóch składowych. Spróbuj osobno.",
                          "Two components not found. Try separately.")));
        } else {
          double correction = Math.pow(2, m.correction / 1200);
          reportInterval(
              m, Dsp.compare(v[0] * correction, v[1] * correction, m.interval), true, state.g);
        }
        muteUntil = now + 4000;
      }
      return;
    }
    if (rms >= threshold) {
      int take = Math.min(n, state.window.length);
      if (state.count + take > state.window.length) {
        int overflow = state.count + take - state.window.length;
        System.arraycopy(state.window, overflow, state.window, 0, state.count - overflow);
        state.count -= overflow;
      }
      System.arraycopy(frame, n - take, state.window, state.count, take);
      state.count += take;
      if (state.count == state.window.length)
        frequency = Dsp.pitch(state.window, rate) * Math.pow(2, m.correction / 1200);
    } else state.count = 0;
    if (m.graphics && frequency > 0 && now - state.lastVisual > 200) {
      state.lastVisual = now;
      double cents =
          1200 * Math.log(frequency / m.targetHz(m.resolved(frequency))) / Math.log(2);
      post(state.g, () -> listener.reading(cents));
    }
    if (m.method == 0) return;
    if ((m.method == 1 ? m.advance : m.method == 3 && m.forkTap)
        && m.interval == 0
        && state.confirmed.observe(rms, threshold, frequency, now, m.gap)) {
      muteUntil = now + 2000;
      state.session.reset();
      state.count = 0;
      post(state.g, listener::advance);
      return;
    }
    double done = state.session.observe(frequency, now);
    if (done <= 0) return;
    if (m.interval != 0) {
      int wanted = m.base() + (state.storedLower > 0 ? m.semitones() : 0);
      if (wanted > 108) {
        post(state.g, () -> listener.result(m.t("Wybierz niższą nutę.", "Choose a lower note.")));
        muteUntil = now + 2000;
        return;
      }
      if (Math.abs(1200 * Math.log(done / m.targetHz(wanted)) / Math.log(2)) > 90) {
        post(
            state.g,
            () ->
                listener.result(
                    m.t("Zagraj właściwą nutę: ", "Play the expected note: ") + m.note(wanted)));
        muteUntil = now + 2000;
        return;
      }
      if (state.storedLower == 0) {
        state.storedLower = done;
        post(
            state.g,
            () ->
                listener.result(
                    m.t(
                        "Dolny zapisany. Zagraj górny i wycisz.",
                        "Lower stored. Play upper and mute.")));
        muteUntil = now + 2000;
      } else {
        reportInterval(m, Dsp.compare(state.storedLower, done, m.interval), false, state.g);
        state.storedLower = 0;
        muteUntil = now + 4000;
      }
      return;
    }
    int midi = m.resolved(done);
    double expected = m.targetHz(midi),
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
            + String.format(m.locale(), "%+.1f Hz; %+.1f ", done - expected, cents)
            + m.t("centów.", "cents.");
    if (Math.abs(cents) <= m.tolerance && (m.method == 1 ? m.advance : m.forkTap)) {
      navigationTolerance = m.tolerance;
      navigationAfterSpeech = expected;
      if (m.method == 2) text += " " + m.t("Stuknij dwa razy, aby przejść dalej.", "Tap twice to advance.");
    } else navigationAfterSpeech = 0;
    if (m.method == 2) state.taps.reset();
    deliverResult(text, messageGeneration);
    if (Math.abs(cents) > m.tolerance) {
      state.confirmed.reset();
      state.taps.reset();
    }
  }

  private void reportInterval(Model m, double[] v, boolean experimental, int g) {
    String text =
        (experimental
                ? m.t("Eksperymentalny pomiar składowych. ", "Experimental component measurement. ")
                : "")
            + String.format(
                m.locale(),
                m.t(
                    "Odchyłka od stroju równomiernego: %+.1f centów. Szacowane dudnienia: %.2f na"
                        + " sekundę.",
                    "Deviation from equal temperament: %+.1f cents. Estimated beats: %.2f per"
                        + " second."),
                v[2],
                v[3]);
    deliverResult(text, g);
  }

  private void deliverResult(String text, int g) {
    holdForSpeech(true);
    ui.post(() -> {
      if (closed) return;
      if (g == generation) listener.result(text);
      else speechFinished();
    });
  }

  private void captureMapping(Mapping current, Model model, short[] frame, int n, long now) {
    Capture captured = current.capture;
    double threshold = new double[] {.004, .008, .016}[model.sensitivity];
    double rms = Dsp.rms(frame, n);
    if (current.count >= current.needed) {
      if (current.tap && captured.taps.observe(Dsp.Taps.transientLevel(frame, n), threshold, now, model.gap))
        ui.post(() -> { if (mapping == current) listener.nextMapped(); });
      return;
    }
    double frequency = 0;
    if (rms >= threshold) {
      int take = Math.min(n, captured.window.length);
      int keep = Math.min(captured.count, captured.window.length - take);
      System.arraycopy(captured.window, captured.count - keep, captured.window, 0, keep);
      System.arraycopy(frame, n - take, captured.window, keep, take);
      captured.count = keep + take;
      if (captured.count == captured.window.length)
        frequency = Dsp.pitch(captured.window, rate) * Math.pow(2, model.correction / 1200);
    } else captured.count = 0;
    double hz = captured.session.observe(frequency, now);
    if (hz <= 0) return;
    current.sum += hz;
    int count = ++current.count;
    double average = current.sum / count;
    speechPending = true;
    ui.post(() -> {
      if (mapping == current) listener.mapped(current.midi, count, current.needed, average);
      else if (mapping != null) speechFinished();
    });
  }

  public void strike() {
    if (closed || mapping != null) return;
    if (speechPending) {
      strikeAfterSpeech = true;
      return;
    }
    Model m = config;
    repeatAt = 0;
    speakerAdvanceUntil = 0;
    int base = m.base();
    if (m.interval != 0 && base + m.semitones() > 108) {
      setPaused(true);
      post(
          generation,
          () ->
              listener.forkPaused(
                  m.t(
                      "Kamerton wstrzymany: wybierz niższy dźwięk albo wyłącz współbrzmienia.",
                      "Fork paused: choose a lower target or turn consonance off.")));
      return;
    }
    long now = SystemClock.elapsedRealtime();
    if (m.method == 0 || m.method == 2) muteUntil = now + 3400;
    int playbackGeneration = generation;
    int request = playbackEpoch.incrementAndGet();
    player.execute(() -> {
      if (closed || generation != playbackGeneration || request != playbackEpoch.get()) return;
      try {
        short[] data =
            Dsp.fork(
                m.targetHz(base),
                m.interval == 0 ? 0 : m.targetHz(base + m.semitones()),
                44100, m.tone(base, "harmonics"), m.tone(base, "volume"),
                m.tone(base + m.semitones(), "harmonics"), m.tone(base + m.semitones(), "volume"), m.forkVolume);
        synchronized (this) {
          if (closed || generation != playbackGeneration || request != playbackEpoch.get()) return;
          releaseFork();
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
          if (fork.write(data, 0, data.length) != data.length)
            throw new IllegalStateException("Incomplete fork buffer");
          fork.setPlaybackPositionUpdateListener(
              new AudioTrack.OnPlaybackPositionUpdateListener() {
                public void onMarkerReached(AudioTrack track) {
                  if (request == playbackEpoch.get()) listener.playbackChanged();
                }

                public void onPeriodicNotification(AudioTrack track) {}
              },
              ui);
          fork.setNotificationMarkerPosition(data.length);
          fork.play();
          post(playbackGeneration, listener::playbackChanged);
        }
      } catch (RuntimeException error) {
        setPaused(true);
        post(
            playbackGeneration,
            () ->
                listener.forkPaused(
                    m.t("Błąd odtwarzania kamertonu: ", "Fork playback failed: ")
                        + error.getMessage()));
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
    monitor.setVolume(config.monitorVolume / 100f);
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
    playbackEpoch.incrementAndGet();
    releaseFork();
  }

  private void releaseFork() {
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
    mapping = null;
    generation++;
    repeatAt = 0;
    speakerAdvanceUntil = 0;
    speechPending = false;
    strikeAfterSpeech = false;
    resultSpeech = false;
    navigationAfterSpeech = 0;
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
