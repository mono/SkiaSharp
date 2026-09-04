[CmdletBinding(DefaultParameterSetName = 'Finish')]
param(
    [Parameter(Mandatory, ParameterSetName = 'Start')]
    [switch] $Start,

    [Parameter(Mandatory, ParameterSetName = 'Finish')]
    [switch] $Finish,

    [Parameter(ParameterSetName = 'Start')]
    [Parameter(ParameterSetName = 'Finish')]
    [string[]] $TraceDirectory,

    [Parameter(ParameterSetName = 'Finish')]
    [string] $RepositoryRoot,

    [Parameter(ParameterSetName = 'Finish')]
    [string] $ReportDirectory,

    [Parameter(ParameterSetName = 'Finish')]
    [switch] $RequireTrace,

    [Parameter(ParameterSetName = 'Finish')]
    [switch] $KeepTrace
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function ConvertTo-AzurePipelinesValue {
    param([Parameter(Mandatory)][string] $Value)

    return $Value.
        Replace('%', '%AZP25').
        Replace("`r", '%0D').
        Replace("`n", '%0A').
        Replace(']', '%5D')
}

function Get-DefaultTraceDirectory {
    $tempDirectory = $env:AGENT_TEMPDIRECTORY
    if ([string]::IsNullOrWhiteSpace($tempDirectory)) {
        $tempDirectory = [IO.Path]::GetTempPath()
    }

    $jobId = if ([string]::IsNullOrWhiteSpace($env:SYSTEM_JOBID)) {
        [Guid]::NewGuid().ToString('N')
    } else {
        $env:SYSTEM_JOBID
    }
    $attempt = if ([string]::IsNullOrWhiteSpace($env:SYSTEM_JOBATTEMPT)) {
        '1'
    } else {
        $env:SYSTEM_JOBATTEMPT
    }

    return Join-Path $tempDirectory "skiasharp-source-dependencies-$jobId-$attempt"
}

if ($Start) {
    $tracePath = if ($TraceDirectory.Count -gt 0) {
        $TraceDirectory[0]
    } else {
        Get-DefaultTraceDirectory
    }
    $tracePath = [IO.Path]::GetFullPath($tracePath)
    New-Item $tracePath -ItemType Directory -Force | Out-Null

    $escapedPath = ConvertTo-AzurePipelinesValue $tracePath
    Write-Host "##vso[task.setvariable variable=SOURCE_DEPENDENCY_TRACE_DIRECTORY]$escapedPath"
    Write-Host "##vso[task.setvariable variable=GIT_TRACE2_EVENT]$escapedPath"
    Write-Host '##vso[task.setvariable variable=GIT_TRACE_REDACT]1'
    Write-Host "Git source dependency tracing enabled at '$tracePath'."
    return
}

function Get-EnvironmentValue {
    param([Parameter(Mandatory)][string] $Name)

    $value = [Environment]::GetEnvironmentVariable($Name)
    if ($null -eq $value) {
        return ''
    }
    return $value
}

function Get-SourceUrlCandidates {
    param([Parameter(Mandatory)][AllowEmptyString()][string] $Text)

    $pattern = '(?i)(?:https?|ssh|git)://[^\s''"`<>]+|[A-Za-z0-9._-]+@[A-Za-z0-9.-]+:[^\s''"`<>]+'
    foreach ($match in [regex]::Matches($Text, $pattern)) {
        $match.Value.TrimEnd('.', ',', ';', ':', ')', ']', '}')
    }
}

function ConvertTo-CanonicalSourceUrl {
    param(
        [Parameter(Mandatory)][string] $Candidate,
        [switch] $FromGit
    )

    $value = $Candidate.Trim().Trim("'`"")
    if ($value -match '^(?<user>[A-Za-z0-9._-]+)@(?<host>[A-Za-z0-9.-]+):(?<path>.+)$') {
        $value = "ssh://$($Matches.user)@$($Matches.host)/$($Matches.path)"
    }

    if ($value -notmatch '^(?<scheme>https?|ssh|git)://(?<remainder>.+)$') {
        return $null
    }

    $scheme = $Matches.scheme.ToLowerInvariant()
    $remainder = $Matches.remainder
    $slash = $remainder.IndexOf('/')
    if ($slash -lt 0) {
        return $null
    }

    $authority = $remainder.Substring(0, $slash)
    $path = ($remainder.Substring($slash) -split '[?#]', 2)[0].TrimEnd('/')
    if ($authority.Contains('@')) {
        $authority = $authority.Substring($authority.LastIndexOf('@') + 1)
    }
    $sourceHost = $authority.Split(':')[0].ToLowerInvariant()
    if ([string]::IsNullOrWhiteSpace($sourceHost) -or [string]::IsNullOrWhiteSpace($path)) {
        return $null
    }

    $sourceHosts = @(
        'bitbucket.org',
        'codeload.github.com',
        'github.com',
        'gitlab.com',
        'raw.githubusercontent.com',
        'ssh.dev.azure.com'
    )
    $isSourceHost =
        $sourceHosts -contains $sourceHost -or
        $sourceHost.EndsWith('.googlesource.com', [StringComparison]::OrdinalIgnoreCase) -or
        $sourceHost.EndsWith('.visualstudio.com', [StringComparison]::OrdinalIgnoreCase) -or
        $sourceHost -eq 'dev.azure.com'
    if (-not $FromGit -and -not $isSourceHost) {
        return $null
    }

    $segments = @($path.Trim('/').Split('/', [StringSplitOptions]::RemoveEmptyEntries))
    if ($sourceHost -in @('github.com', 'raw.githubusercontent.com', 'codeload.github.com')) {
        if ($segments.Count -lt 2) {
            return $null
        }
        $owner = $segments[0]
        $repository = $segments[1].Split('@')[0] -replace '\.git$', ''
        if ($owner -match '[$\{\}]' -or $repository -match '[$\{\}]') {
            return $null
        }
        return "https://github.com/$owner/$repository"
    }

    if ($sourceHost -eq 'bitbucket.org') {
        if ($segments.Count -lt 2) {
            return $null
        }
        $repository = $segments[1].Split('@')[0] -replace '\.git$', ''
        return "https://bitbucket.org/$($segments[0])/$repository"
    }

    if ($sourceHost -eq 'gitlab.com') {
        $marker = [Array]::IndexOf($segments, '-')
        if ($marker -gt 0) {
            $segments = @($segments[0..($marker - 1)])
        }
        if ($segments.Count -lt 2) {
            return $null
        }
        $segments[$segments.Count - 1] =
            $segments[$segments.Count - 1].Split('@')[0] -replace '\.git$', ''
        return "https://gitlab.com/$($segments -join '/')"
    }

    if ($sourceHost -eq 'dev.azure.com' -and
        $path -match '^/(?<org>[^/]+)/(?<project>[^/]+)/_git/(?<repo>[^/]+)') {
        $repo = $Matches.repo.Split('@')[0] -replace '\.git$', ''
        return "https://dev.azure.com/$($Matches.org)/$($Matches.project)/_git/$repo"
    }
    if ($sourceHost -eq 'dev.azure.com') {
        return $null
    }

    if ($sourceHost -eq 'ssh.dev.azure.com' -and
        $path -match '^/v3/(?<org>[^/]+)/(?<project>[^/]+)/(?<repo>[^/]+)') {
        $repo = $Matches.repo.Split('@')[0] -replace '\.git$', ''
        return "https://dev.azure.com/$($Matches.org)/$($Matches.project)/_git/$repo"
    }

    if ($sourceHost.EndsWith('.googlesource.com', [StringComparison]::OrdinalIgnoreCase)) {
        $path = $path.Split('@')[0]
        $plus = $path.IndexOf('/+')
        if ($plus -ge 0) {
            $path = $path.Substring(0, $plus)
        }
        $path = $path -replace '\.git$', ''
        return "https://$sourceHost$path"
    }

    $path = $path.Split('@')[0] -replace '\.git$', ''
    return "${scheme}://$sourceHost$path"
}

function Get-SourceHost {
    param([Parameter(Mandatory)][string] $Url)

    if ($Url -match '^[a-z]+://(?<host>[^/]+)') {
        return $Matches.host
    }
    return ''
}

$records = @{}

function Add-SourceRecord {
    param(
        [Parameter(Mandatory)][string] $Url,
        [Parameter(Mandatory)][ValidateSet('observed', 'declared')][string] $Detection,
        [Parameter(Mandatory)][string] $Kind,
        [string] $Operation,
        [string] $Path,
        [int] $Line,
        [string] $Revision
    )

    if (-not $records.ContainsKey($Url)) {
        $records[$Url] = @{
            Url = $Url
            Host = Get-SourceHost $Url
            Observed = $false
            Declared = $false
            Operations = [Collections.Generic.HashSet[string]]::new(
                [StringComparer]::OrdinalIgnoreCase)
            Revisions = [Collections.Generic.HashSet[string]]::new(
                [StringComparer]::OrdinalIgnoreCase)
            Evidence = @{}
        }
    }

    $record = $records[$Url]
    if ($Detection -eq 'observed') {
        $record.Observed = $true
    } else {
        $record.Declared = $true
    }
    if (-not [string]::IsNullOrWhiteSpace($Operation)) {
        $record.Operations.Add($Operation) | Out-Null
    }
    if (-not [string]::IsNullOrWhiteSpace($Revision)) {
        $record.Revisions.Add($Revision) | Out-Null
    }

    $evidenceKey = "$Kind|$Operation|$Path|$Line"
    if (-not $record.Evidence.ContainsKey($evidenceKey)) {
        $evidence = [ordered]@{ kind = $Kind }
        if (-not [string]::IsNullOrWhiteSpace($Operation)) {
            $evidence.operation = $Operation
        }
        if (-not [string]::IsNullOrWhiteSpace($Path)) {
            $evidence.path = $Path
        }
        if ($Line -gt 0) {
            $evidence.line = $Line
        }
        $record.Evidence[$evidenceKey] = $evidence
    }
}

function Get-GitOperation {
    param([Parameter(Mandatory)][string] $Command)

    foreach ($operation in @('clone', 'fetch', 'pull', 'submodule', 'ls-remote')) {
        if ($Command -match "(?i)(?:^|\s)$([regex]::Escape($operation))(?:\s|$)") {
            return $operation
        }
    }
    return 'transport'
}

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $RepositoryRoot = Get-EnvironmentValue 'BUILD_SOURCESDIRECTORY'
}
if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $RepositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
} else {
    $RepositoryRoot = [IO.Path]::GetFullPath($RepositoryRoot)
}

