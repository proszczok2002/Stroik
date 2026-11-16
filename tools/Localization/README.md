# Shared localization

Supported codes: `pl`, `en`, `de`, `ru`, `fr`, `es`. Existing PL/EN strings are
the source. The four UTF-8 `locales/{code}.json` files map English templates to
translations shared by Windows and Android. Note/octave names have dedicated
musical conventions in `NoteCatalog.cs` and Android `Model.java`.

After changing source strings, run from the repository root:

```powershell
dotnet run --project tools/Localization/Localization.csproj
python tools/Localization/collect_android.py
python tools/Localization/check.py
python tools/Localization/generate_android.py
```

Translate every missing key before shipping. Preserve `{0}`, numeric formats,
`%.1f`, line breaks and intentional trailing spaces in concatenated messages.
`check.py` checks coverage, nonempty values, Unicode replacement characters and
format placeholders. `generate_android.py --check` prevents catalogue drift.
The generated Java catalogue needs no Android APIs, so JVM core tests can use it.

Language choice persists alongside the legacy English boolean; old PL/EN JSON
profiles remain readable. Android selects the corresponding TTS locale, Windows
selects a matching installed SAPI voice or asks NVDA to use that language. Voice
availability and pronunciation depend on the user's installed synthesizers.
System installer dialogs retain the operating system's language.

Update in-app guides, all six release-note sections and platform guides together.
Do not reuse archived index-based import helpers: the JSON files are canonical.
Device node/event checks do not establish human speech quality or natural gestures.
