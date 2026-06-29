#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Deterministic tooling for the SkiaSharp api-docs skill (no LLM).

.DESCRIPTION
    Agents edit the ECMA/mdoc XML directly, so this tool provides the three
    non-LLM gates the skill relies on:

      resolve-scope <all|new|changed|file:PATH>
                                 List the docs to work on (+ candidate C# source
                                 path) for sharding. There is no selector grammar:
                                 `all` lists every type doc (the model picks the
                                 ones matching a natural-language request itself),
                                 `new` lists docs with `To be added.` placeholders,
                                 `changed` lists docs changed vs the git baseline.
      lint <path|all|new|changed|file:PATH>      Objective defect scan -> machine findings.
      validate <path|all|new|changed|file:PATH>  Post-edit structural safety vs the git baseline.

    Uses .NET XmlDocument (CDATA-preserving). pwsh is pre-installed on CI runners.

.EXAMPLE
    pwsh docs-tool.ps1 resolve-scope all
    pwsh docs-tool.ps1 lint file:docs/SkiaSharpAPI/SkiaSharp/SKPaint.xml
    pwsh docs-tool.ps1 validate docs/SkiaSharpAPI/SkiaSharp/SKFont.xml
#>

param(
    [Parameter(Position = 0)]
    [ValidateSet("resolve-scope", "lint", "validate")]
    [string]$Command,

    [Parameter(Position = 1)]
    [string]$Selector
)

$ErrorActionPreference = "Stop"

# ---------------------------------------------------------------------------
# Paths
# ---------------------------------------------------------------------------
$SkillRoot = Split-Path $PSScriptRoot -Parent
$RepoRoot  = (Resolve-Path (Join-Path $PSScriptRoot "../../../..")).Path
$DocsSub   = Join-Path $RepoRoot "docs"
# $DocsGit is the git repo used for baselines/diffs; $DocsRoot is where the .xml
# files live. Both default to the in-repo submodule layout, but can be overridden
# so the tool works when the docs repo is the *primary* checkout (e.g. the gh-aw
# auto-api-docs-writer sandbox clones SkiaSharp as a secondary repo and downloads
# the docs separately). Keeping $RepoRoot pointed at the SkiaSharp clone is what
# lets source lookups (binding/) keep working while docs come from elsewhere.
$DocsGit   = if ($env:DOCS_GIT_ROOT) { (Resolve-Path $env:DOCS_GIT_ROOT).Path } else { $DocsSub }
$DocsRoot  = if ($env:DOCS_DIR) { (Resolve-Path $env:DOCS_DIR).Path } else { Join-Path $DocsSub "SkiaSharpAPI" }
$ObsFile   = Join-Path $SkillRoot "references/obsolete-api-map.md"

$GeneratedNames = @("index.xml", "_filter.xml")
function Test-IsGenerated([string]$path) {
    $name = Split-Path $path -Leaf
    if ($GeneratedNames -contains $name) { return $true }
    if ($name -like "ns-*.xml") { return $true }
    if ($path -match "[\\/]FrameworksIndex[\\/]") { return $true }
    return $false
}

# ---------------------------------------------------------------------------
# Shared helpers
# ---------------------------------------------------------------------------
function Load-XmlPreserving([string]$filePath) {
    $doc = [xml]::new()
    $doc.PreserveWhitespace = $true
    $doc.Load($filePath)
    return $doc
}

function Count-Signatures([xml]$doc) {
    $ms = ($doc.SelectNodes("//MemberSignature") | Measure-Object).Count
    $ts = ($doc.SelectNodes("//TypeSignature") | Measure-Object).Count
    return $ms + $ts
}