if ([string]::IsNullOrWhiteSpace($ReportDirectory)) {
    $ReportDirectory = Join-Path $RepositoryRoot 'output/source-dependencies'
}
$ReportDirectory = [IO.Path]::GetFullPath($ReportDirectory)

$traceDirectories = [Collections.Generic.List[string]]::new()
foreach ($directory in @($TraceDirectory)) {
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        $traceDirectories.Add([IO.Path]::GetFullPath($directory))
    }
}
if ($traceDirectories.Count -eq 0) {
    $environmentTrace = Get-EnvironmentValue 'SOURCE_DEPENDENCY_TRACE_DIRECTORY'
    if (-not [string]::IsNullOrWhiteSpace($environmentTrace)) {
        $traceDirectories.Add([IO.Path]::GetFullPath($environmentTrace))
    }
}
$containerTrace = Join-Path $RepositoryRoot '.source-dependency-trace'
if (Test-Path $containerTrace -PathType Container) {
    $traceDirectories.Add([IO.Path]::GetFullPath($containerTrace))
}

$traceFileCount = 0
$traceEventCount = 0
$malformedTraceLineCount = 0
$sessionOperations = @{}
$traceWorktrees = [Collections.Generic.HashSet[string]]::new(
    [StringComparer]::OrdinalIgnoreCase)

