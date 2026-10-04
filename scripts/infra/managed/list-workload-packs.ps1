#Requires -Version 7.0
<#
.SYNOPSIS
Lists the complete NuGet mirror inventory for a CLI workload-set version.
.DESCRIPTION
Accepts CLI versions such as 10.0.401 or 11.0.100-rc.1.26458.5, not NuGet
package versions. Reads workload-set and manifest packages from NuGet.org.
Includes those metadata packages and all packs, resolving aliases for every
host RID. By default, does not download packs, check a mirror, or install workloads.
Writes only deduplicated "- ID/Version" lines to the success stream, after
the entire inventory has been read successfully.
With -MissingOnly, checks package version indexes on the dotnet-public mirror
and prints only versions absent there. Runs independently of repository files.
.EXAMPLE
.\scripts\infra\managed\list-workload-packs.ps1 -WorkloadSetVersion 10.0.401
.EXAMPLE
.\scripts\infra\managed\list-workload-packs.ps1 -WorkloadSetVersion 10.0.401 -MissingOnly
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $WorkloadSetVersion,
    [switch] $MissingOnly
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ($WorkloadSetVersion -notmatch '^(?<major>[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>[1-9]\d{2})(?<suffix>-(?<label>preview|rc)\.(?<number>[1-9]\d*)\.\d+\.\d+)?$') {
    throw "Invalid CLI workload-set version '$WorkloadSetVersion'; expected e.g. 10.0.401 or 11.0.100-rc.1.26458.5."
}
$bandPatch = [int]([math]::Floor([int]$Matches.patch / 100) * 100)
$band = "$($Matches.major).$($Matches.minor).$bandPatch"
if ($Matches['suffix']) {
    $band += "-$($Matches.label).$($Matches.number)"
}
$setId = "Microsoft.NET.Workloads.$band"
$setVersion = "$($Matches.major).$($Matches.patch).0$($Matches['suffix'])"
Set-Variable -Name SourceFeed -Value 'https://api.nuget.org/v3/index.json' -Option Constant
Set-Variable -Name MirrorFeed -Value 'https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/index.json' -Option Constant
$scratch = Join-Path ([IO.Path]::GetTempPath()) "workload-packs-$([guid]::NewGuid())"
$packages = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$packageIdPattern = '^[A-Za-z0-9_][A-Za-z0-9_.-]*$'
$packageVersionPattern = '^\d+\.\d+\.\d+(?:\.\d+)?(?:-[A-Za-z0-9-]+(?:\.[A-Za-z0-9-]+)*)?$'

function Add-Package([string] $Id, [string] $Version) {
    if ($Id -notmatch $packageIdPattern -or $Version -notmatch $packageVersionPattern) {
        throw "Invalid package identity '$Id/$Version' in workload metadata."
    }
    $null = $packages.Add("$Id/$Version")
}

function Get-PackageBaseAddress([string] $IndexUrl) {
    $index = Invoke-RestMethod -Uri $IndexUrl
    $addresses = @($index.resources | Where-Object { $_.'@type' -eq 'PackageBaseAddress/3.0.0' })
    if ($addresses.Count -ne 1) {
        throw "Expected one NuGet PackageBaseAddress at '$IndexUrl'."
    }
    return $addresses[0].'@id'
}

function Read-PackageJson([string] $Id, [string] $Version, [string] $EntryName) {
    Add-Package $Id $Version
    $lowerId = $Id.ToLowerInvariant()
    $lowerVersion = $Version.ToLowerInvariant()
    $path = Join-Path $scratch "$lowerId.$lowerVersion.nupkg"
    $url = "$($sourceBaseAddress.TrimEnd('/'))/$lowerId/$lowerVersion/$lowerId.$lowerVersion.nupkg"
    Write-Verbose "Reading $Id/$Version from NuGet.org"
    Invoke-WebRequest -Uri $url -OutFile $path
    $zip = [IO.Compression.ZipFile]::OpenRead($path)
    try {
        $entry = $zip.GetEntry($EntryName)
        if (-not $entry) {
            throw "Missing '$EntryName' in '$Id/$Version'."
        }
        $reader = [IO.StreamReader]::new($entry.Open())
        try {
            $json = ConvertFrom-Json -InputObject $reader.ReadToEnd() -AsHashtable
        } finally {
            $reader.Dispose()
        }
        if ($json -isnot [Collections.IDictionary] -or $json.Count -eq 0) {
            throw "Expected a nonempty JSON object in '$Id/$Version/$EntryName'."
        }
        return $json
    } finally {
        $zip.Dispose()
    }
}

