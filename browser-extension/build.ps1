# Builds the store packages of the browser extension :
#   dist/xiletrade-sync-<version>-firefox.zip  (addons.mozilla.org)
#   dist/xiletrade-sync-<version>-chrome.zip   (Chrome Web Store, Edge Add-ons)
# Usage : pwsh ./build.ps1
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem

$root = $PSScriptRoot
$src = Join-Path $root 'src'
$dist = Join-Path $root 'dist'
New-Item -ItemType Directory -Force -Path $dist | Out-Null

$manifestText = Get-Content -Raw -Encoding UTF8 (Join-Path $src 'manifest.json')
$version = ($manifestText | ConvertFrom-Json).version

function New-Package([string]$target, [string]$manifest) {
    $zip = Join-Path $dist "xiletrade-sync-$version-$target.zip"
    if (Test-Path $zip) { Remove-Item $zip }
    $archive = [System.IO.Compression.ZipFile]::Open($zip, 'Create')
    try {
        Get-ChildItem -Path $src -Recurse -File | ForEach-Object {
            # entries with '/' : the stores reject the '\' separators
            $entry = $_.FullName.Substring($src.Length + 1).Replace('\', '/')
            if ($entry -eq 'manifest.json') {
                $e = $archive.CreateEntry($entry, 'Optimal')
                $w = New-Object System.IO.StreamWriter($e.Open(), (New-Object System.Text.UTF8Encoding($false)))
                $w.Write($manifest); $w.Dispose()
            } else {
                [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $_.FullName, $entry, 'Optimal') | Out-Null
            }
        }
    } finally { $archive.Dispose() }
    Write-Host "Built $zip"
}

# Firefox : manifest as is (gecko id, minimal version, data collection)
New-Package 'firefox' $manifestText

# Chrome / Edge : without the Firefox only key
$chrome = $manifestText | ConvertFrom-Json
$chrome.PSObject.Properties.Remove('browser_specific_settings')
New-Package 'chrome' ($chrome | ConvertTo-Json -Depth 10)