foreach ($directory in @($traceDirectories | Select-Object -Unique)) {
    if (-not (Test-Path $directory -PathType Container)) {
        continue
    }

    foreach ($file in Get-ChildItem $directory -File -Recurse -Force -ErrorAction SilentlyContinue) {
        $traceFileCount++
        foreach ($line in [IO.File]::ReadLines($file.FullName)) {
            if ([string]::IsNullOrWhiteSpace($line)) {
                continue
            }
            try {
                $event = $line | ConvertFrom-Json -ErrorAction Stop
            } catch {
                $malformedTraceLineCount++
                continue
            }
            $traceEventCount++

            if ($event.event -eq 'def_repo' -and
                $event.PSObject.Properties.Name -contains 'worktree' -and
                -not [string]::IsNullOrWhiteSpace($event.worktree)) {
                $traceWorktrees.Add([IO.Path]::GetFullPath($event.worktree)) | Out-Null
                continue
            }

            if ($event.event -notin @('start', 'child_start', 'exec')) {
                continue
            }

            $arguments = @($event.argv)
            if ($arguments.Count -eq 0) {
                continue
            }
            $command = $arguments -join ' '
            $operation = Get-GitOperation $command
            if ($event.PSObject.Properties.Name -contains 'sid' -and
                -not [string]::IsNullOrWhiteSpace($event.sid)) {
                if ($event.event -eq 'start') {
                    $sessionOperations[$event.sid] = $operation
                } elseif ($sessionOperations.ContainsKey($event.sid)) {
                    $operation = $sessionOperations[$event.sid]
                }
            }

            foreach ($candidate in Get-SourceUrlCandidates $command) {
                $url = ConvertTo-CanonicalSourceUrl $candidate -FromGit
                if ($null -ne $url) {
                    Add-SourceRecord $url observed 'git-trace' $operation
                }
            }
        }
    }
}

