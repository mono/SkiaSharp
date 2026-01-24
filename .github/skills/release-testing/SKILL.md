---
name: release-testing
description: >
  Run integration tests to verify SkiaSharp NuGet packages work correctly before publishing.
  Use when user asks to: (1) test/verify packages before release, (2) run integration tests,
  (3) test on specific device (iPad, iPhone, Android emulator, Mac, Windows),
  (4) verify SkiaSharp rendering works, (5) check if packages are ready for publishing,
  (6) run smoke/console/blazor/maui tests. Triggers: "test the release", "verify packages",
  "run tests on iPad", "check ios tests", "test mac catalyst", "run android tests".
---

# Release Testing Skill

Verify SkiaSharp packages work correctly before publishing.

## ⚠️ CRITICAL: ANY FAIL IS TOTAL FAIL

**A test either PASSES or FAILS. Period.**

- Test fails → Release FAILS
- Test times out → Release FAILS  
- Screenshot doesn't match → Release FAILS
- "Feature works but test failed" → NO. Test failed. Release FAILS.

**Never rationalize failures.** Fix the issue before proceeding.

## Release Workflow Context

This skill is step 3 of the release process:

1. **release-branch** → creates release branch, triggers CI build
2. **CI builds packages** → 2-4 hours to complete
3. **release-testing** (this skill) → verify packages work on all platforms
4. **release-publish** → publish to NuGet.org, tag, finalize

## Step 1: Get Package Versions

**IMPORTANT:** SkiaSharp and HarfBuzzSharp have DIFFERENT version numbers. You must ask the user for both.

- **SkiaSharp** uses: `{major}.{minor}.{patch}[-preview.N.B]` (e.g., `3.119.2-preview.2.2`)
- **HarfBuzzSharp** uses: `{harfbuzz-major}.{harfbuzz-minor}.{harfbuzz-patch}.{N}[-preview.N.B]` (e.g., `8.3.1.3-preview.2.2`)

The first 3 digits of HarfBuzzSharp match the native HarfBuzz version. The 4th digit increments with each SkiaSharp release. The preview/stable suffix matches SkiaSharp.

**Always ask:** "What are the SkiaSharp and HarfBuzzSharp versions for this release?"

### Preview Releases

Preview packages auto-publish to the preview feed. Verify they're available:

```bash
# Note: nuget list only shows latest, --AllVersions returns too many results
# Use dotnet package search with --exact-match instead
dotnet package search SkiaSharp --source "https://aka.ms/skiasharp-eap/index.json" --prerelease --exact-match | grep {version}
dotnet package search HarfBuzzSharp --source "https://aka.ms/skiasharp-eap/index.json" --prerelease --exact-match | grep {harfbuzz-version}
```

The test project's `nuget.config` already includes this feed—tests will use it automatically.

### Stable Releases

Stable packages are NOT published until after testing. Two options:

**Quick validation (using "stable preview" packages)**

CI publishes a prerelease version with `-stable.N` suffix (e.g., `3.119.0-stable.1`) to the preview feed:

```bash
dotnet package search SkiaSharp --source "https://aka.ms/skiasharp-eap/index.json" | grep stable
```

**Final validation (using local artifacts) — recommended**

1. Ask user to download CI artifacts to a local folder (e.g., `/Users/username/skiasharp-artifacts/`)
2. Clear NuGet cache or use isolated packages folder:

```bash
# Option 1: Clear specific packages from cache
dotnet nuget locals all --list
rm -rf ~/.nuget/packages/skiasharp*
rm -rf ~/.nuget/packages/harfbuzzsharp*

# Option 2: Use isolated packages folder (recommended)
export NUGET_PACKAGES=$(pwd)/packages
```

3. Create nuget.config with local source (use absolute path, not ~):

```bash
# Ask user for the absolute path to artifacts folder
cat > /tmp/nuget.config << EOF
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="LocalArtifacts" value="/Users/username/skiasharp-artifacts/" />
    <add key="dotnet-public" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/index.json" />
  </packageSources>
</configuration>
EOF
```

4. Run tests with custom config:

