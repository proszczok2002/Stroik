param([string]$JavaBin='C:\Program Files\Java\jdk-25.0.2\bin', [string]$Sdk=$(if($env:ANDROID_HOME){$env:ANDROID_HOME}else{'F:\YouTube Downloader by Proszczok\.android-sdk'}))
$ErrorActionPreference='Stop'
$out=Join-Path $PSScriptRoot 'build\core-tests'
New-Item -ItemType Directory -Force -Path $out | Out-Null
$jar=Join-Path $Sdk 'platforms\android-36\android.jar'
& (Join-Path $JavaBin 'javac.exe') --release 17 -encoding UTF-8 -classpath $jar -d $out (Join-Path $PSScriptRoot 'app\src\main\java\pl\proszczok\stroik\Dsp.java') (Join-Path $PSScriptRoot 'app\src\main\java\pl\proszczok\stroik\Model.java') (Join-Path $PSScriptRoot 'tests\CoreChecks.java')
if($LASTEXITCODE -ne 0){throw 'Core test compile failed'}
& (Join-Path $JavaBin 'java.exe') -cp "$out;$jar" pl.proszczok.stroik.CoreChecks
if($LASTEXITCODE -ne 0){throw 'Core tests failed'}
