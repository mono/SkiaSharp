# SkiaSharp Native Dependencies

This document provides a comprehensive reference for all native dependencies used by SkiaSharp, their purpose, security relevance, and platform usage.

## Overview

SkiaSharp wraps Google's Skia library, which has numerous third-party dependencies. However, **SkiaSharp only uses a subset** of these dependencies. Many are for Skia's development tools, experimental features, or codecs that SkiaSharp doesn't enable.

Understanding which dependencies are actually used is critical for:
- **Security audits**: Only audit dependencies that are compiled into SkiaSharp
- **Dependency updates**: Prioritize updates for dependencies that affect users
- **Build optimization**: Reduce build times by not syncing unused dependencies

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

## How Dependencies Are Controlled

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

## Security Audit Process

When auditing dependencies for CVEs:

1. **Only audit security-relevant dependencies** (see table above)
2. **Check current versions** in `externals/skia/DEPS`
3. **Search CVE databases** for each dependency's canonical name
4. **Verify fixes in codebase** - CVE databases often have wrong version ranges
5. **Check existing PRs** in mono/skia for pending updates

See the [security-audit skill](/.github/skills/security-audit/SKILL.md) for detailed workflow.

## Updating Dependencies

When updating a dependency:

1. **Find current commit** in `externals/skia/DEPS`
2. **Find upstream repository** (usually on GitHub)
3. **Identify target version** (for CVE fix or feature)
4. **Update DEPS** with new commit hash
5. **Update cgmanifest.json** with new version for Component Governance
6. **Test builds** on all affected platforms

See the [native-dependency-update skill](/.github/skills/native-dependency-update/SKILL.md) for detailed workflow.

## cgmanifest.json Mapping

The `cgmanifest.json` file maps dependencies to their canonical names for Microsoft Component Governance CVE detection. Use `type: "other"` with the upstream project name:

| DEPS Name | cgmanifest Name | Upstream URL |
|-----------|-----------------|--------------|
| libpng | libpng | https://github.com/glennrp/libpng |
| zlib | zlib | https://github.com/madler/zlib |
| libjpeg-turbo | libjpeg-turbo | https://github.com/libjpeg-turbo/libjpeg-turbo |
| libwebp | libwebp | https://github.com/webmproject/libwebp |
| freetype | freetype | https://gitlab.freedesktop.org/freetype/freetype |
| harfbuzz | harfbuzz | https://github.com/harfbuzz/harfbuzz |
| expat | libexpat | https://github.com/libexpat/libexpat |
| brotli | brotli | https://github.com/google/brotli |
| wuffs | wuffs | https://github.com/aspect-build/aspect-cli |
| dng_sdk | dng_sdk | https://android.googlesource.com/platform/external/dng_sdk |

## Version Checking

To check current dependency versions:

```bash
# View DEPS file
cat externals/skia/DEPS | grep "third_party/externals"

# Check a specific dependency's version
cd externals/skia/third_party/externals/<dep>
git describe --tags --always
git log -1 --format="%H %s"
```

## Adding New Dependencies

If Skia adds a new dependency that SkiaSharp needs:

1. Ensure it's enabled in DEPS (not commented out)
2. Add appropriate `skia_use_*` flags to `native/*/build.cake` if needed
3. If security-relevant, add to the security audit list above
4. Add to `cgmanifest.json` for CVE tracking
5. Document platform usage in this file

## Removing Unused Dependencies

To reduce attack surface, unused dependencies should be commented out in DEPS:

```python
# Before
"third_party/externals/libavif": "https://...",

# After  
# "third_party/externals/libavif": "https://...",
```

After commenting out, test builds on all platforms to ensure nothing breaks.
