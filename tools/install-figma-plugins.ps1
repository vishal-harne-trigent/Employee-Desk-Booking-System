# Build both Figma plugins and verify code.js is ready to import.
# Run from repo root:  powershell -File tools/install-figma-plugins.ps1

$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent

Write-Host 'Building EDBS Wireframes plugin...' -ForegroundColor Cyan
Push-Location (Join-Path $repo 'tools/EDBS_Wireframes')
.\build.ps1
Pop-Location

Write-Host 'Building EDBS_Figma plugin...' -ForegroundColor Cyan
Push-Location (Join-Path $repo 'tools/EDBS_Figma')
.\build.ps1
Pop-Location

$wirePlugin = Join-Path $repo 'tools/EDBS_Wireframes/plugin'
$figmaPlugin = Join-Path $repo 'tools/EDBS_Figma/plugin'

foreach ($dir in @($wirePlugin, $figmaPlugin)) {
  $code = Join-Path $dir 'code.js'
  $manifest = Join-Path $dir 'manifest.json'
  if (-not (Test-Path $code)) { throw "Missing $code" }
  if (-not (Test-Path $manifest)) { throw "Missing $manifest" }
  $size = (Get-Item $code).Length
  if ($size -lt 10000) { throw "$code is only $size bytes - build failed" }
  $text = Get-Content $code -Raw
  if ($text -match 'This plugin template uses TypeScript') {
    throw "$code is the Figma TypeScript stub - import the plugin folder only"
  }
}

Write-Host ''
Write-Host 'Plugins ready. In Figma:' -ForegroundColor Green
Write-Host '  1. Plugins -> Development -> remove ALL old EDBS plugins'
Write-Host '  2. Import plugin from manifest... (pick manifest inside plugin folder only)'
Write-Host ''
Write-Host 'Wireframes:' -ForegroundColor Yellow
Write-Host "  $wirePlugin"
Write-Host 'Hi-fi:' -ForegroundColor Yellow
Write-Host "  $figmaPlugin"
Write-Host ''
Write-Host 'Do NOT import the parent EDBS_Wireframes or EDBS_Figma folders.' -ForegroundColor Red
