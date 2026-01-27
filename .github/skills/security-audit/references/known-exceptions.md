# Known Exceptions

CVEs in these components do **NOT** affect SkiaSharp and should be marked as false positives.

## Contents

- [MiniZip (in zlib)](#minizip-in-zlib--not-used)
- [Actual zlib Usage](#actual-zlib-usage-in-skiasharp)

---

## MiniZip (in zlib) — NOT USED

**Status:** ❌ Not compiled, not linked, not vulnerable

**What is MiniZip?** A zip/unzip library bundled in `zlib/contrib/minizip/`. It provides `unzOpen()`, `zipOpen()`, and related functions for reading/writing ZIP archives.

### Evidence that Skia does NOT use MiniZip

1. **Skia's BUILD.gn excludes MiniZip sources**
   - File: `externals/skia/third_party/zlib/BUILD.gn`
   - The `sources` list contains only core zlib files (adler32.c, compress.c, deflate.c, etc.)
   - No references to `contrib/minizip/` directory
   - Contrast with Chromium's BUILD.gn at `externals/zlib/BUILD.gn` which DOES include MiniZip — but Skia doesn't use that file

2. **No MiniZip headers included anywhere in Skia**
   - Searched: `externals/skia/src/` and `externals/skia/include/`
   - No includes of `unzip.h`, `zip.h`, or `minizip` paths
   - Command: `grep -r "minizip\|unzip\.h\|zip\.h" src/ include/` returns nothing

3. **No MiniZip function calls in Skia**
   - Searched for: `unzOpen`, `zipOpen`, `unzClose`, `zipClose`, `unzReadCurrentFile`
   - Command: `grep -r "unzOpen\|zipOpen" src/ include/` returns nothing

### MiniZip CVE examples that do NOT affect SkiaSharp

- CVE-2023-45853 (if MiniZip-specific — verify the CVE details)
- Any CVE mentioning `unzip.c`, `zip.c`, `ioapi.c`, or MiniZip functions

### How to verify a zlib CVE

1. Check if the CVE mentions MiniZip, `contrib/minizip`, or zip archive handling
2. Check if the affected functions are in Skia's BUILD.gn source list
3. If the CVE is in core zlib (deflate, inflate, adler32, crc32), it DOES affect SkiaSharp

---

## Actual zlib Usage in SkiaSharp

Core zlib (deflate/inflate) **IS** used by SkiaSharp. CVEs affecting these functions DO apply.

| Component | Uses zlib? | Functions Used | Notes |
|-----------|------------|----------------|-------|
| **PDF (SkDeflate.cpp)** | ✅ Yes | `deflateInit2`, `deflate`, `deflateEnd` | PDF stream compression |
| **libpng** | ✅ Yes | Core deflate/inflate | PNG compression (via zlib dependency) |
| **DNG SDK** (Windows only) | ✅ Yes | `compress2` | RAW image compression |
| **FreeType** | ⚠️ Bundled | Uses its own copy | Has `src/gzip/` with zlib subset for compressed fonts |

### FreeType bundles its own zlib

- When `skia_use_system_zlib=false` (SkiaSharp's default), FreeType uses `skia_use_freetype_zlib_bundled=true`
- This means FreeType's zlib copy at `freetype/src/gzip/` is used, NOT Skia's `third_party/zlib`
- FreeType zlib CVEs should be checked against FreeType's bundled version, not the main zlib checkout
- Used for: compressed PCF fonts (common with X11 server distributions)

### DNG SDK (Windows only)

- `skia_use_dng_sdk=true` only on Windows builds
- Uses `compress2()` for RAW/DNG image processing
- Other platforms do not include DNG SDK

---

## Adding New Exceptions

When you discover a component that is NOT used by SkiaSharp:

1. **Verify thoroughly** with multiple pieces of evidence:
   - Check BUILD.gn source lists
   - Search for header includes
   - Search for function calls
   - Check build configuration flags

2. **Document the evidence** in this file following the MiniZip example

3. **Include verification commands** so future audits can re-verify

4. **List example CVEs** that would be false positives
