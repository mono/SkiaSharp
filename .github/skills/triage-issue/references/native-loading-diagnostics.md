# Native Loading Diagnostics

Reference for diagnosing `DllNotFoundException` errors on Linux. Used during triage Step 1d when analyzing native loading issues.

## dlopen() Failure Modes

On Linux, `DllNotFoundException` errors from `dlopen()` have two distinct failure modes with completely different root causes:

| Error pattern | What happened | Root cause |
|---------------|---------------|------------|
| `libSkiaSharp.so: cannot open shared object file: No such file or directory` | The `.so` file was **not found** at any search path | NativeAssets package not deployed — check PackageReference is in the executable project, not a library |
| `libfontconfig.so.1: cannot open shared object file` (or any dependency OF libSkiaSharp) | A `.so` **WAS found** and `dlopen()` loaded it, but the binary has a `DT_NEEDED` dependency on fontconfig that isn't installed | The **wrong binary** was deployed (see below) |

## "Wrong Binary" Diagnosis

`SkiaSharp.NativeAssets.Linux.NoDependencies` ships a `.so` with **zero** external dependencies — only libc/libm/libpthread/libdl. No fontconfig, no uuid, nothing else.

If the user references `NoDependencies` but `dlopen()` reports a missing dependency *of* libSkiaSharp (fontconfig, uuid, etc.):

- The binary being loaded is **not** from `NoDependencies`
- It's from `NativeAssets.Linux` (the fontconfig-dependent variant) or some other source
- **Never** dismiss this as a "red herring" or "loader noise" — it proves the wrong `.so` was deployed

### Investigation checklist

1. Is there a transitive `SkiaSharp.NativeAssets.Linux` reference conflicting with `NoDependencies`?
2. Is the container build tool (e.g., .NET Aspire, Docker SDK) deploying the wrong RID variant?
3. Are both packages present and one winning at deploy time?
4. Is the `NoDependencies` PackageReference in the **executable** project (not a library)?

## .NET Runtime Load Order

The .NET runtime tries multiple paths to load a native library and reports ALL failures in the exception message. The error output typically looks like:

```
System.DllNotFoundException: Unable to load shared library 'libSkiaSharp' or one of its dependencies.
  libfontconfig.so.1: cannot open shared object file: No such file or directory     ← dlopen found a .so, loaded it, dep failed
  /usr/share/dotnet/.../libSkiaSharp.so: cannot open shared object file             ← fallback path, file not found
  /app/liblibSkiaSharp.so: cannot open shared object file                           ← fallback path, file not found
  ...
```

The **first** error is the most diagnostic — it tells you what happened with the binary that was actually found. Subsequent lines are fallback attempts that didn't find a file at all.

## Common Container Deployment Issues

| Problem | Cause | Fix |
|---------|-------|-----|
| `DllNotFoundException: libSkiaSharp` | Native binary not in output | Ensure `SkiaSharp.NativeAssets.Linux.NoDependencies` (or `.Linux`) is a direct `PackageReference` in the application project |
| `libfontconfig.so.1: cannot open` | Using `SkiaSharp.NativeAssets.Linux` in minimal container | Switch to `SkiaSharp.NativeAssets.Linux.NoDependencies` or install fontconfig in Dockerfile |
| Wrong binary for container arch | RID mismatch (glibc vs musl) | Alpine needs `linux-musl-*` RIDs — `SkiaSharp.NativeAssets.Linux.NoDependencies` includes both variants |
| Trimming removes transitive deps | .NET trimmer strips unused assemblies | Add missing assembly as a direct `PackageReference` |

## Learned from #3386

Issue #3386 reported `DllNotFoundException` with `NoDependencies` referenced. The fontconfig error proved the wrong binary was deployed in an Azure Container App via .NET Aspire. The user worked around it by switching to `NativeAssets.Linux` with a custom Docker image containing fontconfig.
