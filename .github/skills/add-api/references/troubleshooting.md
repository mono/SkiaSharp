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

## Tests won't run

**Cause:** Missing externals/native binaries

**Fix:**
```bash
# Download pre-built natives
dotnet cake --target=externals-download

# Verify
ls output/native/
```
