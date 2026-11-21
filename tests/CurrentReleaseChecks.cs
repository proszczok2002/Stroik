using Stroik;
using System.Reflection;
using System.Windows.Forms;

internal static class CurrentReleaseChecks
{
    internal static void HybridProbe()
    {
        Exception? failure=null;
        var thread=new Thread(()=> {
            using var fixture = new PortablePreferencesChecks.Fixture();
            using var form=new TunerForm(fixture.Store());
            var saved=(Preferences)Call(form,"ReadSettings")!;
            Call(form,"ApplySettings",new Preferences {Method=TuningMethod.HybridSpeakers,View=TunerView.Classic,AutomaticUpdates=false,CheckCalibratorOnStartup=false,VisualMode=true,VoiceInVisualMode=true,Language="pl"});
            form.Shown+=async (_,_)=> {
                bool ForkPlaying()=>typeof(TuningFork).GetField("_wave",Hidden)!.GetValue(Get<TuningFork>(form,"_fork")) is not null;
                try {
                    Call(form,"StartMicrophone");
                    if(Get<Microphone?>(form,"_microphone") is null || Get<bool>(form,"_waitingForGuidance") || !ForkPlaying())throw new Exception("Classic microphone/reference did not start immediately without tutorial");
                    Console.WriteLine("Visible Classic hybrid: actual microphone open, immediate reference, no tutorial: PASS");
                    foreach(int pause in new[]{0,1,2}) {
                        Get<EditableNumber>(form,"_hybridPause").Value=pause;
                        Call(form,"ReadResult","Próba wyniku: a kontra, za nisko, minus dwa herce.",-1,55.0,5.0);
                        bool speaking=false;var deadline=DateTime.UtcNow.AddSeconds(30);
                        while(Get<bool>(form,"_waitingForResultSpeech") && DateTime.UtcNow<deadline) {
                            speaking|=NvdaReader.IsSpeaking || Get<SpeechOutput>(form,"_speech").IsSpeaking;
                            if(ForkPlaying())throw new Exception("Reference overlapped actual result speech");
                            await Task.Delay(25);
                        }
                        if(!speaking || Get<bool>(form,"_waitingForResultSpeech"))throw new Exception("Actual result speech/completion not observed");
                        var finished=DateTime.UtcNow;
                        if(pause==0 && !ForkPlaying())throw new Exception("Zero pause did not immediately start the reference");
                        if(pause>0 && ForkPlaying())throw new Exception("Configured result pause ignored");
                        while(!ForkPlaying() && DateTime.UtcNow<finished.AddSeconds(pause+1))await Task.Delay(25);
                        double elapsed=(DateTime.UtcNow-finished).TotalSeconds;
                        if(!ForkPlaying() || elapsed<pause-.15 || elapsed>pause+.4)throw new Exception("Incorrect result pause: "+elapsed);
                        Console.WriteLine($"Actual result speech then reference, pause {pause}s: PASS ({elapsed:F3}s after completion)");
                    }
                    Call(form,"ReadResult","Próba zatrzymania w trakcie odczytu wyniku.",-1,55.0,5.0);
                    await Task.Delay(100);Call(form,"StopMicrophone",false);
                    await Task.Delay(1200);
                    if(ForkPlaying() || Get<bool>(form,"_waitingForResultSpeech"))throw new Exception("Stopped result restarted the reference");
                    Console.WriteLine("Stop during result prevents reference restart: PASS. No instrument measurement or human listening claimed.");
                } catch(Exception error) {failure=error;}
                finally {Call(form,"StopMicrophone",false);Call(form,"ApplySettings",saved);form.Close();}
            };
            Application.Run(form);
        });
        thread.SetApartmentState(ApartmentState.STA);thread.Start();thread.Join();
        if(failure is not null)throw failure;
    }