foreach ($variable in @('BUILD_REPOSITORY_URI', 'SYSTEM_PULLREQUEST_SOURCEREPOSITORYURI')) {
    $candidate = Get-EnvironmentValue $variable
    if (-not [string]::IsNullOrWhiteSpace($candidate)) {
        $url = ConvertTo-CanonicalSourceUrl $candidate -FromGit
        if ($null -ne $url) {
            Add-SourceRecord $url observed 'pipeline' 'checkout'
        }
    }
}

function Invoke-Git {
    param(
        [Parameter(Mandatory)][string] $WorkingDirectory,
        [Parameter(Mandatory)][string[]] $Arguments
    )

    $output = @(& git -C $WorkingDirectory @Arguments 2>$null)
    if ($LASTEXITCODE -ne 0) {
        return @()
    }
    return $output
}

$worktreeSet = [Collections.Generic.HashSet[string]]::new(
    [StringComparer]::OrdinalIgnoreCase)
$worktreeSet.Add($RepositoryRoot) | Out-Null
foreach ($worktree in $traceWorktrees) {
    if ($worktree.StartsWith($RepositoryRoot, [StringComparison]::OrdinalIgnoreCase) -and
        (Test-Path (Join-Path $worktree '.git'))) {
        $worktreeSet.Add($worktree) | Out-Null
    }
}
foreach ($line in Invoke-Git $RepositoryRoot @('submodule', 'status', '--recursive')) {
    if ($line -match '^.?[0-9a-f]{40}\s+(?<path>.+?)(?:\s+\(.+\))?$') {
        $worktree = [IO.Path]::GetFullPath((Join-Path $RepositoryRoot $Matches.path))
        if (Test-Path (Join-Path $worktree '.git')) {
            $worktreeSet.Add($worktree) | Out-Null
        }
    }
}