```bash
dotnet test --configfile /tmp/nuget.config -p:SkiaSharpVersion=X.Y.Z -p:HarfBuzzSharpVersion=X.Y.Z.N
```

## Step 2: Run Integration Tests

```bash
cd tests/SkiaSharp.Tests.Integration

# Run all tests
dotnet test -p:SkiaSharpVersion=X.Y.Z -p:HarfBuzzSharpVersion=X.Y.Z.N

# Run specific platform
dotnet test --filter "FullyQualifiedName~MauiiOSTests" -p:SkiaSharpVersion=X.Y.Z -p:HarfBuzzSharpVersion=X.Y.Z.N
dotnet test --filter "FullyQualifiedName~MauiMacCatalystTests" -p:SkiaSharpVersion=X.Y.Z -p:HarfBuzzSharpVersion=X.Y.Z.N
dotnet test --filter "FullyQualifiedName~BlazorTests" -p:SkiaSharpVersion=X.Y.Z -p:HarfBuzzSharpVersion=X.Y.Z.N
dotnet test --filter "FullyQualifiedName~ConsoleTests" -p:SkiaSharpVersion=X.Y.Z -p:HarfBuzzSharpVersion=X.Y.Z.N
```

## Device Selection

Discover and specify devices:

```bash
# iOS simulators
xcrun simctl list devices available | grep -i "iphone\|ipad"
xcrun simctl list runtimes | grep -i ios

# Android emulators
emulator -list-avds
```

Run with device parameters:
```bash
# iOS
dotnet test --filter "MauiiOSTests" -p:SkiaSharpVersion=X.Y.Z -p:HarfBuzzSharpVersion=X.Y.Z.N \
  -p:iOSDevice="iPad Pro 13-inch (M4)" -p:iOSVersion="18.5"

# Android  
dotnet test --filter "MauiAndroidTests" -p:SkiaSharpVersion=X.Y.Z -p:HarfBuzzSharpVersion=X.Y.Z.N \
  -p:AndroidDevice="Pixel 8" -p:AndroidVersion="14"
```

**Defaults:** iOS=iPhone 16 Pro/18.5, Android=running emulator

## Test Matrix

| Test | Platform | Time | Notes |
|------|----------|------|-------|
| SmokeTests | All | ~2s | Native library loading |
| ConsoleTests | All | ~20s | SkiaSharp + HarfBuzzSharp |
| BlazorTests | All | ~2min | WASM with Playwright |
| MauiiOSTests | macOS only | ~2min | iOS Simulator |
| MauiMacCatalystTests | macOS only | ~2min | Mac Catalyst |
| MauiAndroidTests | All | ~2min | Emulator required |
| MauiWindowsTests | Windows only | ~2min | WinAppDriver |

## Output Files

Screenshots saved to `output/logs/testlogs/integration/`:

| File | Purpose |
|------|---------|
| `*-full.png` | Full screenshot before cropping |
| `*-region.png` | Screenshot with red crop rectangle |
| `*.png` | Cropped canvas screenshot |
| `*-diff.png` | Difference mask (white=diff, black=match) |
| `*-pagesource.xml` | UI tree for debugging |

## Release Criteria

Proceed to **release-publish** ONLY when:

1. ✅ ALL tests pass (no failures, no skips except hardware)
2. ✅ Screenshots exist in output directory
3. ✅ Image similarity ≥ 95% for all comparisons

**Valid skips (hardware only):**
- iOS/Mac tests on non-macOS
- Windows tests on non-Windows
- Android tests without running emulator

## Stable Release Checklist (Stable only)

For stable releases, also verify in CI artifacts:

- [ ] NuGet packages are produced
- [ ] Native assets are 4-6MB per binary
- [ ] All assemblies are strong named
- [ ] NuGet metadata is correct (tags, icons, licenses)
- [ ] Samples build and deploy in Release mode
- [ ] Documentation has no "To be added." placeholders

## References

- **Setup problems?** See [references/setup.md](references/setup.md)
- **Test failures?** See [references/troubleshooting.md](references/troubleshooting.md)
