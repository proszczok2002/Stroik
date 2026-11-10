param(
    [string]$Sdk = 'F:\YouTube Downloader by Proszczok\.android-sdk',
    [string]$JavaBin = 'C:\Program Files\Java\jdk-25.0.2\bin',
    [switch]$Ci,
    [switch]$RebuildSameVersion
)
$ErrorActionPreference='Stop'
$root=$PSScriptRoot
[xml]$manifest=Get-Content -LiteralPath (Join-Path $root 'app\src\main\AndroidManifest.xml')
$version=$manifest.manifest.GetAttribute('versionName','http://schemas.android.com/apk/res/android')
$code=$manifest.manifest.GetAttribute('versionCode','http://schemas.android.com/apk/res/android')
& (Join-Path $root 'test-core.ps1') -JavaBin $JavaBin
if($LASTEXITCODE -ne 0){throw 'Core tests failed'}
$output=Join-Path $root 'releases'
$name="Stroik-Android-$version.apk"
if((Test-Path -LiteralPath (Join-Path $output $name)) -and -not $RebuildSameVersion){throw 'Already built. Increase version or explicitly rebuild before publication.'}
$stage=Join-Path $root ('build\stage-'+[guid]::NewGuid().ToString('N'))
foreach($folder in @($stage,(Join-Path $stage 'classes'),(Join-Path $stage 'generated'),(Join-Path $stage 'dex'),$output)){New-Item -ItemType Directory -Force -Path $folder | Out-Null}
$toolsDir=Join-Path $Sdk 'build-tools\36.0.0'
$androidJar=Join-Path $Sdk 'platforms\android-36\android.jar'
function Check([string]$step){if($LASTEXITCODE -ne 0){throw "$step failed: $LASTEXITCODE"}}
& (Join-Path $toolsDir 'aapt2.exe') compile --dir (Join-Path $root 'app\src\main\res') -o (Join-Path $stage 'resources.zip'); Check 'Resources'
& (Join-Path $toolsDir 'aapt2.exe') link -o (Join-Path $stage 'unsigned.apk') -I $androidJar --manifest (Join-Path $root 'app\src\main\AndroidManifest.xml') --java (Join-Path $stage 'generated') -A (Join-Path $root 'app\src\main\assets') (Join-Path $stage 'resources.zip'); Check 'Link'
$sources=@(Get-ChildItem -LiteralPath (Join-Path $root 'app\src\main\java') -Recurse -Filter '*.java' | ForEach-Object FullName)
$sources+=@(Get-ChildItem -LiteralPath (Join-Path $stage 'generated') -Recurse -Filter '*.java' | ForEach-Object FullName)
& (Join-Path $JavaBin 'javac.exe') --release 17 -encoding UTF-8 -classpath $androidJar -d (Join-Path $stage 'classes') @sources; Check 'Java'
& (Join-Path $JavaBin 'jar.exe') cf (Join-Path $stage 'classes.jar') -C (Join-Path $stage 'classes') .; Check 'Jar'
& (Join-Path $toolsDir 'd8.bat') --min-api 30 --lib $androidJar --output (Join-Path $stage 'dex') (Join-Path $stage 'classes.jar'); Check 'Dex'
& (Join-Path $JavaBin 'jar.exe') uf (Join-Path $stage 'unsigned.apk') -C (Join-Path $stage 'dex') classes.dex; Check 'Add dex'
& (Join-Path $toolsDir 'zipalign.exe') -f 4 (Join-Path $stage 'unsigned.apk') (Join-Path $stage 'aligned.apk'); Check 'Align'
$signing=Join-Path $root '.signing'
New-Item -ItemType Directory -Force -Path $signing | Out-Null
$key=Join-Path $signing $(if($Ci){'ci-only.jks'}else{'stroik-release.jks'})
$password=Join-Path $signing $(if($Ci){'ci-password.txt'}else{'password.txt'})
if(-not(Test-Path -LiteralPath $key)){
    if(-not(Test-Path -LiteralPath $password)){
        $random=New-Object byte[] 32
        [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($random)
        [IO.File]::WriteAllText($password,[Convert]::ToBase64String($random),[Text.Encoding]::ASCII)
    }
    & (Join-Path $JavaBin 'keytool.exe') -genkeypair -keystore $key -alias stroik -storepass:file $password -keypass:file $password -keyalg RSA -keysize 3072 -validity 10000 -dname 'CN=Proszczok, OU=Stroik Android, O=Proszczok, C=PL'; Check 'Signing key'
}
$apk=Join-Path $stage $name
& (Join-Path $toolsDir 'apksigner.bat') sign --ks $key --ks-key-alias stroik --ks-pass "file:$password" --out $apk (Join-Path $stage 'aligned.apk'); Check 'Sign'
& (Join-Path $toolsDir 'apksigner.bat') verify --verbose $apk; Check 'Verify'
$hash=(Get-FileHash -LiteralPath $apk -Algorithm SHA256).Hash.ToLowerInvariant()
[IO.File]::WriteAllText((Join-Path $stage "$name.sha256"),"$hash  $name`n",[Text.Encoding]::ASCII)
if(-not $Ci){
    $archive=[IO.Path]::GetFullPath((Join-Path $root ("..\niepotrzebne\android-"+(Get-Date -Format 'yyyyMMdd-HHmmss')+"-$PID")))
    $workspace=[IO.Path]::GetFullPath((Join-Path $root '..')).TrimEnd('\')+'\'
    if(-not $archive.StartsWith($workspace,[StringComparison]::OrdinalIgnoreCase)){throw 'Unsafe archive path'}
    foreach($old in Get-ChildItem -LiteralPath $output -File){
        $absolute=(Resolve-Path -LiteralPath $old.FullName).ProviderPath
        if(-not $absolute.StartsWith($workspace,[StringComparison]::OrdinalIgnoreCase)){throw 'Unsafe artifact path'}
        New-Item -ItemType Directory -Force -Path $archive | Out-Null
        Move-Item -LiteralPath $absolute -Destination (Join-Path $archive $old.Name)
    }
}
Copy-Item -LiteralPath $apk,(Join-Path $stage "$name.sha256") -Destination $output
foreach($doc in @('README.md','README.en.md')){Copy-Item -LiteralPath (Join-Path $root $doc) -Destination $output}
Copy-Item -LiteralPath (Join-Path $root '..\LICENSE') -Destination $output
[IO.File]::WriteAllText((Join-Path $output 'build-version.txt'),"$version ($code)`n",[Text.Encoding]::ASCII)
Write-Output "APK $version ($code): $hash"
