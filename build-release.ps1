param([switch]$RebuildSameVersion)
$ErrorActionPreference = 'Stop'
$env:DOTNET_CLI_HOME = Join-Path $PSScriptRoot '.dotnet-home'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$dotnet = Join-Path $PSScriptRoot '.dotnet10/dotnet.exe'
if (-not (Test-Path -LiteralPath $dotnet)) { $dotnet = 'dotnet' }

$repoRoot = [System.IO.Path]::GetFullPath($PSScriptRoot).TrimEnd([char]'\')
$output = Join-Path $repoRoot 'dist'
$marker = Join-Path $output 'build-version.txt'
[xml]$project = Get-Content -LiteralPath (Join-Path $repoRoot 'Stroik.csproj')
$version = [string]$project.Project.PropertyGroup.Version
if (-not [version]::TryParse($version, [ref]([version]$null))) { throw "Invalid project version: $version" }
$previousVersion = if (Test-Path -LiteralPath $marker) { (Get-Content -LiteralPath $marker -Raw).Trim() } else { 'unknown' }
if ($previousVersion -eq $version -and (Test-Path -LiteralPath (Join-Path $output 'Stroik.exe')) -and -not $RebuildSameVersion) {
    throw "Version $version is already built. Increase the project version for a new release or explicitly pass -RebuildSameVersion before publication."
}
$archive = Join-Path $output ("niepotrzebne\v$previousVersion-" + (Get-Date -Format 'yyyyMMdd-HHmmss') + "-$PID")
$stage = Join-Path $output ('.build-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $output | Out-Null

function Assert-InWorkspace([string]$path) {
    $full = [System.IO.Path]::GetFullPath($path)
    $prefix = $repoRoot + [System.IO.Path]::DirectorySeparatorChar
    if (-not $full.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Path is outside the project: $full"
    }
    return $full
}

function Move-ToArchive([string]$source, [string]$relativeDestination) {
    if (-not (Test-Path -LiteralPath $source)) { return }
    $resolved = (Resolve-Path -LiteralPath $source).ProviderPath
    $sourcePath = Assert-InWorkspace $resolved
    $item = Get-Item -LiteralPath $sourcePath -Force
    if (($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw "Refusing to archive a reparse point: $sourcePath"
    }
    $destination = Assert-InWorkspace (Join-Path $archive $relativeDestination)
    if (Test-Path -LiteralPath $destination) { throw "Archive destination already exists: $destination" }
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $destination) | Out-Null
    $hash = if ($item.PSIsContainer) { $null } else { (Get-FileHash -LiteralPath $sourcePath -Algorithm SHA256).Hash }
    Move-Item -LiteralPath $sourcePath -Destination $destination
    if (-not (Test-Path -LiteralPath $destination)) { throw "Archive move failed: $destination" }
    if ($hash -and (Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash -ne $hash) {
        throw "Archived file hash mismatch: $destination"
    }
}

function Restore-PreviousRelease {
    $oldFiles = Join-Path $archive 'wydanie'
    if (-not (Test-Path -LiteralPath $oldFiles)) { return }
    foreach ($file in Get-ChildItem -LiteralPath $oldFiles -File) {
        $destination = Assert-InWorkspace (Join-Path $output $file.Name)
        if (Test-Path -LiteralPath $destination) {
            Move-ToArchive $destination (Join-Path 'nieudana-nowa-wersja' $file.Name)
        }
        Move-Item -LiteralPath $file.FullName -Destination $destination
    }
}

# Keep the active EXE in dist until a complete new build is available.
if ($previousVersion -ne 'unknown' -and $previousVersion -ne $version) {
    foreach ($relative in @('bin', 'obj', 'tests\bin', 'tests\obj')) {
        Move-ToArchive (Join-Path $repoRoot $relative) (Join-Path 'kompilacje' $relative)
    }
    foreach ($name in @('verify-public', 'verify-updater')) {
        Move-ToArchive (Join-Path $output $name) (Join-Path 'weryfikacje' $name)
    }
}

try {
    & $dotnet run --project (Join-Path $repoRoot 'tests/PitchChecks.csproj') -c Release
    if ($LASTEXITCODE -ne 0) { throw 'Pitch checks failed.' }
    New-Item -ItemType Directory -Force -Path $stage | Out-Null
    & $dotnet publish (Join-Path $repoRoot 'Stroik.csproj') -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o $stage
    if ($LASTEXITCODE -ne 0) { throw 'Publish failed.' }

    $newExe = Join-Path $stage 'Stroik.exe'
    $newHash = (Get-FileHash -LiteralPath $newExe -Algorithm SHA256).Hash.ToLowerInvariant()
    [System.IO.File]::WriteAllText((Join-Path $stage 'Stroik.exe.sha256'),
        $newHash + '  Stroik.exe' + [Environment]::NewLine, [System.Text.Encoding]::ASCII)
    foreach ($name in @('README.md', 'README.en.md', 'LICENSE')) {
        Copy-Item -LiteralPath (Join-Path $repoRoot $name) -Destination (Join-Path $stage $name)
    }
    $zipName = "Stroik-$version-Windows-x64.zip"
    Compress-Archive -LiteralPath $newExe, (Join-Path $stage 'Stroik.exe.sha256'),
        (Join-Path $stage 'README.md'), (Join-Path $stage 'README.en.md'),
        (Join-Path $stage 'LICENSE') -DestinationPath (Join-Path $stage $zipName)
    if ((Get-FileHash -LiteralPath $newExe -Algorithm SHA256).Hash.ToLowerInvariant() -ne $newHash) {
        throw 'EXE changed while packaging.'
    }

    $activeExe = Join-Path $output 'Stroik.exe'
    $activeChecksum = Join-Path $output 'Stroik.exe.sha256'
    if ((Test-Path -LiteralPath $activeExe) -and (Test-Path -LiteralPath $activeChecksum)) {
        $expected = ((Get-Content -LiteralPath $activeChecksum -Raw).Trim() -split '\s+')[0]
        $actual = (Get-FileHash -LiteralPath $activeExe -Algorithm SHA256).Hash
        if ($expected -ne $actual) { throw 'Active EXE does not match its SHA-256 file.' }
    }

    try {
        foreach ($name in @('Stroik.exe', 'Stroik.exe.sha256', 'README.md', 'README.en.md', 'LICENSE', 'build-version.txt')) {
            Move-ToArchive (Join-Path $output $name) (Join-Path 'wydanie' $name)
        }
        foreach ($file in Get-ChildItem -LiteralPath $output -Filter 'Stroik-*-Windows-x64.zip' -File) {
            Move-ToArchive $file.FullName (Join-Path 'wydanie' $file.Name)
        }
        foreach ($name in @('Stroik.exe', 'Stroik.exe.sha256', 'README.md', 'README.en.md', 'LICENSE', $zipName)) {
            Copy-Item -LiteralPath (Join-Path $stage $name) -Destination (Join-Path $output $name)
        }
        if ((Get-FileHash -LiteralPath $activeExe -Algorithm SHA256).Hash.ToLowerInvariant() -ne $newHash) {
            throw 'Active EXE hash differs from the new build.'
        }
        [System.IO.File]::WriteAllText($marker, $version + [Environment]::NewLine, [System.Text.Encoding]::ASCII)
        Move-ToArchive (Join-Path $output 'publish') 'kompilacje\publish'
        Move-Item -LiteralPath (Assert-InWorkspace $stage) -Destination (Assert-InWorkspace (Join-Path $output 'publish'))
    }
    catch {
        Restore-PreviousRelease
        $oldPublish = Join-Path $archive 'kompilacje\publish'
        if ((Test-Path -LiteralPath $oldPublish) -and -not (Test-Path -LiteralPath (Join-Path $output 'publish'))) {
            $resolvedOld = (Resolve-Path -LiteralPath $oldPublish).ProviderPath
            Move-Item -LiteralPath (Assert-InWorkspace $resolvedOld) -Destination (Assert-InWorkspace (Join-Path $output 'publish'))
        }
        throw
    }
    Write-Host "EXE SHA-256: $newHash"
    Write-Host "Release files: $activeExe, $(Join-Path $output $zipName)"
    if (Test-Path -LiteralPath $archive) { Write-Host "Previous artifacts: $archive" }
}
catch {
    if (Test-Path -LiteralPath $stage) {
        Move-ToArchive $stage 'nieudana-kompilacja'
    }
    throw
}
