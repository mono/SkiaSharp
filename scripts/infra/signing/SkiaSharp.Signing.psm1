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

function Get-SkiaSharpPackageInventory {
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

function Get-SignListRules {
    param(
        [Parameter(Mandatory)]
        [string] $SignListPath,

        [Parameter(Mandatory)]
        [string] $MacCertificateName
    )

    $certificateByCategory = [ordered]@{
        FirstParty = 'Microsoft400'
        ThirdParty = '3PartySHA2'
        Skip = 'None'
        MacDeveloperSign = $MacCertificateName
    }

    [xml] $signList = Get-Content (Resolve-Path $SignListPath) -Raw
    $nodes = @($signList.SelectNodes("/*[local-name()='Project']/*[local-name()='ItemGroup']/*"))
    $rules = [System.Collections.Generic.List[object]]::new()

    foreach ($node in $nodes) {
        $category = $node.LocalName
        if (-not $certificateByCategory.Contains($category)) {
            throw "Unsupported SignList item '$category'. Update the Arcade adapter before using this signing policy."
        }

        $include = [string] $node.GetAttribute('Include')
        if ([string]::IsNullOrWhiteSpace($include)) {
            throw "SignList item '$category' is missing its Include value."
        }

        foreach ($pattern in @($include.Split(';') | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })) {
            $rules.Add([pscustomobject]@{
                Category = $category
                Pattern = $pattern.Trim()
                CertificateName = $certificateByCategory[$category]
            })
        }
    }

    if ($rules.Count -eq 0) {
        throw "The signing policy '$SignListPath' did not contain any supported rules."
    }

    return $rules.ToArray()
}

function Get-SkiaSharpSigningPolicy {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [object[]] $Inventory,

        [Parameter(Mandatory)]
        [string] $SignListPath,

        [string] $MacCertificateName = 'MacDeveloperVNext'
    )

    $rules = @(Get-SignListRules $SignListPath $MacCertificateName)
    $assignmentsByPath = [System.Collections.Generic.Dictionary[string, object]]::new(
        [StringComparer]::Ordinal)
    $assignmentsByName = [System.Collections.Generic.Dictionary[string, object]]::new(
        [StringComparer]::OrdinalIgnoreCase)
    $resolvedRules = [System.Collections.Generic.List[object]]::new()
    $wildcardOptions =
        [System.Management.Automation.WildcardOptions]::IgnoreCase -bor
        [System.Management.Automation.WildcardOptions]::CultureInvariant

    foreach ($rule in $rules) {
        $wildcard = [System.Management.Automation.WildcardPattern]::new(
            $rule.Pattern,
            $wildcardOptions)
        $matches = @(
            $Inventory |
                Where-Object {
                    -not $_.IsSignatureMetadata -and $wildcard.IsMatch($_.Name)
                } |
                Sort-Object Path
        )
        if ($matches.Count -eq 0) {
            Write-Warning "SignList rule '$($rule.Category):$($rule.Pattern)' did not match any package payload."
            $resolvedRules.Add([pscustomobject]@{
                Category = $rule.Category
                Pattern = $rule.Pattern
                CertificateName = $rule.CertificateName
                MatchedPaths = @()
            })
            continue
        }

        foreach ($match in $matches) {
            if ($assignmentsByPath.ContainsKey($match.Path)) {
                $existing = $assignmentsByPath[$match.Path]
                if ($existing.Category -ne $rule.Category -or
                    $existing.CertificateName -ne $rule.CertificateName) {
                    throw "Package entry '$($match.Path)' has conflicting signing rules '$($existing.Category)' and '$($rule.Category)'."
                }
                continue
            }

            if ($assignmentsByName.ContainsKey($match.Name)) {
                $existing = $assignmentsByName[$match.Name]
                if ($existing.Category -ne $rule.Category -or
                    $existing.CertificateName -ne $rule.CertificateName) {
                    throw "Arcade signs by basename, but '$($match.Name)' resolves to conflicting signing rules."
                }
                $existing.Paths.Add($match.Path)
                $assignmentsByPath.Add($match.Path, $existing)
                continue
            }

            $assignment = [pscustomobject]@{
                Name = $match.Name
                Category = $rule.Category
                CertificateName = $rule.CertificateName
                Paths = [System.Collections.Generic.List[string]]::new()
            }
            $assignment.Paths.Add($match.Path)
            $assignmentsByName.Add($match.Name, $assignment)
            $assignmentsByPath.Add($match.Path, $assignment)
        }

        $resolvedRules.Add([pscustomobject]@{
            Category = $rule.Category
            Pattern = $rule.Pattern
            CertificateName = $rule.CertificateName
            MatchedPaths = @($matches.Path)
        })
    }

    $signableExtensions = @('.dll', '.exe', '.winmd', '.dylib')
    $unclassified = @(
        $Inventory |
            Where-Object {
                -not $_.IsSignatureMetadata -and
                $_.Extension -in $signableExtensions -and
                -not $assignmentsByPath.ContainsKey($_.Path)
            } |
            Sort-Object Path
    )
    if ($unclassified.Count -ne 0) {
        $paths = $unclassified.Path -join [Environment]::NewLine
        throw "SignList.xml does not classify these signable package entries:$([Environment]::NewLine)$paths"
    }

    return [pscustomobject]@{
        Rules = $resolvedRules.ToArray()
        Files = @($assignmentsByName.Values | Sort-Object Name)
    }
}

