#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Extract and merge ECMA XML API documentation via JSON.

.DESCRIPTION
    Uses .NET XmlDocument which natively preserves CDATA sections.
    No external dependencies required (pwsh is pre-installed on all GHA runners).

.EXAMPLE
    pwsh docs-tool.ps1 extract docs/SkiaSharpAPI/ -Output output/docs-work/
    pwsh docs-tool.ps1 merge output/docs-work/
#>

param(
    [Parameter(Position = 0)]
    [ValidateSet("extract", "merge")]
    [string]$Command,

    [Parameter(Position = 1)]
    [string]$Path,

    [string]$Output,
    [switch]$DryRun
)

# ---------------------------------------------------------------------------
# Shared helpers
# ---------------------------------------------------------------------------

function Load-XmlPreserving([string]$filePath) {
    $doc = [xml]::new()
    $doc.PreserveWhitespace = $true
    $doc.Load($filePath)
    return $doc
}

function Find-DocsByDocId([xml]$doc, [string]$docId) {
    $xpath = "//Member[MemberSignature[@Language='DocId' and @Value='$docId']]/Docs"
    return $doc.SelectSingleNode($xpath)
}

function Get-ElementText([System.Xml.XmlElement]$elem) {
    return $elem.InnerXml
}

function Set-ElementContent([System.Xml.XmlElement]$elem, [string]$content) {
    if ([string]::IsNullOrWhiteSpace($content)) {
        $elem.InnerXml = ""
        return
    }
    # InnerXml handles mixed content: plain text + <see cref/> + <paramref/> + CDATA
    try {
        $elem.InnerXml = $content
    }
    catch {
        # If XML parsing fails, set as plain text
        $elem.InnerText = $content
    }
}

function Count-Signatures([xml]$doc) {
    $ms = ($doc.SelectNodes("//MemberSignature") | Measure-Object).Count
    $ts = ($doc.SelectNodes("//TypeSignature") | Measure-Object).Count
    return $ms + $ts
}

# ---------------------------------------------------------------------------
# Extract
# ---------------------------------------------------------------------------

function Extract-Docs([string]$inputPath, [string]$outputDir) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null

    $xmlFiles = if (Test-Path $inputPath -PathType Leaf) {
        @(Get-Item $inputPath)
    }
    else {
        Get-ChildItem -Path $inputPath -Filter "*.xml" -Recurse | Sort-Object FullName
    }

    $totalEntries = 0
    $totalFiles = 0

    foreach ($xmlFile in $xmlFiles) {
        try {
            $doc = Load-XmlPreserving $xmlFile.FullName
        }
        catch {
            Write-Warning "Skipping $($xmlFile.Name): $_"
            continue
        }

        $root = $doc.DocumentElement
        $typeName = if ($root.GetAttribute("FullName")) { $root.GetAttribute("FullName") } else { $root.GetAttribute("Name") }
        $entries = @()

        # Type-level docs
        $typeDocs = $root.SelectSingleNode("Docs")
        if ($typeDocs) {
            $fields = Extract-DocsBlock $typeDocs
            if ($fields.Count -gt 0) {
                # Pre-fill remarks template for types — agent completes the blanks
                if ($fields.ContainsKey("remarks")) {
                    $fields["remarks"] = "<format type=`"text/markdown`"><![CDATA[`n## Remarks`n`n[Describe what this type does and when to use it]`n`n[If IDisposable: mention using statement / disposal]`n`n## Examples`n`n``````csharp`n[Show the most common usage pattern, 5-15 lines]`n```````n]]></format>"
                    $fields["remarksRequired"] = $true
                }
                $entries += @{
                    docId      = $null
                    memberType = "type"
                    fields     = $fields
                }
            }
        }

        # Member-level docs
        foreach ($member in $root.SelectNodes("Members/Member")) {
            $docId = ($member.SelectSingleNode("MemberSignature[@Language='DocId']"))?.GetAttribute("Value")
            $csSig = ($member.SelectSingleNode("MemberSignature[@Language='C#']"))?.GetAttribute("Value")
            $memberType = $member.SelectSingleNode("MemberType")?.InnerText
            $memberName = $member.GetAttribute("MemberName")
            $docs = $member.SelectSingleNode("Docs")

            if (-not $docs) { continue }

            $fields = Extract-DocsBlock $docs
            if ($fields.Count -gt 0) {
                $entries += @{
                    docId      = $docId
                    memberType = $memberType
                    memberName = $memberName
                    signature  = $csSig
                    fields     = $fields
                }
            }
        }

        if ($entries.Count -eq 0) { continue }

        $basePath = if (Test-Path $inputPath -PathType Container) {
            $resolvedInput = (Resolve-Path $inputPath).Path.TrimEnd([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar)
            $xmlFile.FullName.Substring($resolvedInput.Length + 1)
        }
        else { $xmlFile.Name }
        $jsonName = $basePath -replace '[/\\]', '__' -replace '\.xml$', '.json'
        $jsonPath = Join-Path $outputDir $jsonName

        $result = @{
            file     = $xmlFile.FullName
            typeName = $typeName
            entries  = $entries
        }

        $result | ConvertTo-Json -Depth 10 | Set-Content -Path $jsonPath -Encoding UTF8
        $totalEntries += $entries.Count
        $totalFiles++
        Write-Host "  $($xmlFile.Name): $($entries.Count) entries"
    }

    Write-Host "`nExtracted $totalEntries entries from $totalFiles files to $outputDir/"
}

