# Files Changed in a Typical Update

| Repository | File | Change |
|-----------|------|--------|
| Paired Skia repository | `BUILD.gn` | Merge conflict resolution (most complex) |
| Paired Skia repository | `DEPS` | Merge conflict resolution |
| Paired Skia repository | `include/core/SkMilestone.h` | New milestone number (from upstream) |
| Paired Skia repository | `include/c/sk_types.h` | Enum/type updates, `SK_C_INCREMENT` reset |
| Paired Skia repository | `src/c/*.cpp` | C API fixes for new C++ APIs |
| Paired Skia repository | `src/c/sk_enums.cpp` | Enum mapping updates |
| Paired Skia repository | `src/c/sk_types_priv.h` | Include path + type conversion updates |
| Current SkiaSharp repository | `.gitmodules` | Submodule branch name |
| Current SkiaSharp repository | `externals/skia` | Submodule pointer |
| Current SkiaSharp repository | `scripts/VERSIONS.txt` | All version numbers |
| Current SkiaSharp repository | `cgmanifest.json` | Security tracking |
| Current SkiaSharp repository | `scripts/azure-templates-variables.yml` | CI config (`SKIASHARP_VERSION`) |
| Current SkiaSharp repository | `native/*/build.cake` | Per-platform GN flag updates (check for removed declare_args) |
| Current SkiaSharp repository | `binding/SkiaSharp/SkiaApi.generated.cs` | Regenerated |
| Current SkiaSharp repository | `binding/SkiaSharp/Definitions.cs` | Type definitions, new enums |
| Current SkiaSharp repository | `binding/SkiaSharp/EnumMappings.cs` | Enum mappings |
| Current SkiaSharp repository | `binding/SkiaSharp/GRDefinitions.cs` | GPU type changes |
| Current SkiaSharp repository | `binding/libSkiaSharp.json` | Type config |
| Current SkiaSharp repository | `tests/Tests/SkiaSharp/*.cs` | Test updates |
