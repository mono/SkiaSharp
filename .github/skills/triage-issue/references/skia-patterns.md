# SkiaSharp Patterns

AI-specific domain knowledge for triage. Contains heuristics, platform quirks, and common traps that can't be inferred from general .NET knowledge.

For factual troubleshooting content (dlopen modes, container checklist), see `documentation/packages.md#troubleshooting-native-loading`.

## Native Loading Heuristics

### First Error is Diagnostic

In a `DllNotFoundException` stack, the .NET runtime reports ALL fallback paths. Only the **first** error line matters — subsequent lines are "file not found" noise from fallback paths.

### "Wrong Binary" Pattern

When user claims `NoDependencies` but error shows `libfontconfig.so.1`:

- ❌ **WRONG conclusion:** "Fontconfig error is a red herring / loader noise"
- ✅ **RIGHT conclusion:** The loaded binary is NOT from `NoDependencies` — deployment is broken

`NoDependencies` has **zero** external deps. Any dependency error PROVES a different binary was loaded. This is the #1 AI misdiagnosis — never dismiss dependency errors when `NoDependencies` is referenced.

### Issue Lessons

| Issue | Pattern | Learning |
|-------|---------|----------|
| #3386 | NoDependencies + fontconfig error | .NET Aspire / Azure Container Apps deployed wrong binary despite correct PackageReference. Workaround: switch to `NativeAssets.Linux` + install fontconfig. |

## Platform Quirks

### Linux
- **fontconfig vs NoDependencies** — Two fundamentally different binaries. Mixing them (transitive refs) is a common deployment failure.
- **Alpine needs musl** — `linux-musl-*` RIDs. `NoDependencies` includes both glibc and musl. `NativeAssets.Linux` also includes both.
- **Diagnostic command:** `ldd libSkiaSharp.so` reveals which variant is deployed. NoDependencies shows only libc/libm/libpthread/libdl.

### Windows
- **ARM64 + VC++ Redistributable** — Published apps on ARM64 Windows may need VC++ Redistributable installed. Self-contained publish doesn't always include it.
- **NanoServer** — Requires `SkiaSharp.NativeAssets.NanoServer`, not Win32. NanoServer has a minimal API surface.

### macOS / iOS / Mac Catalyst
- **Universal binaries** — macOS uses a fat binary (Intel + Apple Silicon). If user reports "wrong architecture" issues, the fat binary should handle both.
- **Mac Catalyst** — Uses a separate NativeAssets package (`NativeAssets.MacCatalyst`), not macOS.

### WebAssembly
- **Static linking, not P/Invoke** — WASM uses `.a` files linked into `dotnet.wasm` at build time via `NativeFileReference`. There is no `.so` or `.dll` to deploy.
- **Unity WebGL** — Uses Emscripten like Blazor WASM but doesn't go through MSBuild. Unity-specific integration requires manual configuration. SkiaSharp doesn't officially support Unity WebGL.
- **Threading** — WASM packages include single-threaded (st) and multi-threaded (mt) variants.

### Android
- **GPU availability** — Serverless/CI environments may not have GPU drivers. `SKGLView` will fail; use `SKCanvasView` (CPU) as fallback.
- **Architecture matrix** — x86, x64, arm, arm64 all included. Emulator typically uses x64.

## Common Traps

### "It works locally but fails in container/CI"
Almost always a deployment issue: wrong NativeAssets package, missing native binary in output, or RID mismatch. Check `documentation/packages.md#container-deployment-checklist`.

### "DllNotFoundException after publishing"
Self-contained publish should include native binaries. If not: NativeAssets package is in a library project (not the executable), or trimming removed it. Direct `PackageReference` in the application project fixes this.

### "EntryPointNotFoundException"
The native binary exists but doesn't contain the expected function. Usually means: mismatched SkiaSharp managed + native versions, or (for contributors) native library not rebuilt after C API changes.

### "AccessViolationException" / hard crash
Memory management bug in the C# wrapper. Common causes: disposing an object that's still referenced by native code, using a disposed object, or threading violation (sharing SKCanvas/SKPaint between threads).

### Premature Disposal
Some methods return the **same instance** they received. Disposing the result also disposes the input. Always check `result != source` before disposing. Methods that may return same instance: `Subset()`, `ToRasterImage()`, `ToRasterImage(false)`.
