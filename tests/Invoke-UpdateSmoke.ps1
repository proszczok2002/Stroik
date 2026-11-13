param([switch]$LegacyClient)
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$id = [guid]::NewGuid().ToString('N')
$fixture = Join-Path $root ("dist\verify-updater\process-$id\Folder z odstepem " + [char]0x142)
$download = Join-Path $env:LOCALAPPDATA "Stroik\updates\$id"
if (-not $fixture.StartsWith($root + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Fixture outside workspace' }
if ([IO.Path]::GetDirectoryName($download) -ne (Join-Path $env:LOCALAPPDATA 'Stroik\updates')) { throw 'Invalid download fixture' }
New-Item -ItemType Directory -Path $fixture, $download -Force | Out-Null
$current = Join-Path $fixture 'Stroik.exe'
$source = Join-Path $root 'dist\Stroik.exe'
$expected = (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash
Copy-Item -LiteralPath $source -Destination (Join-Path $download 'Stroik.exe')
$settings = Join-Path $env:LOCALAPPDATA 'Stroik\settings.json'
$before = if (Test-Path -LiteralPath $settings) { [IO.File]::ReadAllBytes($settings) } else { $null }
$env:DOTNET_ROOT = Join-Path $root '.dotnet10'
$client = $null
$helper = $null
$installed = $null
try {
    if ($LegacyClient) {
        # The prior release's protocol starts a helper with three arguments.
        # A lock-holding test process stands in for that old client's lifetime.
        Copy-Item -LiteralPath $source -Destination $current
        $holder = Join-Path $fixture 'holder.ps1'
        [IO.File]::WriteAllText($holder, @'
param($target)
$file = [IO.File]::Open($target, 'Open', 'Read', 'Read')
[IO.File]::WriteAllText($target + '.locked', 'ready')
while (-not (Test-Path -LiteralPath ($target + '.release'))) { Start-Sleep -Milliseconds 100 }
$file.Dispose()
'@)
        $client = Start-Process powershell.exe -WindowStyle Hidden -ArgumentList @('-NoProfile','-ExecutionPolicy','Bypass','-File', ('"' + $holder + '"'), ('"' + $current + '"')) -PassThru
        $null = $client.Handle
        $deadline = [DateTime]::UtcNow.AddSeconds(15)
        while (-not (Test-Path -LiteralPath ($current + '.locked'))) {
            if ([DateTime]::UtcNow -gt $deadline -or $client.HasExited) { throw 'Lock holder failed' }
            Start-Sleep -Milliseconds 150
        }
        $helper = Start-Process (Join-Path $download 'Stroik.exe') -WindowStyle Hidden -ArgumentList @('--apply-update', ('"' + $current + '"'), $client.Id) -PassThru
        Start-Sleep -Milliseconds 750
        [IO.File]::WriteAllText($current + '.release', 'exit')
    } else {
        $testOutput = Join-Path $root 'tests\bin\Release\net10.0-windows'
        foreach ($name in @('PitchChecks.dll','PitchChecks.runtimeconfig.json','PitchChecks.deps.json')) {
            Copy-Item -LiteralPath (Join-Path $testOutput $name) -Destination $fixture
        }
        Copy-Item -LiteralPath (Join-Path $testOutput 'PitchChecks.exe') -Destination $current
        $client = Start-Process $current -WindowStyle Hidden -ArgumentList @('--replacement-probe', ('"' + (Join-Path $download 'Stroik.exe') + '"')) -PassThru -RedirectStandardOutput (Join-Path $fixture 'client.log') -RedirectStandardError (Join-Path $fixture 'client-error.log')
        $null = $client.Handle
    }
    $deadline = [DateTime]::UtcNow.AddSeconds(100)
    do {
        Start-Sleep -Milliseconds 300
        $installed = Get-Process Stroik -ErrorAction SilentlyContinue | Where-Object { $_.Id -ne $client.Id -and $_.Path -eq $current -and $_.MainWindowHandle -ne 0 } | Select-Object -First 1
        if ([DateTime]::UtcNow -gt $deadline) { throw 'Updated window did not appear; inspect update.log' }
    } while (-not $installed)
    $client.WaitForExit(10000) | Out-Null
    if (-not $client.HasExited -or $client.ExitCode -ne 0) { throw "Probe client failed to exit normally: exited=$($client.HasExited), code=$($client.ExitCode)" }
    $deadline = [DateTime]::UtcNow.AddSeconds(40)
    while ((Test-Path -LiteralPath $download) -or (Get-ChildItem -LiteralPath $fixture -Filter 'Stroik.exe.old-*')) {
        if ([DateTime]::UtcNow -gt $deadline) { throw 'Old EXE/download cleanup did not complete' }
        Start-Sleep -Milliseconds 250
    }
    if ((Get-FileHash -LiteralPath $current -Algorithm SHA256).Hash -ne $expected) { throw 'Installed EXE differs from release' }
    $report = [ordered]@{ time = [DateTimeOffset]::Now.ToString('O'); legacyProtocol = [bool]$LegacyClient; sha256 = $expected;
        bytes = (Get-Item -LiteralPath $current).Length; title = $installed.MainWindowTitle; windowHandle = $installed.MainWindowHandle.ToInt64();
        installedPid = $installed.Id; clientExitCode = $client.ExitCode; oldExeRemoved = $true; downloadRemoved = $true }
    $report | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $fixture 'result.json') -Encoding UTF8
    $report | ConvertTo-Json
} finally {
    if ($installed -and -not $installed.HasExited) {
        $installed.CloseMainWindow() | Out-Null
        if (-not $installed.WaitForExit(10000)) { $installed.Kill(); $installed.WaitForExit(5000) | Out-Null }
    }
    foreach ($owned in @($client, $helper)) { if ($owned -and -not $owned.HasExited) { $owned.Kill(); $owned.WaitForExit(5000) | Out-Null } }
    # The isolated GUI can persist its own settings on startup/closure.
    if ($null -ne $before) { [IO.File]::WriteAllBytes($settings, $before) }
    elseif (Test-Path -LiteralPath $settings) { Remove-Item -LiteralPath $settings }
}
