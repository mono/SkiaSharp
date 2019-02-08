$errorActionPreference = 'Stop'
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

Write-Host "Downloading..."
$headers = @{
  "Authorization"="token $env:GITHUB_TOKEN";
  "Accept"="application/vnd.github.v3.raw"
}
Invoke-WebRequest -Uri "$env:PROVISIONATOR_URL" -Headers $headers -OutFile "bootstrapinator.ps1"

Write-Host "Preparing..."
& .\bootstrapinator.ps1 -executeScript "provision-skiasharp.csx"

if ($IsMacOS) {
  Write-Host "Authorizing..."
  & .\provisionator.ps1 keychain set github.com "$env:GITHUB_TOKEN"
}

Write-Host "Executing..."
& .\provisionator.ps1 "provision-skiasharp.csx"

Write-Host "Complete."