    internal static void GuidanceProbe()
    {
        Exception? failure=null;
        var thread=new Thread(()=> {
            using var fixture = new PortablePreferencesChecks.Fixture();
            using var form=new TunerForm(fixture.Store());
            var saved=(Preferences)Call(form,"ReadSettings")!;
            Call(form,"ApplySettings",new Preferences {Method=TuningMethod.HybridSpeakers,View=TunerView.Beginner,AutomaticUpdates=false,CheckCalibratorOnStartup=false,VisualMode=true,VoiceInVisualMode=false,Language="pl"});
            form.Shown+=async (_,_)=> {
                try {
                    if(!NvdaReader.Available)throw new Exception("NVDA not available");
                    typeof(TunerForm).GetField("_microphone",Hidden)!.SetValue(form,new Microphone());
                    typeof(TunerForm).GetField("_waitingForGuidance",Hidden)!.SetValue(form,true);
                    Get<TextBox>(form,"_methodHelp").Text="Stroik. Próba oczekiwania na koniec wskazówek. Kamerton powinien zagrać dopiero po tej wypowiedzi.";
                    Get<TextBox>(form,"_beginnerHint").Text="Koniec wskazówek testowych.";
                    Call(form,"ReadGuidanceBeforeFork");
                    bool speaking=false;DateTime deadline=DateTime.UtcNow.AddSeconds(30);
                    while(Get<bool>(form,"_waitingForGuidance") && DateTime.UtcNow<deadline) {
                        speaking|=NvdaReader.IsSpeaking;
                        if(typeof(TuningFork).GetField("_wave",Hidden)!.GetValue(Get<TuningFork>(form,"_fork")) is not null)throw new Exception("Fork started before guidance completed");
                        await Task.Delay(50);
                    }
                    bool waiting=Get<bool>(form,"_waitingForGuidance");
                    bool playing=typeof(TuningFork).GetField("_wave",Hidden)!.GetValue(Get<TuningFork>(form,"_fork")) is not null;
                    if(!speaking || waiting || !playing)throw new Exception($"Guidance did not release reference playback: speaking={speaking}, waiting={waiting}, playing={playing}");
                    Call(form,"StopMicrophone",false);
                    Console.WriteLine("Actual NVDA guidance in visible form: fork silent during speech, started after completion. No microphone/instrument measurement performed.");
                } catch(Exception error) {failure=error;}
                finally {Call(form,"ApplySettings",saved);form.Close();}
            };
            Application.Run(form);
        });
        thread.SetApartmentState(ApartmentState.STA);thread.Start();thread.Join();
        if(failure is not null)throw failure;
    }
    private const BindingFlags Hidden=BindingFlags.NonPublic|BindingFlags.Instance;
    private static T Get<T>(TunerForm form,string name)=>(T)typeof(TunerForm).GetField(name,Hidden)!.GetValue(form)!;
    private static object? Call(TunerForm form,string method,params object[] args)=>typeof(TunerForm).GetMethod(method,Hidden)!.Invoke(form,args);
    internal static void Run(TunerForm form)
    {
        var saved=(Preferences)Call(form,"ReadSettings")!;
        try {
            foreach(string code in UiLanguages.Codes) {
                if(ReleaseNotes.Current(code).Length<300)throw new Exception("Missing localized release notes: "+code);
                foreach(TunerView view in Enum.GetValues<TunerView>()) {
                    Call(form,"ApplySettings",new Preferences {Language=code,View=view,Method=TuningMethod.HybridSpeakers,AutomaticUpdates=false,VisualMode=true,VoiceInVisualMode=false,HybridPauseSeconds=13});
                    var roundtrip=(Preferences)Call(form,"ReadSettings")!;
                    if(roundtrip.Language!=code || roundtrip.HybridPauseSeconds!=13)throw new Exception("Language/pause profile round trip: "+code);
                    var pause=Get<EditableNumber>(form,"_hybridPause");
                    pause.Value=3;
                    foreach(int expected in new[]{2,1,0}) {
                        typeof(EditableNumber).GetMethod("OnKeyDown",Hidden)!.Invoke(pause,new object[]{new KeyEventArgs(Keys.Down)});
                        if(pause.Value!=expected || ((Preferences)Call(form,"ReadSettings")!).HybridPauseSeconds!=expected)throw new Exception("Hybrid pause arrow cannot reach "+expected);
                    }
                    pause.Text="2";typeof(EditableNumber).GetMethod("OnKeyDown",Hidden)!.Invoke(pause,new object[]{new KeyEventArgs(Keys.Enter)});
                    if(pause.Value!=2)throw new Exception("Hybrid pause numeric entry");
                    if(Get<ComboBox>(form,"_language").Items.Count!=6)throw new Exception("Six languages missing");
                    if(Get<ComboBox>(form,"_target").SelectedItem!.ToString()!.Contains(KeyboardGuide.Position(roundtrip.TargetMidi,code))!=(view==TunerView.Beginner))throw new Exception("Classic description leak: "+code);
                    var input=Get<EditableNumber>(form,"_concertA");
                    if(input.AccessibilityObject.Role!=AccessibleRole.Text || input.Controls.Count!=0 || input.AccessibilityObject.Value!=input.Text)throw new Exception("Numeric input duplicates its value or spin role");
                    if(string.IsNullOrEmpty(input.AccessibleDescription)!=(view==TunerView.Classic))throw new Exception("Arrow hint view mismatch");
                    input.Text="4";
                    _=input.Value;
                    if(input.Text!="4")throw new Exception("Reading a numeric value rewrites partially typed input");
                    input.Text="440";form.ValidateChildren();
                    foreach(string help in new[]{"_methodHelp","_beginnerHint"}) {
                        var box=Get<TextBox>(form,help);
                        if(!box.TabStop || !box.ReadOnly || box.Text.Length<40 || box.Visible!=(view==TunerView.Beginner))throw new Exception("Keyboard guidance inaccessible: "+help);
                    }
                    if(!string.IsNullOrEmpty(Get<CheckBox>(form,"_autoStrike").AccessibleDescription))throw new Exception("Verbose automatic checkbox");
                }
                var language=Get<ComboBox>(form,"_language");
                language.SelectedIndex=(Array.IndexOf(UiLanguages.Codes,code)+1)%6;
                if(((Preferences)Call(form,"ReadSettings")!).Language!=UiLanguages.Codes[language.SelectedIndex])throw new Exception("Language selector does not save");
            }
            Call(form,"ApplySettings",new Preferences {Method=TuningMethod.TuningFork,ForkTapAdvance=true,AutoStrike=true,AutomaticUpdates=false});
            if(Get<Button>(form,"_start").Visible)throw new Exception("Fork exposes mic controls");
            // Exercise the timer's real command independently of a microphone driver.
            typeof(TunerForm).GetField("_automaticStrikesRunning",Hidden)!.SetValue(form,true);
            Call(form,"UpdateStrikeTimer");
            if(!Get<System.Windows.Forms.Timer>(form,"_strikeTimer").Enabled || Get<System.Windows.Forms.Timer>(form,"_strikeTimer").Interval<6500)throw new Exception("Tap listening disables fork automation or leaves no listening gap");
            var tap=new TapNavigator();
            DateTime at=DateTime.UtcNow;
            if(tap.Observe(.1,.01,at,1200) || tap.Observe(.1,.01,at.AddMilliseconds(300),1200))throw new Exception("Mic startup counted as a gesture");
            tap.Observe(0,.01,at.AddSeconds(1),1200);tap.Observe(0,.01,at.AddSeconds(1.2),1200);
            if(tap.Observe(.1,.01,at.AddSeconds(1.3),1200))throw new Exception("Single attack advanced");
            tap.Observe(0,.01,at.AddSeconds(1.5),1200);
            if(!tap.Observe(.08,.01,at.AddSeconds(2),1200))throw new Exception("Unequal two attacks not recognized");
            var impulseNavigator = new TapNavigator();
            short[] capture = new short[48000 * 2];
            for (int i = 4800; i < 17000; i++)
                capture[i] = (short)(8000 * Math.Sin(i * .32));
            for (int i = 24000; i < 27840; i++)
                capture[i] = (short)(8000 * Math.Sin(i * .17));
            if (impulseNavigator.ObserveImpulses(capture, 48000, .008, at, 1200))
                throw new Exception("A note followed by a vocal-like swell advanced the fork.");
            Array.Clear(capture);
            for (int i = 4800; i < 5200; i++) capture[i] = (short)(i % 2 == 0 ? 8000 : -8000);
            if (impulseNavigator.ObserveImpulses(capture, 48000, .008, at.AddSeconds(2), 1200))
                throw new Exception("A single impulsive tap advanced the fork.");
            impulseNavigator.Reset();
            for (int i = 24000; i < 24500; i++) capture[i] = (short)(i % 2 == 0 ? 8000 : -8000);
            if (!impulseNavigator.ObserveImpulses(capture, 48000, .008, at.AddSeconds(4), 1200))
                throw new Exception("Two genuine impulsive taps did not advance the fork.");
            if (ForkCalibration.Effective(null, 21) != new ForkTone(200, 150, 50)
                || ForkCalibration.Effective(null, 89) != ForkTone.Default
                || ForkCalibration.Effective(new() { Notes = new() { [21] = new ForkTone(80, 95) } }, 21) != new ForkTone(80, 95, 50))
                throw new Exception("Factory calibration or legacy override");
            Call(form, "ApplySettings", new Preferences { AutomaticUpdates = false });
            var defaults = (Preferences)Call(form, "ReadSettings")!;
            if (defaults.Method != TuningMethod.HybridSpeakers || defaults.TargetMidi != 21)
                throw new Exception("Initial ascending piano target or tuning method");
            Call(form, "NavigateTargetKey", Keys.C);
            Call(form, "NavigateTargetKey", Keys.D4);
            if (((Preferences)Call(form, "ReadSettings")!).TargetMidi != 60) throw new Exception("C4 note jump");
            Call(form, "NavigateTargetKey", Keys.Control | Keys.Up);
            if (((Preferences)Call(form, "ReadSettings")!).TargetMidi != 72) throw new Exception("Ctrl+Up octave jump");
            Console.WriteLine("Short acoustic impulses, factory preset, C4 and octave navigation: OK");
            var cycle=new PracticeCycle();cycle.Result(at,13);cycle.Speech(at.AddSeconds(20),13);
            if(cycle.NeedsFork(at.AddSeconds(32)) || !cycle.NeedsFork(at.AddSeconds(33)))throw new Exception("Hybrid fails to wait after speech completion");
            int black=60;
            foreach(int expected in new[]{61,63,66,68,70,73}) {black=NoteCatalog.NextBlack(black,1);if(black!=expected)throw new Exception("Black-key navigation");}
            foreach(int expected in new[]{70,68,66,63,61}) {black=NoteCatalog.NextBlack(black,-1);if(black!=expected)throw new Exception("Reverse black-key navigation");}
            if(NoteCatalog.NextBlack(21,-1)!=21 || NoteCatalog.NextBlack(108,1)!=108)throw new Exception("Black-key bounds");
            foreach(int midi in Enumerable.Range(21,88))
            foreach(int span in new[]{0,7,12}) {
                if(midi+span>108)continue;
                using var wave=TuningFork.BuildWave(PitchDetector.NoteFromMidi(midi,440).Frequency,span==0?null:PitchDetector.NoteFromMidi(midi+span,440).Frequency, ForkCalibration.Effective(null,midi), ForkCalibration.Effective(null,midi+span));
                byte[] data=wave.ToArray();int peak=0;
                for(int i=44;i<data.Length;i+=2)peak=Math.Max(peak,Math.Abs((int)BitConverter.ToInt16(data,i)));
                if(peak>=32760 || peak<500)throw new Exception("Fork clipping/silence: "+midi+"/"+span+" peak="+peak);
            }
            foreach(int midi in new[]{21,22,32,33,34,35}) {
                double frequency=PitchDetector.NoteFromMidi(midi,440).Frequency;
                double partial=0,fundamental=0;
                const int count=44100;
                for(int i=0;i<count;i++) {double t=i/44100.0;double sample=TuningFork.Oscillator(frequency,t);partial+=sample*Math.Sin(2*Math.PI*frequency*4*t);fundamental+=sample*Math.Sin(2*Math.PI*frequency*t);}
                if(partial/fundamental<.2)throw new Exception("Lowest register lacks audible upper partials");
            }
            CheckMappingAndSpeakerAdvance(form);
            Console.WriteLine("Six languages, accessible edits/guidance, fork controls, quiet startup, speech pause, black keys and 245 unclipped fork combinations: OK");
        } finally { Call(form,"ApplySettings",saved); }
    }

