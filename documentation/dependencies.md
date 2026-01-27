# SkiaSharp Native Dependencies

This document is the **single source of truth** for all native dependencies used by SkiaSharp, including security tracking, version management, and CVE handling.

## Overview

SkiaSharp wraps Google's Skia library, which has numerous third-party dependencies. However, **SkiaSharp only uses a subset** of these dependencies. Many are for Skia's development tools, experimental features, or codecs that SkiaSharp doesn't enable.

### Architecture

```
SkiaSharp repo
└── externals/skia (mono/skia fork, submodule)
    ├── DEPS                    # Pins dependencies to commit hashes
    └── third_party/
        ├── {dep}/BUILD.gn      # Build configuration per dependency
        └── externals/{dep}/    # Source checkout (after sync)
```

**Key files:**
- `externals/skia/DEPS` — Defines which dependencies to fetch and at what commit
- `cgmanifest.json` — Maps dependencies to canonical names for CVE detection
- `native/<platform>/build.cake` — Platform-specific GN flag overrides

Understanding which dependencies are actually used is critical for:
- **Security audits**: Only audit dependencies that are compiled into SkiaSharp
- **Dependency updates**: Prioritize updates for dependencies that affect users
- **Build optimization**: Reduce build times by not syncing unused dependencies

---

## Dependency Categories

### 🔒 Security-Relevant Dependencies

These dependencies process untrusted input (images, fonts, data) and are compiled into SkiaSharp binaries. **Security vulnerabilities in these libraries directly affect SkiaSharp users.**

| Dependency | Purpose | Platforms | CVE Database Name |
|------------|---------|-----------|-------------------|
| **libpng** | PNG image decode/encode | All | libpng |
| **zlib** | Compression (PNG, PDF, fonts) | All | zlib |
| **libjpeg-turbo** | JPEG image decode/encode | All | libjpeg-turbo |
| **libwebp** | WebP image decode/encode | All | libwebp |
| **freetype** | Font rendering | Android, Linux, WASM | freetype |
| **harfbuzz** | Text shaping | All (built, but disabled by default in SkiaSharp) | harfbuzz |
| **expat** | XML parsing (SVG, PDF) | All | libexpat |
| **brotli** | WOFF2 font decompression | All | brotli |
| **wuffs** | GIF image decoding | All | wuffs |
| **dng_sdk** | RAW/DNG image decoding | Windows only | dng_sdk |

### 🎮 GPU/Graphics Dependencies

These are used for GPU-accelerated rendering backends.

| Dependency | Purpose | Platforms | Notes |
|------------|---------|-----------|-------|
| **vulkanmemoryallocator** | Vulkan memory management | Android, Linux, Windows (Vulkan builds) | Used when `skia_use_vulkan=true` |
| **d3d12allocator** | Direct3D 12 memory management | Windows | Used when `skia_use_direct3d=true` |
| **spirv-cross** | SPIR-V shader translation | Vulkan/Metal backends | Required for cross-platform shaders |
| **vulkan-headers** | Vulkan API definitions | Vulkan builds | Headers only, via `include/third_party/vulkan` |

### 📦 Supporting Dependencies

These support the main functionality but don't directly process untrusted input.

| Dependency | Purpose | Platforms | Notes |
|------------|---------|-----------|-------|
| **piex** | RAW image preview extraction | All except Windows, WASM | Helps dng_sdk |
| **buildtools** | Build toolchain (libc++, clang) | All | Required for compilation |

### ❌ Unused Dependencies

These dependencies exist in Skia's DEPS but are **NOT used by SkiaSharp**. They should be commented out to reduce attack surface and sync time.

| Dependency | Why Not Used | Skia Feature Flag |
|------------|--------------|-------------------|
| **abseil-cpp** | Only for Dawn (WebGPU), which is disabled | `skia_use_dawn=false` |
| **egl-registry** | No references in SkiaSharp builds | - |
| **highway** | Only for libjxl, which is disabled | `skia_use_libjxl_decode=false` |
| **imgui** | Only for Skia's viewer tool | `skia_enable_tools=false` |
| **libavif** | AVIF codec disabled | `skia_use_libavif=false` |
| **libgav1** | AV1 decoder, only for libavif | `skia_use_libavif=false` |
| **libgrapheme** | Unicode grapheme library, disabled | `skia_use_libgrapheme=false` |
| **libjxl** | JPEG XL codec disabled | `skia_use_libjxl_decode=false` |
| **libyuv** | YUV conversion, only for libavif | `skia_use_libavif=false` |
| **microhttpd** | Only for skiaserve debug tool | `skia_enable_tools=false` |
| **oboe** | Android audio, only for viewer tool | `skia_enable_tools=false` |
| **opengl-registry** | No references in SkiaSharp builds | - |
| **spirv-headers** | Only for spirv-tools (dev builds) | `is_skia_dev_build=false` |
| **spirv-tools** | Only for SPIR-V validation in dev builds | `is_skia_dev_build=false` |
| **vello** | Experimental GPU renderer, disabled | `skia_enable_vello_shaders=false` |
| **vulkan-deps** | Meta-repo, not directly used | - |
| **vulkan-tools** | Debugging tools only | `skia_enable_tools=false` |
| **vulkan-utility-libraries** | Debugging utilities only | `skia_enable_tools=false` |