function Write-SkiaSharpArcadeSigningProps {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [object] $Policy,

        [Parameter(Mandatory)]
        [string] $OutputPath
    )

    $directory = Split-Path $OutputPath -Parent
    New-Item $directory -ItemType Directory -Force | Out-Null

    $settings = [System.Xml.XmlWriterSettings]::new()
    $settings.Indent = $true
    $settings.Encoding = [System.Text.UTF8Encoding]::new($false)
    $writer = [System.Xml.XmlWriter]::Create($OutputPath, $settings)

    try {
        $writer.WriteStartDocument()
        $writer.WriteStartElement('Project')

        $writer.WriteStartElement('PropertyGroup')
        $writer.WriteElementString('EnableDefaultArtifacts', 'false')
        $writer.WriteElementString('UseDotNetCertificate', 'false')
        $writer.WriteElementString('DoStrongNameCheck', 'false')
        $writer.WriteEndElement()

        $writer.WriteStartElement('ItemGroup')

        foreach ($itemName in @('ItemsToSign', 'StrongNameSignInfo', 'FileSignInfo', 'FileExtensionSignInfo')) {
            $writer.WriteStartElement($itemName)
            $writer.WriteAttributeString('Remove', "@($itemName)")
            $writer.WriteEndElement()
        }

        $writer.WriteStartElement('ItemsToSign')
        $writer.WriteAttributeString(
            'Include',
            '$([MSBuild]::EnsureTrailingSlash(''$(SigningPackageDirectory)''))*.nupkg')
        $writer.WriteEndElement()

        foreach ($extension in @(
            @{ Name = '.nupkg'; Certificate = 'NuGet' },
            @{ Name = '.zip'; Certificate = 'None' }
        )) {
            $writer.WriteStartElement('FileExtensionSignInfo')
            $writer.WriteAttributeString('Include', $extension.Name)
            $writer.WriteAttributeString('CertificateName', $extension.Certificate)
            $writer.WriteEndElement()
        }

        foreach ($file in @($Policy.Files | Sort-Object Name)) {
            $writer.WriteStartElement('FileSignInfo')
            $writer.WriteAttributeString('Include', $file.Name)
            $writer.WriteAttributeString('CertificateName', $file.CertificateName)
            $writer.WriteEndElement()

            if ($file.Category -eq 'FirstParty') {
                $writer.WriteStartElement('ItemsToSkip3rdPartyCheck')
                $writer.WriteAttributeString('Include', $file.Name)
                $writer.WriteEndElement()
            }
        }

        $writer.WriteEndElement()
        $writer.WriteEndElement()
        $writer.WriteEndDocument()
    } finally {
        $writer.Dispose()
    }
}

function Write-SkiaSharpSigningManifest {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string] $PackageDirectory,

        [Parameter(Mandatory)]
        [string] $SignListPath,

        [Parameter(Mandatory)]
        [object[]] $Inventory,

        [Parameter(Mandatory)]
        [object] $Policy,

        [Parameter(Mandatory)]
        [string] $OutputPath
    )

    $assignmentsByPath = @{}
    foreach ($file in $Policy.Files) {
        foreach ($path in $file.Paths) {
            $assignmentsByPath[$path] = $file
        }
    }

    $packages = @(
        Get-ChildItem (Resolve-Path $PackageDirectory) -Filter '*.nupkg' -File |
            Sort-Object Name |
            ForEach-Object {
                [ordered]@{
                    name = $_.Name
                    length = $_.Length
                    sha256 = (Get-FileHash $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
                }
            }
    )

    $entries = @(
        $Inventory |
            Sort-Object Path |
            ForEach-Object {
                $assignment = $assignmentsByPath[$_.Path]
                [ordered]@{
                    path = $_.Path
                    name = $_.Name
                    length = $_.Length
                    sha256 = $_.Sha256
                    isContainer = $_.IsContainer
                    isSignatureMetadata = $_.IsSignatureMetadata
                    category = if ($assignment) { $assignment.Category } else { $null }
                    certificateName = if ($assignment) { $assignment.CertificateName } else { $null }
                }
            }
    )

    $manifest = [ordered]@{
        formatVersion = 1
        generatedAtUtc = [DateTime]::UtcNow.ToString('O')
        signListSha256 = (Get-FileHash (Resolve-Path $SignListPath) -Algorithm SHA256).Hash.ToLowerInvariant()
        packages = $packages
        rules = @($Policy.Rules)
        entries = $entries
    }

    $directory = Split-Path $OutputPath -Parent
    New-Item $directory -ItemType Directory -Force | Out-Null
    $manifest |
        ConvertTo-Json -Depth 10 |
        Set-Content $OutputPath -Encoding utf8NoBOM
}

Export-ModuleMember -Function `
    Get-SkiaSharpPackageInventory, `
    Get-SkiaSharpSigningPolicy, `
    Write-SkiaSharpArcadeSigningProps, `
    Write-SkiaSharpSigningManifest
