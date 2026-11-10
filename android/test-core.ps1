param([string]$JavaBin='C:\Program Files\Java\jdk-25.0.2\bin')
$ErrorActionPreference='Stop'
$out=Join-Path $PSScriptRoot 'build\core-tests'
New-Item -ItemType Directory -Force -Path $out | Out-Null
& (Join-Path $JavaBin 'javac.exe') --release 17 -encoding UTF-8 -d $out (Join-Path $PSScriptRoot 'app\src\main\java\pl\proszczok\stroik\Dsp.java') (Join-Path $PSScriptRoot 'tests\CoreChecks.java')
if($LASTEXITCODE -ne 0){throw 'Core test compile failed'}
& (Join-Path $JavaBin 'java.exe') -cp $out pl.proszczok.stroik.CoreChecks
if($LASTEXITCODE -ne 0){throw 'Core tests failed'}
