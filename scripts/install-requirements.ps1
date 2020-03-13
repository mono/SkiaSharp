$ErrorActionPreference = 'Stop'

$temp = "$env:TEMP\SkiaSharpRequirements"
New-Item $temp -Type Directory -Force | Out-Null

Write-Host "Installing the nano-api-scan .NET Core tool..."
dotnet tool update -g nano-api-scan

Write-Host "Installing the Windows 10 SDK 10.0.10240..."
& "$PSScriptRoot\download-file.ps1" -Uri 'https://go.microsoft.com/fwlink/p/?LinkId=619296' -OutFile "$temp\sdksetup.exe"
& "$env:TEMP\SkiaSharpRequirements\sdksetup.exe" /norestart /quiet | Out-Null

Write-Host "Installing GTK# 2.12..."
& "$PSScriptRoot\download-file.ps1" -Uri 'https://xamarin.azureedge.net/GTKforWindows/Windows/gtk-sharp-2.12.45.msi' -OutFile "$temp\gtk-sharp.msi"
msiexec /i "$env:TEMP\SkiaSharpRequirements\gtk-sharp.msi" /norestart /quiet

Write-Host "Installation complete."
