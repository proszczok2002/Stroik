# Stroik Android

- Keep the Windows release channel intact. Android releases use `android-v...` tags with `gh release create --latest=false`; never mark an Android release as GitHub's latest Windows release.
- Android uses the raw `android/update.json` manifest. Publish signed APK and checksum, download and compare the public files, then update the manifest last.
- Preserve the release key/password under ignored `.signing`. Do not print, commit or publish these files. CI uses a separate disposable key; CI APKs cannot update production installs.
- Keep only the newest APK, checksum, guides, license and build marker under `releases`. Archive previous files reversibly under the project-root `niepotrzebne` folder. Preserve editable source, SDK access and build scripts.
- Always use Android's system confirmation for app updates. Do not replace an updater test with silent `adb install -r`.
- Use the actual connected device for UI checks. Keep TalkBack services enabled during instrumentation. Distinguish geometry/label/event checks from human speech/gesture listening and real instrument tests.
- Record incomplete device checks explicitly. Do not publish the requested 1.0 and 1.01 updater test releases before the necessary device installation/testing gate is resolved.
