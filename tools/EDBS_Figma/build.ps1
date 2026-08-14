# Regenerate code.js and copy into the Figma import folder (plugin/)
node "$PSScriptRoot\source\build-code.js"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
$pluginDir = Join-Path $PSScriptRoot "plugin"
Write-Host "OK: EDBS Figma plugin ready" -ForegroundColor Green
Write-Host "Import this folder in Figma:" -ForegroundColor Cyan
Write-Host "  $pluginDir" -ForegroundColor Yellow
