# Extract large icon (256x256) from Visual Studio devenv.exe
# Usage: .\extract_large_icon.ps1 [-ExePath <path>] [-OutputPath <path>] [-Size <int>]

param(
    [string]$ExePath = 'C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\devenv.exe',
    [string]$OutputPath = "$PSScriptRoot\..\src\CodingWithCalvin.VSToolbox\Assets\vs_icon_256.png",
    [int]$Size = 256
)

Add-Type -TypeDefinition @"
using System;
using System.Drawing;
using System.Runtime.InteropServices;

public class IconExtractor
{
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int PrivateExtractIcons(
        string lpszFile,
        int nIconIndex,
        int cxIcon,
        int cyIcon,
        IntPtr[] phicon,
        int[] piconid,
        int nIcons,
        int flags);

    public static Icon ExtractIcon(string file, int size)
    {
        IntPtr[] hIcon = new IntPtr[1];
        int[] id = new int[1];
        int count = PrivateExtractIcons(file, 0, size, size, hIcon, id, 1, 0);
        if (count > 0 && hIcon[0] != IntPtr.Zero)
        {
            return Icon.FromHandle(hIcon[0]);
        }
        return null;
    }
}
"@ -ReferencedAssemblies System.Drawing

if (-not (Test-Path $ExePath)) {
    Write-Error "Visual Studio executable not found at: $ExePath"
    exit 1
}

# Ensure output directory exists
$outputDir = Split-Path -Parent $OutputPath
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

# Extract icon at requested size
$icon = [IconExtractor]::ExtractIcon($ExePath, $Size)
if ($icon -ne $null) {
    $bitmap = $icon.ToBitmap()
    $bitmap.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    Write-Host "${Size}x${Size} icon saved to $OutputPath"
} else {
    Write-Host "Could not extract ${Size}x${Size} icon, trying 128..."
    $icon = [IconExtractor]::ExtractIcon($ExePath, 128)
    if ($icon -ne $null) {
        $bitmap = $icon.ToBitmap()
        $bitmap.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
        Write-Host "128x128 icon saved to $OutputPath"
    } else {
        Write-Error "Could not extract icon from $ExePath"
        exit 1
    }
}
