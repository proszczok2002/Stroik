param([Parameter(Mandatory=$true)][string]$Serial)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$adb='F:\YouTube Downloader by Proszczok\.android-sdk\platform-tools\adb.exe'
$candidate=Get-ChildItem -LiteralPath (Join-Path $root 'releases') -File -Filter '*.apk' | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if(-not$candidate){throw 'No candidate APK'}
$expected=(Get-FileHash -LiteralPath $candidate.FullName).Hash.ToLowerInvariant()
& $adb -s $Serial install -r (Join-Path $root 'build\device-tests.apk')
if($LASTEXITCODE -ne 0){throw 'Test harness installation failed'}
$baseline=& $adb -s $Serial shell am instrument -w -e scenario prepare-update pl.proszczok.stroik.tests/pl.proszczok.stroik.DeviceTests
if(($baseline -join "`n") -match 'FAIL:'){throw 'Cannot capture settings baseline'}
$directory=Join-Path $root ('device-results\install-'+(Get-Date -Format yyyyMMdd-HHmmss))
New-Item -ItemType Directory -Force -Path $directory | Out-Null
$operation=Start-Process $adb -WindowStyle Hidden -ArgumentList @('-s',$Serial,'shell','am','instrument','-w','-e','scenario','install-local','pl.proszczok.stroik.tests/pl.proszczok.stroik.DeviceTests') -PassThru -RedirectStandardOutput (Join-Path $directory 'install.txt') -RedirectStandardError (Join-Path $directory 'error.txt')
$null=$operation.Handle
$deadline=[DateTime]::UtcNow.AddSeconds(90)
$trace=New-Object 'System.Collections.Generic.List[string]'
$hash=''
while([DateTime]::UtcNow -lt $deadline){
 $path=((& $adb -s $Serial shell pm path pl.proszczok.stroik) -join '').Trim().Substring(8)
 $hash=((& $adb -s $Serial shell sha256sum $path) -split '\s+')[0]
 if($hash -eq $expected){break}
 $ui=(& $adb -s $Serial shell am instrument -w pl.proszczok.stroik.tests/pl.proszczok.stroik.UiBridge) -join "`n"
 $trace.Add($ui)
 if($ui -match 'com.google.android.packageinstaller.*text=Zaktualizuj \|'){
   $null=& $adb -s $Serial shell am instrument -w -e text Zaktualizuj -e package com.google.android.packageinstaller pl.proszczok.stroik.tests/pl.proszczok.stroik.UiBridge
   'Confirmed visible Android update dialog'
 }elseif($ui -match 'com.android.vending.*text=Skanuj aplikację \|'){
   $null=& $adb -s $Serial shell "am instrument -w -e text 'Skanuj aplikację' -e package com.android.vending pl.proszczok.stroik.tests/pl.proszczok.stroik.UiBridge"
   'Requested visible Play Protect scan'
 }elseif($ui -match 'Ta aplikacja wygląda bezpiecznie' -and $ui -match 'com.android.vending.*text=Zainstaluj \|'){
   $null=& $adb -s $Serial shell am instrument -w -e text Zainstaluj -e package com.android.vending pl.proszczok.stroik.tests/pl.proszczok.stroik.UiBridge
   'Confirmed visible installation after safe scan result'
 }elseif($ui -match 'Zablokowano|szkodliw|niebezpieczna'){
   throw 'Security warning encountered; installer action stopped'
 }
 Start-Sleep -Milliseconds 500
}
[IO.File]::WriteAllLines((Join-Path $directory 'ui-trace.txt'),$trace,(New-Object System.Text.UTF8Encoding($false)))
if($hash -ne $expected){throw 'Expected APK not installed; unlock the phone or inspect current system dialog'}
& $adb -s $Serial pull $path (Join-Path $directory 'installed.apk')
if((Get-FileHash -LiteralPath (Join-Path $directory 'installed.apk')).Hash.ToLowerInvariant() -ne $expected){throw 'Installed APK mismatch'}
$verified=& $adb -s $Serial shell am instrument -w -e scenario verify-update pl.proszczok.stroik.tests/pl.proszczok.stroik.DeviceTests
[IO.File]::WriteAllLines((Join-Path $directory 'persistence.txt'),[string[]]$verified,(New-Object System.Text.UTF8Encoding($false)))
$verified
if(($verified -join "`n") -match 'FAIL:'){throw 'Settings/signing persistence check failed'}
"Installed candidate: $expected; evidence: $directory"
