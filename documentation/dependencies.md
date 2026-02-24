# SkiaSharp Native Dependencies

Single source of truth for native dependencies: what's used, what's not, and how to track security vulnerabilities.

## Contents

- [Active Dependencies](#active-dependencies) — What SkiaSharp actually compiles
- [cgmanifest.json](#cgmanifestjson) — CVE detection setup
- [Known False Positives](#known-false-positives) — CVEs that don't affect SkiaSharp

---

## Active Dependencies

SkiaSharp uses only a subset of Skia's dependencies. Unused dependencies are commented out in `externals/skia/DEPS` to reduce attack surface.

### Security-Relevant (process untrusted input)

| Dependency | Purpose | CVE Name | Platforms |
|------------|---------|----------|-----------|
| **libpng** | PNG codec | libpng | All |
| **zlib** | Compression | zlib | All |
| **libjpeg-turbo** | JPEG codec | libjpeg-turbo | All |
| **libwebp** | WebP codec | libwebp | All |
| **freetype** | Font rendering | freetype | Android, Linux, WASM |
| **harfbuzz** | Text shaping | harfbuzz | All (disabled in SkiaSharp) |
| **expat** | XML parsing | libexpat | All |
| **brotli** | WOFF2 fonts | brotli | All |
| **wuffs** | GIF codec | wuffs | All |
| **dng_sdk** | RAW images | dng_sdk | Windows |

### GPU/Graphics

| Dependency | Purpose | Platforms |
|------------|---------|-----------|
| **vulkanmemoryallocator** | Vulkan memory | Android, Linux, Windows |
| **d3d12allocator** | Direct3D memory | Windows |
| **spirv-cross** | Shader translation | Vulkan/Metal |
| **vulkan-headers** | Vulkan API | Vulkan builds |

### Supporting

| Dependency | Purpose | Platforms |
|------------|---------|-----------|
| **piex** | RAW preview | All except Windows, WASM |
| **buildtools** | Compiler toolchain | All |

---

## cgmanifest.json

Enables Microsoft Component Governance CVE detection.

**Problem:** Skia mirrors dependencies from chromium.googlesource.com, but CVE databases use upstream names.

**Solution:** Use `type: "other"` with canonical names:

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

### Name Mapping

| DEPS Name | cgmanifest Name | Upstream |
|-----------|-----------------|----------|
| libpng | `libpng` | github.com/glennrp/libpng |
| zlib | `zlib` | github.com/madler/zlib |
| libjpeg-turbo | `libjpeg-turbo` | github.com/libjpeg-turbo/libjpeg-turbo |
| libwebp | `libwebp` | github.com/webmproject/libwebp |
| freetype | `freetype` | gitlab.freedesktop.org/freetype/freetype |
| harfbuzz | `harfbuzz` | github.com/harfbuzz/harfbuzz |
| expat | `libexpat` | github.com/libexpat/libexpat |
| brotli | `brotli` | github.com/google/brotli |
| wuffs | `wuffs` | github.com/google/wuffs-mirror-release-c |
| dng_sdk | `dng_sdk` | android.googlesource.com/.../dng_sdk |

---

## Known False Positives

Some CVEs flagged against dependencies **don't affect SkiaSharp** because the vulnerable component isn't compiled.

### MiniZip (in zlib) — NOT USED

**Status:** ❌ Not compiled, not linked

MiniZip is bundled in `zlib/contrib/minizip/` but Skia's BUILD.gn excludes it. CVEs mentioning `unzip.c`, `zip.c`, `ioapi.c`, or functions like `unzOpen`/`zipOpen` are false positives.

**Evidence:**
- `externals/skia/third_party/zlib/BUILD.gn` lists only core zlib sources
- No MiniZip includes: `grep -r "minizip\|unzip\.h" externals/skia/src/` returns nothing

**Core zlib IS used** — CVEs affecting deflate/inflate/adler32/crc32 DO apply.

### FreeType's Bundled zlib

FreeType has its own zlib copy at `freetype/src/gzip/`. When checking zlib CVEs:
- Check if it affects FreeType's bundled copy (different version)
- Core Skia zlib and FreeType zlib are separate

---

## Related Skills

- **[security-audit](/.github/skills/security-audit/SKILL.md)** — Find CVEs, verify fixes, generate reports
- **[native-dependency-update](/.github/skills/native-dependency-update/SKILL.md)** — Update dependencies, create PRs
