# PackageThunderstore-NoDll.ps1
# Creates a Thunderstore.io package WITHOUT requiring the DLL.
# The user will need to add their compiled DLL manually.
#
# Usage: Run from the project root or from the scripts/ directory.
#   .\scripts\PackageThunderstore-NoDll.ps1
#
# This script:
#   1. Validates required Thunderstore files (icon.png, README.md, CHANGELOG.md, manifest.json)
#   2. Validates icon dimensions (256x256)
#   3. Validates manifest.json structure per Thunderstore requirements
#   4. Creates the package WITHOUT requiring the DLL
#   5. Creates the proper folder structure: plugins/CheatMenu/ with a README
#   6. Creates a release.zip ready for Thunderstore upload

param(
    [string]$OutputName = "release.zip"
)

$ErrorActionPreference = "Stop"

# Resolve project root (script lives in scripts/)
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptDir

Write-Host "=== Thunderstore Package Builder (No DLL) ===" -ForegroundColor Cyan
Write-Host "Project root: $projectRoot" -ForegroundColor Gray
Write-Host ""

Push-Location $projectRoot
try {
    # --- Validate required files ---
    Write-Host "Validating required files..." -ForegroundColor White

    $requiredFiles = @(
        @{ Path = "icon.png";         Desc = "Icon (256x256 PNG)" },
        @{ Path = "README.md";        Desc = "Readme" },
        @{ Path = "CHANGELOG.md";     Desc = "Changelog" },
        @{ Path = "manifest.json";    Desc = "Manifest" }
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
    Write-Host "  All required files present." -ForegroundColor Green

    # --- Validate icon dimensions ---
    Write-Host ""
    Write-Host "Validating icon dimensions..." -ForegroundColor White

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
    Write-Host ""
    Write-Host "Validating manifest.json..." -ForegroundColor White

    $manifest = Get-Content "manifest.json" -Raw | ConvertFrom-Json
    $manifestErrors = @()

    # Thunderstore required fields
    if (-not $manifest.name) { 
        $manifestErrors += "  - Missing 'name'" 
    }
    if (-not $manifest.version_number) { 
        $manifestErrors += "  - Missing 'version_number'" 
    }
    if (-not $manifest.description) { 
        $manifestErrors += "  - Missing 'description'" 
    }

    # Thunderstore validation rules
    if ($manifest.description -and $manifest.description.Length -gt 250) { 
        $manifestErrors += "  - 'description' exceeds 250 characters ($($manifest.description.Length))" 
    }
    if ($manifest.name -and $manifest.name -notmatch '^[a-zA-Z0-9_-]+$') { 
        $manifestErrors += "  - 'name' must contain only alphanumeric characters, underscores, or hyphens" 
    }
    if ($manifest.version_number -and $manifest.version_number -notmatch '^\d+\.\d+\.\d+$') { 
        $manifestErrors += "  - 'version_number' must be in semver format (e.g., 1.0.0)" 
    }

    # Optional but recommended fields
    if (-not $manifest.website_url) {
        Write-Host "  WARNING: 'website_url' is recommended" -ForegroundColor Yellow
    }
    if (-not $manifest.dependencies) {
        Write-Host "  WARNING: 'dependencies' is recommended" -ForegroundColor Yellow
    }

    if ($manifestErrors.Count -gt 0) {
        Write-Host "ERROR: manifest.json validation failed:" -ForegroundColor Red
        $manifestErrors | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        exit 1
    }
    Write-Host "  manifest.json: v$($manifest.version_number) OK" -ForegroundColor Green
    Write-Host "    Name: $($manifest.name)" -ForegroundColor Gray
    Write-Host "    Dependencies: $($manifest.dependencies -join ', ')" -ForegroundColor Gray

    # --- Build the package ---
    Write-Host ""
    Write-Host "Building package..." -ForegroundColor White

    $tempDir = Join-Path $env:TEMP "thunderstore_package_nodll_$(Get-Random)"
    New-Item -ItemType Directory -Path $tempDir -Force | Out-Null

    # Copy root level files
    Write-Host "  Copying root files..." -ForegroundColor Gray
    Copy-Item "icon.png"           (Join-Path $tempDir "icon.png")
    Copy-Item "README.md"          (Join-Path $tempDir "README.md")
    Copy-Item "CHANGELOG.md"       (Join-Path $tempDir "CHANGELOG.md")
    Copy-Item "manifest.json"      (Join-Path $tempDir "manifest.json")

    # Create plugins/CheatMenu/ directory structure
    $pluginsDir = Join-Path $tempDir "plugins\CheatMenu"
    New-Item -ItemType Directory -Path $pluginsDir -Force | Out-Null

    # Create README in plugins/CheatMenu/ explaining where to put the DLL
    Write-Host "  Creating DLL placeholder README..." -ForegroundColor Gray

    $pluginReadmeContent = @"
# CheatMenu DLL Placeholder

## IMPORTANT: Add Your DLL Here

This folder is where your compiled `CheatMenu.dll` should be placed.

### Instructions:

1. **Build the mod:**
   ```
   dotnet build
   ```

2. **Copy the DLL:**
   Copy `bin/CheatMenu/CheatMenu.dll` to this folder:
   ```
   copy bin\CheatMenu\CheatMenu.dll plugins\CheatMenu\CheatMenu.dll
   ```

3. **Verify:**
   After adding the DLL, your folder structure should look like:
   ```
   plugins/
   └── CheatMenu/
       ├── CheatMenu.dll    <-- Your compiled mod
       └── README.txt       <-- This file (you can delete it)
   ```

### Why is the DLL not included?

This package is designed for:
- Users who want to build the mod themselves
- Developers who need to test their builds
- Scenarios where the DLL needs to be compiled with specific options

For pre-built releases with the DLL included, check the Thunderstore releases page.

---
Generated by PackageThunderstore-NoDll.ps1
"@

    $pluginReadmePath = Join-Path $pluginsDir "README.txt"
    Set-Content -Path $pluginReadmePath -Value $pluginReadmeContent -Encoding UTF8

    # Create the output zip
    $outputPath = Join-Path $projectRoot $OutputName
    if (Test-Path $outputPath) {
        Remove-Item $outputPath -Force
    }

    Write-Host "  Creating release.zip..." -ForegroundColor Gray

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::CreateFromDirectory($tempDir, $outputPath)

    Remove-Item $tempDir -Recurse -Force

    # Display results
    $zipSize = (Get-Item $outputPath).Length
    $zipSizeKB = [math]::Round($zipSize / 1024, 1)

    Write-Host ""
    Write-Host "=== Thunderstore Package Created ===" -ForegroundColor Green
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
    Write-Host "IMPORTANT: Add your compiled DLL to plugins/CheatMenu/ before uploading!" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Ready to upload to thunderstore.io!" -ForegroundColor Green
}
finally {
    Pop-Location
}