    private static short[] Tone(double hz, int frame) => Enumerable.Range(0, 4096)
        .Select(i => (short)(9000 * Math.Sin(2 * Math.PI * hz * (i + frame * 4096) / 48000))).ToArray();

    private static void CheckMappingAndSpeakerAdvance(TunerForm form)
    {
        Call(form,"ApplySettings",new Preferences { Language="en", Method=TuningMethod.HybridSpeakers, TargetMidi=69,
            ForkTapAdvance=true, VisualMode=true, VoiceInVisualMode=false, AutomaticUpdates=false });
        var tabs = Get<TabControl>(form,"_tabs");
        tabs.SelectedTab = Get<TabPage>(form,"_mappingTab");
        Get<ComboBox>(form,"_mapCount").SelectedIndex = 1;
        typeof(TunerForm).GetField("_mapMic",Hidden)!.SetValue(form,new Microphone());
        var at = DateTime.UtcNow;
        foreach (double hz in new[]{421.0, 433.0})
        {
            for (int frame=0; frame<8; frame++) { Call(form,"CaptureMappingAt",Tone(hz,frame),48000,at); at=at.AddMilliseconds(86); }
            for (int frame=0; frame<8; frame++) { Call(form,"CaptureMappingAt",new short[4096],48000,at); at=at.AddMilliseconds(86); }
            Application.DoEvents();
            if (hz == 421 && Get<InstrumentMap>(form,"_mapDraft").Notes.ContainsKey(69)) throw new Exception("Mapping committed before the requested strike count");
        }
        double average = Get<InstrumentMap>(form,"_mapDraft").Notes[69];
        if (Math.Abs(average-427)>.15) throw new Exception("Mapping lost selected note or averaged incorrectly: "+average);
        var useMap = Get<RadioButton>(form,"_mapUse");
        var standard = Get<RadioButton>(form,"_mapClear");
        useMap.PerformClick();
        if (Math.Abs((double)Call(form,"TargetFrequency",69,440.0)!-427)>.15 || (double)Call(form,"TargetFrequency",81,440.0)! != 880)
            throw new Exception("Active partial map did not supply reference/fallback");
        standard.PerformClick();
        var inactive = (Preferences)Call(form, "ReadSettings")!;
        if (!standard.Checked || useMap.Checked || inactive.UseInstrumentMap || inactive.InstrumentMap?.Notes[69] != average ||
            (double)Call(form, "TargetFrequency", 69, 440.0)! != 440.0)
            throw new Exception("Standard tuning removed the map or left it enabled.");
        useMap.PerformClick();
        if (!useMap.Checked || standard.Checked) throw new Exception("Map option does not expose its selection.");
        tabs.SelectedTab = Get<TabPage>(form,"_tunerTab");
        typeof(TunerForm).GetField("_microphone",Hidden)!.SetValue(form,new Microphone());
        at = DateTime.UtcNow;
        Call(form,"ReadResult","In tune",69,427.0,5.0);
        var silence = new short[4096];
        var random = new Random(42);
        var tap = new short[4096];
        for (int i=0;i<96;i++) tap[300+i]=(short)random.Next(-1900,1901);
        foreach(int offset in new[]{0,86,172,258})Call(form,"ProcessFrameAt",silence,at.AddMilliseconds(offset));
        Call(form,"ProcessFrameAt",tap,at.AddMilliseconds(400));
        Call(form,"ProcessFrameAt",silence,at.AddMilliseconds(500));
        Call(form,"ProcessFrameAt",tap,at.AddMilliseconds(700));
        Application.DoEvents();
        if (Get<int>(form,"_targetMidi")!=70) throw new Exception("Speaker hybrid missed two acoustic taps after a good result");
        Call(form,"StopMicrophone",false);
        var preset = new ForkCalibration { Notes = new() { [69] = new ForkTone(21, 67) } };
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid()+".json");
        try { TuningFiles.Write(path,preset); var restored=TuningFiles.Read<ForkCalibration>(path,preset.Format); restored.Validate(); if(restored.Tone(69)!=preset.Tone(69)||restored.Tone(21)!=ForkTone.Default)throw new Exception("Calibration JSON round trip"); }
        finally { File.Delete(path); }
        Console.WriteLine("Mapped two out-of-standard readings by chosen MIDI, reused mean and fallback, advanced on two acoustic taps, calibration JSON round trip: OK");
    }
}
