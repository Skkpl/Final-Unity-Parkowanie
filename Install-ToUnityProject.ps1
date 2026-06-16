param(
    [string]$ProjectPath = "C:\Users\szymo\OneDrive\Pulpit\Unity parkowanie\Automatic parking"
)

$ErrorActionPreference = "Stop"

$sourceAssets = Join-Path $PSScriptRoot "Assets"
$targetAssets = Join-Path $ProjectPath "Assets"

if (-not (Test-Path -LiteralPath $sourceAssets)) {
    throw "Nie znaleziono folderu Assets w paczce: $sourceAssets"
}

if (-not (Test-Path -LiteralPath $targetAssets)) {
    throw "Nie znaleziono folderu Assets projektu Unity: $targetAssets"
}

Copy-Item -LiteralPath (Join-Path $sourceAssets "Scripts") -Destination $targetAssets -Recurse -Force
Copy-Item -LiteralPath (Join-Path $sourceAssets "Editor") -Destination $targetAssets -Recurse -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "README.md") -Destination (Join-Path $ProjectPath "README_AutomatyczneParkowanie.md") -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "README.pdf") -Destination (Join-Path $ProjectPath "README_AutomatyczneParkowanie.pdf") -Force

$sourceDocs = Join-Path $PSScriptRoot "Documentation"
if (Test-Path -LiteralPath $sourceDocs) {
    Copy-Item -LiteralPath $sourceDocs -Destination (Join-Path $ProjectPath "Documentation_AutomatyczneParkowanie") -Recurse -Force
}

Write-Host "Zainstalowano skrypty automatycznego parkowania w: $targetAssets"
Write-Host "Dokumentacja zostala skopiowana do: Documentation_AutomatyczneParkowanie"
Write-Host "W Unity wybierz: Tools -> Parking Project -> Build Demo Scenes"

$nestedAssets = Join-Path $targetAssets "Assets"
if (Test-Path -LiteralPath $nestedAssets) {
    Write-Host "Uwaga: wykryto folder Assets\\Assets. To zwykle efekt recznego wklejenia calego folderu Assets do srodka Assets."
    Write-Host "Jesli Unity pokazuje duplikaty, przenies/skasuj ten zagniezdzony folder po upewnieniu sie, ze masz kopie paczki."
}
