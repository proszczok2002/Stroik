using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Stroik;

internal static class ReleaseRegression
{
    private const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
    private static T Field<T>(TunerForm form, string name) => (T)typeof(TunerForm).GetField(name, Hidden)!.GetValue(form)!;
    private static object? Call(TunerForm form, string method, params object[] args) => typeof(TunerForm).GetMethod(method, Hidden)!.Invoke(form, args);

    internal static void Navigation(TunerForm form)
    {
        var saved = (Preferences)Call(form, "ReadSettings")!;
        var target = Field<ComboBox>(form, "_target");
        try
        {
            foreach (bool english in new[] {false, true})
            foreach (bool automatic in new[] {false, true})
            foreach (bool open in new[] {false, true})
            {
                Call(form, "ApplySettings", new Preferences {Method = TuningMethod.TuningFork, Instrument = InstrumentKind.Piano,
                    TargetMidi = 60, PlanAnchorMidi = 60, PianoOrder = PianoOrder.Ascending, English = english,
                    AutoStrike = automatic, AutomaticUpdates = false});
                target.Focus();
                target.DroppedDown = open;
                var info = new ComboInfo { Size = Marshal.SizeOf<ComboInfo>() };
                if (!GetComboBoxInfo(target.Handle, ref info)) throw new Exception("Cannot inspect native note list.");
                IntPtr window = open ? info.List : target.Handle;
                foreach (int expected in new[] {62,64,65,67,69,71,72}) Key(Keys.Down, expected);
                foreach (int expected in new[] {71,69,67,65,64,62,60}) Key(Keys.Up, expected);
                Key(Keys.Right, 61); Key(Keys.Right, 62); Key(Keys.Left, 61); Key(Keys.Up, 60);
                target.DroppedDown = false;
                // The explicit ascending plan includes every chromatic note; arrows choose white keys separately.
                for (int expected = 61; expected <= 72; expected++)
                {
                    object?[] args = [Message.Create(target.Handle, 0x100, (IntPtr)Keys.N, IntPtr.Zero), Keys.Alt | Keys.N];
                    if (!(bool)typeof(TunerForm).GetMethod("ProcessCmdKey", Hidden)!.Invoke(form, args)!) throw new Exception("Alt+N not handled.");
                    CheckTone(expected);
                }

                void Key(Keys key, int expected)
                {
                    var message = Message.Create(window, 0x100, (IntPtr)key, IntPtr.Zero);
                    if (!Application.FilterMessage(ref message)) throw new Exception($"Native arrow {key} was not handled (open={open}, modifiers={Control.ModifierKeys}).");
                    CheckTone(expected);
                    if (target.AccessibleDescription != NoteCatalog.Spoken(expected, english)) throw new Exception("Missing immediate selected-note description.");
                    if (target.DroppedDown != open) throw new Exception("Arrow changed dropdown state.");
                }
                void CheckTone(int expected)
                {
                    if (Field<int>(form, "_targetMidi") != expected) throw new Exception($"Wrong fork target; expected MIDI {expected}.");
                    var wave = (MemoryStream?)typeof(TuningFork).GetField("_wave", Hidden)!.GetValue(Field<TuningFork>(form, "_fork"));
                    if (wave is null) throw new Exception("New target was not played.");
                    byte[] bytes = wave.ToArray();
                    var samples = new short[8192];
                    Buffer.BlockCopy(bytes, 44 + 4096 * 2, samples, 0, samples.Length * 2);
                    var measured = PitchDetector.Estimate(samples, 44100);
                    double hz = PitchDetector.NoteFromMidi(expected, 440).Frequency;
                    if (measured is null || Math.Abs(measured.Value.Frequency - hz) > 0.6)
                        throw new Exception($"Selected MIDI {expected}, synthesized {measured?.Frequency} Hz instead of {hz} Hz.");
                }
            }
            foreach (Consonance kind in new[] {Consonance.Fifth, Consonance.Octave})
            {
                Call(form, "ApplySettings", new Preferences {Method = TuningMethod.TuningFork, Consonance = kind,
                    TargetMidi = 108, AutoStrike = true, AutomaticUpdates = false});
                Call(form, "Strike");
                if (Field<System.Windows.Forms.Timer>(form, "_strikeTimer").Enabled || Field<bool>(form, "_automaticStrikesRunning"))
                    throw new Exception("Invalid interval left automatic strikes running.");
                if (typeof(TuningFork).GetField("_wave", Hidden)!.GetValue(Field<TuningFork>(form, "_fork")) is not null)
                    throw new Exception("Invalid upper note played stale fork audio.");
                int last = IntervalAnalysis.HighestTarget(kind);
                Call(form, "SelectTarget", last);
                Call(form, "AdvanceTarget", last);
                if (Field<int>(form, "_targetMidi") != last) throw new Exception("Interval plan crossed instrument boundary.");
            }
            var unrelated = Message.Create(Field<ComboBox>(form, "_method").Handle, 0x100, (IntPtr)Keys.Down, IntPtr.Zero);
            if (Application.FilterMessage(ref unrelated)) throw new Exception("Note navigation intercepted another control.");
        }
        finally { target.DroppedDown = false; Call(form, "ApplySettings", saved); }
        Console.WriteLine("Native note-list arrows, PL/EN, open/closed, manual/automatic fork, ascending wave frequencies and interval bounds: OK");
    }

