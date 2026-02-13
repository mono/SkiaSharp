# Platform: Console Application (Default)

The default reproduction strategy. Use for any bug without platform-specific signals.

## Signals

No platform-specific keywords, or explicitly: console app, command line, library-only,
core SkiaSharp, SKBitmap, SKCanvas, SKPaint, SKImage, SKPath, SKMatrix, SKCodec, SKData.

Also use as **fallback** when a platform-specific file reports `needs-platform` — many
platform-reported bugs are actually in core SkiaSharp and reproducible in a console app.

## Prerequisites

- .NET SDK (8.0+ recommended; match `{reporter_tfm}` if specified)
- No special workloads needed

## Create Project

```bash
mkdir -p /tmp/repro-{number} && cd /tmp/repro-{number}
dotnet new console -n Repro --framework {reporter_tfm}
cd Repro
dotnet add package SkiaSharp --version {reporter_version}
# Add other packages the reporter mentioned (e.g., HarfBuzzSharp, SkiaSharp.NativeAssets.Linux)
```

## Add Repro Code

- **Use the reporter's code** if provided — copy it as closely as possible
- If no code provided: create minimal code from the issue description
- The code should **clearly demonstrate** the bug (print values, save images, assert conditions)
- Add a success marker: `Console.WriteLine("SUCCESS: test completed");`

**Capture file content:** Record `Program.cs` and `.csproj` in `reproductionSteps[].filesCreated[].content`.

## Build

```bash
dotnet build
```

Record: exit code, warnings, errors. Build failure may itself be the reproduction
(e.g., reporter says "CS0117 error on MakeIdentity").

## Run & Verify

```bash
dotnet run
```

Capture:

| What | How | Limit |
|------|-----|-------|
| Command run | Exact command (redact absolute paths) | — |
| Exit code | 0 = success, non-zero = failure | — |
| Output | stdout/stderr | 2KB success, 4KB failure |
| Errors | Error message + first 50 lines of stack trace | 5KB |

**Verification:**
- Exit code 0 + expected output → `success`
- Non-zero exit code or crash → `failure`
- Exit code 0 but wrong values/output → `wrong-output`

## Iterate

If the first attempt doesn't clearly reproduce:

1. Try different input data (different images, fonts, matrix values)
2. Try a different API approach (the reporter may have simplified)
3. Try a nearby NuGet version (maybe reporter was slightly off)
4. Simplify to the absolute minimum
5. If platform-specific → try Docker Linux (read `platform-docker-linux.md`)

## Conclusion Mapping

| Observation | Conclusion |
|------------|------------|
| Crash/exception matching report | `reproduced` |
| Wrong output values matching report | `wrong-output` |
| Code runs correctly, no errors | `not-reproduced` |
| Build error matching report | `reproduced` |

## Main Source Testing (Phase 3C)

When Phase 3C requires testing against the main branch source:

```bash
# Return to the SkiaSharp repo root
cd "$(git rev-parse --show-toplevel)"
[ -d "output/native" ] && ls output/native/ | head -5 || dotnet cake --target=externals-download

# Build and run the console sample (uses project references to local source)
dotnet build samples/Basic/Console/SkiaSharpSample/SkiaSharpSample.csproj
dotnet run --project samples/Basic/Console/SkiaSharpSample/SkiaSharpSample.csproj
```

The console sample draws text to an `SKBitmap` and saves a PNG — it exercises the same
core SkiaSharp APIs. If you need to test the reporter's specific code, temporarily modify
`samples/Basic/Console/SkiaSharpSample/Program.cs` with the repro code, then revert with
`git checkout` after recording the result.
