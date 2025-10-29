param(
    [string]$DocsRoot = "docs/SkiaSharpAPI"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-TypeXmlFiles {
    param([string]$Root)
    Get-ChildItem -LiteralPath $Root -Directory | Where-Object {
        $_.Name -notin @('images','breadcrumb','FrameworksIndex','xml')
    } | ForEach-Object {
        Get-ChildItem -LiteralPath $_.FullName -Filter *.xml -File -ErrorAction SilentlyContinue
    }
}

if (-not (Test-Path -LiteralPath $DocsRoot)) {
    throw "Docs root not found: $DocsRoot"
}

$typesByNs = [ordered]@{}

foreach ($file in Get-TypeXmlFiles -Root $DocsRoot) {
    try {
        [xml]$xml = Get-Content -LiteralPath $file.FullName -Raw
        if ($null -eq $xml.Type) { continue }
        $typeNode = $xml.Type

        # Type DocId and FullName
        $typeDocId = $null
        foreach ($ts in $typeNode.TypeSignature) {
            if ($ts.Language -eq 'DocId') { $typeDocId = $ts.Value; break }
        }
        if (-not $typeDocId) {
            $typeFullName = if ($typeNode.FullName) { $typeNode.FullName } elseif ($typeNode.Name) { $typeNode.Name } else { $null }
            if ($typeFullName) { $typeDocId = "T:$typeFullName" }
        }
        if (-not $typeDocId) { continue }

    # Derive namespace from directory name (docs are organized by namespace folders)
    $ns = Split-Path -Leaf $file.DirectoryName
    # Display name uses DocId-style with nested types shown using '.'
    $typeFullName = ($typeDocId.Substring(2)) -replace '\+', '.'

        if (-not $typesByNs.Contains($ns)) { $typesByNs[$ns] = @{} }
        if (-not $typesByNs[$ns].ContainsKey($typeFullName)) { $typesByNs[$ns][$typeFullName] = [System.Collections.Generic.List[string]]::new() }
        $members = $typesByNs[$ns][$typeFullName]

        # Member DocIds
        $memberNodes = $typeNode.SelectNodes('Members/Member')
        foreach ($m in $memberNodes) {
            $docId = $null
            foreach ($ms in $m.MemberSignature) { if ($ms.Language -eq 'DocId') { $docId = $ms.Value; break } }
            if ($docId) { $members.Add($docId) }
        }
    }
    catch {
        Write-Warning "Failed to parse $($file.FullName): $($_.Exception.Message)"
    }
}

# Print Markdown to stdout
Write-Output "# API Docs Migration Checklist"
Write-Output ""
Write-Output "> Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm zzz')"
Write-Output "> Source: $DocsRoot"
Write-Output ""

$nsNames = $typesByNs.Keys | Sort-Object
foreach ($ns in $nsNames) {
    Write-Output "## Namespace $ns"
    Write-Output ""

    $typeNames = $typesByNs[$ns].Keys | Sort-Object
    foreach ($t in $typeNames) {
        Write-Output "- [ ] $t"
        $memberDocIds = $typesByNs[$ns][$t] | Sort-Object
        foreach ($docId in $memberDocIds) {
            Write-Output "  - [ ] $docId"
        }
    }
    Write-Output ""
}
