# SkiaSharp

SkiaSharp is a cross-platform 2D graphics API for .NET wrapping Google's Skia library.

**Architecture:** `C# Wrapper` -> `P/Invoke` -> `C API` -> `C++ Skia`
**Principle:** C# validates parameters, C API trusts and passes through.

---

## Critical Rules (Read First)

These rules are **non-negotiable**. Violating them causes broken builds, crashes, or downstream breakage.

### 1. Bootstrap First

Before C# code can build, native binaries must exist in `output/native/`. **How** you produce them depends on what you're changing:

| You are changing… | Bootstrap with |
|---|---|
| **Only C# code** (no files under `externals/skia/`, no `DEPS`, no submodule bump) | `dotnet cake --target=externals-download` (downloads pre-built natives from the **current** milestone) |
| **Native code, C API, `DEPS`, or the Skia submodule** (incl. milestone updates) | `dotnet cake --target=externals-{platform} --arch={arch}` — build from source. |

> **🛑 If you are doing a Skia milestone update, a C API change, or anything under `externals/skia/`, STOP. Do not run `externals-download` — ever. The downloaded binaries are from the OLD milestone and do not contain your changes; using them produces silently-wrong builds and `EntryPointNotFoundException` at runtime.** When source builds fail (missing `gn`, network errors, etc.), debug the source build — do not fall back to download.

### 2. Never `externals-download` After Native Changes

If you have modified **any** of the following, `externals-download` is FORBIDDEN until your changes ship to the pre-built artifact server (which only happens after merge):

- `externals/skia/**` (including the submodule SHA)
- `externals/skia/src/c/**`, `externals/skia/include/c/**`
- `externals/skia/DEPS`
- Any milestone bump or version file (`VERSIONS.txt`, `sk_types.h SK_C_INCREMENT`)

Falling back to `externals-download` because a native build failed is the #1 way agents corrupt milestone updates. Fix the source build instead.

### 3. Never Edit Generated Files

Files matching `*.generated.cs` and `docs/` are auto-generated.