---

## Platform-Specific Usage

### macOS / iOS
```
libpng, zlib, libjpeg-turbo, libwebp, expat, brotli, wuffs, piex
+ Metal backend (spirv-cross for shader translation)
```

### Android
```
libpng, zlib, libjpeg-turbo, libwebp, freetype, expat, brotli, wuffs, piex
+ Optional: vulkanmemoryallocator (when Vulkan enabled)
```

### Windows
```
libpng, zlib, libjpeg-turbo, libwebp, expat, brotli, wuffs, dng_sdk, piex
+ Optional: d3d12allocator (when Direct3D enabled)
+ Optional: vulkanmemoryallocator (when Vulkan enabled)
```

### Linux
```
libpng, zlib, libjpeg-turbo, libwebp, freetype, expat, brotli, wuffs, piex
+ Optional: vulkanmemoryallocator (when Vulkan enabled)
```

### WebAssembly (WASM)
```
libpng, zlib, libjpeg-turbo, libwebp, freetype, expat, brotli, wuffs
```

---

## Security & CVE Tracking

### cgmanifest.json

The `cgmanifest.json` file enables Microsoft Component Governance to detect security vulnerabilities.

**The problem:** Skia uses mirrors/forks (chromium.googlesource.com, skia.googlesource.com) but CVE databases track vulnerabilities against **upstream project names** (freetype, libpng, etc.). A `git` component pointing to a mirror URL won't match CVE databases.

**The solution:** Use `type: "other"` with the **canonical upstream name and version**:

```json
{
  "component": {
    "type": "other",
    "other": {
      "name": "libpng",
      "version": "1.6.44",
      "downloadUrl": "https://github.com/glennrp/libpng"
    }
  }
}
```

#### Canonical Name Mapping

| DEPS Name | cgmanifest `name` | Upstream URL |
|-----------|-------------------|--------------|
| libpng | `libpng` | https://github.com/glennrp/libpng |
| zlib | `zlib` | https://github.com/madler/zlib |
| libjpeg-turbo | `libjpeg-turbo` | https://github.com/libjpeg-turbo/libjpeg-turbo |
| libwebp | `libwebp` | https://github.com/webmproject/libwebp |
| freetype | `freetype` | https://gitlab.freedesktop.org/freetype/freetype |
| harfbuzz | `harfbuzz` | https://github.com/harfbuzz/harfbuzz |
| expat | `libexpat` | https://github.com/libexpat/libexpat |
| brotli | `brotli` | https://github.com/google/brotli |
| wuffs | `wuffs` | https://github.com/google/wuffs-mirror-release-c |
| dng_sdk | `dng_sdk` | https://android.googlesource.com/platform/external/dng_sdk |

### CVE Verification Process

> ⚠️ **Do NOT trust CVE database version ranges!** They are often inaccurate.

CVE databases (NVD, security news) frequently have **wrong version ranges**. For example:
- NVD may claim "affects ≤2.13.3, fixed in 2.13.4"
- But the fix commit was actually merged before 2.13.1

**Always verify fix commits against the actual codebase:**

```bash
cd externals/skia/third_party/externals/{dependency}

# Check if fix commit is ancestor of current HEAD
git merge-base --is-ancestor {fix_commit} HEAD && echo "FIXED" || echo "VULNERABLE"

# Or check if fix is in a tag that's ancestor of HEAD
git merge-base --is-ancestor {fixed_version_tag} HEAD && echo "FIXED" || echo "VULNERABLE"
```

**Example:** In January 2026, CVE-2025-27363 was incorrectly flagged for FreeType 2.13.3 because web sources claimed "fix in 2.13.4". Verification showed the fix commit was actually included in 2.13.1, and SkiaSharp's 2.13.3 was already patched.

### Known False Positives

Some components bundled in dependencies are **NOT used** by SkiaSharp. CVEs in these components are false positives.

#### MiniZip (in zlib) — NOT USED

**Status:** ❌ Not compiled, not linked, not vulnerable

**What is MiniZip?** A zip/unzip library bundled in `zlib/contrib/minizip/`. It provides `unzOpen()`, `zipOpen()`, and related functions for reading/writing ZIP archives.

**Evidence that Skia does NOT use MiniZip:**

1. **Skia's BUILD.gn excludes MiniZip sources**
   - File: `externals/skia/third_party/zlib/BUILD.gn`
   - The `sources` list contains only core zlib files (adler32.c, compress.c, deflate.c, etc.)
   - No references to `contrib/minizip/` directory

2. **No MiniZip headers included anywhere in Skia**
   ```bash
   grep -r "minizip\|unzip\.h\|zip\.h" externals/skia/src/ externals/skia/include/
   # Returns nothing
   ```

3. **No MiniZip function calls in Skia**
   ```bash
   grep -r "unzOpen\|zipOpen" externals/skia/src/ externals/skia/include/
   # Returns nothing
   ```

