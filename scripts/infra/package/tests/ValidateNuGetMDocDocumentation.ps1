[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string] $SourcePath
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$minimumDocumentedMemberPercentage = 90

if (-not (Test-Path $SourcePath -PathType Container)) {
    throw "NuGet package directory does not exist: $SourcePath"
}

Add-Type -AssemblyName System.IO.Compression

$packages = @(
    Get-ChildItem -Path $SourcePath -Filter '*.nupkg' -File |
        Where-Object {
            $_.Name -match '^(SkiaSharp|HarfBuzzSharp).*\.nupkg$' -and
            $_.Name -notlike '*.symbols.nupkg'
        }
)
if ($packages.Count -eq 0) {
    throw "No SkiaSharp or HarfBuzzSharp NuGet packages found in: $SourcePath"
}

$failures = [System.Collections.Generic.List[string]]::new()

foreach ($package in $packages) {
    $stream = [IO.File]::Open($package.FullName, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::Read)
    try {
        $archive = [IO.Compression.ZipArchive]::new($stream, [IO.Compression.ZipArchiveMode]::Read, $false)
        try {
            $entries = @{}
            foreach ($entry in $archive.Entries) {
                $entries[$entry.FullName] = $entry
            }

            foreach ($dll in @($archive.Entries | Where-Object { $_.FullName -match '^lib/[^/]+/[^/]+\.dll$' })) {
                $xmlPath = "$($dll.FullName.Substring(0, $dll.FullName.Length - 4)).xml"
                if (-not $entries.ContainsKey($xmlPath)) {
                    $failures.Add("$($package.Name): missing '$xmlPath' for '$($dll.FullName)'.")
                    continue
                }

                $reader = [IO.StreamReader]::new($entries[$xmlPath].Open())
                try {
                    $contents = $reader.ReadToEnd()
                } finally {
                    $reader.Dispose()
                }

                try {
                    $xml = [xml] $contents
                } catch {
                    $failures.Add("$($package.Name): '$xmlPath' is not valid XML: $($_.Exception.Message)")
                    continue
                }

                $members = @($xml.SelectNodes('/doc/members/member'))
                if ([string]::IsNullOrWhiteSpace($contents) -or $members.Count -eq 0) {
                    $failures.Add("$($package.Name): '$xmlPath' does not contain exported API members.")
                    continue
                }
                if ($contents -match '(?i)To be added\.') {
                    $failures.Add("$($package.Name): '$xmlPath' contains a 'To be added.' placeholder.")
                }

                $documentedMembers = @(
                    $members | Where-Object {
                        $summary = $_.SelectSingleNode('summary')
                        $summary -and -not [string]::IsNullOrWhiteSpace($summary.InnerText)
                    }
                )
                $documentedMemberPercentage = 100 * $documentedMembers.Count / $members.Count
                if ($documentedMemberPercentage -lt $minimumDocumentedMemberPercentage) {
                    $failures.Add((
                        "{0}: '{1}' documents {2:N1}% of exported API members; at least {3}% require non-empty mdoc summaries." -f
                        $package.Name,
                        $xmlPath,
                        $documentedMemberPercentage,
                        $minimumDocumentedMemberPercentage
                    ))
                }
            }
        } finally {
            $archive.Dispose()
        }
    } finally {
        $stream.Dispose()
    }
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host $_ }
    throw "NuGet mdoc documentation validation failed with $($failures.Count) issue(s)."
}

Write-Host "NuGet mdoc documentation validation passed for $($packages.Count) package(s)."
