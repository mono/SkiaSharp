Set-StrictMode -Version Latest

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

function Get-StreamSha256 {
    param(
        [Parameter(Mandatory)]
        [System.IO.Stream] $Stream
    )

    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try {
        return [Convert]::ToHexString($sha256.ComputeHash($Stream)).ToLowerInvariant()
    } finally {
        $sha256.Dispose()
    }
}

function Add-ArchiveInventory {
    param(
        [Parameter(Mandatory)]
        [System.IO.Stream] $Stream,

        [Parameter(Mandatory)]
        [string] $ArchivePath,

        [Parameter(Mandatory)]
        [string] $RootPackage,

        [Parameter(Mandatory)]
        [bool] $IsNuGetArchive,

        [Parameter(Mandatory)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]] $Results
    )

    $archive = [System.IO.Compression.ZipArchive]::new(
        $Stream,
        [System.IO.Compression.ZipArchiveMode]::Read,
        $true)

    try {
        foreach ($entry in $archive.Entries) {
            if ([string]::IsNullOrEmpty($entry.Name)) {
                continue
            }

            $path = "$ArchivePath!/$($entry.FullName)"
            $isSignatureMetadata = $IsNuGetArchive -and (
                $entry.FullName.Equals('.signature.p7s', [StringComparison]::OrdinalIgnoreCase) -or
                $entry.FullName.Equals('[Content_Types].xml', [StringComparison]::OrdinalIgnoreCase) -or
                $entry.FullName.StartsWith('_rels/', [StringComparison]::OrdinalIgnoreCase) -or
                $entry.FullName.StartsWith('package/', [StringComparison]::OrdinalIgnoreCase))

            $entryStream = $entry.Open()
            try {
                $hash = Get-StreamSha256 $entryStream
            } finally {
                $entryStream.Dispose()
            }

            $extension = [IO.Path]::GetExtension($entry.Name).ToLowerInvariant()
            $isArchiveCandidate = $extension -eq '.nupkg' -or $extension -eq '.zip'
            $item = [pscustomobject]@{
                RootPackage = $RootPackage
                Path = $path
                Name = $entry.Name
                Extension = $extension
                Length = $entry.Length
                Sha256 = $hash
                IsContainer = $false
                IsSignatureMetadata = $isSignatureMetadata
            }
            $Results.Add($item)

            if (-not $isArchiveCandidate -or $isSignatureMetadata) {
                continue
            }

            $nestedStream = [System.IO.MemoryStream]::new()
            try {
                $source = $entry.Open()
                try {
                    $source.CopyTo($nestedStream)
                } finally {
                    $source.Dispose()
                }
                $nestedStream.Position = 0

                try {
                    Add-ArchiveInventory `
                        -Stream $nestedStream `
                        -ArchivePath $path `
                        -RootPackage $RootPackage `
                        -IsNuGetArchive ($extension -eq '.nupkg') `
                        -Results $Results
                    $item.IsContainer = $true
                } catch [System.IO.InvalidDataException] {
                    $item.IsContainer = $false
                }
            } finally {
                $nestedStream.Dispose()
            }
        }
    } finally {
        $archive.Dispose()
    }
}

function Get-NuGetPackageInventory {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string] $PackageDirectory
    )

    $resolvedDirectory = (Resolve-Path $PackageDirectory).Path
    $packages = @(
        Get-ChildItem $resolvedDirectory -Filter '*.nupkg' -File |
            Sort-Object Name
    )
    if ($packages.Count -eq 0) {
        throw "No NuGet packages were found in '$resolvedDirectory'."
    }

    $results = [System.Collections.Generic.List[object]]::new()
    foreach ($package in $packages) {
        $stream = [IO.File]::OpenRead($package.FullName)
        try {
            Add-ArchiveInventory `
                -Stream $stream `
                -ArchivePath $package.Name `
                -RootPackage $package.Name `
                -IsNuGetArchive $true `
                -Results $results
        } finally {
            $stream.Dispose()
        }
    }

    return $results.ToArray()
}

function Get-ArcadeSigningPolicy {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [object[]] $Inventory,

        [Parameter(Mandatory)]
        [string] $SigningPropsPath
    )

    $categoryDefinitions = [ordered]@{
        FirstPartyFile = [pscustomobject]@{
            Category = 'FirstParty'
            CertificateName = 'Microsoft400'
        }
        MacDeveloperFile = [pscustomobject]@{
            Category = 'MacDeveloper'
            CertificateName = 'MacDeveloperVNext'
        }
        ThirdPartyFile = [pscustomobject]@{
            Category = 'ThirdParty'
            CertificateName = '3PartySHA2'
        }
        SkippedFile = [pscustomobject]@{
            Category = 'Skip'
            CertificateName = 'None'
        }
    }

    [xml] $signingProps = Get-Content (Resolve-Path $SigningPropsPath) -Raw
    $assignmentsByName = [System.Collections.Generic.Dictionary[string, object]]::new(
        [StringComparer]::OrdinalIgnoreCase)

    foreach ($itemName in $categoryDefinitions.Keys) {
        $definition = $categoryDefinitions[$itemName]
        $nodes = @($signingProps.SelectNodes(
            "/*[local-name()='Project']/*[local-name()='ItemGroup']/*[local-name()='$itemName']"))

        foreach ($node in $nodes) {
            $name = [string] $node.GetAttribute('Include')
            if ([string]::IsNullOrWhiteSpace($name)) {
                throw "Signing policy item '$itemName' is missing its Include value."
            }
            if ([IO.Path]::GetFileName($name) -ne $name) {
                throw "Signing policy item '$name' must be an exact basename, not a path."
            }
            if ($name.IndexOfAny([char[]] '*?[') -ge 0) {
                throw "Signing policy item '$name' must not contain wildcards."
            }
            if ($assignmentsByName.ContainsKey($name)) {
                $existing = $assignmentsByName[$name]
                throw "Signing policy item '$name' appears in both '$($existing.Category)' and '$($definition.Category)'."
            }

            $assignmentsByName.Add($name, [pscustomobject]@{
                Name = $name
                Category = $definition.Category
                CertificateName = $definition.CertificateName
                Paths = [System.Collections.Generic.List[string]]::new()
            })
        }
    }

    foreach ($assignment in $assignmentsByName.Values) {
        $matches = @($Inventory | Where-Object {
            -not $_.IsSignatureMetadata -and
            $_.Name.Equals($assignment.Name, [StringComparison]::OrdinalIgnoreCase)
        })
        if ($matches.Count -eq 0) {
            throw "Signing policy item '$($assignment.Name)' did not match any package payload."
        }
        foreach ($match in $matches) {
            $assignment.Paths.Add($match.Path)
        }
    }

    $signableExtensions = @('.dll', '.exe', '.winmd', '.dylib', '.js', '.py')
    $unclassified = @(
        $Inventory |
            Where-Object {
                -not $_.IsSignatureMetadata -and
                $_.Extension -in $signableExtensions -and
                -not $assignmentsByName.ContainsKey($_.Name)
            } |
            Sort-Object Path
    )
    if ($unclassified.Count -ne 0) {
        $paths = $unclassified.Path -join [Environment]::NewLine
        throw "eng/Signing.props does not classify these signable package entries:$([Environment]::NewLine)$paths"
    }

    return [pscustomobject]@{
        Files = @($assignmentsByName.Values | Sort-Object Name)
    }
}

Export-ModuleMember -Function `
    Get-NuGetPackageInventory, `
    Get-ArcadeSigningPolicy
