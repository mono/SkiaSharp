#!/usr/bin/env pwsh

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

$script:TestsRun = 0
$scriptPath = Resolve-Path (Join-Path $PSScriptRoot '../Update-SkiaSharpSkiaCommit.ps1')
$workRoot = Join-Path $PSScriptRoot '.Update-SkiaSharpSkiaCommit.test-work'
$previousGitAllowProtocol = $env:GIT_ALLOW_PROTOCOL
$previousGitExecPath = $env:GIT_EXEC_PATH
$previousFixtureNativeUrl = $env:REPIN_FIXTURE_NATIVE_URL

function Assert-True([bool] $Condition, [string] $Message) {
    $script:TestsRun++
    if (-not $Condition) {
        throw $Message
    }
}

function Invoke-Git {
    param(
        [string] $Directory,
        [string[]] $Arguments,
        [switch] $Bare
    )

    $nativePreference = $PSNativeCommandUseErrorActionPreference
    $PSNativeCommandUseErrorActionPreference = $false
    try {
        if ($Bare) {
            $output = @(& git "--git-dir=$Directory" @Arguments 2>&1)
        } else {
            $output = @(& git -C $Directory @Arguments 2>&1)
        }
        $exitCode = $LASTEXITCODE
    } finally {
        $PSNativeCommandUseErrorActionPreference = $nativePreference
    }
    if ($exitCode -ne 0) {
        throw "git -C $Directory $($Arguments -join ' ') failed:`n$($output -join "`n")"
    }
    return $output
}

function Invoke-Repin {
    param(
        [string] $Repository,
        [string] $TargetSha,
        [string] $SkiaSha,
        [string] $ReviewedSha,
        [string] $SkiaBranch = 'skiasharp',
        [switch] $Apply,
        [switch] $Push,
        [string] $TargetBranch = 'skia-sync/test'
    )

    Push-Location $Repository
    try {
        $arguments = @(
            '-NoLogo',
            '-NoProfile',
            '-File',
            $scriptPath,
            '-SkiaSharpBranch',
            $TargetBranch,
            '-SkiaBranch',
            $SkiaBranch,
            '-ReviewedSkiaSha',
            $ReviewedSha,
            '-ExpectedTargetSha',
            $TargetSha,
            '-ExpectedSkiaSha',
            $SkiaSha
        )
        if ($Apply) {
            $arguments += '-Apply'
        }
        if ($Push) {
            $arguments += '-Push'
        }
        $nativePreference = $PSNativeCommandUseErrorActionPreference
        $PSNativeCommandUseErrorActionPreference = $false
        try {
            $output = @(& pwsh @arguments 2>&1)
            $exitCode = $LASTEXITCODE
        } finally {
            $PSNativeCommandUseErrorActionPreference = $nativePreference
        }
        return [PSCustomObject]@{
            ExitCode = $exitCode
            Output = $output -join "`n"
        }
    } finally {
        Pop-Location
    }
}

