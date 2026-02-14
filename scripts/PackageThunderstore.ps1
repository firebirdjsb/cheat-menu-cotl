# PackageThunderstore.ps1
# Creates a valid Thunderstore.io package zip from the project files.
# Usage: Run from the project root or from the scripts/ directory.
#
# Required files:
#   - icon.png (256x256 PNG)
#   - README.md
#   - CHANGELOG.md
#   - manifest.json
#   - bin/cheat_menu.dll (build output)

param(
    [string]$OutputName = "release.zip"
)

$ErrorActionPreference = "Stop"

# Resolve project root (script lives in scripts/)
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptDir

Push-Location $projectRoot
try {
    # --- Validate required files ---
    $requiredFiles = @(
        @{ Path = "icon.png";         Desc = "Icon (256x256 PNG)" },
        @{ Path = "README.md";        Desc = "Readme" },
        @{ Path = "CHANGELOG.md";     Desc = "Changelog" },
        @{ Path = "manifest.json";    Desc = "Manifest" },
        @{ Path = "bin\cheat_menu.dll"; Desc = "Mod DLL (run build first)" }
    )

    $missing = @()
    foreach ($f in $requiredFiles) {
        if (-not (Test-Path $f.Path)) {
            $missing += "  - $($f.Path) ($($f.Desc))"
        }
    }
    if ($missing.Count -gt 0) {
        Write-Host "ERROR: Missing required files:" -ForegroundColor Red
        $missing | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        exit 1
    }

    # --- Validate icon dimensions ---
    try {
        Add-Type -AssemblyName System.Drawing
        $iconBytes = [System.IO.File]::ReadAllBytes("icon.png")
        $ms = New-Object System.IO.MemoryStream(,$iconBytes)
        $img = [System.Drawing.Image]::FromStream($ms)
        $w = $img.Width
        $h = $img.Height
        $img.Dispose()
        $ms.Dispose()
        if ($w -ne 256 -or $h -ne 256) {
            Write-Host "ERROR: icon.png must be 256x256, but is ${w}x${h}" -ForegroundColor Red
            exit 1
        }
        Write-Host "  icon.png: ${w}x${h} OK" -ForegroundColor Green
    }
    catch {
        Write-Host "WARNING: Could not validate icon dimensions: $_" -ForegroundColor Yellow
    }

    # --- Validate manifest.json ---
    $manifest = Get-Content "manifest.json" -Raw | ConvertFrom-Json
    $manifestErrors = @()
    if (-not $manifest.name)           { $manifestErrors += "  - Missing 'name'" }
    if (-not $manifest.version_number) { $manifestErrors += "  - Missing 'version_number'" }
    if (-not $manifest.description)    { $manifestErrors += "  - Missing 'description'" }
    if ($manifest.description.Length -gt 250) { $manifestErrors += "  - 'description' exceeds 250 characters ($($manifest.description.Length))" }
    if ($manifestErrors.Count -gt 0) {
        Write-Host "ERROR: manifest.json validation failed:" -ForegroundColor Red
        $manifestErrors | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        exit 1
    }
    Write-Host "  manifest.json: v$($manifest.version_number) OK" -ForegroundColor Green

    # --- Build the zip ---
    $tempDir = Join-Path $env:TEMP "thunderstore_package_$(Get-Random)"
    New-Item -ItemType Directory -Path $tempDir -Force | Out-Null

    Copy-Item "icon.png"           (Join-Path $tempDir "icon.png")
    Copy-Item "README.md"          (Join-Path $tempDir "README.md")
    Copy-Item "CHANGELOG.md"       (Join-Path $tempDir "CHANGELOG.md")
    Copy-Item "manifest.json"      (Join-Path $tempDir "manifest.json")
    Copy-Item "bin\cheat_menu.dll"  (Join-Path $tempDir "cheat_menu.dll")

    $outputPath = Join-Path $projectRoot $OutputName
    if (Test-Path $outputPath) {
        Remove-Item $outputPath -Force
    }

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::CreateFromDirectory($tempDir, $outputPath)

    Remove-Item $tempDir -Recurse -Force

    $zipSize = (Get-Item $outputPath).Length
    $zipSizeKB = [math]::Round($zipSize / 1024, 1)

    Write-Host ""
    Write-Host "=== Thunderstore package created ===" -ForegroundColor Green
    Write-Host "  Output: $outputPath" -ForegroundColor Cyan
    Write-Host "  Size:   ${zipSizeKB} KB" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Package contents:" -ForegroundColor White
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [System.IO.Compression.ZipFile]::OpenRead($outputPath)
    foreach ($entry in $zip.Entries) {
        $sizeKB = [math]::Round($entry.Length / 1024, 1)
        Write-Host "  - $($entry.FullName) (${sizeKB} KB)"
    }
    $zip.Dispose()
    Write-Host ""
    Write-Host "Ready to upload to thunderstore.io!" -ForegroundColor Green
}
finally {
    Pop-Location
}
