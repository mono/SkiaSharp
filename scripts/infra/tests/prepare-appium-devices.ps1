param(
    [Parameter(Mandatory)]
    [ValidateSet('Android', 'iOS')]
    [string] $Platform,
    [ValidateSet('', '24', '37')]
    [string] $AndroidApi = ''
)

$ErrorActionPreference = 'Stop'
if ($env:TF_BUILD -ne 'True') { throw 'Device creation is an explicit CI prerequisite only.' }

function Invoke-Checked([string[]] $Arguments) {
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) { throw "dotnet failed ($LASTEXITCODE): $($Arguments -join ' ')" }
}

if ($Platform -eq 'Android') {
    if ($AndroidApi -notin @('24', '37')) { throw 'Declare the assigned Android API job.' }
    $architecture = if ([System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture -eq 'Arm64') { 'arm64-v8a' } else { 'x86_64' }
    $profiles = @(
        @{ Name = 'Pixel_API_24'; Image = "system-images;android-24;google_apis;$architecture" },
        @{ Name = 'Pixel_API_37'; Image = "system-images;android-37.1;google_apis_ps16k;$architecture" }
    )
    $profiles = @($profiles | Where-Object { $_.Name -eq "Pixel_API_$AndroidApi" })
    foreach ($profile in $profiles) {
        & pwsh -NoProfile -File scripts/infra/managed/install-android-package.ps1 -Package $profile.Image
        if ($LASTEXITCODE -ne 0) { throw "Image installation failed: $($profile.Image)" }
        Invoke-Checked @('android', 'avd', 'create', '--name', $profile.Name, '--sdk', $profile.Image, '--device', 'pixel_9')
    }
}
else {
    foreach ($version in @('18.6', '26.2')) {
        $devices = & dotnet apple simulator list --available --name 'iPhone 16 Pro' --runtime "iOS $version" --format json
        if ($LASTEXITCODE -ne 0) { throw "Could not inspect iOS $version." }
        $matches = @($devices | ConvertFrom-Json | Where-Object { $_.name -eq 'iPhone 16 Pro' -and $_.runtime.version -eq $version })
        if ($matches.Count -gt 1) { throw "Ambiguous iPhone 16 Pro / iOS $version selection." }
        if ($matches.Count -eq 0) {
            Invoke-Checked @('apple', 'simulator', 'create', 'iPhone 16 Pro', '--device-type', 'iPhone 16 Pro', '--runtime', "iOS $version", '--format', 'json')
        }
    }
}
