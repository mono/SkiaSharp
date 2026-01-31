# ARM64 Linux UUID Symbol Fix

## Issue Summary

Starting with SkiaSharp version 3.117.0, applications using SkiaSharp on Linux ARM64 platforms experienced hard crashes with no error messages. This affected:

- Raspberry Pi 4 and 5
- AWS Graviton instances  
- Toradex IMX8
- Other ARM64 Linux devices

The crashes occurred immediately when making SkiaSharp drawing calls, with symptoms including:
- Application closes without any trace in logs
- No exceptions thrown
- Undefined symbol errors in some cases: `uuid_generate_random`, `uuid_parse`

### Related Issues

- [#3369](https://github.com/mono/SkiaSharp/issues/3369) - Hard crash on Linux ARM64 on 3.119.0
- [#3272](https://github.com/mono/SkiaSharp/issues/3272) - undefined symbol: uuid_generate_random on Debian Bookworm ARM64
- [#3436](https://github.com/mono/SkiaSharp/issues/3436) - undefined symbol exception in Linux ARM64

## Root Cause

The native `libSkiaSharp.so` library for Linux ARM64 was not linked against `libuuid`, even though it uses UUID functions from that library. 

### Technical Details

1. **Where UUID is used**: Skia's PDF subsystem (`SkPDFMetadata.cpp`) uses UUID functions to generate unique document identifiers
2. **Why it broke**: Modern Linux distributions (Ubuntu 24.04, Debian Bookworm) don't implicitly link system libraries
3. **Why it only affected ARM64**: The issue likely manifested on ARM64 first due to:
   - Timing of when ARM64 builds started using newer toolchains
   - Stricter linker behavior on ARM64 platforms
   - Different default linking behavior compared to x86_64

### The Missing Symbols

```c
// From libuuid (util-linux package)
void uuid_generate_random(uuid_t out);
int uuid_parse(const char *in, uuid_t uu);
```

These functions are called by Skia but were not being resolved at runtime because `libuuid.so` was not listed as a dependency in the ELF header.

## The Fix

Added `-luuid` to the linker flags in `native/linux/build.cake`:

```diff
-            $"extra_ldflags=[ '-static-libstdc++', '-static-libgcc', '-Wl,--version-script={map}' ] " +
+            $"extra_ldflags=[ '-static-libstdc++', '-static-libgcc', '-Wl,--version-script={map}', '-luuid' ] " +
```

This tells the linker to:
1. Look for UUID symbols in `libuuid.so`
2. Add `libuuid.so.1` as a NEEDED dependency in the ELF header
3. Allow the dynamic linker to resolve these symbols at runtime

## Affected Platforms

This fix applies to **all** Linux variants built by `native/linux/build.cake`:

- Linux glibc: x64, x86, arm64, arm, riscv64, loongarch64
- Alpine musl: x64, x86, arm64, arm, riscv64, loongarch64  
- Linux no-dependencies variant
- Cross-compiled Linux builds

## Testing

### Before the Fix

```bash
$ dotnet run
# Process exits immediately with no output or error
```

### After the Fix

```bash
$ dotnet run
Starting SkiaSharp test on ARM64...
Runtime: linux-arm64
Architecture: Arm64

Test 1: Create bitmap
  ✓ Bitmap created successfully
Test 2: Create canvas
  ✓ Canvas created successfully
...
ALL TESTS PASSED! ✓
```

### Verification Commands

After building with the fix, verify the dependency is correctly declared:

```bash
# Check that libuuid is listed as a dependency
readelf -d libSkiaSharp.so | grep NEEDED | grep uuid
# Expected output: 0x0000000000000001 (NEEDED)   Shared library: [libuuid.so.1]

# Verify UUID symbols are defined as external
nm -D libSkiaSharp.so | grep uuid
# Expected output:
#                  U uuid_generate_random
#                  U uuid_parse

# Check runtime dependencies
ldd libSkiaSharp.so | grep uuid
# Expected output: libuuid.so.1 => /lib/aarch64-linux-gnu/libuuid.so.1
```

## Test Script

A test script is provided at `scripts/test/test-arm64-linux.sh` that:
1. Checks system requirements (ARM64, Linux, libuuid)
2. Creates a test project with SkiaSharp
3. Runs comprehensive drawing tests
4. Verifies the fix works correctly

To run on an ARM64 Linux machine:

```bash
./scripts/test/test-arm64-linux.sh
```

## System Requirements

For runtime use, ensure `libuuid` is installed:

```bash
# Debian/Ubuntu
sudo apt-get install libuuid1

# RHEL/CentOS/Fedora
sudo yum install libuuid

# Alpine
sudo apk add util-linux
```

Most Linux distributions include `libuuid1` by default, but minimal containers may need it explicitly installed.

## Building with the Fix

To build SkiaSharp native libraries with this fix:

```bash
# Build Linux ARM64 native library
dotnet cake --target=externals-linux --arch=arm64

# Or build all Linux architectures
dotnet cake --target=externals-linux
```

The fix will be included in SkiaSharp releases after version 3.119.0.

## Last Known Good Version

- **Version 3.116.0 and earlier**: Did not exhibit this issue
- **Version 3.117.0 and later**: Affected by the bug
- **Version 3.119.1+** (with this fix): Issue resolved

## References

- Skia PDF UUID implementation: `externals/skia/src/pdf/SkPDFMetadata.cpp`
- Linux build script: `native/linux/build.cake`
- libuuid documentation: `man uuid_generate`, `man uuid_parse`
