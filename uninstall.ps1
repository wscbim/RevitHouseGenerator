# RevitHouseGenerator - Uninstaller
# Run: powershell -ExecutionPolicy Bypass -File uninstall.ps1

$addinsDir = "$env:APPDATA\Autodesk\Revit\Addins\2025"
$pluginDir = "$addinsDir\RevitHouseGenerator"
$addinFile = "$addinsDir\RevitHouseGenerator.addin"

Write-Host "RevitHouseGenerator - Uninstaller" -ForegroundColor Cyan

if (Test-Path $pluginDir) {
    Remove-Item $pluginDir -Recurse -Force
    Write-Host "Removed: $pluginDir" -ForegroundColor Green
}

if (Test-Path $addinFile) {
    Remove-Item $addinFile -Force
    Write-Host "Removed: $addinFile" -ForegroundColor Green
}

Write-Host "Uninstall complete. Restart Revit to apply." -ForegroundColor Green
