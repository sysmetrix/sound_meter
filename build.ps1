$ErrorActionPreference = "Stop"

$compiler = "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path $compiler)) {
    $compiler = "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe"
}

if (-not (Test-Path $compiler)) {
    throw "C# compiler not found. Install .NET Framework 4.x or the .NET SDK."
}

$outDir = Join-Path $PSScriptRoot "bin"
$objDir = Join-Path $PSScriptRoot "obj"
$distDir = Join-Path $PSScriptRoot "dist"
$assetsDir = Join-Path $PSScriptRoot "assets"
$iconPath = Join-Path $assetsDir "SoundMeter.ico"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
New-Item -ItemType Directory -Force -Path $objDir | Out-Null
New-Item -ItemType Directory -Force -Path $distDir | Out-Null
New-Item -ItemType Directory -Force -Path $assetsDir | Out-Null

& $compiler `
    /nologo `
    /codepage:65001 `
    /target:exe `
    /platform:anycpu `
    /out:"$objDir\IconGenerator.exe" `
    /reference:System.dll `
    /reference:System.Drawing.dll `
    "$PSScriptRoot\IconGenerator.cs" `
    "$PSScriptRoot\AudioIconFactory.cs"

if ($LASTEXITCODE -ne 0) {
    throw "Icon generator build failed with exit code $LASTEXITCODE."
}

& "$objDir\IconGenerator.exe" "$iconPath"

if ($LASTEXITCODE -ne 0) {
    throw "Icon generation failed with exit code $LASTEXITCODE."
}

Write-Host "Generated: $iconPath"

& $compiler `
    /nologo `
    /codepage:65001 `
    /target:winexe `
    /platform:anycpu `
    /win32icon:"$iconPath" `
    /out:"$outDir\SoundMeter.exe" `
    /reference:System.dll `
    /reference:System.Core.dll `
    /reference:System.Drawing.dll `
    /reference:System.Xml.dll `
    /reference:System.Windows.Forms.dll `
    "$PSScriptRoot\Program.cs" `
    "$PSScriptRoot\AudioDevice.cs" `
    "$PSScriptRoot\AppSettings.cs" `
    "$PSScriptRoot\AudioIconFactory.cs" `
    "$PSScriptRoot\CoreAudio.cs" `
    "$PSScriptRoot\DevicePreferenceService.cs" `
    "$PSScriptRoot\HotkeyService.cs" `
    "$PSScriptRoot\NativeUi.cs" `
    "$PSScriptRoot\QuickSwitchForm.cs" `
    "$PSScriptRoot\SettingsForm.cs" `
    "$PSScriptRoot\SoundTrayApp.cs"

if ($LASTEXITCODE -ne 0) {
    throw "Build failed with exit code $LASTEXITCODE."
}

Write-Host "Built: $outDir\SoundMeter.exe"

& $compiler `
    /nologo `
    /codepage:65001 `
    /target:exe `
    /platform:anycpu `
    /out:"$outDir\AudioProbe.exe" `
    /reference:System.dll `
    /reference:System.Core.dll `
    "$PSScriptRoot\AudioProbe.cs" `
    "$PSScriptRoot\AudioDevice.cs" `
    "$PSScriptRoot\CoreAudio.cs"

if ($LASTEXITCODE -ne 0) {
    throw "Probe build failed with exit code $LASTEXITCODE."
}

Write-Host "Built: $outDir\AudioProbe.exe"

$payload = [Convert]::ToBase64String([IO.File]::ReadAllBytes((Join-Path $outDir "SoundMeter.exe")))
$payloadPath = Join-Path $objDir "InstallerPayload.Generated.cs"
$payloadSource = @"
namespace SoundMeterInstaller
{
    internal static class InstallerPayload
    {
        internal const string SoundMeterExeBase64 = "$payload";
    }
}
"@
$utf8NoBom = New-Object System.Text.UTF8Encoding $false
[IO.File]::WriteAllText($payloadPath, $payloadSource, $utf8NoBom)

& $compiler `
    /nologo `
    /codepage:65001 `
    /target:winexe `
    /platform:anycpu `
    /win32icon:"$iconPath" `
    /out:"$distDir\SoundMeterSetup.exe" `
    /reference:System.dll `
    /reference:System.Core.dll `
    /reference:System.Drawing.dll `
    /reference:System.Windows.Forms.dll `
    "$PSScriptRoot\Installer.cs" `
    "$payloadPath"

if ($LASTEXITCODE -ne 0) {
    throw "Installer build failed with exit code $LASTEXITCODE."
}

Write-Host "Built: $distDir\SoundMeterSetup.exe"
