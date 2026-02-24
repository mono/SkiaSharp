<#
.SYNOPSIS
    Convert a cached issue JSON file into annotated markdown for AI analysis.
.EXAMPLE
    pwsh scripts/issue-to-markdown.ps1 .data-cache/github/items/2794.json
    Get-Content issue.json -Raw | pwsh scripts/issue-to-markdown.ps1
.NOTES
    Annotations: [OP]/[MEMBER]/[CONTRIBUTOR]/[BOT] tags, time-deltas,
    attachments table, bot comments collapsed, all URLs preserved.
#>
param(
    [Parameter(Position = 0)]
    [string]$Path,

    [Parameter(ValueFromPipeline)]
    [string]$InputJson
)

begin {
    $ErrorActionPreference = 'Stop'
    $pipeLines = [System.Collections.Generic.List[string]]::new()
}

process {
    if ($InputJson) { $pipeLines.Add($InputJson) }
}

end {

# ── Load JSON ─────────────────────────────────────────────────────

if ($pipeLines.Count -gt 0) {
    $data = ($pipeLines -join "`n") | ConvertFrom-Json
}
elseif ($Path) {
    if (-not (Test-Path $Path)) {
        Write-Error "❌ File not found: $Path"
        exit 2
    }
    $data = Get-Content $Path -Raw | ConvertFrom-Json
}
elseif (-not [Console]::IsInputRedirected) {
    Write-Error "Usage: pwsh issue-to-markdown.ps1 <file.json>  or  cat file.json | pwsh issue-to-markdown.ps1"
    exit 2
}
else {
    # stdin piped from external process (e.g., cat file.json | pwsh script.ps1)
    $data = ($input | Out-String) | ConvertFrom-Json
}

# ── Regex patterns ────────────────────────────────────────────────

$rxImgSrc    = [regex]'<img[^>]+src="([^"]+)"'
$rxMdImage   = [regex]'!\[[^\]]*\]\(([^)]+)\)'
$rxZip       = [regex]'(?i)\[([^\]]*\.(?:zip|rar|7z|tar\.gz))\]\(([^)]+)\)'
$rxRepo      = [regex]'https://github\.com/([^/\s]+/[^/\s]+?)(?:/tree/[^\s)]+|(?=[\s)\]]|$))'
$rxIssueUrl  = [regex]'https://github\.com/mono/SkiaSharp/issues/(\d+)'
$rxIssueHash = [regex]'(?<!\w)#([1-9]\d*)\b'

# ── Helper functions ──────────────────────────────────────────────

function Get-AuthorRole($login, $issueAuthor, $authorType, $authorAssociation) {
    if (-not $login) { return '' }
    if ($login.EndsWith('[bot]')) { return '[BOT]' }
    if ($login -eq $issueAuthor) { return '[OP]' }
    if ($authorAssociation -in 'MEMBER', 'OWNER') { return '[MEMBER]' }
    if ($authorType -in 'microsoft', 'member') { return '[MEMBER]' }
    if ($authorAssociation -eq 'CONTRIBUTOR') { return '[CONTRIBUTOR]' }
    return ''
}

function Get-CommentAuthor($comment) {
    if ($comment.user) {
        return @{ Login = $comment.user.login; Assoc = $comment.author_association; Type = $null }
    }
    if ($comment.author -is [PSCustomObject]) {
        return @{ Login = $comment.author.login; Assoc = $null; Type = $comment.author.type }
    }
    if ($comment.author -is [string]) {
        return @{ Login = $comment.author; Assoc = $null; Type = $null }
    }
    return @{ Login = ''; Assoc = $null; Type = $null }
}

function Format-TimeDelta([datetime]$from, [datetime]$to) {
    $days = ($to - $from).Days
    if ($days -lt 0) { $days = 0 }
    switch ($days) {
        { $_ -eq 0 }   { return '+0d' }
        { $_ -lt 30 }  { return "+${days}d" }
        { $_ -lt 365 } { return "+$([math]::Floor($days / 30))mo" }
        default         { return "+$([math]::Floor($days / 365))y" }
    }
}

function Format-Reactions($reactions) {
    $map = [ordered]@{
        '+1' = '👍'; '-1' = '👎'; 'laugh' = '😄'; 'confused' = '😕'
        'heart' = '❤️'; 'hooray' = '🎉'; 'rocket' = '🚀'; 'eyes' = '👀'
    }

    if ($reactions -is [array]) {
        if ($reactions.Count -eq 0) { return @{ Str = ''; Total = 0 } }
        $counts = $reactions | Group-Object content -AsHashTable
        $parts = foreach ($k in $map.Keys) {
            if ($counts[$k]) { "$($map[$k])$($counts[$k].Count)" }
        }
        return @{ Str = $parts -join ' '; Total = $reactions.Count }
    }

    if ($reactions.total_count -and $reactions.total_count -gt 0) {
        $parts = foreach ($k in $map.Keys) {
            $v = $reactions.$k
            if ($v -and $v -gt 0) { "$($map[$k])$v" }
        }
        return @{ Str = $parts -join ' '; Total = $reactions.total_count }
    }

    return @{ Str = ''; Total = 0 }
}

function Get-Links($text) {
    $images = @($rxImgSrc.Matches($text) | ForEach-Object { $_.Groups[1].Value })
    $images += @($rxMdImage.Matches($text) | ForEach-Object { $_.Groups[1].Value })

    $zips = @($rxZip.Matches($text) | ForEach-Object {
        @{ Filename = $_.Groups[1].Value; Url = $_.Groups[2].Value }
    })

    $zipUrls = $zips.Url
    $repos = @($rxRepo.Matches($text) | ForEach-Object {
        $slug = $_.Groups[1].Value
        if ($slug -ne 'mono/SkiaSharp') {
            $url = "https://github.com/$slug"
            if ($url -notin $zipUrls) { $url }
        }
    }) | Select-Object -Unique

    $issues = @($rxIssueUrl.Matches($text) | ForEach-Object { [int]$_.Groups[1].Value })
    $issues += @($rxIssueHash.Matches($text) | ForEach-Object { [int]$_.Groups[1].Value } |
        Where-Object { $_ -gt 0 })
    $issues = @($issues | Select-Object -Unique)

    return @{ Images = $images; Zips = $zips; Repos = $repos; Issues = $issues }
}

function Parse-DateSafe($str) {
    if (-not $str) { return $null }
    [datetime]::Parse($str, [cultureinfo]::InvariantCulture, [System.Globalization.DateTimeStyles]::RoundtripKind)
}

function Clean-Body($text) {
    if (-not $text) { return '' }
    $text -replace '\r\n', "`n" -replace '\r', "`n" -replace '\n{3,}', "`n`n" -split "`n" |
        ForEach-Object { $_.TrimEnd() } | Join-String -Separator "`n" | ForEach-Object { $_.Trim() }
}

# ── Determine format ──────────────────────────────────────────────

$issueAuthor = $data.user?.login ?? $data.author?.login ?? ($data.author -as [string]) ?? 'unknown'
$authorAssoc = $data.author_association

$number  = $data.number ?? 0
$title   = $data.title ?? 'Untitled'
$state   = $data.state?.stringValue ?? ($data.state -as [string]) ?? 'unknown'
$created = Parse-DateSafe ($data.created_at ?? $data.createdAt)
$updated = Parse-DateSafe ($data.updated_at ?? $data.updatedAt)
$closed  = Parse-DateSafe ($data.closed_at ?? $data.closedAt)

$labelNames = @($data.labels | ForEach-Object { $_.name ?? $_ })
$comments   = @($data.engagement?.comments | Where-Object { $_ })

# ── Count author roles ────────────────────────────────────────────

$roleCounts = @{ '[OP]' = 0; '[MEMBER]' = 0; '[CONTRIBUTOR]' = 0; '[BOT]' = 0; '' = 0 }
foreach ($c in $comments) {
    $ca = Get-CommentAuthor $c
    $role = Get-AuthorRole $ca.Login $issueAuthor $ca.Type $ca.Assoc
    if (-not $roleCounts.ContainsKey($role)) { $roleCounts[$role] = 0 }
    $roleCounts[$role]++
}

$totalComments = $data.comments ?? $data.commentCount ?? $comments.Count
$distParts = @(
    if ($roleCounts['[OP]'])          { "$($roleCounts['[OP]']) OP" }
    if ($roleCounts['[MEMBER]'])      { "$($roleCounts['[MEMBER]']) member" }
    if ($roleCounts['[CONTRIBUTOR]']) { "$($roleCounts['[CONTRIBUTOR]']) contributor" }
    if ($roleCounts[''])              { "$($roleCounts['']) community" }
    if ($roleCounts['[BOT]'])         { "$($roleCounts['[BOT]']) bot" }
)
$commentSummary = if ($distParts) { "$totalComments ($($distParts -join ', '))" } else { "$totalComments" }

# Reactions
$reactionsRaw = $data.engagement?.reactions ?? $data.reactions
$rxn = Format-Reactions $reactionsRaw

# ── Build output ──────────────────────────────────────────────────

$out = [System.Text.StringBuilder]::new()
$null = $out.AppendLine("# Issue #${number}: $title")
$null = $out.AppendLine()
$null = $out.AppendLine('| Field | Value |')
$null = $out.AppendLine('|-------|-------|')
$null = $out.AppendLine("| State | $state |")
$null = $out.AppendLine("| Author | $issueAuthor |")
if ($created) { $null = $out.AppendLine("| Created | $($created.ToString('yyyy-MM-dd')) |") }
if ($updated) { $null = $out.AppendLine("| Updated | $($updated.ToString('yyyy-MM-dd')) |") }
if ($closed)  { $null = $out.AppendLine("| Closed | $($closed.ToString('yyyy-MM-dd')) |") }
if ($labelNames) { $null = $out.AppendLine("| Labels | $($labelNames -join ', ') |") }
if ($rxn.Total -gt 0) { $null = $out.AppendLine("| Reactions | $($rxn.Str) ($($rxn.Total) total) |") }
$null = $out.AppendLine("| Comments | $commentSummary |")
if ($data.milestone) {
    $msName = $data.milestone.title ?? "$($data.milestone)"
    $null = $out.AppendLine("| Milestone | $msName |")
}

# ── Collect links ─────────────────────────────────────────────────

$body = $data.body ?? ''
$sources = [System.Collections.Generic.List[object]]::new()
$bodyLinks = Get-Links $body

foreach ($url in $bodyLinks.Images)  { $sources.Add(@{ Type = '📸 screenshot';          Source = 'description [OP]'; Url = $url }) }
foreach ($z in $bodyLinks.Zips)      { $sources.Add(@{ Type = "📎 $($z.Filename)";      Source = 'description [OP]'; Url = $z.Url }) }
foreach ($url in $bodyLinks.Repos)   { $sources.Add(@{ Type = '🔗 repo';                Source = 'description [OP]'; Url = $url }) }
foreach ($num in $bodyLinks.Issues)  { $sources.Add(@{ Type = '🔗 issue';               Source = 'description [OP]'; Url = "#$num" }) }

for ($ci = 0; $ci -lt $comments.Count; $ci++) {
    $c = $comments[$ci]
    $ca = Get-CommentAuthor $c
    $role = Get-AuthorRole $ca.Login $issueAuthor $ca.Type $ca.Assoc
    $sourceLabel = "comment $($ci + 1) $role $($ca.Login)".Trim()

    $cLinks = Get-Links ($c.body ?? '')
    foreach ($url in $cLinks.Images)  { $sources.Add(@{ Type = '📸 screenshot';       Source = $sourceLabel; Url = $url }) }
    foreach ($z in $cLinks.Zips)      { $sources.Add(@{ Type = "📎 $($z.Filename)";   Source = $sourceLabel; Url = $z.Url }) }
    foreach ($url in $cLinks.Repos)   { $sources.Add(@{ Type = '🔗 repo';             Source = $sourceLabel; Url = $url }) }
    foreach ($num in $cLinks.Issues) {
        if ($num -notin $bodyLinks.Issues) {
            $sources.Add(@{ Type = '🔗 issue'; Source = $sourceLabel; Url = "#$num" })
        }
    }
}

if ($sources.Count -gt 0) {
    $null = $out.AppendLine()
    $null = $out.AppendLine('## Attachments & Links')
    $null = $out.AppendLine()
    $null = $out.AppendLine('| Type | Source | URL |')
    $null = $out.AppendLine('|------|--------|-----|')
    foreach ($s in $sources) {
        $null = $out.AppendLine("| $($s.Type) | $($s.Source) | $($s.Url) |")
    }
}

# ── Description ───────────────────────────────────────────────────

$descRole = Get-AuthorRole $issueAuthor $issueAuthor $null $authorAssoc
$dateStr = if ($created) { $created.ToString('yyyy-MM-dd') } else { '?' }
$null = $out.AppendLine()
$null = $out.AppendLine('## Description')
$null = $out.AppendLine()
$null = $out.AppendLine("$descRole $issueAuthor — $dateStr")
$null = $out.AppendLine()
$null = $out.AppendLine((Clean-Body $body))

# ── Comments ──────────────────────────────────────────────────────

if ($comments.Count -gt 0) {
    $null = $out.AppendLine()
    $null = $out.AppendLine('## Comments')

    $prevDate = $created
    for ($ci = 0; $ci -lt $comments.Count; $ci++) {
        $c = $comments[$ci]
        $ca = Get-CommentAuthor $c
        $role = Get-AuthorRole $ca.Login $issueAuthor $ca.Type $ca.Assoc
        $cDate = Parse-DateSafe ($c.created_at ?? $c.createdAt)
        $cDateStr = if ($cDate) { $cDate.ToString('yyyy-MM-dd') } else { '?' }

        $deltaStr = ''
        if ($cDate -and $prevDate) { $deltaStr = " ($(Format-TimeDelta $prevDate $cDate))" }

        $cBody = $c.body ?? ''
        $cRxn = Format-Reactions $c.reactions
        $rxnStr = if ($cRxn.Str) { " $($cRxn.Str)" } else { '' }

        $null = $out.AppendLine()
        if ($role -eq '[BOT]') {
            $firstLine = ($cBody -split "`n")[0]
            if ($firstLine.Length -gt 100) { $firstLine = $firstLine.Substring(0, 100) }
            $null = $out.AppendLine("> 🤖 $($ca.Login): $firstLine...")
        }
        else {
            $null = $out.AppendLine("### Comment $($ci + 1) — $role $($ca.Login) — $cDateStr$deltaStr$rxnStr")
            $null = $out.AppendLine()
            $null = $out.AppendLine((Clean-Body $cBody))
        }

        if ($cDate) { $prevDate = $cDate }
    }
}

[Console]::Write($out.ToString())

} # end
