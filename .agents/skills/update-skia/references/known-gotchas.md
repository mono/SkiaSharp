# Known Gotchas & Troubleshooting

Hard-won findings from past Skia milestone updates. Check these proactively — they will save hours of debugging.

## C++ ↔ C API Layer

### 1. `DEF_STRUCT_MAP` vs Type Aliases

When upstream changes a C++ type from a `struct` to a `using` alias (e.g., `GrVkYcbcrConversionInfo` → `using VulkanYcbcrConversionInfo`), the `DEF_STRUCT_MAP` macro in `sk_types_priv.h` forward-declares `struct X`, which conflicts with the alias. Fix by switching to `DEF_MAP_WITH_NS(namespace, ActualType, CType)` and wrapping in the appropriate platform guard (e.g., `#if SK_VULKAN`).

### 2. Struct Size `static_assert` Failures

`sk_structs.cpp` asserts `sizeof(our_c_type) == sizeof(CppType)` for every struct mapped via `reinterpret_cast`. When upstream adds fields (e.g., `SkPngEncoder::Options` gaining gainmap pointers in m133), the assert fires. During analysis, proactively check every asserted struct against the target milestone — don't wait for the build to catch it.

### 3. Custom Patches May Partially Survive Merges

SkiaSharp adds custom methods to upstream headers (e.g., `SkTypeface::RefDefault()`). After merge, implementations in `.cpp` files may survive but header declarations can be silently removed by upstream changes.

When header declarations are lost:
1. **Check if upstream provides a replacement API** — if so, update the C API to use it
2. **If no replacement exists**, re-add the declaration. But consider moving custom methods out of upstream headers and into our C API layer (`src/c/`) to avoid this recurring conflict
3. **Never leave a mismatched header/implementation** — it compiles but crashes at runtime on some platforms

### 4. Fontconfig `#ifdef` Guards

Platform font manager calls (e.g., `SkFontMgr_New_FontConfig`) must be guarded by feature-availability macros (`SK_FONTMGR_FONTCONFIG_AVAILABLE`), not platform macros (`SK_BUILD_FOR_UNIX`). When `skia_use_fontconfig=false`, platform macros leave the symbol unresolved. The `-Wl,--no-undefined` linker flag catches this at link time.

## Build System

### 5. `git-sync-deps` emsdk Failure

Upstream m121+ added an `activate-emsdk` call in `tools/git-sync-deps`. Since SkiaSharp comments out emsdk in DEPS, this call fails. Set `GIT_SYNC_DEPS_SKIP_EMSDK=1` in `scripts/cake/native-shared.cake`.

### 6. `BUILD.gn` Legacy Flags

Upstream progressively deprecates legacy APIs behind flags (e.g., `SK_DEFAULT_TYPEFACE_IS_EMPTY`, `SK_DISABLE_LEGACY_DEFAULT_TYPEFACE`). When these flags break SkiaSharp's C API:

1. **Check what the flag removes** — read the upstream commit that added it
2. **If the C API uses the removed behavior**, update the C API to use the replacement API. This is the real fix — upstream is signaling that the old API will be removed entirely in a future milestone.
3. **Only as a short-term bridge** (with a TODO comment and tracking issue), you may comment out the flag to unblock the build while you work on the proper fix. Never leave a commented-out flag without a plan to address it.

Also watch for renamed/removed GN flags between milestones — obsolete flags cause `Unknown GN flag` errors. Always diff the target `BUILD.gn` against the current one.

### 7. `.gitmodules` Branch Name

When the mono/skia target branch name changes, `.gitmodules` must be updated to track the new branch. Easy to forget; causes silent submodule tracking failures.

## Dependencies & Bindings

### 8. DEPS: Fork-Customized Dependencies

SkiaSharp's fork often has **newer** dependency versions than upstream. When resolving DEPS conflicts, do NOT blindly take upstream's hashes — you may downgrade and break the build.

```bash
git log --oneline skiasharp | grep -i "update\|bump\|libpng\|zlib\|expat\|brotli\|webp\|harfbuzz\|vulkan"
```

Keep fork's hash for customized deps. Common ones: libwebp, brotli, expat, libpng, zlib, vulkanmemoryallocator, **harfbuzz**.

### 9. HarfBuzz — ALWAYS Separate

HarfBuzz updates require hand-written C# delegate proxies and must be done via the `native-dependency-update` skill. During a milestone update, ALWAYS:
1. Keep the fork's harfbuzz hash in DEPS
2. Revert any generated HarfBuzz binding changes: `git checkout HEAD -- binding/HarfBuzzSharp/HarfBuzzApi.generated.cs`