function Extract-DocsBlock([System.Xml.XmlElement]$docs) {
    $fields = @{}

    foreach ($child in $docs.ChildNodes) {
        if ($child.NodeType -ne "Element") { continue }
        $text = Get-ElementText $child

        switch ($child.LocalName) {
            "param" {
                if ($text -match "To be added") {
                    if (-not $fields.ContainsKey("params")) { $fields["params"] = @{} }
                    $fields["params"][$child.GetAttribute("name")] = $text
                }
            }
            "typeparam" {
                if ($text -match "To be added") {
                    if (-not $fields.ContainsKey("typeparams")) { $fields["typeparams"] = @{} }
                    $fields["typeparams"][$child.GetAttribute("name")] = $text
                }
            }
            { $_ -in "summary", "returns", "value", "remarks" } {
                if ($text -match "To be added") {
                    $fields[$child.LocalName] = $text
                }
            }
        }
    }
    return $fields
}

# ---------------------------------------------------------------------------
# Merge
# ---------------------------------------------------------------------------

function Merge-Docs([string]$inputPath) {
    $jsonFiles = if (Test-Path $inputPath -PathType Leaf) {
        @(Get-Item $inputPath)
    }
    else {
        Get-ChildItem -Path $inputPath -Filter "*.json" | Sort-Object Name
    }

    $totalUpdates = 0
    $totalFiles = 0

    foreach ($jsonFile in $jsonFiles) {
        $data = Get-Content -Raw $jsonFile.FullName | ConvertFrom-Json
        $xmlPath = $data.file

        if (-not (Test-Path $xmlPath)) {
            Write-Warning "XML file not found: $xmlPath"
            continue
        }

        $doc = Load-XmlPreserving $xmlPath
        $sigsBefore = Count-Signatures $doc
        $updates = 0

        foreach ($entry in $data.entries) {
            $docId = $entry.docId
            $memberType = $entry.memberType

            # Find target Docs block
            $docs = if ($memberType -eq "type" -or -not $docId) {
                $doc.DocumentElement.SelectSingleNode("Docs")
            }
            else {
                Find-DocsByDocId $doc $docId
            }

            if (-not $docs) {
                if ($docId) { Write-Warning "DocId not found: $docId" }
                continue
            }

            $fields = $entry.fields

            # Update scalar fields
            foreach ($fieldName in @("summary", "returns", "value", "remarks")) {
                $content = $fields.$fieldName
                if ($null -ne $content -and $content -notmatch "To be added") {
                    $elem = $docs.SelectSingleNode($fieldName)
                    if ($elem) {
                        if ($DryRun) {
                            Write-Host "  Would update $($docId ?? 'type').$fieldName"
                        }
                        else {
                            Set-ElementContent $elem $content
                        }
                        $updates++
                    }
                }
            }

            # Update params
            if ($fields.params) {
                $paramMap = if ($fields.params -is [hashtable]) { $fields.params } else {
                    $h = @{}; $fields.params.PSObject.Properties | ForEach-Object { $h[$_.Name] = $_.Value }; $h
                }
                foreach ($kv in $paramMap.GetEnumerator()) {
                    if ($null -ne $kv.Value -and $kv.Value -notmatch "To be added") {
                        $elem = $docs.SelectSingleNode("param[@name='$($kv.Key)']")
                        if ($elem) {
                            if (-not $DryRun) { Set-ElementContent $elem $kv.Value }
                            $updates++
                        }
                    }
                }
            }

            # Update typeparams
            if ($fields.typeparams) {
                $tpMap = if ($fields.typeparams -is [hashtable]) { $fields.typeparams } else {
                    $h = @{}; $fields.typeparams.PSObject.Properties | ForEach-Object { $h[$_.Name] = $_.Value }; $h
                }
                foreach ($kv in $tpMap.GetEnumerator()) {
                    if ($null -ne $kv.Value -and $kv.Value -notmatch "To be added") {
                        $elem = $docs.SelectSingleNode("typeparam[@name='$($kv.Key)']")
                        if ($elem) {
                            if (-not $DryRun) { Set-ElementContent $elem $kv.Value }
                            $updates++
                        }
                    }
                }
            }
        }

        if ($updates -eq 0) { continue }

        # Safety assertion
        $sigsAfter = Count-Signatures $doc
        if ($sigsAfter -ne $sigsBefore) {
            Write-Error "FATAL: Signature count changed in $xmlPath ($sigsBefore -> $sigsAfter)"
            exit 2
        }

        if (-not $DryRun) {
            $doc.Save($xmlPath)
            # Validate
            try {
                $validate = [xml]::new()
                $validate.Load($xmlPath)
            }
            catch {
                Write-Error "Malformed output: $xmlPath : $_"
            }
        }

        $totalFiles++
        $totalUpdates += $updates
        $action = if ($DryRun) { "Would update" } else { "Updated" }
        Write-Host "  $($jsonFile.Name): $updates fields"
    }

    $action = if ($DryRun) { "Would update" } else { "Merged" }
    Write-Host "`n$action $totalUpdates fields across $totalFiles files"
}

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

switch ($Command) {
    "extract" {
        if (-not $Output) { Write-Error "-Output is required for extract"; exit 1 }
        Extract-Docs $Path $Output
    }
    "merge" {
        Merge-Docs $Path
    }
    default {
        Write-Host "Usage: docs-tool.ps1 <extract|merge> <path> [-Output dir] [-DryRun]"
        exit 1
    }
}
