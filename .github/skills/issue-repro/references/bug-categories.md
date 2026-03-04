# Bug Categories

Identification signals and reproduction strategies by bug category. For platform-specific
instructions (how to create/build/run), see the `platform-*.md` files in this directory.

## Contents
1. [C# API Bugs](#1-c-api-bugs)
2. [Native Loading / P/Invoke](#2-native-loading--pinvoke)
3. [Rendering / Visual Bugs](#3-rendering--visual-bugs)
4. [Platform-Specific Bugs](#4-platform-specific-bugs)
5. [Build / Deployment Bugs](#5-build--deployment-bugs)
6. [Memory / Disposal Bugs](#6-memory--disposal-bugs)
7. [Enhancement / Feature Request](#7-enhancement--feature-request)
8. [Platform Parity Gap](#8-platform-parity-gap)
9. [Documentation Issue](#9-documentation-issue)
10. [General Tips](#general-tips)

**Constraints applying to ALL categories:**
- No native rebuilds — use `dotnet cake --target=externals-download` only
- Output limits: 2KB per step, 4KB for failure steps
- Binary assets: reference by URL or filename, never inline
- For platform-specific setup: see `platform-console.md`, `platform-docker-linux.md`, `platform-wasm-blazor.md`, etc.

---

## 1. C# API Bugs

**Identification signals:** Wrong return value, incorrect calculation, missing null check, `ArgumentException`, unexpected behavior from a documented method, "returns X but should return Y".

### Strategy

1. **Isolate the API call.** Create a **standalone console project** using the reporter's NuGet version:
   ```bash
   dotnet new console -n Repro && cd Repro
   dotnet add package SkiaSharp --version {reporter_version}
   ```
2. **Write reproduction code** that calls the suspect method with exact inputs from the issue.
3. **Run and observe:**
   ```bash
   dotnet run
   ```
4. **Compare actual vs expected.** Print values, assert conditions, make the failure obvious.
5. **Test edge cases.** Try zero, negative, null, empty, and boundary inputs.
6. **If reproduced, test latest release:**
   ```bash
   dotnet add package SkiaSharp --version {latest_stable}
   dotnet run
   ```

### Example: SKMatrix.MapRect (#2997)

```csharp
[SkippableFact]
public void MapRectShouldNotNormalize()
{
    var matrix = SKMatrix.CreateScale(1, -1);
    var source = new SKRect(0, 0, 100, 50);
    var mapped = matrix.MapRect(source);
    // Bug: output was normalized (top < bottom enforced)
    // Expected: top=0, bottom=-50 (preserving flip)
    Assert.Equal(0, mapped.Top);
    Assert.Equal(-50, mapped.Bottom);
}
```

### Pitfalls
- Some SKRect/SKRectI methods normalize (enforce left < right, top < bottom) — this may be the bug itself, not a test error.
- `float` precision: use `Assert.Equal(expected, actual, precision: 4)` for floating-point comparisons.
- Ensure you test with the **same SkiaSharp version** as the issue reporter.

### Only bail when
- The API doesn't exist in the current codebase (removed or not yet added).
- The issue describes behavior in a fork or custom build.

---

## 2. Native Loading / P/Invoke

**Identification signals:** `DllNotFoundException`, `EntryPointNotFoundException`, "libSkiaSharp not found", "unable to load shared library", deployment or packaging errors.

### Strategy

1. **Check local loading first:**
   ```csharp
   Console.WriteLine($"Arch: {RuntimeInformation.ProcessArchitecture}");
   Console.WriteLine($"OS: {RuntimeInformation.OSDescription}");
   using var bitmap = new SKBitmap(1, 1);
   Console.WriteLine("Native load succeeded");
   ```
2. **Verify NativeAssets package.** Check the issue's `.csproj` for correct `PackageReference`. Common mistakes: wrong `NativeAssets.Linux` vs `NoDependencies` variant, missing NativeAssets entirely, version mismatch between managed and native packages.
3. **Check deployed library paths:** `find . -name "libSkiaSharp*" -o -name "SkiaSharp.dll" 2>/dev/null`
4. **Docker for Linux (if available).** See [platform-docker-linux.md](platform-docker-linux.md).

### Pitfalls
- Only the **first** error line in a `DllNotFoundException` stack matters. Subsequent "file not found" lines are fallback noise.
- `NoDependencies` has zero external deps. Any dependency error (e.g., `libfontconfig`) proves the wrong binary was loaded.
- `EntryPointNotFoundException` means the binary exists but is the wrong version — not a loading failure.

### Only bail when
- The issue requires a platform you can't access (e.g., Windows ARM64, iOS device) and Docker can't simulate it.
- The issue is specific to a deployment environment (Azure Container Apps, AWS Lambda) you can't replicate.

### macOS limitation
Docker on macOS uses a VM, which introduces differences from bare-metal Linux. Native loading issues that depend on exact glibc versions or kernel features may not reproduce. Document this as a limitation if Docker results are inconclusive.

---

## 3. Rendering / Visual Bugs

**Identification signals:** Wrong colors, missing content, garbled output, incorrect dimensions, "image looks wrong", blank output, clipping issues, antialiasing artifacts.

### Strategy

1. **Create a surface, draw, and save to PNG:**
   ```csharp
   using var bitmap = new SKBitmap(width, height);
   using var canvas = new SKCanvas(bitmap);
   canvas.Clear(SKColors.White);
   // Reproduce drawing operations from the issue
   using var image = SKImage.FromBitmap(bitmap);
   using var data = image.Encode(SKEncodedImageFormat.Png, 100);
   using var stream = File.OpenWrite("output.png");
   data.SaveTo(stream);
   ```
2. **Describe the visual result.** No automated pixel comparison exists. Describe colors, dimensions, presence/absence of content, and how it differs from expected.
3. **Test with known-good inputs.** Use test assets from `PathToImages` or `PathToFonts` as baselines.

### Key: Process may exit 0 but output is wrong
Rendering bugs rarely throw exceptions. The process succeeds, but the output is incorrect. Always inspect the saved image. Use conclusion `reproduced` with step result `wrong-output` when the rendering is incorrect.

### Pitfalls
- Color space matters. `SKColorType.Rgba8888` vs `Bgra8888` will swap red/blue channels.
- `SKBitmap` default color type is platform-dependent. Explicitly specify color type when reproducing.
- Font rendering varies by platform (different font fallbacks, hinting). A font bug on Windows may look different on macOS.
- DPI/scale: `SKCanvas` doesn't auto-scale. If the issue mentions high-DPI, check for missing scale transforms.

### Only bail when
- The issue requires a specific font file that isn't provided and can't be substituted.
- The issue involves GPU rendering (`GRContext`, `SKGLView`) and no GPU is available.

---

## 4. Platform-Specific Bugs

**Identification signals:** "works on Windows but not Linux", "macOS-only crash", "fails on ARM64", "Android emulator", platform name in title, RID-specific issues.

### Strategy

1. **Test on current platform first.** Document the result even if it works — "works on macOS ARM64" is useful data.
2. **Docker for Linux variants.** See [platform-docker-linux.md](platform-docker-linux.md). Use `--platform linux/amd64` or `linux/arm64`. For Alpine, use `sdk:8.0-alpine` with `sh` (not `bash`). **Always install `libfontconfig1`** (Debian) or `fontconfig` (Alpine) first.
3. **SkiaSharp 1.68.x on Apple Silicon?** Use `--platform linux/amd64` — there are no arm64 natives for 1.x.
4. **Record platform matrix:**
   | Platform | Result | Notes |
   |----------|--------|-------|
   | macOS ARM64 | ✅ Pass | Host machine |
   | Linux x64 (Docker) | ❌ Fail | Segfault on load |
   | Windows x64 | ⬜ Untested | No access |

### Pitfalls
- Docker on macOS uses QEMU for non-native architectures — note emulated vs native in results.
- macOS/iOS/Mac Catalyst cannot be tested in Docker.

### Only bail when
- The bug requires a platform you can't access and Docker can't simulate (iOS, Windows, Android device).
- Conclude `needs-platform` with a clear statement of which platform is needed and why.

---

## 5. Build / Deployment Bugs

**Identification signals:** NuGet restore failures, TFM incompatibility, MSBuild errors, "project won't build", publish errors, trimming issues, AOT failures, compiler errors (CSnnnn).

### Strategy

1. **Create a minimal project from scratch** — don't modify existing test projects:
   ```bash
   mkdir -p /tmp/skiasharp/repro/{number} && cd /tmp/skiasharp/repro/{number}
   dotnet new console --framework net8.0
   dotnet add package SkiaSharp --version <version-from-issue>
   ```
2. **Match the reporter's configuration.** Copy their `.csproj` settings exactly: TFM, RuntimeIdentifier, PublishTrimmed, PublishAot, SelfContained.
3. **Reproduce the exact build step that fails** (`dotnet build`, `dotnet publish -r linux-x64`, etc.).
   - **Crucial:** If `dotnet build` fails with the *same errors* as the reporter, this is a successful reproduction (`reproduced`).
   - Mark the step as `result: "failure"` (because the command exited with error).
   - **Do NOT mark it `success` just because "it failed as expected".**
   - Do NOT mark it `not-reproduced` just because the build error looks correct/intentional.
   - If it looks like an intentional API rename/breaking change, record that in `notes`, but keep `conclusion: "reproduced"`.
4. **Check output directory:** `find bin/ -name "libSkiaSharp*" -o -name "libHarfBuzzSharp*" 2>/dev/null`

### Sub-pattern: API migration / version upgrade errors

When the reporter upgrades SkiaSharp (e.g. 2.x → 3.x) and gets compiler errors (CS0117, CS1501, CS0246):

1. **Use the OLD API calls the reporter used** — that's the reproduction. Do NOT substitute the new API names.
2. If `dotnet build` fails with the same errors → `conclusion: "reproduced"`, step `result: "failure"`.
3. Put "this is an intentional breaking change / API rename" in `notes`, NOT in `conclusion`.
4. Optionally add a second step showing the new API works — but that doesn't change the conclusion.

**⚠️ This is the #1 misclassification risk.** The temptation is strong to say "not a bug, so not reproduced." Resist it — reproduction is about whether the reported behavior occurred, not whether it's a defect.

### Pitfalls
- `dotnet restore` caches aggressively — use `--no-cache` if the issue involves version confusion.
- Trimming can remove P/Invoke targets — test with `PublishTrimmed` if mentioned.
- NativeAssets package must be referenced from the **application** project, not just a library.

### Only bail when
- The issue requires a specific SDK version you don't have and can't install.
- The issue is about a third-party build system (Unity, Uno Platform) with proprietary tooling.

---

## 6. Memory / Disposal Bugs

**Identification signals:** `AccessViolationException`, `ObjectDisposedException`, segfault, "hard crash", use-after-free, memory leak, "disposed object", double dispose.

### Strategy

1. **Test disposal ordering** — the most common cause:
   ```csharp
   var surface = SKSurface.Create(new SKImageInfo(100, 100));
   var canvas = surface.Canvas;
   surface.Dispose();
   canvas.DrawRect(0, 0, 50, 50, new SKPaint()); // Likely crash — canvas owned by surface
   ```
2. **Test Handle access after Dispose:**
   ```csharp
   var paint = new SKPaint();
   paint.Dispose();
   try { _ = paint.Handle; }
   catch (ObjectDisposedException) { Console.WriteLine("Correctly threw"); }
   ```
3. **Test same-instance returns.** `Subset()`, `ToRasterImage()`, `ToRasterImage(false)` may return the same object — disposing it kills the source. Always check `result != source` before disposing.
4. **Stress test for leaks:**
   ```csharp
   for (int i = 0; i < 10000; i++)
   {
       using var bmp = new SKBitmap(100, 100);
       using var cnv = new SKCanvas(bmp);
       cnv.Clear(SKColors.Red);
   }
   Console.WriteLine($"Memory: {GC.GetTotalMemory(true) / 1024}KB");
   ```

### Pitfalls
- `AccessViolationException` crashes the process — isolate risky operations in a separate process if possible.
- Memory leaks may not manifest in short runs — use tight loops of 10K+ iterations.
- Threading bugs cause intermittent crashes — test with concurrent access if the issue mentions "random crash".

### Only bail when
- The crash requires specific native memory state that can't be induced from C#.
- The issue describes a leak observable only via external profiling tools you don't have.

---

## General Tips

- Print only what's needed to confirm or deny the bug.
- Always include SkiaSharp version and runtime info in output.
- Save artifacts (PNGs, crash logs) to files — reference by filename, don't inline.
- Test helpers available in repo: `PathToImages`, `PathToFonts`, `IsWindows`/`IsMac`/`IsLinux`.

---

## 7. Enhancement / Feature Request

Not a bug — the issue requests new functionality that doesn't exist yet.

### Signals
- Triage JSON has `type/enhancement` or `type/feature-request`
- Issue title contains "add", "expose", "support", "new method", "feature request"
- No error, crash, or incorrect behavior reported — issue describes desired behavior

### Strategy
1. **Create a project that demonstrates the gap** — build a minimal project that attempts to use the requested feature. This may compile but not work, or may not compile at all. Either outcome is valid evidence.
2. **Document what exists** — note related infrastructure that's already in place (e.g., shared enums, sibling platform implementations).
3. **Test across versions** — the feature may exist in one version but not another, or may have been removed. Test reporter's version AND latest stable, same as bugs.
4. **Look for workarounds** — the feature may be achievable through alternative APIs or approaches. Document any workarounds found.

### Conclusion
- `confirmed` — the requested feature is verified as missing (reporter's claim is correct).
- `not-confirmed` — the feature actually exists (reporter missed it, or it's available under a different name/API).
- `assessment: "feature-request"`
- Include code investigation steps showing the feature is absent or present.

### Example
Issue: "Add wheel support to GTK3 views"
- Step 1: Create a GTK3 console project with SkiaSharp.Views.Gtk3 → project builds
- Step 2: grep for `WheelEvent` or `ScrollEvent` in GTK3 view → not found
- Step 3: confirm `SKTouchAction.WheelChanged` enum exists (shared infra) but GTK3 `SKDrawingArea` doesn't subscribe to it
- Step 4: Test on latest stable → same gap
- Conclusion: `confirmed`, assessment: `feature-request`, scope: `platform-specific/linux`

---

## 8. Platform Parity Gap

A cross-platform API exists and works on some platforms but not others. This is a **bug** (broken API contract), not an enhancement.

### Signals
- API surface exists (enum, property, interface) across all platforms
- At least one platform fully implements the feature
- Another platform silently drops or ignores the functionality
- Issue says "X works on Windows but not on Mac" or "events not delivered on platform Y"

### Strategy
1. **Verify the API surface exists** — find the shared enum/property/interface.
2. **Find a working platform implementation** — grep for the feature in platform handlers.
3. **Confirm the broken platform lacks it** — grep the specific platform handler for the missing code.
4. **Test on the broken platform** if accessible — run a minimal app to confirm silence.

### Conclusion
- `reproduced` — the API is non-functional on the reported platform (confirmed broken contract).
- `assessment: "likely-bug"`
- **NOT `confirmed`** — this is a bug in an existing API, not a new feature request.

### Example
Issue: "Mouse wheel events not delivered on Mac Catalyst"
- Step 1: find `SKTouchAction.WheelChanged` in shared code → exists
- Step 2: find `OnPointerWheelChanged` in Windows handler → fully implemented
- Step 3: grep Apple handler for wheel/scroll → not found
- Conclusion: `reproduced`, assessment: `likely-bug` (cross-platform API silently non-functional)

---

## 9. Documentation Issue

The issue reports missing, incorrect, or outdated documentation.

### Signals
- Triage JSON has `type/documentation`
- Issue mentions "docs", "documentation", "API reference", "missing example"
- No code bug — the library works correctly but documentation is wrong or absent

### Strategy
1. **Verify the doc gap** — check if the referenced documentation page/section exists and is accurate.
2. **Compare with source code** — confirm whether the API behavior matches or contradicts the docs.
3. **Test across versions** — documentation accuracy may vary by version. Test reporter's version AND latest stable to determine if docs were once correct but are now outdated.

### Conclusion
- `confirmed` — docs ARE missing or wrong (reporter's claim is correct).
- `not-confirmed` — docs are actually correct (reporter misread or looked in wrong place).
- `assessment: "docs-gap"`
- Include investigation steps showing the doc gap or correctness. Use `layer: "investigation"`.

### Example
Issue: "SKPaint.FilterQuality docs say it affects image scaling but it's deprecated"
- Step 1: check API reference for SKPaint.FilterQuality → still documented as active
- Step 2: check source code → marked `[Obsolete]` since v3.x
- Conclusion: `confirmed`, assessment: `docs-gap`