    internal static async Task UpdateFiles()
    {
        string root = Path.Combine(Environment.CurrentDirectory, "dist", "verify-updater", "file-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string current = Path.Combine(root, "Stroik.exe"), stage = current + ".new-test";
        foreach (bool fail in new[] {false, true})
        {
            string backup = current + ".old-" + Guid.NewGuid().ToString("N");
            File.WriteAllText(current, "previous EXE"); File.WriteAllText(stage, "new EXE");
            try { UpdateInstaller.ReplaceFiles(stage, current, backup, () => { if (fail) throw new IOException("Simulated startup failure"); }); }
            catch (IOException) when (fail) { }
            if (File.ReadAllText(current) != (fail ? "previous EXE" : "new EXE")) throw new Exception("EXE replacement/rollback failed.");
            if (!fail && File.ReadAllText(backup) != "previous EXE") throw new Exception("Previous EXE not retained until startup.");
        }
        File.WriteAllText(stage, "new after lock");
        var held = new FileStream(current, FileMode.Open, FileAccess.Read, FileShare.Read);
        var release = Task.Run(async () => { await Task.Delay(750); held.Dispose(); });
        UpdateInstaller.ReplaceFiles(stage, current, current + ".old-" + Guid.NewGuid().ToString("N"), () => {});
        await release;
        if (File.ReadAllText(current) != "new after lock") throw new Exception("Temporary sharing lock was not retried.");
        try { UpdateInstaller.ReplaceFiles(stage, current, Path.Combine(root, "Other.exe"), () => {}); throw new Exception("Unsafe backup path accepted."); }
        catch (InvalidDataException) { }
        using var input = new MemoryStream(new byte[190000]); using var output = new MemoryStream();
        if (await Updater.CopyWithDeadlineAsync(input, output) != input.Length || output.Length != input.Length) throw new Exception("Truncated streamed copy.");
        using var stalled = new StalledStream();
        try { await Updater.CopyWithDeadlineAsync(stalled, output, TimeSpan.FromMilliseconds(60)); throw new Exception("Stalled download never timed out."); }
        catch (OperationCanceledException) { }
        Console.WriteLine("Updater replacement, startup rollback, sharing-lock retry and stalled download deadline: OK");
    }

    [StructLayout(LayoutKind.Sequential)] private struct ComboInfo
    { public int Size; public System.Drawing.Rectangle ItemRectangle, ButtonRectangle; public int ButtonState; public IntPtr Combo, Item, List; }
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetComboBoxInfo(IntPtr window, ref ComboInfo info);

    private sealed class StalledStream : Stream
    {
        public override bool CanRead => true; public override bool CanSeek => false; public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException(); public override long Position {get => 0; set => throw new NotSupportedException();}
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        { await Task.Delay(Timeout.Infinite, cancellationToken); return 0; }
        public override int Read(byte[] buffer,int offset,int count) => throw new NotSupportedException();
        public override long Seek(long offset,SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer,int offset,int count) => throw new NotSupportedException();
        public override void Flush() { }
    }
}
