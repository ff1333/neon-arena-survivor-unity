[CmdletBinding()]
param(
    [string]$Version = "1.3.0"
)

$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $PSScriptRoot
$buildRoot = Join-Path $projectRoot "Builds"
$packageRoot = Join-Path $buildRoot "Packages\v$Version"
$windowsRoot = Join-Path $buildRoot "Windows\v$Version"
$webglRoot = Join-Path $buildRoot "WebGL\v$Version"
$androidRoot = Join-Path $buildRoot "Android\v$Version"

$windowsFiles = @(
    (Join-Path $windowsRoot "NeonArenaRebuild.exe"),
    (Join-Path $windowsRoot "UnityPlayer.dll"),
    (Join-Path $windowsRoot "UnityCrashHandler64.exe"),
    (Join-Path $windowsRoot "MonoBleedingEdge"),
    (Join-Path $windowsRoot "NeonArenaRebuild_Data")
)
$d3d12 = Join-Path $windowsRoot "D3D12"
if (Test-Path -LiteralPath $d3d12) { $windowsFiles += $d3d12 }
$webglFiles = @(
    (Join-Path $webglRoot "index.html"),
    (Join-Path $webglRoot "Build"),
    (Join-Path $webglRoot "TemplateData")
)
$androidApk = Join-Path $androidRoot "NeonArenaRebuild-v$Version.apk"

foreach ($path in $windowsFiles + $webglFiles + @($androidApk)) {
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Missing build input: $path"
    }
}

New-Item -ItemType Directory -Path $packageRoot -Force | Out-Null

$windowsZip = Join-Path $packageRoot "NeonArenaRebuild-Windows-v$Version.zip"
$webglZip = Join-Path $packageRoot "NeonArenaRebuild-WebGL-v$Version.zip"
$packagedApk = Join-Path $packageRoot "NeonArenaRebuild-Android-v$Version-test.apk"

Compress-Archive -LiteralPath $windowsFiles -DestinationPath $windowsZip -Force
Compress-Archive -LiteralPath $webglFiles -DestinationPath $webglZip -Force
Copy-Item -LiteralPath $androidApk -Destination $packagedApk -Force

$packages = @($windowsZip, $webglZip, $packagedApk)
$hashLines = foreach ($package in $packages) {
    $hash = Get-FileHash -LiteralPath $package -Algorithm SHA256
    "{0}  {1}" -f $hash.Hash.ToLowerInvariant(), (Split-Path $package -Leaf)
}
$hashPath = Join-Path $packageRoot "SHA256SUMS.txt"
$hashLines | Set-Content -LiteralPath $hashPath -Encoding utf8

Write-Host "Release candidate packages created in:"
Write-Host $packageRoot
$packages + @($hashPath) | ForEach-Object { Write-Host "- $_" }
