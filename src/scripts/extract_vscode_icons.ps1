# Extract VS Code Icons
# This script extracts icons from installed VS Code instances

param(
    [string]$OutputDir = "$PSScriptRoot\..\src\CodingWithCalvin.VSToolbox\Assets",
    [int]$Size = 128
)

Add-Type -AssemblyName System.Drawing

function Extract-VSCodeIcon {
    param(
        [string]$ExePath,
        [string]$OutputPath,
        [int]$IconSize
    )

    if (-not (Test-Path $ExePath)) {
        Write-Warning "Executable not found: $ExePath"
        return $false
    }

    try {
        $icon = [System.Drawing.Icon]::ExtractAssociatedIcon($ExePath)
        if ($null -eq $icon) {
            Write-Warning "Could not extract icon from: $ExePath"
            return $false
        }

        $bitmap = $icon.ToBitmap()
        
        # Resize if needed
        if ($bitmap.Width -ne $IconSize -or $bitmap.Height -ne $IconSize) {
            $resized = New-Object System.Drawing.Bitmap($IconSize, $IconSize)
            $graphics = [System.Drawing.Graphics]::FromImage($resized)
            $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $graphics.DrawImage($bitmap, 0, 0, $IconSize, $IconSize)
            $graphics.Dispose()
            $bitmap.Dispose()
            $bitmap = $resized
        }

        $bitmap.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
        $bitmap.Dispose()
        $icon.Dispose()
        
        Write-Host "✓ Icon saved: $OutputPath" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Warning "Error extracting icon: $_"
        return $false
    }
}

# Ensure output directory exists
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

Write-Host "Extracting VS Code icons..." -ForegroundColor Cyan
Write-Host "Output directory: $OutputDir" -ForegroundColor Gray
Write-Host "Icon size: ${Size}x${Size}" -ForegroundColor Gray
Write-Host ""

$extracted = 0

# Try to find VS Code
$vsCodePaths = @(
    "$env:LOCALAPPDATA\Programs\Microsoft VS Code\Code.exe",
    "${env:ProgramFiles}\Microsoft VS Code\Code.exe"
)

foreach ($path in $vsCodePaths) {
    if (Test-Path $path) {
        Write-Host "Found VS Code: $path" -ForegroundColor Yellow
        $outputPath = Join-Path $OutputDir "vscode_icon.png"
        if (Extract-VSCodeIcon -ExePath $path -OutputPath $outputPath -IconSize $Size) {
            $extracted++
        }
        break
    }
}

# Try to find VS Code Insiders
$vsCodeInsidersPaths = @(
    "$env:LOCALAPPDATA\Programs\Microsoft VS Code Insiders\Code - Insiders.exe",
    "${env:ProgramFiles}\Microsoft VS Code Insiders\Code - Insiders.exe"
)

foreach ($path in $vsCodeInsidersPaths) {
    if (Test-Path $path) {
        Write-Host "Found VS Code Insiders: $path" -ForegroundColor Yellow
        $outputPath = Join-Path $OutputDir "vscode_insiders_icon.png"
        if (Extract-VSCodeIcon -ExePath $path -OutputPath $outputPath -IconSize $Size) {
            $extracted++
        }
        break
    }
}

Write-Host ""
if ($extracted -eq 0) {
    Write-Host "No VS Code installations found!" -ForegroundColor Red
    Write-Host "Please install VS Code and try again." -ForegroundColor Yellow
} else {
    Write-Host "Successfully extracted $extracted icon(s)!" -ForegroundColor Green
}

Write-Host ""
Write-Host "Done!" -ForegroundColor Cyan
