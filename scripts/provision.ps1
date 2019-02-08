$errorActionPreference = 'Stop'
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$headers = @{
  "Authorization"="token $env:GITHUB_TOKEN";
  "Accept"="application/vnd.github.v3.raw"
}

Invoke-WebRequest -Uri "$env:PROVISIONATOR_URL" -Headers $headers -OutFile "bootstrapinator.ps1"

& .\bootstrapinator.ps1 -executeScript "provision-skiasharp.csx" "$env:GITHUB_TOKEN"

if ($IsMacOS) {
  & .\provisionator.ps1 keychain set github.com "$env:GITHUB_TOKEN"
}

& .\provisionator.ps1 "provision-skiasharp.csx"
