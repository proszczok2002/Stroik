param([string]$Sdk='F:\YouTube Downloader by Proszczok\.android-sdk',[string]$JavaBin='C:\Program Files\Java\jdk-25.0.2\bin')
$ErrorActionPreference='Stop'
$root=$PSScriptRoot
$stage=Join-Path $root ('build\tests-'+[guid]::NewGuid().ToString('N'))
foreach($dir in @($stage,(Join-Path $stage 'classes'),(Join-Path $stage 'dex'))){New-Item -ItemType Directory -Force -Path $dir | Out-Null}
$toolsDir=Join-Path $Sdk 'build-tools\36.0.0'
$jar=Join-Path $Sdk 'platforms\android-36\android.jar'
$main=Get-ChildItem (Join-Path $root 'build') -Directory -Filter 'stage-*' | Sort-Object LastWriteTime -Descending | Select-Object -First 1
function Check([string]$step){if($LASTEXITCODE -ne 0){throw "$step failed $LASTEXITCODE"}}
& (Join-Path $toolsDir 'aapt2.exe') link -o (Join-Path $stage 'unsigned.apk') -I $jar --manifest (Join-Path $root 'tests\AndroidManifest.xml'); Check 'Test manifest'
& (Join-Path $JavaBin 'javac.exe') --release 17 -encoding UTF-8 -classpath "$jar;$($main.FullName)\classes.jar" -d (Join-Path $stage 'classes') (Join-Path $root 'tests\DeviceTests.java') (Join-Path $root 'tests\UiBridge.java'); Check 'Test compile'
& (Join-Path $JavaBin 'jar.exe') cf (Join-Path $stage 'classes.jar') -C (Join-Path $stage 'classes') .; Check 'Test jar'
& (Join-Path $toolsDir 'd8.bat') --min-api 30 --lib $jar --classpath (Join-Path $main.FullName 'classes.jar') --output (Join-Path $stage 'dex') (Join-Path $stage 'classes.jar'); Check 'Test dex'
& (Join-Path $JavaBin 'jar.exe') uf (Join-Path $stage 'unsigned.apk') -C (Join-Path $stage 'dex') classes.dex; Check 'Test package'
& (Join-Path $toolsDir 'zipalign.exe') -f 4 (Join-Path $stage 'unsigned.apk') (Join-Path $stage 'aligned.apk'); Check 'Test align'
& (Join-Path $toolsDir 'apksigner.bat') sign --ks (Join-Path $root '.signing\stroik-release.jks') --ks-key-alias stroik --ks-pass ("file:"+(Join-Path $root '.signing\password.txt')) --out (Join-Path $root 'build\device-tests.apk') (Join-Path $stage 'aligned.apk'); Check 'Test sign'
