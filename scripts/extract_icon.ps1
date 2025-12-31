# Extract icon from Visual Studio devenv.exe
# Usage: .\extract_icon.ps1 [-ExePath <path>] [-OutputPath <path>]

param(
    [string]$ExePath = 'C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\devenv.exe',
    [string]$OutputPath = "$PSScriptRoot\..\src\CodingWithCalvin.VSToolbox\Assets\vs_icon.png"
)

Add-Type -AssemblyName System.Drawing

if (-not (Test-Path $ExePath)) {
    Write-Error "Visual Studio executable not found at: $ExePath"
    exit 1
}

$icon = [System.Drawing.Icon]::ExtractAssociatedIcon($ExePath)
$bitmap = $icon.ToBitmap()

# Ensure output directory exists
$outputDir = Split-Path -Parent $OutputPath
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

$bitmap.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
Write-Host "Icon saved to $OutputPath"
