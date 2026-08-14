# Regenerate code.js and copy into the Figma import folder (plugin/)
node "$PSScriptRoot\source\build-code.js"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
$pluginDir = Join-Path $PSScriptRoot "plugin"
$codeJs = Join-Path $pluginDir "code.js"
$size = (Get-Item $codeJs).Length
Write-Host "OK: plugin ready ($size bytes)" -ForegroundColor Green
Write-Host "In Figma: Plugins -> Development -> Import plugin from manifest..." -ForegroundColor Cyan
Write-Host "Select THIS folder only:" -ForegroundColor Cyan
Write-Host "  $pluginDir" -ForegroundColor Yellow