$worktrees = @($worktreeSet | Sort-Object)
foreach ($worktree in $worktrees) {
    $relativePath = [IO.Path]::GetRelativePath($RepositoryRoot, $worktree).
        Replace('\', '/')
    if ($relativePath -eq '.') {
        $relativePath = '.'
    }
    $revision = (Invoke-Git $worktree @('rev-parse', 'HEAD') | Select-Object -First 1)
    foreach ($remote in Invoke-Git $worktree @('config', '--get', 'remote.origin.url')) {
        $url = ConvertTo-CanonicalSourceUrl $remote -FromGit
        if ($null -ne $url) {
            Add-SourceRecord $url observed 'workspace' 'present' $relativePath 0 $revision
        }
    }
}

function Add-DeclarationsFromFile {
    param(
        [Parameter(Mandatory)][string] $File,
        [Parameter(Mandatory)][string] $DisplayPath
    )

    $lineNumber = 0
    foreach ($line in [IO.File]::ReadLines($File)) {
        $lineNumber++
        foreach ($candidate in Get-SourceUrlCandidates $line) {
            $url = ConvertTo-CanonicalSourceUrl $candidate
            if ($null -ne $url) {
                Add-SourceRecord $url declared 'declaration' '' $DisplayPath $lineNumber
            }
        }
    }
}

$declarationFiles = @{}
foreach ($worktree in $worktrees) {
    foreach ($name in @('.gitmodules', 'DEPS')) {
        $path = Join-Path $worktree $name
        if (Test-Path $path -PathType Leaf) {
            $displayPath = [IO.Path]::GetRelativePath($RepositoryRoot, $path).
                Replace('\', '/')
            $declarationFiles[$path] = $displayPath
        }
    }
}

foreach ($trackedFile in Invoke-Git $RepositoryRoot @('ls-files')) {
    $normalized = $trackedFile.Replace('\', '/')
    $extension = [IO.Path]::GetExtension($normalized)
    $isBuildFile =
        $normalized -eq 'build.cake' -or
        $normalized.StartsWith('native/', [StringComparison]::Ordinal) -or
        $normalized.StartsWith('scripts/', [StringComparison]::Ordinal)
    $isTestFixture =
        $normalized.Contains('/tests/', [StringComparison]::OrdinalIgnoreCase) -or
        [IO.Path]::GetFileName($normalized).Contains('.Tests.', [StringComparison]::OrdinalIgnoreCase)
    $isScannable =
        $extension -in @('.cake', '.ps1', '.py', '.sh', '.yaml', '.yml') -or
        [IO.Path]::GetFileName($normalized) -eq 'Dockerfile'
    if ($isBuildFile -and $isScannable -and -not $isTestFixture) {
        $path = Join-Path $RepositoryRoot $trackedFile
        if (Test-Path $path -PathType Leaf) {
            $declarationFiles[$path] = $normalized
        }
    }
}

foreach ($entry in $declarationFiles.GetEnumerator()) {
    Add-DeclarationsFromFile $entry.Key $entry.Value
}

$componentManifestPath = Join-Path $RepositoryRoot 'cgmanifest.json'
if (Test-Path $componentManifestPath -PathType Leaf) {
    $componentManifest = Get-Content $componentManifestPath -Raw | ConvertFrom-Json
    foreach ($registration in @($componentManifest.registrations)) {
        $identity = $null
        $revision = $null
        if ($registration.PSObject.Properties.Name -contains 'skia_dependency') {
            $identity = $registration.skia_dependency.version_reviewed_identity
            $revision = $registration.skia_dependency.revision
        }
        if (-not [string]::IsNullOrWhiteSpace($identity)) {
            $url = ConvertTo-CanonicalSourceUrl $identity
            if ($null -ne $url) {
                Add-SourceRecord `
                    $url `
                    declared `
                    'component-manifest' `
                    '' `
                    'cgmanifest.json' `
                    0 `
                    $revision
            }
        }

        if ($registration.component.type -eq 'git') {
            $url = ConvertTo-CanonicalSourceUrl $registration.component.git.repositoryUrl
            if ($null -ne $url) {
                Add-SourceRecord `
                    $url `
                    declared `
                    'component-manifest' `
                    '' `
                    'cgmanifest.json' `
                    0 `
                    $registration.component.git.commitHash
            }
        }
    }
}

if ($RequireTrace -and $traceFileCount -eq 0) {
    throw 'Git Trace2 did not produce any files. The source dependency report would be incomplete.'
}

$repositoryOutput = @(
    foreach ($record in @($records.Values | Sort-Object Url)) {
        [ordered]@{
            url = $record.Url
            host = $record.Host
            status = if ($record.Observed) {
                if ($record.Declared) { 'observed-and-declared' } else { 'observed' }
            } else {
                'declared'
            }
            observed = $record.Observed
            declared = $record.Declared
            operations = @($record.Operations | Sort-Object)
            revisions = @($record.Revisions | Sort-Object)
            evidence = @($record.Evidence.Values | Sort-Object kind, path, line, operation)
        }
    }
)

$observedCount = @($repositoryOutput | Where-Object observed).Count
$declaredOnlyCount = @($repositoryOutput | Where-Object { -not $_.observed }).Count
$report = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = [DateTime]::UtcNow.ToString('o')
    scope = 'Git repositories and repository-backed source downloads used or declared by this job'
    build = [ordered]@{
        buildId = Get-EnvironmentValue 'BUILD_BUILDID'
        buildNumber = Get-EnvironmentValue 'BUILD_BUILDNUMBER'
        jobName = Get-EnvironmentValue 'SYSTEM_JOBNAME'
        jobAttempt = Get-EnvironmentValue 'SYSTEM_JOBATTEMPT'
        repository = Get-EnvironmentValue 'BUILD_REPOSITORY_URI'
        commit = Get-EnvironmentValue 'BUILD_SOURCEVERSION'
    }
    coverage = [ordered]@{
        gitTrace2 = [ordered]@{
            traceDirectories = $traceDirectories.Count
            traceFiles = $traceFileCount
            events = $traceEventCount
            malformedLines = $malformedTraceLineCount
        }
        workspaceRepositories = $worktrees.Count
        declarationFiles = $declarationFiles.Count
        observedRepositories = $observedCount
        declaredOnlyRepositories = $declaredOnlyCount
        methods = @(
            'Git Trace2 process events, including indirect Git calls from gclient and build scripts'
            'Git remotes and exact revisions from repositories remaining in the workspace'
            'Repository URLs declared in .gitmodules, DEPS, Cake, scripts, Dockerfiles, and pipeline YAML'
            'Pinned source identities from cgmanifest.json, excluding canonical upstream CVE aliases'
        )
        limitations = @(
            'Package feeds, SDK/tool installers, and arbitrary non-repository HTTP downloads are outside this source-repository report.'
            'A declared-only repository may belong to a build path that this job did not execute.'
            'A source URL assembled entirely at runtime is reported when Git observes it; it cannot be recovered statically from an unexecuted path.'
        )
    }
    repositories = $repositoryOutput
}

New-Item $ReportDirectory -ItemType Directory -Force | Out-Null
$jsonPath = Join-Path $ReportDirectory 'source-dependencies.json'
$markdownPath = Join-Path $ReportDirectory 'source-dependencies.md'
$report | ConvertTo-Json -Depth 8 | Set-Content $jsonPath -Encoding utf8NoBOM

$markdown = [Collections.Generic.List[string]]::new()
$markdown.Add('# Source dependency report')
$markdown.Add('')
$markdown.Add("Job: ``$($report.build.jobName)``; build: ``$($report.build.buildNumber)``; commit: ``$($report.build.commit)``")
$markdown.Add('')
$markdown.Add(
    "Found **$observedCount observed** source repositories and **$declaredOnlyCount declared-only** repositories.")
$markdown.Add('')
$markdown.Add('| Repository | Detection | Revision | Evidence |')
$markdown.Add('|---|---|---|---|')
foreach ($repository in $repositoryOutput) {
    $revision = if ($repository.revisions.Count -eq 0) {
        ''
    } else {
        (@($repository.revisions | ForEach-Object {
            if ($_.Length -gt 12) { $_.Substring(0, 12) } else { $_ }
        }) -join ', ')
    }
    $evidence = @(
        $repository.evidence | Select-Object -First 6 | ForEach-Object {
            if ($_.kind -eq 'declaration') {
                "$($_.path):$($_.line)"
            } elseif ($_.kind -eq 'workspace') {
                "workspace:$($_.path)"
            } elseif ($_.PSObject.Properties.Name -contains 'operation') {
                "$($_.kind):$($_.operation)"
            } else {
                $_.kind
            }
        }
    ) -join ', '
    $markdown.Add(
        "| $($repository.url.Replace('|', '\|')) | $($repository.status) | $revision | $($evidence.Replace('|', '\|')) |")
}
$markdown.Add('')
$markdown.Add('## Coverage and limitations')
$markdown.Add('')
foreach ($method in $report.coverage.methods) {
    $markdown.Add("- $method")
}
$markdown.Add('')
foreach ($limitation in $report.coverage.limitations) {
    $markdown.Add("- $limitation")
}
$markdown | Set-Content $markdownPath -Encoding utf8NoBOM

Write-Host "##[section]Source dependency report: $observedCount observed, $declaredOnlyCount declared-only"
foreach ($repository in $repositoryOutput) {
    Write-Host "[$($repository.status)] $($repository.url)"
}
Write-Host "Reports written to '$ReportDirectory'."

if (-not $KeepTrace) {
    foreach ($directory in @($traceDirectories | Select-Object -Unique)) {
        Remove-Item $directory -Recurse -Force -ErrorAction SilentlyContinue
    }
}
