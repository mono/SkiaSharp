$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$scriptPath = Join-Path $PSScriptRoot '../install-spectre.ps1'
$tokens = $null
$parseErrors = $null
$ast = [Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
if ($parseErrors.Count -ne 0) {
    throw "Provisioning script has syntax errors: $parseErrors"
}
foreach ($function in $ast.FindAll({
    param($node)
    $node -is [Management.Automation.Language.FunctionDefinitionAst]
}, $false)) {
    . ([scriptblock]::Create($function.Extent.Text))
}

function Assert-Equal($Actual, $Expected, [string] $Description) {
    if ($Actual -cne $Expected) {
        throw "$Description expected '$Expected' but got '$Actual'."
    }
}

function Get-Statement([string] $Prefix) {
    $statements = @($ast.EndBlock.Statements | Where-Object { $_.Extent.Text.StartsWith($Prefix) })
    Assert-Equal $statements.Count 1 "Statement matching $Prefix"
    return [scriptblock]::Create($statements[0].Extent.Text)
}

$fixture = Join-Path ([IO.Path]::GetTempPath()) "skiasharp-spectre-$([guid]::NewGuid())"
try {
    $InstallationPath = Join-Path $fixture 'Visual Studio'
    $build = Join-Path $InstallationPath 'VC/Auxiliary/Build'
    New-Item $build -ItemType Directory -Force | Out-Null
    $defaultFile = Join-Path $build 'Microsoft.VCToolsVersion.default.txt'
    $v143File = Join-Path $build 'Microsoft.VCToolsVersion.v143.default.txt'
    Set-Content $v143File '14.44.35207'

    foreach ($case in @(
        @{ Default = '14.30.30705'; Expected = '14.30.30705' }
        @{ Default = '14.44.35207'; Expected = '14.44.35207' }
        @{ Default = '14.49.12345'; Expected = '14.49.12345' }
        @{ Default = '14.50.35717'; Expected = '14.44.35207' }
        @{ Default = '14.29.30133'; Expected = '14.44.35207' }
        @{ Default = '15.0.12345'; Expected = '14.44.35207' }
    )) {
        Set-Content $defaultFile " $($case.Default)`r`n"
        Assert-Equal (Get-ToolsetVersion) $case.Expected "Toolset selection for $($case.Default)"
    }

    $version = '14.44.35207'
    $Architecture = 'x64'
    $libraries = Join-Path $InstallationPath "VC/Tools/MSVC/$version/lib/spectre/x64"
    Assert-Equal (Test-SpectreLibraries $version) $false 'Missing Spectre directory'
    New-Item $libraries -ItemType Directory -Force | Out-Null
    $names = @('libcmt.lib', 'libcpmt.lib', 'libvcruntime.lib')
    foreach ($name in $names) {
        Set-Content (Join-Path $libraries $name) 'fixture'
    }
    Assert-Equal (Test-SpectreLibraries $version) $true 'All required libraries installed'
    foreach ($name in $names) {
        Remove-Item (Join-Path $libraries $name)
        Assert-Equal (Test-SpectreLibraries $version) $false "Missing $name"
        Set-Content (Join-Path $libraries $name) 'fixture'
    }
    Assert-Equal (Test-SpectreLibraries '14.50.35717') $false 'Libraries for a different toolset'
    $Architecture = 'x86'
    Assert-Equal (Test-SpectreLibraries $version) $false 'Libraries for a different architecture'

    # Exercise the actual catalogue selection and uniqueness guard, without an installer.
    $selectComponents = Get-Statement '$components ='
    $requireUnique = Get-Statement 'if ($components.Count'
    . (Get-Statement '$parsed =')
    . (Get-Statement '$prefix =')
    $matchingId = 'Microsoft.VisualStudio.Component.VC.14.44.17.14.x86.x64.Spectre'
    foreach ($case in @(
        @{ Ids = @($matchingId, $matchingId, 'Microsoft.VisualStudio.Component.VC.14.50.18.0.x86.x64.Spectre'); Count = 1 }
        @{ Ids = @('Microsoft.VisualStudio.Component.VC.14.50.18.0.x86.x64.Spectre'); Count = 0 }
        @{ Ids = @($matchingId, 'Microsoft.VisualStudio.Component.VC.14.44.17.15.x86.x64.Spectre'); Count = 2 }
    )) {
        $catalog = @{
            packages = @($case.Ids | ForEach-Object { @{ type = 'Component'; id = $_ } }) +
                @(@{ type = 'Package'; id = $matchingId })
        }
        . $selectComponents
        Assert-Equal $components.Count $case.Count 'Matching catalogue components'
        $rejected = $false
        try {
            . $requireUnique
        } catch {
            if ($_.Exception.Message -notlike '*no unique matching Spectre component*') {
                throw
            }
            $rejected = $true
        }
        Assert-Equal $rejected ($components.Count -ne 1) 'Catalogue uniqueness guard'
        if (!$rejected) {
            Assert-Equal $components[0] $matchingId 'Exact v143 Spectre component'
        }
    }

    $workflowPath = Join-Path $PSScriptRoot '../../../../../.github/workflows/track-benchmarks.yml'
    $workflow = Get-Content $workflowPath -Raw
    if ($workflow -notmatch 'select-vs\.ps1\r?\n\s+\./scripts/infra/native/windows/install-spectre\.ps1 -InstallationPath \$env:VS_INSTALL -Architecture x64\r?\n') {
        throw 'Windows benchmark must provision Spectre libraries immediately after selecting Visual Studio.'
    }
    $postInstallCheck = Get-Statement 'if (!(Test-SpectreLibraries'
    $Architecture = 'x64'
    . $postInstallCheck
    Remove-Item (Join-Path $libraries 'libcmt.lib')
    $rejected = $false
    try {
        . $postInstallCheck
    } catch {
        if ($_.Exception.Message -notlike '*installation completed without matching*') {
            throw
        }
        $rejected = $true
    }
    Assert-Equal $rejected $true 'Incomplete installation must fail'
    Write-Host 'PASS: v143 selection, library completeness/isolation, catalogue selection, post-install guard, and benchmark caller contract.'
} finally {
    if (Test-Path $fixture) {
        Remove-Item $fixture -Recurse -Force
    }
}