### 10. Enum Value Renumbering

When upstream inserts new enum values mid-sequence, ALL subsequent values shift. This affects `sk_enums.cpp`, `Definitions.cs`, `EnumMappings.cs`, and any test hardcoding enum integers. Always regenerate bindings — never hand-edit enum values.

## Diff Reading Traps

### 11. Deleted Files ≠ Deleted Functionality

Skia relocates files, it rarely removes them. Example: `src/utils/SkJSON.h` → `modules/jsonreader/SkJSONReader.h` in m133. Always search the target branch for where content moved before removing references:

```bash
git ls-tree -r upstream/chrome/m{TARGET} --name-only | grep -i "FILENAME_STEM"
```

### 12. Reordered Fields ≠ Removed Fields

A symbol on a diff `-` line may have been moved within the same file, not removed. Always confirm on the target branch:

```bash
git show upstream/chrome/m{TARGET}:FILEPATH | grep "SYMBOL"
```

## Merge Strategy

### 13. Genuine Merge Required

Never use a tree-override merge (`git merge -s ours`, `git read-tree --reset`). This destroys `git blame` attribution for all C API files. Always resolve each conflict individually. Use `git merge --no-commit` for manual control.

### 14. Conflict Resolution by File Category

| File Category | Strategy |
|--------------|----------|
| `BUILD.gn` | **Combine both** — most complex; keep upstream structure AND SkiaSharp's platform flags/targets |
| `DEPS` | **Combine** — keep our dependency pins, accept upstream structure |
| `RELEASE_NOTES.md`, `infra/bots/` | **Take upstream** |
| C API headers (`include/c/`) | **Keep SkiaSharp** — these don't exist upstream |
| C API source (`src/c/`) | **Keep SkiaSharp + adapt** — fix includes and API calls in post-merge commits |
| Other upstream source (`src/`, `include/`) | **Check history first** — see gotcha #15 |

### 15. Never `--theirs` Without Checking File History

**Failure mode**: A merge conflict in an upstream file (outside `src/c/` / `include/c/`) is resolved
with `git checkout --theirs`, silently overwriting an intentional SkiaSharp fork patch.

**Mandatory process for EVERY conflicted file:**

```bash
# BEFORE resolving, check if the fork has intentional patches
git log --oneline skiasharp -- <conflicted-file>
```

- If the log shows fork-specific commits (look for "Restore", "patch", "fix", or any non-merge
  commit), **keep our version** and only absorb upstream's harmless additive changes (new includes).
- If the log shows only merge commits from prior upstream merges, taking `--theirs` is likely safe.
- **Never use `git checkout --theirs` as a shortcut** for files you haven't investigated.

**Key signal words** in commit messages that indicate intentional fork patches:
`Restore`, `patch`, `fix for`, `platform`, `workaround`, `SkiaSharp`, `iOS`, `Tizen`

## Testing

### 16. Version Compatibility Errors

`InvalidOperationException: The version of the native libSkiaSharp library (X) is incompatible` means VERSIONS.txt wasn't fully updated. Fix the root cause — do NOT work around it.

### 17. Pixel Value Precision

Upstream periodically improves color conversion precision, shifting expected pixel values by ±1. When pixel-exact test assertions break, check if upstream changed the conversion and update expected values.

### 18. Test Runner

Tests use `Skip.If()` for unsupported platforms. Run `dotnet test tests/SkiaSharp.Tests.Console.sln` for the full suite. Backend-specific tests self-skip when hardware isn't available.

---

## Troubleshooting

| Error | Cause | Fix |
|-------|-------|-----|
| `EntryPointNotFoundException` | Native lib not rebuilt after C API change | `dotnet cake --target=externals-{platform}` |
| `error CS0246` missing type | Binding not regenerated | `pwsh ./utils/generate.ps1` |
| `static_assert` sizeof failure | Upstream struct gained/lost fields | Update C API struct in `sk_types.h` |
| `#include` file not found | Upstream moved file to new path | Search target branch, update path |
| `LNK2001 unresolved external` | C function name mismatch or missing lib | Verify names; check system library linkage |
| `Unknown GN flag` error | Obsolete build flag | Remove flag; diff target BUILD.gn |
| `git blame` all from merge commit | Tree-override merge was used | Redo as genuine conflict-resolved merge |
| Merge conflict in DEPS | Both forks updated deps | Keep our pins, accept upstream structure |
| Enum values don't match | Mid-sequence insertion | Regenerate bindings — never hand-edit |
| Pixel mismatch by ±1 | Upstream precision change | Update expected test values |
