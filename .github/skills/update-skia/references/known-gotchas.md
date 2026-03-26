# Known Gotchas & Troubleshooting

Hard-won findings from past Skia milestone updates. Check these proactively — they will save hours of debugging.

## 1. `DEF_STRUCT_MAP` vs Type Aliases

When upstream changes a C++ type from a `struct` to a `using` alias (e.g., `GrVkYcbcrConversionInfo` → `using VulkanYcbcrConversionInfo`), the `DEF_STRUCT_MAP` macro in `sk_types_priv.h` forward-declares `struct X`, which conflicts with the alias. Fix by switching to `DEF_MAP_WITH_NS(namespace, ActualType, CType)` and wrapping in the appropriate platform guard (e.g., `#if SK_VULKAN`).

## 2. `git-sync-deps` emsdk Failure

Upstream m121+ added an `activate-emsdk` call in `tools/git-sync-deps`. Since SkiaSharp comments out emsdk in DEPS, this call fails. Set the environment variable `GIT_SYNC_DEPS_SKIP_EMSDK=1` in `scripts/cake/native-shared.cake` to prevent build failures during dependency sync.

## 3. `BUILD.gn` Legacy Flags

Upstream may introduce defines like `SK_DEFAULT_TYPEFACE_IS_EMPTY` and `SK_DISABLE_LEGACY_DEFAULT_TYPEFACE` that break SkiaSharp's C API, which still relies on legacy typeface/fontmgr APIs. Comment these defines out in `BUILD.gn` when they cause compilation errors in the C API shim layer.

## 4. Custom Patches May Partially Survive Merges

SkiaSharp adds custom methods to upstream headers (e.g., `SkTypeface::RefDefault()`, `SkTypeface::UniqueID()`, `SkFontMgr::MakeDefault()`). After an upstream merge, implementations in `.cpp` files may survive but header declarations can be silently removed by upstream changes. Always verify that header declarations in `include/` still match the implementations in `src/`.

## 5. Version Compatibility Errors Mean You Missed a Step

If you get `InvalidOperationException: The version of the native libSkiaSharp library (X) is incompatible`, this means the native milestone and C# expected milestone don't match. This is always caused by an incomplete Phase 6 (VERSIONS.txt not fully updated) or a stale build. Go back and fix the root cause — do NOT work around it with `--no-incremental` or by manually copying native libraries.

## 6. Test Runner

Tests use runtime `Skip.If()` calls to self-skip on unsupported platforms. Run all tests
with `dotnet test tests/SkiaSharp.Tests.Console.sln` — this runs core, Vulkan, and Direct3D
test projects. Backend-specific tests self-skip when hardware isn't available. CI handles
WASM, Android, and iOS testing separately.

## 7. HarfBuzz Binding Generation Failures

HarfBuzz generated bindings may fail due to system header issues (`inttypes.h` not found). This is independent of SkiaSharp bindings — if it happens, restore the file from git with `git checkout -- binding/HarfBuzzSharp/HarfBuzzApi.generated.cs` and continue. SkiaSharp bindings generate independently.

## 8. New C API Functions From Upstream

Upstream may add new C API functions (e.g., `sk_surface_draw_with_sampling`) that weren't in the previous milestone. The regeneration step (`pwsh ./utils/generate.ps1`) picks these up automatically. Always review the diff of `*.generated.cs` files for new functions that may need corresponding C# wrappers in `binding/SkiaSharp/`.

## 9. DEPS: Fork-Customized Dependencies

SkiaSharp's fork often has **newer** dependency versions than upstream Skia (from custom security/bug-fix updates via the `native-dependency-update` skill). When merging upstream and resolving DEPS, do NOT blindly update all hashes to the upstream milestone's versions — you may **downgrade** dependencies and break the build.

**Check the skiasharp branch commit log** for custom dependency updates:
```bash
git log --oneline skiasharp | grep -i "update\|bump\|libpng\|zlib\|expat\|brotli\|webp\|harfbuzz\|vulkan"
```

For each dep with a custom update, **keep the fork's hash**. Only update deps that the fork hasn't customized. Common fork-customized deps: libwebp, brotli, expat, libpng, zlib, vulkanmemoryallocator, **harfbuzz**.

> ⚠️ **HarfBuzz is ALWAYS a fork-customized dep.** HarfBuzz updates require hand-written C# delegate
> proxies and must be done as a separate task via the `native-dependency-update` skill. During a Skia
> milestone update, ALWAYS keep the fork's harfbuzz hash in DEPS and ALWAYS revert any changes to
> `binding/HarfBuzzSharp/HarfBuzzApi.generated.cs`.

**Symptoms of getting this wrong**: Build failures referencing missing source files (e.g., `palette.c` in libwebp), or HarfBuzz generated binding errors from a version mismatch.

## 10. HarfBuzz Generated Bindings — ALWAYS Revert

HarfBuzz updates are **always separate** from Skia milestone updates. When the harfbuzz DEPS version changes (even accidentally during a merge), the code generator picks up new APIs (paint/draw/colorline callbacks) that require hand-written delegate proxy implementations in `binding/HarfBuzzSharp/DelegateProxies.*.cs`. This causes `CS8795` errors for missing partial method implementations.

**During a Skia milestone update, ALWAYS:**
1. Keep the fork's harfbuzz hash in DEPS (do not accept upstream's version)
2. Revert any generated HarfBuzz binding changes: `git checkout HEAD -- binding/HarfBuzzSharp/HarfBuzzApi.generated.cs`

HarfBuzz version bumps should be done separately via the `native-dependency-update` skill, which includes writing the required delegate proxies.

---

## Troubleshooting

| Error | Cause | Fix |
|-------|-------|-----|
| `EntryPointNotFoundException` | Native lib not rebuilt after C API change | `dotnet cake --target=externals-{platform}` |
| `error CS0246` missing type | Binding not regenerated | `pwsh ./utils/generate.ps1` |
| Merge conflict in DEPS | Both forks updated deps independently | Keep our DEPS pins, accept upstream structure |
| `LNK2001 unresolved external` | C function name mismatch | Verify C API function names match exactly |
| Build fails after merge | Missing `#include` for moved headers | Check upstream header relocation notes |
