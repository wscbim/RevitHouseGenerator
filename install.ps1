# RevitHouseGenerator - Installer for Revit 2025
# Run: powershell -ExecutionPolicy Bypass -File install.ps1

$ErrorActionPreference = "Stop"

$addinsDir = "$env:APPDATA\Autodesk\Revit\Addins\2025"
$pluginDir = "$addinsDir\RevitHouseGenerator"

Write-Host "RevitHouseGenerator - Installer" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan

# Check if Revit addins directory exists
if (-not (Test-Path $addinsDir)) {
    Write-Host "ERROR: Revit 2025 addins directory not found: $addinsDir" -ForegroundColor Red
    Write-Host "Make sure Autodesk Revit 2025 is installed." -ForegroundColor Yellow
    exit 1
}

# Create plugin directory
if (-not (Test-Path $pluginDir)) {
    New-Item -ItemType Directory -Path $pluginDir | Out-Null
    Write-Host "Created: $pluginDir" -ForegroundColor Green
}

# Determine source directory (same folder as script)
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Copy DLL
$dll = Join-Path $scriptDir "RevitHouseGenerator.dll"
if (-not (Test-Path $dll)) {
    Write-Host "ERROR: RevitHouseGenerator.dll not found in $scriptDir" -ForegroundColor Red
    exit 1
}
Copy-Item $dll $pluginDir -Force
Write-Host "Copied: RevitHouseGenerator.dll -> $pluginDir" -ForegroundColor Green

# Create .addin manifest pointing to plugin directory
$addinContent = @"
<?xml version="1.0" encoding="utf-8"?>
<RevitAddIns>
  <AddIn Type="Application">
    <Name>RevitHouseGenerator</Name>
    <Assembly>$pluginDir\RevitHouseGenerator.dll</Assembly>
    <AddInId>A1B2C3D4-E5F6-7890-ABCD-EF1234567890</AddInId>
    <FullClassName>RevitHouseGenerator.App</FullClassName>
    <VendorId>PK-KPA</VendorId>
    <VendorDescription>Wojciech Cieplucha | Chair of Architectural Design</VendorDescription>
  </AddIn>
</RevitAddIns>
"@

$addinPath = "$addinsDir\RevitHouseGenerator.addin"
$addinContent | Out-File -FilePath $addinPath -Encoding utf8
Write-Host "Created: $addinPath" -ForegroundColor Green

Write-Host ""
Write-Host "Installation complete!" -ForegroundColor Green
Write-Host "Restart Revit 2025 to load the plugin." -ForegroundColor Yellow
Write-Host 'Look for the "DevWC" tab in the ribbon.' -ForegroundColor Yellow