- **NEVER** manually edit these files
- **ALWAYS** regenerate after C API changes (see [Commands](#commands))

### 4. ABI Stability

SkiaSharp maintains stable ABI. Breaking changes break downstream apps.

| Allowed | Never |
|---------|-------|
| Add new overloads | Modify existing signatures |
| Add new methods | Remove public APIs |
| Add new classes | Change return types |

### 5. Tests Are Mandatory

**Building alone is NOT sufficient.** Run tests before claiming completion (see [Commands](#commands)).

### 6. Branch Protection (COMPLIANCE REQUIRED)

**Direct commits to protected branches are a policy violation.**

| Repository | Protected Branches |
|------------|-------------------|
| SkiaSharp (parent) | `main` |
| externals/skia (submodule) | `main`, `skiasharp` |

**Required workflow:**

1. **Create a feature branch FIRST** — Use naming convention: `dev/issue-NNNN-description`
2. **Make all commits on the feature branch** — Never commit directly to protected branches
3. **Submit a Pull Request** — Changes must be reviewed before merging

```bash
# CORRECT — Always create a feature branch first
git checkout -b dev/issue-1234-fix-description

# For submodule changes:
cd externals/skia
git checkout -b dev/issue-1234-add-c-api

# NEVER DO THIS — Policy violation
git checkout main && git commit  # FORBIDDEN
git checkout skiasharp && git commit  # FORBIDDEN (in skia submodule)
```

**This applies to BOTH repositories.** The skia submodule has its own protected branches that must be respected.

---

## Commands

Single source of truth for all commands:

| Task | Command |
|------|---------|
| **Bootstrap (C#-only work — see Rule #1, FORBIDDEN for native changes)** | `dotnet cake --target=externals-download` |
| **Build Native (macOS ARM64)** | `dotnet cake --target=externals-macos --arch=arm64` |
| **Build Native (macOS Intel)** | `dotnet cake --target=externals-macos --arch=x64` |
| **Build Native (Windows x64)** | `dotnet cake --target=externals-windows --arch=x64` |
| **Build Native (Linux x64)** | `dotnet cake --target=externals-linux --arch=x64` |
| **Build Native (Linux ARM64)** | `dotnet cake --target=externals-linux --arch=arm64` |
| **Build C#** | `dotnet build binding/SkiaSharp/SkiaSharp.csproj` |
| **Test** | `dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj` |
| **Regenerate** | `pwsh ./utils/generate.ps1` |
| **Diff branch for release notes** | `python3 .agents/skills/release-notes/scripts/generate-release-notes.py --branch main` |
| **Update release notes TOC** | `python3 .agents/skills/release-notes/scripts/generate-release-notes.py --update-toc` |

### When to Use Which Bootstrap

| What You Changed | Command Required |
|------------------|------------------|
| C# code only (`binding/SkiaSharp/*.cs`) | `externals-download` (pre-built natives) |
| C API (`externals/skia/src/c/`, `externals/skia/include/c/`) | **`externals-{platform}` (MUST rebuild natives — `externals-download` is FORBIDDEN)** |
| Dependencies (`externals/skia/DEPS`) | **`externals-{platform}` (MUST rebuild natives — `externals-download` is FORBIDDEN)** |
| Skia submodule SHA / milestone update | **`externals-{platform}` (MUST rebuild natives — `externals-download` is FORBIDDEN)** |

> **CRITICAL:** If you modify ANY native code (C API headers/implementations), you MUST rebuild
> the native library with `dotnet cake --target=externals-{platform}`. Using `externals-download`
> after native changes will cause `EntryPointNotFoundException` at runtime because the downloaded
> binaries don't contain your new functions.

> **Note:** For release verification, see `/release-testing` command for the full platform matrix.

**Recovery Commands:**

| Problem | Command |
|---------|---------|
| Clean rebuild (**C#-only work**) | `dotnet cake --target=clean && dotnet cake --target=externals-download` |
| Clean rebuild (**any native or milestone work**) | `dotnet cake --target=clean && dotnet cake --target=externals-{platform} --arch={arch}` |
| Reset submodule | `git submodule update --init --recursive` |

> **Native build failing?** Do **NOT** "fall back" to `externals-download`. Common causes: missing `gn`/`ninja`, missing depot_tools on PATH, missing network access to `chromium.googlesource.com`. Diagnose and fix the source build. Using `externals-download` to make the failure go away will produce a build that runs against stale binaries and silently corrupts milestone updates.

---

## Architecture & Directories

### Layer Overview

```
C# Wrapper (binding/SkiaSharp/)  ->  P/Invoke  ->  C API (externals/skia/src/c/)  ->  C++ Skia
```

### Directory Guide

| Directory | Editable? | Notes |
|-----------|-----------|-------|
| `binding/SkiaSharp/` | Yes | C# wrappers |
| `externals/skia/src/c/` | Yes | C API implementation (our shim) |
| `externals/skia/include/c/` | Yes | C API headers (our shim) |
| `externals/skia/**` (other) | No | Upstream Skia — never modify |
| `*.generated.cs` | No | Run `pwsh ./utils/generate.ps1` |
| `docs/` | No | Auto-generated |
| `documentation/dev/` | Yes | Architecture guides |
| `documentation/docfx/releases/` | Yes | Website release notes (template-formatted) |
| `documentation/docfx/releases/TEMPLATE.md` | Yes | Template for AI formatting |

---

## Writing Code

This section covers memory management, code patterns, and error handling together — they're tightly coupled when writing wrappers.

### Step 1: Identify Pointer Type

```
Is it wrapped in sk_sp<T>?
+- Yes -> SkRefCnt?      -> ISKReferenceCounted
|         SkNVRefCnt<T>? -> ISKNonVirtualReferenceCounted
+- No  -> Parameter?     -> owns: false
          Otherwise      -> DisposeNative()
```

| Type | C++ | C# | Examples |
|------|-----|-----|----------|
| Raw | `T*` param | `owns: false` | Temporary refs |
| Owned | Manual delete | `DisposeNative()` | Canvas, Paint, Path |
| Ref-counted | `sk_sp<T>` | `ISKReferenceCounted` | Image, Shader, Surface |

### Step 2: Choose Pattern

**Factory method** — return null on failure, validate inputs:

```csharp
public static SKImage FromPixels(SKImageInfo info, SKData data, int rowBytes)
{
    if (data == null)
        throw new ArgumentNullException(nameof(data));
    var cinfo = SKImageInfoNative.FromManaged(ref info);
    return GetObject(SkiaApi.sk_image_new_raster_data(&cinfo, data.Handle, (IntPtr)rowBytes));
}
```

**Instance method** — validate then call:

```csharp
public void DrawRect(SKRect rect, SKPaint paint)
{
    if (paint == null)
        throw new ArgumentNullException(nameof(paint));
    SkiaApi.sk_canvas_draw_rect(Handle, &rect, paint.Handle);
}
```

**C API** — naming convention `sk_<type>_<action>`:

```cpp
sk_image_t* sk_image_new_from_encoded(const sk_data_t* cdata) {
    return ToImage(SkImages::DeferredFromEncodedData(sk_ref_sp(AsData(cdata))).release());
}
```

### Step 3: Error Handling

| Layer | On Failure |
|-------|------------|
| C API | Return `nullptr` or `false` |
| C# Factory | Return `null` |
| C# Constructor | Throw |

### Step 4: Same-Instance Returns

Some methods return the **same instance**. Always check before disposing:

```csharp
// CORRECT — always use this pattern
var source = GetImage();
var result = source.Subset(bounds);
if (result != source)
    source.Dispose();
return result;
```

**Methods that may return same instance:** `Subset()`, `ToRasterImage()`, `ToRasterImage(false)`

### API Design Rules

- **Overloads, not defaults** — Default parameters break ABI
- **Deprecate, don't remove** — Use `[Obsolete]` with migration guidance
- **Naming:** `SK` prefix, PascalCase methods, camelCase parameters

**Adding overloads (ABI-safe):**

```csharp
// Existing method (don't modify)
public void DrawText(string text, float x, float y, SKPaint paint)

// New overload (safe to add)
public void DrawText(string text, SKPoint point, SKPaint paint)
    => DrawText(text, point.X, point.Y, paint);
```

### Threading Rules

Skia is **NOT thread-safe**.

| Never share between threads | Safe to share (immutable) |
|-----------------------------|---------------------------|
| `SKCanvas`, `SKPaint`, `SKPath` | `SKImage`, `SKShader`, `SKData` |

```csharp
// Thread-safe pattern — each thread gets own Paint
ThreadLocal<SKPaint> paint = new(() => new SKPaint());
```

### Anti-Patterns (Never Do This)

| Anti-Pattern | Why |
|-------------|-----|
| `canvas.Dispose()` while using derived objects | Crashes |
| Sharing `SKPaint` between threads | Race conditions |
| Modifying method signatures | ABI breaking |
| Manual edits to `*.generated.cs` | Overwritten on regenerate |
| Using default parameters in public APIs | ABI breaking |
| **Skipping failing tests** | **Unacceptable — tests must pass** |
| **Using `externals-download` after C API changes** | **Causes `EntryPointNotFoundException`** |
| Passing `fixed` pointers to native objects that outlive the block | GC moves memory -> corruption. Use `GCHandle.Alloc(Pinned)` or `Marshal.AllocCoTaskMem` |
| Testing WASM version changes without cleaning `bin/obj/_framework` | Stale cached native `.wasm` files produce false results |

---

## Testing & Debugging

### Running Tests

```bash
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj
```

### Tests MUST Pass

> **NON-NEGOTIABLE:** Tests must PASS before claiming completion.
>
> - Do NOT skip failing tests
> - Do NOT claim completion if tests fail
> - Do NOT use `SkipException` to work around failures
>
> **Skipping is ONLY acceptable for hardware limitations:**
> - No GPU drivers available
> - No display attached
> - Platform doesn't support the feature (e.g., Metal on Windows)

### Writing Tests

```csharp
[SkippableFact]
public void FeatureWorks()
{
    using var data = SKData.Create(Path.Combine(PathToImages, "baboon.jpg"));
    using var image = SKImage.FromEncodedData(data);
    Assert.NotNull(image);
}
```

**BaseTest helpers:** `PathToImages`, `PathToFonts`, `IsWindows/Mac/Linux`

**Philosophy:** Tests FAIL when wrong, never skip (except missing hardware).

### Debugging Methodology

1. **Establish baseline** — What's the known-good state?
2. **One change at a time** — Verify each change before proceeding
3. **Track changes in a table** — Log what you changed and the result
4. **Platform differences are signals** — If X works and Y fails, the difference IS the answer
5. **Revert if worse** — Don't pile fixes on top of failures

### Failure Recognition

| Error | Likely Cause | Fix |
|-------|--------------|-----|
| `error CS0246` (missing type) | Missing binding | Run `pwsh ./utils/generate.ps1` |
| `LNK2001 unresolved external` | C API signature mismatch | Check C function names match |
| `AccessViolationException` | Memory management bug | Check disposal patterns |
| `NullReferenceException` | Factory returned null | Check C API return value |
| Random crashes | Threading violation | Check Canvas/Paint thread scope |
| **`EntryPointNotFoundException`** | **Native library not rebuilt after C API change** | **Run `dotnet cake --target=externals-{platform}`** |

See [documentation/dev/debugging-methodology.md](documentation/dev/debugging-methodology.md).

---

## Slash Commands

Custom slash commands are available for specialized workflows. Use these for complex tasks that benefit from structured processes.

### When to Use Commands

| Task | Command | Triggers |
|------|---------|----------|
| Triage issue | `/issue-triage` | "triage #NNNN", "classify issue", "analyze issue" |
| Reproduce bug | `/issue-repro` | "repro #NNNN", "reproduce issue", "create reproduction" |
| Fix bug | `/issue-fix` | "investigate #NNNN", "fix issue", crash, exception, segfault, "doesn't work" |
| Bulk process issues | `/issue-bulk-process` | "triage these issues", "process issues #1 #2 #3" |
| Add new API | `/api-add-review` | "expose", "wrap method", issue requests new functionality |
| Update dependency | `/native-dependency-update` | "bump libpng", "fix CVE in zlib" |
| Write XML docs | `/api-docs` | "document", "fill in missing docs" |
| Security check | `/security-audit` | "audit CVEs", "security overview" (read-only) |
| Start release (Step 1/4) | `/release-branch` | "release now", "start release X" |
| Check release status (Step 2/4) | `/release-status` | "check release status", "how is the build", "pipeline status" |
| Test release (Step 3/4) | `/release-testing` | "test the release", "verify packages" |
| Publish release (Step 4/4) | `/release-publish` | "push to nuget", "tag release" |
| Release notes | `/release-notes` | "generate release notes", "regenerate 3.119.x", "write release notes for" |
| Skia analyst | `/skia-analyst` | "what changed", "what are we missing", "feature gap", "changelog", "scout features", "diff tags" |
| Update Skia | `/update-skia` | "update to milestone NNN", "bump Skia" |
| Review Skia update | `/review-skia-update` | "review the Skia merge PR" |
| PR commit message | `/pr-commit-message` | "write commit message for PR" |
| Validate samples | `/validate-samples` | "build samples", "test sample projects" |
| Scout GM samples | `/sample-scout` | "find demos to port", "what samples are we missing", "gallery ideas" |
| Create/improve skill | `/skill-creator` | "create a new skill", "improve skill X" |

### Issue Pipeline (3 steps)

The first three commands form a pipeline. Each can run standalone, but they work best in sequence:

| Step | Command | Produces |
|------|---------|----------|
| 1 | `/issue-triage` | `ai-triage/{n}.json` |
| 2 | `/issue-repro` | `ai-repro/{n}.json` |
| 3 | `/issue-fix` | `ai-fix/{n}.json` + PR |

See [documentation/dev/issue-pipeline.md](documentation/dev/issue-pipeline.md) for handoff contracts and feedback loop.

### Issue Classification (#NNNN)

| If Issue Contains... | Type | Command |
|---------------------|------|---------|
| "triage", "classify", "analyze issue" | Triage | `/issue-triage` |
| "repro", "reproduce", "reproduction" | Reproduction | `/issue-repro` |
| "crash", "exception", "wrong", "fails", "broken", "segfault" | Bug | `/issue-fix` |
| "add", "expose", "missing API", "feature request" | New API | `/api-add-review` |
| "docs", "documentation", "XML", "comments" | Docs | `/api-docs` |
| CVE, security, vulnerability | Security | `/security-audit` then `/native-dependency-update` |

### When NOT to Use Commands

Work directly for:
- Trivial fixes (typos, whitespace, obvious one-liners)
- Changes only to `documentation/dev/` (non-generated docs)
- Build/test-only tasks (no reported bug)
- Questions about code or architecture
- Refactoring without a reported problem
- Performance optimization (unless there's a "slow" bug report)

---

## Further Reading

| Topic | Document |
|-------|----------|
| Architecture | `documentation/dev/architecture.md` |
| Memory Management | `documentation/dev/memory-management.md` |
| Adding APIs | `documentation/dev/adding-apis.md` |
| API Design | `documentation/dev/api-design.md` |
| Error Handling | `documentation/dev/error-handling.md` |
| Debugging | `documentation/dev/debugging-methodology.md` |
| NuGet Packages | `documentation/dev/packages.md` |
| Release Notes & API Changelogs | `documentation/dev/release-notes-and-changelogs.md` |