try {
    $env:GIT_ALLOW_PROTOCOL = 'file'
    Remove-Item -Recurse -Force $workRoot -ErrorAction Ignore
    New-Item -ItemType Directory -Path $workRoot | Out-Null
    $gitExecPath = Join-Path $workRoot 'git-exec'
    New-Item -ItemType Directory -Path $gitExecPath | Out-Null
    $gitExecSource = @(
        (& git --exec-path).Trim(),
        '/usr/lib/git-core',
        '/usr/libexec/git-core',
        '/Library/Developer/CommandLineTools/usr/libexec/git-core',
        '/Applications/Xcode.app/Contents/Developer/usr/libexec/git-core'
    ) | Where-Object { Test-Path (Join-Path $_ 'git-submodule') } | Select-Object -First 1
    if (-not $gitExecSource) {
        throw 'Could not locate a Git exec path containing git-submodule.'
    }
    Copy-Item -Path (Join-Path $gitExecSource '*') -Destination $gitExecPath -Recurse
@'
#!/bin/sh
set -eu

if [ "$1" != update ]; then
  echo "Unsupported fixture git submodule command: $*" >&2
  exit 1
fi

path=externals/skia
url=${REPIN_FIXTURE_NATIVE_URL:-$(git config -f .gitmodules --get submodule.externals/skia.url)}
if [ ! -e "$path/.git" ]; then
  git clone "$url" "$path"
  if [ -n "${REPIN_FIXTURE_NATIVE_URL:-}" ]; then
    git -C "$path" remote set-url origin https://github.com/mono/skia.git
    git -C "$path" config "url.${REPIN_FIXTURE_NATIVE_URL}.insteadOf" https://github.com/mono/skia.git
  fi
fi
sha=$(git ls-tree HEAD -- "$path" | awk '{print $3}')
git -C "$path" checkout --detach "$sha"
'@ | Set-Content -Path (Join-Path $gitExecPath 'git-submodule') -NoNewline
    & /bin/chmod '+x' (Join-Path $gitExecPath 'git-submodule')
    if ($LASTEXITCODE -ne 0) {
        throw 'Could not make the fixture git-submodule shim executable.'
    }
    $env:GIT_EXEC_PATH = $gitExecPath
    $nativeBare = Join-Path $workRoot 'native.git'
    $env:REPIN_FIXTURE_NATIVE_URL = $nativeBare
    $nativeSource = Join-Path $workRoot 'native-source'
    $parentBare = Join-Path $workRoot 'parent.git'
    $parentSource = Join-Path $workRoot 'parent-source'

    & git init --bare $nativeBare | Out-Null
    Invoke-Git $workRoot @('init', '-b', 'skiasharp', $nativeSource) | Out-Null
    Invoke-Git $nativeSource @('config', 'user.name', 'Fixture') | Out-Null
    Invoke-Git $nativeSource @('config', 'user.email', 'fixture@example.invalid') | Out-Null
    Set-Content -Path (Join-Path $nativeSource 'native.txt') -Value 'reviewed' -NoNewline
    Invoke-Git $nativeSource @('add', 'native.txt') | Out-Null
    Invoke-Git $nativeSource @('commit', '-m', 'reviewed native commit') | Out-Null
    $reviewedNativeSha = (Invoke-Git $nativeSource @('rev-parse', 'HEAD')).Trim()
    Invoke-Git $nativeSource @('remote', 'add', 'origin', $nativeBare) | Out-Null
    Invoke-Git $nativeSource @('push', 'origin', 'skiasharp') | Out-Null

    & git init --bare $parentBare | Out-Null
    Invoke-Git $workRoot @('init', '-b', 'skia-sync/test', $parentSource) | Out-Null
    Invoke-Git $parentSource @('config', 'user.name', 'Fixture') | Out-Null
    Invoke-Git $parentSource @('config', 'user.email', 'fixture@example.invalid') | Out-Null
    Invoke-Git $parentSource @('config', 'protocol.file.allow', 'always') | Out-Null
    @"
[submodule "externals/skia"]
    path = externals/skia
    url = https://github.com/mono/skia.git
"@ | Set-Content -Path (Join-Path $parentSource '.gitmodules') -NoNewline
    @"
    {"registrations":[{"component":{"type":"git","git":{"repositoryUrl":"https://github.com/mono/skia.git","commitHash":"$reviewedNativeSha"}}}]}
"@ | Set-Content -Path (Join-Path $parentSource 'cgmanifest.json') -NoNewline
    Invoke-Git $parentSource @('add', '.gitmodules', 'cgmanifest.json') | Out-Null
    Invoke-Git $parentSource @('update-index', '--add', '--cacheinfo', "160000,$reviewedNativeSha,externals/skia") | Out-Null
    Invoke-Git $parentSource @('commit', '-m', 'reviewed SkiaSharp PR') | Out-Null
    $initialParentSha = (Invoke-Git $parentSource @('rev-parse', 'HEAD')).Trim()
    Invoke-Git $parentSource @('remote', 'add', 'origin', $parentBare) | Out-Null
    Invoke-Git $parentSource @('push', 'origin', 'skia-sync/test') | Out-Null
    Invoke-Git $parentSource @('submodule', 'update', '--init', 'externals/skia') | Out-Null
    $fixtureManifest = Get-Content (Join-Path $parentSource 'cgmanifest.json') -Raw | ConvertFrom-Json
    Assert-True ($fixtureManifest.registrations[0].component.git.commitHash -eq $reviewedNativeSha) `
        "The fixture manifest SHA was not $reviewedNativeSha."

    $staleTarget = Invoke-Repin $parentSource ('1' * 40) $reviewedNativeSha $reviewedNativeSha
    Assert-True ($staleTarget.ExitCode -ne 0 -and $staleTarget.Output -match 'expected') `
        "A changed expected target SHA was not rejected. Exit $($staleTarget.ExitCode): $($staleTarget.Output)"

    Invoke-Git $nativeSource @('checkout', '-b', 'invalid-shape', $reviewedNativeSha) | Out-Null
    Invoke-Git $nativeSource @('commit', '--allow-empty', '-m', 'not a merge') | Out-Null
    $invalidNativeSha = (Invoke-Git $nativeSource @('rev-parse', 'HEAD')).Trim()
    Invoke-Git $nativeSource @('push', 'origin', 'invalid-shape') | Out-Null
    $invalidMerge = Invoke-Repin $parentSource $initialParentSha $invalidNativeSha $reviewedNativeSha 'invalid-shape'
    Assert-True ($invalidMerge.ExitCode -ne 0 -and $invalidMerge.Output -match 'not a two-parent merge') `
        "A non-merge native tip was not rejected. Exit $($invalidMerge.ExitCode): $($invalidMerge.Output)"

    Invoke-Git $parentSource @('checkout', '-b', 'skia-sync/already-current') | Out-Null
    Invoke-Git (Join-Path $parentSource 'externals/skia') @('checkout', '--detach', $invalidNativeSha) | Out-Null
    $alreadyCurrentManifest = (Get-Content (Join-Path $parentSource 'cgmanifest.json') -Raw).Replace(
        $reviewedNativeSha,
        $invalidNativeSha)
    [System.IO.File]::WriteAllText(
        (Join-Path $parentSource 'cgmanifest.json'),
        $alreadyCurrentManifest,
        [System.Text.UTF8Encoding]::new($false))
    Invoke-Git $parentSource @('add', 'cgmanifest.json', 'externals/skia') | Out-Null
    Invoke-Git $parentSource @('commit', '-m', 'already current invalid native commit') | Out-Null
    $alreadyCurrentSha = (Invoke-Git $parentSource @('rev-parse', 'HEAD')).Trim()
    Invoke-Git $parentSource @('push', 'origin', 'skia-sync/already-current') | Out-Null
    $alreadyCurrentInvalid = Invoke-Repin `
        $parentSource $alreadyCurrentSha $invalidNativeSha $reviewedNativeSha `
        'invalid-shape' -TargetBranch 'skia-sync/already-current'
    Assert-True ($alreadyCurrentInvalid.ExitCode -ne 0 -and
        $alreadyCurrentInvalid.Output -match 'not a two-parent merge') `
        'An already-current target accepted a native SHA that was not a merge.'
    Invoke-Git $parentSource @('checkout', 'skia-sync/test') | Out-Null
    Invoke-Git $parentSource @('submodule', 'update', '--init', 'externals/skia') | Out-Null

    Invoke-Git $nativeSource @('checkout', '-b', 'mismatch-first-parent', $reviewedNativeSha) | Out-Null
    Set-Content -Path (Join-Path $nativeSource 'native.txt') -Value 'different tree' -NoNewline
    Invoke-Git $nativeSource @('add', 'native.txt') | Out-Null
    Invoke-Git $nativeSource @('commit', '-m', 'change native tree') | Out-Null
    $mismatchFirstParentSha = (Invoke-Git $nativeSource @('rev-parse', 'HEAD')).Trim()
    $mismatchSha = (Invoke-Git $nativeSource @(
        'commit-tree',
        "${mismatchFirstParentSha}^{tree}",
        '-p',
        $mismatchFirstParentSha,
        '-p',
        $reviewedNativeSha,
        '-m',
        'invalid merged tree')).Trim()
    Invoke-Git $nativeSource @('push', 'origin', "${mismatchSha}:refs/heads/mismatch") | Out-Null
    $treeMismatch = Invoke-Repin $parentSource $initialParentSha $mismatchSha $reviewedNativeSha 'mismatch'
    Assert-True ($treeMismatch.ExitCode -ne 0 -and $treeMismatch.Output -match 'differs') `
        'A native merge with a different tree was not rejected.'

    Invoke-Git $nativeSource @('checkout', '-B', 'skiasharp', $reviewedNativeSha) | Out-Null
    Invoke-Git $nativeSource @('checkout', '-b', 'first-parent', $reviewedNativeSha) | Out-Null
    Invoke-Git $nativeSource @('commit', '--allow-empty', '-m', 'merge first parent') | Out-Null
    $firstParentSha = (Invoke-Git $nativeSource @('rev-parse', 'HEAD')).Trim()
    $mergedNativeSha = (Invoke-Git $nativeSource @(
        'commit-tree',
        "${reviewedNativeSha}^{tree}",
        '-p',
        $firstParentSha,
        '-p',
        $reviewedNativeSha,
        '-m',
        'merged native commit')).Trim()
    Invoke-Git $nativeSource @('branch', '-f', 'skiasharp', $mergedNativeSha) | Out-Null
    Invoke-Git $nativeSource @('checkout', 'skiasharp') | Out-Null
    Invoke-Git $nativeSource @('push', 'origin', 'skiasharp') | Out-Null

    Invoke-Git $parentSource @('checkout', '-b', 'skia-sync/first-parent') | Out-Null
    Invoke-Git (Join-Path $parentSource 'externals/skia') @(
        'fetch',
        'origin',
        '+refs/heads/skiasharp:refs/remotes/origin/skiasharp') | Out-Null
    Invoke-Git (Join-Path $parentSource 'externals/skia') @('checkout', '--detach', $firstParentSha) | Out-Null
    $firstParentManifest = (Get-Content (Join-Path $parentSource 'cgmanifest.json') -Raw).Replace(
        $reviewedNativeSha,
        $firstParentSha)
    [System.IO.File]::WriteAllText(
        (Join-Path $parentSource 'cgmanifest.json'),
        $firstParentManifest,
        [System.Text.UTF8Encoding]::new($false))
    Invoke-Git $parentSource @('add', 'cgmanifest.json', 'externals/skia') | Out-Null
    Invoke-Git $parentSource @('commit', '-m', 'pin native merge first parent') | Out-Null
    $firstParentTargetSha = (Invoke-Git $parentSource @('rev-parse', 'HEAD')).Trim()
    Invoke-Git $parentSource @('push', 'origin', 'skia-sync/first-parent') | Out-Null
    $firstParentRejected = Invoke-Repin `
        $parentSource $firstParentTargetSha $mergedNativeSha $firstParentSha `
        'skiasharp' -TargetBranch 'skia-sync/first-parent'
    Assert-True ($firstParentRejected.ExitCode -ne 0 -and
        $firstParentRejected.Output -match 'second parent') `
        'The script accepted the native merge first parent as the reviewed SHA.'
    Invoke-Git $parentSource @('checkout', 'skia-sync/test') | Out-Null
    Invoke-Git $parentSource @('submodule', 'update', '--init', 'externals/skia') | Out-Null

    $dryRun = Invoke-Repin $parentSource $initialParentSha $mergedNativeSha $reviewedNativeSha
    Assert-True ($dryRun.ExitCode -eq 0 -and
        $dryRun.Output -match 'no commit, tracked source files, or remote refs were changed') `
        'The valid repin dry run did not succeed without mutation.'
    Assert-True ((Invoke-Git $parentBare @('rev-parse', 'refs/heads/skia-sync/test') -Bare).Trim() -eq $initialParentSha) `
        'The dry run unexpectedly updated the remote target branch.'

    $applyAndPush = Invoke-Repin $parentSource $initialParentSha $mergedNativeSha $reviewedNativeSha -Apply -Push
    Assert-True ($applyAndPush.ExitCode -ne 0 -and $applyAndPush.Output -match 'cannot be used together') `
        'The script accepted Apply and Push together.'
    $apply = Invoke-Repin $parentSource $initialParentSha $mergedNativeSha $reviewedNativeSha -Apply
    Assert-True ($apply.ExitCode -eq 0 -and $apply.Output -match 'locally.*without pushing') `
        'The local Apply repin did not succeed without a push.'
    $appliedParentSha = (Invoke-Git $parentSource @('rev-parse', 'HEAD')).Trim()
    Assert-True ($appliedParentSha -ne $initialParentSha) 'Apply did not create the local target commit.'
    Assert-True ((Invoke-Git $parentBare @('rev-parse', 'refs/heads/skia-sync/test') -Bare).Trim() -eq $initialParentSha) `
        'Apply unexpectedly updated the remote target branch.'
    Invoke-Git $parentSource @('reset', '--hard', $initialParentSha) | Out-Null
    Invoke-Git $parentSource @('submodule', 'update', '--init', 'externals/skia') | Out-Null

    $push = Invoke-Repin $parentSource $initialParentSha $mergedNativeSha $reviewedNativeSha -Push
    Assert-True ($push.ExitCode -eq 0 -and $push.Output -match 'Updated and pushed') `
        'The direct repin push did not succeed.'
    $updatedParentSha = (Invoke-Git $parentBare @('rev-parse', 'refs/heads/skia-sync/test') -Bare).Trim()
    Assert-True ($updatedParentSha -ne $initialParentSha) 'The direct repin did not create a target commit.'
    $changedPaths = (@(Invoke-Git $parentBare @('diff', '--name-only', $initialParentSha, $updatedParentSha) -Bare) |
        Sort-Object) -join ','
    Assert-True ($changedPaths -ceq 'cgmanifest.json,externals/skia') `
        'The direct repin changed paths other than cgmanifest.json and externals/skia.'
    $updatedGitlink = ((Invoke-Git $parentBare @('ls-tree', $updatedParentSha, 'externals/skia') -Bare) -split '\s+')[2]
    Assert-True ($updatedGitlink -eq $mergedNativeSha) 'The direct repin did not update the gitlink.'
    $manifest = Invoke-Git $parentBare @('show', "${updatedParentSha}:cgmanifest.json") -Bare | ConvertFrom-Json
    Assert-True ($manifest.registrations[0].component.git.commitHash -eq $mergedNativeSha) `
        'The direct repin did not update cgmanifest.json.'

    $idempotent = Invoke-Repin $parentSource $updatedParentSha $mergedNativeSha $reviewedNativeSha -Push
    Assert-True ($idempotent.ExitCode -eq 0 -and $idempotent.Output -match 'Already current') `
        'An already-correct target branch was not idempotent.'
    Assert-True ((Invoke-Git $parentBare @('rev-parse', 'refs/heads/skia-sync/test') -Bare).Trim() -eq $updatedParentSha) `
        'The idempotent repin unexpectedly created another commit.'

    Write-Host "Passed $script:TestsRun direct repin fixture tests."
} finally {
    Remove-Item -Recurse -Force $workRoot -ErrorAction Ignore
    $env:GIT_ALLOW_PROTOCOL = $previousGitAllowProtocol
    $env:GIT_EXEC_PATH = $previousGitExecPath
    $env:REPIN_FIXTURE_NATIVE_URL = $previousFixtureNativeUrl
}
