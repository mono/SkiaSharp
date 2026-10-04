#Requires -Version 7.0
<#
.SYNOPSIS
Lists the complete NuGet mirror inventory for a CLI workload-set version.
.DESCRIPTION
Accepts CLI versions such as 10.0.401 or 11.0.100-rc.1.26458.5, not NuGet
package versions. Reads only workload-set and manifest packages from the
enabled dotnet-public/dotnet-eng sources in the repository NuGet.config.
Includes those metadata packages and all packs, resolving aliases for every
host RID. Does not download packs, check a mirror, or install workloads.
Writes only deduplicated "- ID/Version" lines to the success stream, after
the entire inventory has been read successfully.
.EXAMPLE
.\scripts\infra\managed\list-workload-packs.ps1 -WorkloadSetVersion 10.0.401
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $WorkloadSetVersion
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
$repoRoot = $PSScriptRoot
1..3 | ForEach-Object { $repoRoot = Split-Path -Parent $repoRoot }
$scratch = Join-Path $repoRoot 'output' -AdditionalChildPath 'tmp', "workload-packs-$([guid]::NewGuid())"
$packages = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$packageIdPattern = '^[A-Za-z0-9_][A-Za-z0-9_.-]*$'
$packageVersionPattern = '^\d+\.\d+\.\d+(?:\.\d+)?(?:-[A-Za-z0-9-]+(?:\.[A-Za-z0-9-]+)*)?$'

function Add-Package([string] $Id, [string] $Version) {
    if ($Id -notmatch $packageIdPattern -or $Version -notmatch $packageVersionPattern) {
        throw "Invalid package identity '$Id/$Version' in workload metadata."
    }
    $null = $packages.Add("$Id/$Version")
}

function Read-PackageJson([string] $Id, [string] $Version, [string] $EntryName) {
    Add-Package $Id $Version
    $lowerId = $Id.ToLowerInvariant()
    $lowerVersion = $Version.ToLowerInvariant()
    $path = Join-Path $scratch "$lowerId.$lowerVersion.nupkg"
    $found = $false
    foreach ($baseAddress in $baseAddresses) {
        $url = "$($baseAddress.TrimEnd('/'))/$lowerId/$lowerVersion/$lowerId.$lowerVersion.nupkg"
        Write-Verbose "Reading $Id/$Version from $baseAddress"
        try {
            Invoke-WebRequest -Uri $url -OutFile $path
            $found = $true
            break
        } catch {
            if ($_.Exception.PSObject.Properties['Response'] -and
                $_.Exception.Response -and [int]$_.Exception.Response.StatusCode -eq 404) {
                continue
            }
            throw
        }
    }
    if (-not $found) {
        throw "Missing metadata package '$Id/$Version' in the configured approved sources."
    }
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
    [xml]$config = Get-Content -LiteralPath (Join-Path $repoRoot 'NuGet.config') -Raw
    $baseAddresses = @(
        foreach ($source in $config.SelectNodes('/configuration/packageSources/add')) {
            if ($source.key -notin @('dotnet-public', 'dotnet-eng')) { continue }
            $disabled = $config.SelectNodes('/configuration/disabledPackageSources/add') |
                Where-Object { $_.key -eq $source.key -and $_.value -eq 'true' }
            if ($disabled) { continue }
            $index = Invoke-RestMethod -Uri $source.value
            $addresses = @($index.resources | Where-Object { $_.'@type' -eq 'PackageBaseAddress/3.0.0' })
            if ($addresses.Count -ne 1) {
                throw "Expected one NuGet PackageBaseAddress in '$($source.key)'."
            }
            $addresses[0].'@id'
        }
    )
    if ($baseAddresses.Count -eq 0) {
        throw 'No enabled dotnet-public/dotnet-eng sources in repository NuGet.config.'
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
[Array]::Sort($inventory, [StringComparer]::OrdinalIgnoreCase)
$inventory | ForEach-Object { "- $_" }
