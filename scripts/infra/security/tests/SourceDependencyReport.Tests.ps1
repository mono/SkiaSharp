$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../..'))
$scriptPath = Join-Path $repoRoot 'scripts/infra/security/source-dependency-report.ps1'
$testRoot = Join-Path ([IO.Path]::GetTempPath()) "skiasharp-source-report-tests-$([Guid]::NewGuid().ToString('N'))"
$sourceRoot = Join-Path $testRoot 'source'
$traceRoot = Join-Path $testRoot 'trace'
$reportRoot = Join-Path $testRoot 'report'

function Assert-Repository {
    param(
        [Parameter(Mandatory)] $Report,
        [Parameter(Mandatory)][string] $Url,
        [switch] $Observed,
        [switch] $Declared
    )

    $repository = @($Report.repositories | Where-Object url -eq $Url)
    if ($repository.Count -ne 1) {
        throw "Expected one '$Url' repository, found $($repository.Count)."
    }
    if ($Observed -and -not $repository[0].observed) {
        throw "Expected '$Url' to be observed."
    }
    if ($Declared -and -not $repository[0].declared) {
        throw "Expected '$Url' to be declared."
    }
}

try {
    New-Item $sourceRoot, $traceRoot -ItemType Directory -Force | Out-Null
    & git -C $sourceRoot init --quiet
    & git -C $sourceRoot config user.email source-report@example.invalid
    & git -C $sourceRoot config user.name 'Source Report Tests'
    & git -C $sourceRoot remote add origin `
        'https://build-user:super-secret@github.com/mono/SkiaSharp.git?access_token=do-not-publish'

    @'
[submodule "depot_tools"]
    path = externals/depot_tools
    url = https://chromium.googlesource.com/chromium/tools/depot_tools.git
'@ | Set-Content (Join-Path $sourceRoot '.gitmodules')
    @'
var dawn = "https://github.com/google/dawn/releases/download/{TAG}/source.zip";
'@ | Set-Content (Join-Path $sourceRoot 'build.cake')
    $fixtureDirectory = Join-Path $sourceRoot 'scripts/infra/security/tests'
    New-Item $fixtureDirectory -ItemType Directory -Force | Out-Null
    'https://dev.azure.com/example/project/_git/test-fixture' |
        Set-Content (Join-Path $fixtureDirectory 'Fake.Tests.ps1')
    @'
{
  "registrations": [
    {
      "component": {
        "type": "other",
        "other": {
          "name": "example",
          "version": "1.0",
          "downloadUrl": "https://github.com/upstream/not-actually-downloaded"
        }
      },
      "skia_dependency": {
        "revision": "0123456789abcdef0123456789abcdef01234567",
        "version_reviewed_identity": "https://skia.googlesource.com/example/source.git@0123456789abcdef0123456789abcdef01234567"
      }
    }
  ]
}
'@ | Set-Content (Join-Path $sourceRoot 'cgmanifest.json')
    & git -C $sourceRoot add .
    & git -C $sourceRoot commit --quiet -m initial

    @'
{"event":"start","sid":"clone-session","argv":["git","clone","https://trace-user:trace-password@github.com/google/angle.git?token=hidden","externals/angle"]}
{"event":"child_start","sid":"fetch-session","child_class":"transport/https","argv":["git","remote-https","origin","https://github.com/emscripten-core/emsdk.git"]}
{"event":"start","sid":"checkout-session","argv":["git","-c","http.https://github.com/mono/SkiaSharp.extraheader=AUTHORIZATION: bearer secret","fetch","origin"]}
not-json
'@ | Set-Content (Join-Path $traceRoot 'trace-event')

    $startTrace = Join-Path $testRoot 'start-trace'
    $startOutput = & $scriptPath -Start -TraceDirectory $startTrace 6>&1 | Out-String
    if ($startOutput -notmatch 'variable=GIT_TRACE2_EVENT') {
        throw 'Start mode did not configure Git Trace2.'
    }
    if (-not (Test-Path $startTrace -PathType Container)) {
        throw 'Start mode did not create the trace directory.'
    }

    & $scriptPath `
        -Finish `
        -TraceDirectory $traceRoot `
        -RepositoryRoot $sourceRoot `
        -ReportDirectory $reportRoot `
        -JobName test_job `
        -RequireTrace

    $jsonPath = Join-Path $reportRoot 'source-dependencies.json'
    $markdownPath = Join-Path $reportRoot 'source-dependencies.md'
    if (-not (Test-Path $jsonPath -PathType Leaf) -or
        -not (Test-Path $markdownPath -PathType Leaf)) {
        throw 'The JSON and Markdown reports were not both created.'
    }

    $json = Get-Content $jsonPath -Raw
    if ($json -match 'super-secret|trace-password|access_token|token=hidden') {
        throw 'The report leaked credentials or URL query parameters.'
    }
    $report = $json | ConvertFrom-Json
    if ($report.schemaVersion -ne 1) {
        throw "Unexpected schema version '$($report.schemaVersion)'."
    }
    if ($report.build.jobName -ne 'test_job') {
        throw "Unexpected report job name '$($report.build.jobName)'."
    }
    if ($report.coverage.gitTrace2.traceFiles -ne 1 -or
        $report.coverage.gitTrace2.malformedLines -ne 1) {
        throw 'Trace coverage counters are incorrect.'
    }

    Assert-Repository $report 'https://github.com/mono/SkiaSharp' -Observed
    Assert-Repository $report 'https://github.com/google/angle' -Observed
    Assert-Repository $report 'https://github.com/emscripten-core/emsdk' -Observed
    Assert-Repository $report 'https://github.com/google/dawn' -Declared
    Assert-Repository $report `
        'https://chromium.googlesource.com/chromium/tools/depot_tools' `
        -Declared
    Assert-Repository $report 'https://skia.googlesource.com/example/source' -Declared
    if (@($report.repositories | Where-Object {
        $_.url -in @(
            'https://dev.azure.com/example/project/_git/test-fixture'
            'https://github.com/upstream/not-actually-downloaded'
        )
    }).Count -ne 0 -or $json -match 'SkiaSharp\.extraheader') {
        throw 'Test fixtures, Git auth config, or canonical CVE aliases leaked into the report.'
    }

    if (Test-Path $traceRoot) {
        throw 'Raw Trace2 data was not removed after report generation.'
    }

    $missingTraceRejected = $false
    try {
        & $scriptPath `
            -Finish `
            -TraceDirectory (Join-Path $testRoot 'missing') `
            -RepositoryRoot $sourceRoot `
            -ReportDirectory (Join-Path $testRoot 'missing-report') `
            -RequireTrace
    } catch {
        $missingTraceRejected = $_.Exception.Message -like '*Trace2 did not produce any files*'
    }
    if (-not $missingTraceRejected) {
        throw 'Required Trace2 coverage accepted a missing trace.'
    }

    Write-Host 'Source dependency report tests passed.'
} finally {
    Remove-Item $testRoot -Recurse -Force -ErrorAction Ignore
}
