Param([string]$githubToken)

$env:GITHUB_TOKEN = $githubToken

$errorActionPreference = 'Stop'
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$uri = "https://api.github.com/repos/xamarin/components/contents/provisionator/bootstrapinator.ps1"
$headers = @{
  "Authorization"="token $env:GITHUB_TOKEN";
  "Accept"="application/vnd.github.v3.raw"
}

Invoke-WebRequest -Uri $uri -Headers $headers -OutFile "bootstrapinator.ps1"

& .\bootstrapinator.ps1 -executeScript "provision-skiasharp.csx"
& .\provisionator.ps1 "provision-skiasharp.csx"
