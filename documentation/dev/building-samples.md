# Building and Validating Samples

The Samples suite builds generated applications against produced NuGet packages,
runs Console/Web APIs on the host and in Docker, and checks actual browser UIs.

## Run the suite

Install the repository SDK and the workloads required by your host's samples.
Docker build/run tests run on any host with usable Docker, including macOS.
Put `docker` on `PATH` and start a daemon using Windows containers on Windows
or Linux containers on macOS/Linux. CI does not provision Docker on macOS.
Unavailable Docker produces visible skips; wrong container mode and sample
failures fail the tests.

**Bootstrap before staging packages:** bootstrap resets `output/`.
For managed-only work:

```sh
dotnet cake --target=externals-download
```

For native or Skia changes, build natives from source instead; see
[AGENTS.md](../../AGENTS.md). Do not download prebuilt natives after native changes.

Stage one producing build's non-symbol packages in `output/nugets/`, including
SkiaSharp, HarfBuzzSharp, and the required native/view packages. Its base versions
must match `scripts/VERSIONS.txt`. Download the public `nuget` build artifact,
or use the PR helper:

```powershell
$pr = 1234 # Producing PR number.
pwsh scripts/get-skiasharp-pr.ps1 $pr -SuccessfulOnly -Force
New-Item output/nugets -ItemType Directory -Force | Out-Null
Copy-Item "$HOME/.skiasharp/hives/pr-$pr/packages/*.nupkg" output/nugets/
```

Pass the producing package suffix explicitly. For example, packages ending in
`-pr.1234.26509.10` require:

```sh
dotnet cake --target=samples --previewLabel=pr.1234 --buildNumber=26509.10
```

This generates the samples and runs the suite. Do not mix producing builds:
generation uses the supplied arguments, not package-filename version detection.

## IDE and direct test runs

After staging packages, generate once before opening the test project in an IDE
or running it directly:

```sh
dotnet cake --target=samples-generate --previewLabel=pr.1234 --buildNumber=26509.10
dotnet test tests/SkiaSharp.Tests.Samples.slnx \
  -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0 -- --report-trx
```

The solution includes the runner's source dependencies; the properties above
limit their build to the runner's .NET 10 target.

Direct tests use the generated inputs without invoking Cake or downloading
packages. The runner uses the repository SDK; sample builds use `dotnet` from
the inherited `PATH`, without changing project TFMs. Set the intended SDK and
platform environment before launching the terminal or IDE.

## Coverage and diagnostics

Build theories cover eligible Basic and Gallery solutions; Gallery remains
build-only. Named runtime facts check host and Docker Console exit codes,
output and PNGs. Basic Web and Docker Web API retain their HTTP/image tests
without a browser; Basic Web also has a separate page screenshot test.
Browser facts cover host Web, WASM and Blazor UIs. Device execution is not part
of this suite. Docker retains the samples' original .NET 10 images; its SDK is
independent of the host SDK.
Generated-project tests check exact package versions and Uno's SkiaSharp override.
Integration/MSBuild suites remain separate.

Use IDE filters or select one test:

```sh
dotnet test tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj \
  -- --filter-method '*BasicSampleTests.WebSample*'
```

Find stdout/stderr, binlogs, results, and images under
`output/logs/testlogs/samples/`, or in CI's `sample_logs_*` artifact. For restore
failures, compare generated versions with the staged packages and inspect the
binlog. Tests use private workspaces/caches; do not clear shared caches.

PNG references are platform-specific under `tests/SkiaSharp.Tests.Samples/Expected/`.
Dimensions must match; at most **0.075%** of pixels may differ, with zero channel
tolerance, except for the animated GPU page described below.
Failures retain the actual image; mismatches also retain a diff.
Review replacement references on the affected platform; tests never accept them
automatically.

## Browser runs

After bootstrap, package acquisition and generation, explicitly install the
Chromium revision matching the runner's Playwright dependency:

```sh
dotnet build tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj
pwsh tests/SkiaSharp.Tests.Samples/bin/Debug/net10.0/playwright.ps1 install --with-deps chromium
dotnet test tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj \
  -- --filter-trait Category=Browser --report-trx
```

Use the Release script path for a Release runner build. CI installs Chromium
explicitly too; tests never download browsers. The build supplies the installer
for the exact Playwright version. System Chrome/Edge installed through
Homebrew or winget need an explicit browser channel and are not the Chromium
used by these tests. See [Playwright browser installation](https://playwright.dev/dotnet/docs/browsers).
On Linux, `--with-deps` may require elevated privileges.

`BrowserRunningApp.Capture` handles a single page check. For interactions, use
`BrowserRunningApp.Run` with `Navigate`, `WaitForElement` and `Screenshot`;
its `Page` exposes Playwright locators, clicks and pointer input.
The callback owns no browser resources: the helper closes them and retains
browser errors and diagnostics.

Screenshots use a 1280 x 900 viewport, scale 1 and light theme. Review page
references under `Expected/Host/<host-platform>/` separately from API PNGs.
The animated GPU page alone permits **6% average RGBA color error**, rather
than the default differing-pixel limit. This loose smoke check can miss
uniform dark output; it is not an exact shader regression test.
Missing browsers, browser errors and unavailable GPU rendering fail.
Browser-only filtering does not replace the unfiltered sample suite.