try {
    $sourceBaseAddress = Get-PackageBaseAddress $SourceFeed
    $mirrorBaseAddress = $null
    if ($MissingOnly) {
        $mirrorBaseAddress = Get-PackageBaseAddress $MirrorFeed
    }
    $null = New-Item -ItemType Directory -Path $scratch
    $set = Read-PackageJson $setId $setVersion 'data/microsoft.net.workloads.workloadset.json'
    foreach ($manifest in $set.GetEnumerator()) {
        if ($manifest.Value -isnot [string]) {
            throw "Invalid manifest reference '$($manifest.Key)' in '$setId/$setVersion'."
        }
        $parts = $manifest.Value.Split('/')
        if ($parts.Count -notin @(1, 2)) {
            throw "Invalid manifest reference '$($manifest.Key)/$($manifest.Value)'."
        }
        # WorkloadSet.FromDictionaryForJson uses the set's feature band when omitted.
        $manifestBand = if ($parts.Count -eq 2) { $parts[1] } else { $band }
        if ($manifestBand -notmatch '^[1-9]\d*\.\d+\.[1-9]00(?:-(?:preview|rc)\.[1-9]\d*)?$') {
            throw "Invalid manifest feature band '$manifestBand'."
        }
        $id = "$($manifest.Key).Manifest-$manifestBand"
        $definition = Read-PackageJson $id $parts[0] 'data/WorkloadManifest.json'
        if ($definition.packs -isnot [Collections.IDictionary] -or $definition.packs.Count -eq 0) {
            throw "Missing or invalid packs dictionary in '$id/$($parts[0])'."
        }
        foreach ($pack in $definition.packs.GetEnumerator()) {
            if ($pack.Value -isnot [Collections.IDictionary] -or $pack.Value.version -isnot [string]) {
                throw "Invalid pack definition '$($pack.Key)' in '$id'."
            }
            if ($pack.Value.Contains('alias-to')) {
                $aliases = $pack.Value['alias-to']
                if ($aliases -isnot [Collections.IDictionary] -or $aliases.Count -eq 0) {
                    throw "Invalid alias-to dictionary for '$($pack.Key)' in '$id'."
                }
                foreach ($target in $aliases.Values) {
                    if ($target -isnot [string]) {
                        throw "Invalid alias target for '$($pack.Key)' in '$id'."
                    }
                    Add-Package $target $pack.Value.version
                }
            } else {
                Add-Package $pack.Key $pack.Value.version
            }
        }
    }
} finally {
    if (Test-Path -LiteralPath $scratch) {
        Remove-Item -LiteralPath $scratch -Recurse -Force
    }
}

$inventory = @($packages)
if ($MissingOnly) {
    if (-not $mirrorBaseAddress) {
        throw 'Missing dotnet-public mirror package base address.'
    }
    $checkJob = $inventory | Group-Object { $_.Split('/')[0] } | ForEach-Object -Parallel {
        $ErrorActionPreference = 'Stop'
        $remaining = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
        foreach ($package in $_.Group) {
            $null = $remaining.Add($package.Split('/')[1])
        }
        $baseAddress = $using:mirrorBaseAddress
        $url = "$($baseAddress.TrimEnd('/'))/$($_.Name.ToLowerInvariant())/index.json"
        $response = Invoke-WebRequest -Uri $url -SkipHttpErrorCheck -TimeoutSec 30
        if ($response.StatusCode -ne 404) {
            if ($response.StatusCode -ne 200) {
                throw "Package index request failed: HTTP $($response.StatusCode) at '$url'."
            }
            $index = ConvertFrom-Json -InputObject $response.Content
            if (-not $index.PSObject.Properties['versions'] -or $index.versions -isnot [array]) {
                throw "Invalid package version index at '$url'."
            }
            foreach ($version in $index.versions) {
                if ($version -isnot [string]) {
                    throw "Invalid package version in index at '$url'."
                }
                $null = $remaining.Remove($version)
            }
        }
        foreach ($package in $_.Group) {
            if ($remaining.Contains($package.Split('/')[1])) { $package }
        }
    } -ThrottleLimit 12 -AsJob
    try {
        $inventory = @(Receive-Job -Job $checkJob -Wait -ErrorAction Stop)
    } finally {
        Stop-Job -Job $checkJob
        Remove-Job -Job $checkJob -Force
    }
}
[Array]::Sort($inventory, [StringComparer]::OrdinalIgnoreCase)
$inventory | ForEach-Object { "- $_" }
