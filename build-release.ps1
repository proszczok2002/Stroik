$ErrorActionPreference = 'Stop'
$env:DOTNET_CLI_HOME = Join-Path $PSScriptRoot '.dotnet-home'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$dotnet = Join-Path $PSScriptRoot '.dotnet10/dotnet.exe'
if (-not (Test-Path -LiteralPath $dotnet)) { $dotnet = 'dotnet' }

& $dotnet run --project (Join-Path $PSScriptRoot 'tests/PitchChecks.csproj') -c Release
if ($LASTEXITCODE -ne 0) { throw 'Pitch checks failed.' }

$output = Join-Path $PSScriptRoot 'dist'
$publish = Join-Path $output 'publish'
New-Item -ItemType Directory -Force -Path $publish | Out-Null
& $dotnet publish (Join-Path $PSScriptRoot 'Stroik.csproj') -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o $publish
if ($LASTEXITCODE -ne 0) { throw 'Publish failed.' }

$exe = Join-Path $output 'Stroik.exe'
Copy-Item -LiteralPath (Join-Path $publish 'Stroik.exe') -Destination $exe -Force
$hash = (Get-FileHash -LiteralPath $exe -Algorithm SHA256).Hash.ToLowerInvariant()
[System.IO.File]::WriteAllText((Join-Path $output 'Stroik.exe.sha256'), "$hash  Stroik.exe`n", [System.Text.Encoding]::ASCII)
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'README.md') -Destination (Join-Path $output 'README.md') -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'README.en.md') -Destination (Join-Path $output 'README.en.md') -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'LICENSE') -Destination (Join-Path $output 'LICENSE') -Force
[xml]$project = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Stroik.csproj')
$version = $project.Project.PropertyGroup.Version
$zip = Join-Path $output "Stroik-$version-Windows-x64.zip"
Compress-Archive -LiteralPath $exe, (Join-Path $output 'Stroik.exe.sha256'), (Join-Path $output 'README.md'), (Join-Path $output 'README.en.md'), (Join-Path $output 'LICENSE') -DestinationPath $zip -Force
Write-Host "EXE SHA-256: $hash"
Write-Host "Release files: $exe, $zip"