function Get-RelToRepo([string]$absPath) {
    $full = (Resolve-Path $absPath -ErrorAction SilentlyContinue)?.Path
    if (-not $full) { $full = $absPath }
    if ($full.StartsWith($RepoRoot)) {
        return $full.Substring($RepoRoot.Length).TrimStart('/', '\') -replace '\\', '/'
    }
    return $full -replace '\\', '/'
}

function Get-RelToDocs([string]$absPath) {
    $full = (Resolve-Path $absPath).Path
    return $full.Substring($DocsGit.Length).TrimStart('/', '\') -replace '\\', '/'
}

# Best-effort C# source path for a docs xml file.
function Get-SourcePath([string]$xmlPath) {
    $type = [IO.Path]::GetFileNameWithoutExtension($xmlPath)
    $ns   = Split-Path (Split-Path $xmlPath -Parent) -Leaf
    $candidates = @(
        (Join-Path $RepoRoot "binding/$ns/$type.cs"),
        (Join-Path $RepoRoot "binding/SkiaSharp/$type.cs"),
        (Join-Path $RepoRoot "binding/HarfBuzzSharp/$type.cs")
    )
    foreach ($c in $candidates) { if (Test-Path $c) { return (Get-RelToRepo $c) } }
    $hit = Get-ChildItem -Path (Join-Path $RepoRoot "binding") -Filter "$type.cs" -Recurse -ErrorAction SilentlyContinue |
        Select-Object -First 1
    if ($hit) { return (Get-RelToRepo $hit.FullName) }
    return "NONE"
}

function Get-AllDocFiles {
    Get-ChildItem -Path $DocsRoot -Filter "*.xml" -Recurse -ErrorAction Stop |
        Where-Object { -not (Test-IsGenerated $_.FullName) } |
        Sort-Object FullName
}

# ---------------------------------------------------------------------------
# Scope core (shared by resolve-scope, lint, validate)
# ---------------------------------------------------------------------------
function Get-ScopeFiles([string]$sel) {
    if (-not $sel) { Write-Error "a mode is required (all|new|changed|file:PATH)"; exit 1 }
    $files = @()

    switch -regex ($sel) {
        '^file:(.+)$' {
            $p = $Matches[1]
            if (-not [IO.Path]::IsPathRooted($p)) { $p = Join-Path $RepoRoot $p }
            if (Test-Path $p) { $files = @((Get-Item $p)) }
            break
        }
        '^all$' {
            $files = Get-AllDocFiles
            break
        }
        '^(new|changed)$' {
            $base = $env:DOCS_REVIEW_BASE
            if (-not $base) { $base = "origin/main" }
            $diff = & git -C $DocsGit diff --name-only --diff-filter=ACM "$base...HEAD" 2>$null
            if (-not $diff) { $diff = & git -C $DocsGit diff --name-only --diff-filter=ACM 2>$null }
            $files = foreach ($d in $diff) {
                $abs = Join-Path $DocsGit $d
                if ($d -match '\.xml$' -and (Test-Path $abs) -and -not (Test-IsGenerated $abs)) { Get-Item $abs }
            }
            break
        }
        default { Write-Error "Unrecognized mode '$sel' (use all|new|changed|file:PATH)"; exit 1 }
    }

    return @($files | Sort-Object FullName -Unique)
}

function Resolve-Scope([string]$sel) {
    $files = Get-ScopeFiles $sel
    foreach ($f in $files) {
        $rel = Get-RelToRepo $f.FullName
        $src = Get-SourcePath $f.FullName
        Write-Host "FILE | $rel | source:$src"
    }
    Write-Host "COUNT | $($files.Count)"
}

# ---------------------------------------------------------------------------
# Obsolete map (parse the fenced ```obsolete-map table)
# ---------------------------------------------------------------------------
function Read-ObsoleteErrorMembers {
    $members = @()
    if (-not (Test-Path $ObsFile)) { return $members }
    $inBlock = $false
    foreach ($line in Get-Content $ObsFile) {
        if ($line -match '^```obsolete-map') { $inBlock = $true; continue }
        if ($inBlock -and $line -match '^```') { break }
        if (-not $inBlock) { continue }
        $cols = $line.Split('|') | ForEach-Object { $_.Trim() }
        if ($cols.Count -lt 4) { continue }
        if ($cols[0] -eq 'Type' -or $cols[0] -eq '') { continue }
        if ($cols[3] -ne 'error') { continue }
        # member token without any (...) signature suffix
        $member = ($cols[1] -replace '\(.*$', '').Trim()
        if ($member) { $members += $member }
    }
    return $members | Sort-Object -Unique
}

# ---------------------------------------------------------------------------
# lint
# ---------------------------------------------------------------------------
$MisspellMap = @{
    'teh' = 'the'; 'recieve' = 'receive'; 'seperate' = 'separate'; 'occured' = 'occurred';
    'paramter' = 'parameter'; 'retreive' = 'retrieve'; 'initalize' = 'initialize';
    'lenght' = 'length'; 'widht' = 'width'; 'colour' = 'color'; 'visable' = 'visible';
    'arguement' = 'argument'; 'depricated' = 'deprecated'; 'existant' = 'existent'
}
$CrefPrefixes = @('T:', 'M:', 'P:', 'F:', 'E:', 'N:', 'Overload:')

function Get-MemberDocId([System.Xml.XmlNode]$member) {
    $n = $member.SelectSingleNode("MemberSignature[@Language='DocId']")
    if ($n) { return $n.GetAttribute("Value") }
    return $null
}

function Emit-Finding([string]$sev, [string]$class, [string]$file, [string]$docId, [string]$msg) {
    if (-not $docId) { $docId = "-" }
    Write-Host "$sev | $class | $file | $docId | $msg"
}

function Get-ProseSegments([System.Xml.XmlNode]$node) {
    # Prose text for natural-language checks (repeated word, spelling).
    # Returns one string per descendant text/CDATA node, with fenced code
    # blocks stripped out of CDATA. Splitting per text node means empty inline
    # elements (e.g. <see cref=".." />) act as boundaries, so the words on
    # either side are never treated as adjacent.
    $segments = @()
    foreach ($t in $node.SelectNodes(".//text()")) {
        $val = $t.Value
        if ([string]::IsNullOrWhiteSpace($val)) { continue }
        if ($t.NodeType -eq 'CDATA') {
            $val = [regex]::Replace($val, '(?s)```.*?```', ' ')
        }
        $segments += $val
    }
    return $segments
}

function Lint-File([string]$xmlPath, [string[]]$obsoleteMembers) {
    $rel = Get-RelToRepo $xmlPath
    $count = 0
    $doc = $null
    try { $doc = Load-XmlPreserving $xmlPath }
    catch { Emit-Finding "CRITICAL" "malformed-xml" $rel "-" "XML will not parse: $($_.Exception.Message)"; return 1 }

    $root = $doc.DocumentElement

    # Walk type-level Docs + each Member's Docs
    $units = @()
    $typeDocs = $root.SelectSingleNode("Docs")
    if ($typeDocs) {
        $tn = if ($root.GetAttribute("FullName")) { "T:" + $root.GetAttribute("FullName") } else { "T:" + $root.GetAttribute("Name") }
        $units += [pscustomobject]@{ docId = $tn; docs = $typeDocs; member = $root; isProp = $false; hasSet = $false }
    }
    foreach ($m in $root.SelectNodes("Members/Member")) {
        $d = $m.SelectSingleNode("Docs")
        if (-not $d) { continue }
        $sig = ($m.SelectSingleNode("MemberSignature[@Language='C#']"))?.GetAttribute("Value")
        $mt  = ($m.SelectSingleNode("MemberType"))?.InnerText
        $isProp = ($mt -eq "Property")
        $hasSet = $isProp -and $sig -match 'set\s*;'
        $units += [pscustomobject]@{ docId = (Get-MemberDocId $m); docs = $d; member = $m; isProp = $isProp; hasSet = $hasSet }
    }
    # MemberGroup carries shared remarks/examples for overload sets (e.g. DrawText)
    foreach ($g in $root.SelectNodes("Members/MemberGroup")) {
        $d = $g.SelectSingleNode("Docs")
        if (-not $d) { continue }
        $gname = $g.GetAttribute("MemberName")
        $tn = if ($root.GetAttribute("FullName")) { $root.GetAttribute("FullName") } else { $root.GetAttribute("Name") }
        $units += [pscustomobject]@{ docId = "G:$tn.$gname"; docs = $d; member = $g; isProp = $false; hasSet = $false }
    }

    foreach ($u in $units) {
        $docs = $u.docs
        $docId = $u.docId

        # Empty summary/value/returns (remarks may be self-closing)
        foreach ($tag in @("summary", "value", "returns")) {
            $node = $docs.SelectSingleNode($tag)
            if ($node -and [string]::IsNullOrWhiteSpace($node.InnerXml)) {
                Emit-Finding "IMPORTANT" "empty-tag" $rel $docId "<$tag> is empty"; $count++
            }
        }

        # Per-field text checks
        foreach ($node in $docs.ChildNodes) {
            if ($node.NodeType -ne 'Element') { continue }
            $inner = $node.InnerXml
            $text  = $node.InnerText
            if ([string]::IsNullOrWhiteSpace($inner)) { continue }

            # Placeholder
            if ($text -match 'To be added\.?' ) { Emit-Finding "IMPORTANT" "placeholder" $rel $docId "<$($node.Name)> still 'To be added.'"; $count++ }
            if ($inner -match '\[Describe ' -or $inner -match '\[Show ') { Emit-Finding "IMPORTANT" "placeholder" $rel $docId "<$($node.Name)> has an unfilled remarks scaffold"; $count++ }

            # Repeated words + spelling run on prose only (code fences stripped;
            # per text node so empty inline elements don't fuse adjacent words)
            $reportedRepeat = $false
            foreach ($seg in (Get-ProseSegments $node)) {
                if (-not $reportedRepeat -and $seg -match '(?i)(?<![-\w])([A-Za-z]{2,})\s+\1\b') {
                    if ($Matches[1].ToLowerInvariant() -notin @('that', 'had')) {
                        Emit-Finding "CRITICAL" "repeated-word" $rel $docId "repeated word '$($Matches[1])' in <$($node.Name)>"; $count++; $reportedRepeat = $true
                    }
                }
                foreach ($bad in $MisspellMap.Keys) {
                    if ($seg -match "(?i)\b$bad\b") { Emit-Finding "CRITICAL" "spelling" $rel $docId "'$bad' -> '$($MisspellMap[$bad])' in <$($node.Name)>"; $count++ }
                }
            }

            # see cref prefix
            foreach ($mm in [regex]::Matches($inner, '<see\s+cref="([^"]+)"')) {
                $target = $mm.Groups[1].Value
                $ok = $false
                foreach ($p in $CrefPrefixes) { if ($target.StartsWith($p)) { $ok = $true; break } }
                if (-not $ok) { Emit-Finding "IMPORTANT" "invalid-cref" $rel $docId "<see cref='$target'> missing DocId prefix (T:/M:/P:/F:)"; $count++ }
            }

            # xref prefix inside CDATA remarks
            foreach ($mm in [regex]::Matches($inner, '<xref:(T:|M:|P:|F:)')) {
                Emit-Finding "IMPORTANT" "bad-xref" $rel $docId "<xref:$($mm.Groups[1].Value)...> uses a DocId prefix; xref takes the bare UID"; $count++
            }
            # destroyed CDATA (escaped xref)
            if ($inner -match '&lt;xref:') { Emit-Finding "CRITICAL" "broken-cdata" $rel $docId "escaped '&lt;xref:' — CDATA was destroyed"; $count++ }

            # Obsolete members in csharp fences
            foreach ($fence in [regex]::Matches($inner, '(?s)```csharp(.*?)```')) {
                $code = $fence.Groups[1].Value
                foreach ($om in $obsoleteMembers) {
                    if ($code -match "\.$([regex]::Escape($om))\b") {
                        Emit-Finding "CRITICAL" "obsolete-in-example" $rel $docId "example uses obsolete member '.$om' (see obsolete-api-map.md)"; $count++
                    }
                }
            }
        }

        # Accessor verb mismatch (properties)
        if ($u.isProp) {
            $summary = $docs.SelectSingleNode("summary")
            if ($summary -and -not [string]::IsNullOrWhiteSpace($summary.InnerText)) {
                $s = $summary.InnerText.TrimStart()
                if ($u.hasSet) {
                    if ($s -match '^(?i)Gets\b' -and $s -notmatch '^(?i)Gets or sets\b') {
                        Emit-Finding "IMPORTANT" "accessor-verb" $rel $docId "settable property summary should be 'Gets or sets'"; $count++
                    }
                }
                else {
                    if ($s -match '^(?i)Gets or sets\b') {
                        Emit-Finding "IMPORTANT" "accessor-verb" $rel $docId "read-only property summary should be 'Gets' (not 'Gets or sets')"; $count++
                    }
                }
            }
        }
    }
    return $count
}

# ---------------------------------------------------------------------------
# validate (structural, vs git baseline)
# ---------------------------------------------------------------------------
function Get-StrippedShape([xml]$doc) {
    # Clone, blank every <Docs> body, return OuterXml of everything-but-Docs.
    $clone = [xml]::new()
    $clone.PreserveWhitespace = $true
    $clone.LoadXml($doc.OuterXml)
    foreach ($d in $clone.SelectNodes("//Docs")) { $d.InnerXml = "" }
    return (($clone.OuterXml -replace "`r`n", "`n").Trim())
}

function Validate-File([string]$xmlPath) {
    $rel = Get-RelToRepo $xmlPath
    $cur = $null
    try { $cur = Load-XmlPreserving $xmlPath }
    catch { Write-Host "VALIDATE | FAIL | $rel | not well-formed: $($_.Exception.Message)"; return $false }

    $docsRel = Get-RelToDocs $xmlPath
    $baseRaw = & git -C $DocsGit show "HEAD:$docsRel" 2>$null
    if ($LASTEXITCODE -ne 0 -or -not $baseRaw) {
        Write-Host "VALIDATE | OK | $rel | new file (well-formed; no baseline)"
        return $true
    }

    $base = [xml]::new(); $base.PreserveWhitespace = $true
    $base.LoadXml(($baseRaw -join "`n"))

    $cb = Count-Signatures $base
    $cc = Count-Signatures $cur
    if ($cb -ne $cc) {
        Write-Host "VALIDATE | FAIL | $rel | signature count changed ($cb -> $cc)"; return $false
    }
    if ((Get-StrippedShape $base) -ne (Get-StrippedShape $cur)) {
        Write-Host "VALIDATE | FAIL | $rel | content changed outside <Docs>"; return $false
    }
    Write-Host "VALIDATE | OK | $rel | docs-only, signatures preserved"
    return $true
}

# ---------------------------------------------------------------------------
# Resolve a path-or-mode argument to a concrete file list (for lint/validate)
# ---------------------------------------------------------------------------
function Expand-Target([string]$arg) {
    if (-not $arg) { Write-Error "a path or mode is required (all|new|changed|file:PATH)"; exit 1 }
    # Inventory mode -> use the shared scope core
    if ($arg -match '^file:' -or $arg -in @('all', 'new', 'changed')) {
        return @(Get-ScopeFiles $arg | ForEach-Object { $_.FullName })
    }
    # Plain path (file or directory)
    $p = $arg
    if (-not [IO.Path]::IsPathRooted($p)) { $p = Join-Path $RepoRoot $p }
    if (Test-Path $p -PathType Leaf) { return @($p) }
    if (Test-Path $p -PathType Container) {
        return @(Get-ChildItem $p -Filter "*.xml" -Recurse |
            Where-Object { -not (Test-IsGenerated $_.FullName) } | ForEach-Object FullName)
    }
    Write-Error "Target not found: $arg"; exit 1
}

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------
switch ($Command) {
    "resolve-scope" {
        Resolve-Scope $Selector
    }
    "lint" {
        $targets = Expand-Target $Selector
        $obs = Read-ObsoleteErrorMembers
        $total = 0; $n = 0
        foreach ($t in $targets) { $total += (Lint-File $t $obs); $n++ }
        Write-Host "LINT-SUMMARY | files:$n | findings:$total"
    }
    "validate" {
        $targets = Expand-Target $Selector
        $fail = 0; $n = 0
        foreach ($t in $targets) { if (-not (Validate-File $t)) { $fail++ }; $n++ }
        Write-Host "VALIDATE-SUMMARY | files:$n | failures:$fail"
        if ($fail -gt 0) { exit 2 }
    }
    default {
        Write-Host "Usage: docs-tool.ps1 <resolve-scope|lint|validate> <path|all|new|changed|file:PATH>"
        Write-Host "  resolve-scope lists docs to work on; the model picks theme matches from 'all' itself."
        exit 1
    }
}
