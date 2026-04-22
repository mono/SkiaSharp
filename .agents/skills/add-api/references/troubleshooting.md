# Troubleshooting

## Changes lost when running git command

**Cause:** Didn't commit in submodule before staging in parent

**Fix:** Always commit inside `externals/skia/` FIRST, then `git add externals/skia`

```bash
# CORRECT order:
cd externals/skia
git add include/c/sk_*.h src/c/sk_*.cpp
git commit -m "Add sk_foo_bar to C API"
cd ../..  # Back to repo root
git add externals/skia  # NOW stage in parent
```

## Generated file doesn't have my function

**Cause:** Didn't run generator after C API changes

**Fix:** Run `pwsh ./utils/generate.ps1` — NEVER skip this

**Verify:** `git diff binding/SkiaSharp/SkiaApi.generated.cs` should show new function

## EntryPointNotFoundException at runtime

> **🛑 This is the #1 mistake when adding new APIs**

**Cause:** You added a new C API function but didn't rebuild the native library. The pre-built natives from `externals-download` don't contain your new function.

**Fix:** Build the native library for your platform:

```bash
# macOS (Apple Silicon)
dotnet cake --target=externals-macos --arch=arm64

# macOS (Intel)
dotnet cake --target=externals-macos --arch=x64

# Windows (x64)
dotnet cake --target=externals-windows --arch=x64
```

**Do NOT:**
- Skip the test
- Use `SkipException` to work around it
- Claim completion with failing tests

**Verification:** After building, run tests again — they should pass.

## Test compiles but fails at runtime

**Cause:** C API implementation bug, wrong pointer handling, incorrect parameters

**Debug steps:**
1. Check conversion macros (`AsType`/`ToType`) match the C++ types
2. Verify pointer ownership (ref-counted vs owned vs raw)
3. Check parameter order matches C++ signature
4. Verify `sk_ref_sp()` used for ref-counted parameters
5. Verify `.release()` used for ref-counted returns

## Generator fails

**Cause:** Environment issue, missing dependencies, network problem

**Fix:**
1. Retry once (transient failures happen)
2. Check C API syntax is valid C (not C++)
3. Verify header guards are correct
4. If still failing, **STOP** and notify developer — don't work around by manual editing

## Tests won't run (missing natives)

**Cause:** Missing externals/native binaries

**Fix depends on what you changed:**

```bash
# If you ONLY changed C# code:
dotnet cake --target=externals-download

# If you changed C API (added/modified functions):
dotnet cake --target=externals-{platform} --arch={arch}
```

**Verify:**
```bash
ls output/native/
```
