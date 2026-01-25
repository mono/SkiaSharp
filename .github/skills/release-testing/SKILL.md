---
name: release-testing
description: >
  Run integration tests to verify SkiaSharp NuGet packages work correctly before publishing.
  
  Use when user asks to:
  - Test/verify packages before release
  - Run integration tests
  - Test on specific device (iPad, iPhone, Android emulator, Mac, Windows)
  - Verify SkiaSharp rendering works
  - Check if packages are ready for publishing
  - Run smoke/console/blazor/maui tests
  - Continue with release
  - Test version X
  
  Triggers: "test the release", "verify packages", "run tests on iPad", "check ios tests",
  "test mac catalyst", "run android tests", "continue", "test 3.119.2-preview.2".
---

# Release Testing Skill

Verify SkiaSharp packages work correctly before publishing.

⚠️ **NO UNDO:** This is step 2 of 3. See [releasing.md](../../../documentation/releasing.md) for full workflow.

## CRITICAL: ANY FAIL IS TOTAL FAIL

- Test fails → Release FAILS
- Test times out → Release FAILS
- Screenshot doesn't match → Release FAILS

**Never rationalize failures.** Fix the issue before proceeding.

---

## Step 1: Resolve Package Versions

**DO NOT ask user for exact NuGet versions.** Resolve automatically:

1. Fetch release branch and read `scripts/VERSIONS.txt`:
   - `SkiaSharp nuget` line → base version (e.g., `3.119.2`)
   - `HarfBuzzSharp nuget` line → base version (e.g., `8.3.1.4`)

2. Read `PREVIEW_LABEL` from `scripts/azure-templates-variables.yml` (e.g., `preview.2` or `stable`)

3. Search preview feed:
   ```bash
   dotnet package search SkiaSharp --source "https://aka.ms/skiasharp-eap/index.json" --exact-match --prerelease --format json
   ```

4. Filter versions matching `{base}-{preview-label}.{build}`, pick latest

5. Report to user:
   ```
   Resolved versions:
     SkiaSharp:     3.119.2-preview.2.3
     HarfBuzzSharp: 8.3.1.4-preview.2.3
     Build number:  3
   
   Proceed with testing?
   ```

**Note:** SkiaSharp and HarfBuzzSharp have different base versions but share the same preview label and build number.

### No packages found?

CI build hasn't completed. Check Azure Pipelines, wait 2-4 hours.

### Stable releases

PREVIEW_LABEL = `stable`, packages appear as `X.Y.Z-stable.{build}`.

---

## Step 2: Run Integration Tests

```bash
cd tests/SkiaSharp.Tests.Integration
dotnet test -p:SkiaSharpVersion={skia-version} -p:HarfBuzzSharpVersion={hb-version}
```

**Specific platforms:**
```bash
dotnet test --filter "FullyQualifiedName~MauiiOSTests" ...
dotnet test --filter "FullyQualifiedName~MauiMacCatalystTests" ...
dotnet test --filter "FullyQualifiedName~BlazorTests" ...
dotnet test --filter "FullyQualifiedName~ConsoleTests" ...
```

### Device Selection

```bash
# iOS simulators
xcrun simctl list devices available | grep -i "iphone\|ipad"

# Android emulators  
emulator -list-avds
```

With device parameters:
```bash
dotnet test --filter "MauiiOSTests" ... -p:iOSDevice="iPad Pro 13-inch (M4)" -p:iOSVersion="18.5"
dotnet test --filter "MauiAndroidTests" ... -p:AndroidDevice="Pixel 8"
```

### Test Matrix

| Test | Platform | Time |
|------|----------|------|
| SmokeTests | All | ~2s |
| ConsoleTests | All | ~20s |
| BlazorTests | All | ~2min |
| MauiiOSTests | macOS only | ~2min |
| MauiMacCatalystTests | macOS only | ~2min |
| MauiAndroidTests | All | ~2min |
| MauiWindowsTests | Windows only | ~2min |

### Output Files

Screenshots saved to `output/logs/testlogs/integration/`:
- `*-full.png` — Full screenshot
- `*-region.png` — With crop rectangle
- `*.png` — Cropped canvas
- `*-diff.png` — Difference mask

---

## Step 3: Testing with Local Artifacts (Stable releases)

For final validation before stable release:

1. Ask user for path to downloaded CI artifacts
2. Clear NuGet cache: `rm -rf ~/.nuget/packages/skiasharp* ~/.nuget/packages/harfbuzzsharp*`
3. Create nuget.config pointing to local folder
4. Run tests with `--configfile /tmp/nuget.config`

---

## Step 4: Verify Release Criteria

Proceed to **release-publish** ONLY when:

- ✅ ALL tests pass (no failures, no skips except hardware)
- ✅ Screenshots exist in output directory
- ✅ Image similarity ≥ 95%

**Valid skips:** iOS/Mac on non-macOS, Windows on non-Windows, Android without emulator.

### Stable Release Checklist

- [ ] NuGet packages produced
- [ ] Native assets 4-6MB per binary
- [ ] Assemblies strong named
- [ ] NuGet metadata correct
- [ ] Samples build in Release mode
- [ ] No "To be added." in docs

---

## References

- **Setup problems?** See [references/setup.md](references/setup.md)
- **Test failures?** See [references/troubleshooting.md](references/troubleshooting.md)