**MiniZip CVE examples that do NOT affect SkiaSharp:**
- CVE-2023-45853 (if MiniZip-specific)
- Any CVE mentioning `unzip.c`, `zip.c`, `ioapi.c`, or MiniZip functions

**How to verify a zlib CVE:**
1. Check if the CVE mentions MiniZip, `contrib/minizip`, or zip archive handling
2. Check if the affected functions are in Skia's BUILD.gn source list
3. If the CVE is in core zlib (deflate, inflate, adler32, crc32), it DOES affect SkiaSharp

#### FreeType Bundles Its Own zlib

- When `skia_use_system_zlib=false` (SkiaSharp's default), FreeType uses `skia_use_freetype_zlib_bundled=true`
- This means FreeType's zlib copy at `freetype/src/gzip/` is used, NOT Skia's `third_party/zlib`
- FreeType zlib CVEs should be checked against FreeType's bundled version, not the main zlib checkout
- Used for: compressed PCF fonts (common with X11 server distributions)

#### Actual zlib Usage in SkiaSharp

Core zlib (deflate/inflate) **IS** used by SkiaSharp. CVEs affecting these functions DO apply.

| Component | Uses zlib? | Functions Used | Notes |
|-----------|------------|----------------|-------|
| **PDF (SkDeflate.cpp)** | ✅ Yes | `deflateInit2`, `deflate`, `deflateEnd` | PDF stream compression |
| **libpng** | ✅ Yes | Core deflate/inflate | PNG compression (via zlib dependency) |
| **DNG SDK** (Windows only) | ✅ Yes | `compress2` | RAW image compression |
| **FreeType** | ⚠️ Bundled | Uses its own copy | Has `src/gzip/` with zlib subset |

---

## Build Configuration

### GN Build Flags

Skia uses GN build flags to enable/disable features. Key flags in `externals/skia/gn/skia.gni`:

```gn
# Image codecs
skia_use_libpng_decode = true
skia_use_libjpeg_turbo_decode = true
skia_use_libwebp_decode = true
skia_use_wuffs = true              # GIF decoder
skia_use_libavif = false           # AVIF - disabled
skia_use_libjxl_decode = false     # JPEG XL - disabled

# Font rendering
skia_use_freetype = is_android || is_fuchsia || is_linux || is_wasm
skia_use_harfbuzz = true           # SkiaSharp overrides to false

# Other
skia_use_expat = !is_wasm          # SkiaSharp overrides to true for WASM
skia_use_zlib = true
skia_use_dng_sdk = ...             # Complex condition, Windows mainly
```

### SkiaSharp Overrides

SkiaSharp sets specific GN args in `native/<platform>/build.cake` files:

```csharp
// Common across platforms
$"skia_use_harfbuzz=false " +      // Use system text shaping
$"skia_use_icu=false " +           // Use system ICU
$"skia_use_system_expat=false " +  // Bundle expat
$"skia_use_system_libpng=false " + // Bundle libpng
// ... etc
```

---

## Workflows

### Updating a Dependency

For the full update workflow, see the [native-dependency-update skill](/.github/skills/native-dependency-update/SKILL.md).

**Quick checklist:**
1. Check `externals/skia/DEPS` for current version
2. Find target version (CVE fix or upstream update)
3. Update DEPS with new commit hash
4. Update `cgmanifest.json` with new version
5. Build locally: `dotnet cake --target=externals-macos --arch=arm64`
6. Create PRs (mono/skia first, then SkiaSharp)

### Security Audit

For the full audit workflow, see the [security-audit skill](/.github/skills/security-audit/SKILL.md).

**Quick checklist:**
1. Search GitHub issues for CVE mentions
2. Check open PRs in mono/skia and mono/SkiaSharp
3. For each security-relevant dependency, search for CVEs
4. **Verify** any flagged CVE against actual codebase (don't trust version ranges)
5. Generate report with status and recommended actions

---

## Version Checking

To check current dependency versions:

```bash
# View active dependencies in DEPS
grep -E '^\s*"third_party/externals/' externals/skia/DEPS | grep -v "^#"

# Check a specific dependency's version
cd externals/skia/third_party/externals/<dep>
git describe --tags --always
git log -1 --format="%H %s"

# Check version from README (some deps have this)
cat README.chromium 2>/dev/null | head -10
```

---

## Adding/Removing Dependencies

### Adding New Dependencies

If Skia adds a new dependency that SkiaSharp needs:

1. Ensure it's enabled in DEPS (not commented out)
2. Add appropriate `skia_use_*` flags to `native/*/build.cake` if needed
3. If security-relevant, add to the tables above
4. Add to `cgmanifest.json` for CVE tracking
5. Document platform usage in this file

### Removing Unused Dependencies

To reduce attack surface, unused dependencies should be commented out in DEPS:

```python
# Before
"third_party/externals/libavif": "https://...",

# After (with explanation)
# SkiaSharp: libavif disabled (skia_use_libavif=false)
# "third_party/externals/libavif": "https://...",
```

After commenting out, test builds on all platforms:
```bash
dotnet cake --target=externals-macos --arch=arm64
dotnet cake --target=externals-ios
dotnet cake --target=externals-android --arch=arm64
```
