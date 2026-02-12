# Debugging Methodology for Complex Issues

This document outlines best practices for debugging complex cross-platform build failures and other intricate issues in SkiaSharp. These guidelines help avoid common pitfalls that lead to wasted time and compounding errors.

## Core Principles

### 1. Understand Before Fixing

**Problem**: Jumping to fixes when errors appear rather than understanding the system first.

**Solution**: Before any fix, answer these questions:
1. What is the expected behavior?
2. What is the actual behavior?
3. When did this start? What changed?
4. Why does it happen on platform X but not platform Y?

### 2. Establish Baseline First

Before any investigation, determine:
- What's the known-good state/commit?
- What's currently working vs failing?
- What changed since the last known-good state?

### 3. Track Changes and Effects

Maintain a running log during debugging sessions:

| Time/Commit | Change Made | Result | Notes |
|-------------|-------------|--------|-------|
| baseline | none | macOS ✓, Windows ✗ | |
| commit abc | added define X | macOS ✗, Windows ✗ | X broke macOS! |

Never lose track of cause and effect.

## Platform-Specific Issues

### Use Differences as Diagnostic Signals

When platform X works and platform Y fails, the difference between X and Y IS the answer. Focus investigation there.

**Example**: If ARM64 builds pass but x86/x64 fail, immediately ask "what's x86-specific?" (e.g., AVX2, SSE instructions, different compiler flags).

### Map Conditional Compilation

For cross-platform issues, understand exactly which code paths are active on each platform:

1. **Draw the decision tree** for preprocessor directives
2. **Evaluate each branch** for EACH platform/configuration
3. **Don't proceed** until you can explain what code path each platform takes

**Example of tracing preprocessor logic**:
```c
// cpu.h - trace this for each platform:
#if defined(_MSC_VER) && _MSC_VER >= 1700    // Windows MSVC/clang-cl: YES
#define WEBP_MSC_AVX2                         // macOS clang: NO
#endif

#if defined(__AVX2__) || defined(WEBP_MSC_AVX2)  // Evaluate for each platform
#define WEBP_USE_AVX2
#endif
```

### Verify Compiler Behavior

Different compilers define different macros:
- **clang** (macOS/Linux): Does NOT define `_MSC_VER` or `__AVX2__` by default
- **clang-cl** (Windows): Defines `_MSC_VER` for MSVC compatibility
- **MSVC**: Defines `_MSC_VER`, may define `__AVX2__` with `/arch:AVX2`

**Don't assume** - verify with a minimal test or documentation if uncertain.

## Common Pitfalls

### Pitfall 1: `#if defined(X)` vs `#if X`

```c
#define FEATURE 0

#if defined(FEATURE)   // TRUE - macro EXISTS
  // This code IS compiled!
#endif

#if FEATURE            // FALSE - value is 0
  // This code is NOT compiled
#endif
```

Setting `FEATURE=0` does NOT disable code guarded by `defined(FEATURE)`.

### Pitfall 2: Defensive Broad Fixes

If you need to apply a fix to "all platforms just to be safe," you probably don't understand the problem yet. Broad fixes:
- Obscure the root cause
- May have unintended side effects
- Make future debugging harder

**Prefer surgical fixes** that target exactly the affected platforms.

### Pitfall 3: Parallel Operations on Shared Resources

Running parallel builds that write to the same output file causes race conditions. When building multiple architectures:
- Build sequentially if they share output paths, OR
- Ensure output paths are distinct

### Pitfall 4: Dismissing Errors Without Explanation

Never say an error is "safe to ignore" without explaining exactly WHY. If you can't explain why it's safe, it's not safe.

## Debugging Process Checklist

### Before Starting
- [ ] What is the known-good baseline?
- [ ] What changed since baseline?
- [ ] Can we reproduce the issue locally?

### During Investigation
- [ ] Am I tracking changes and their effects?
- [ ] Have I traced conditional code paths for ALL affected platforms?
- [ ] Am I using success/failure patterns as diagnostic signals?

### Before Implementing a Fix
- [ ] Can I explain WHY the fix will work?
- [ ] Have I tested the hypothesis minimally first?
- [ ] Is this a surgical fix or a broad defensive fix?
- [ ] Have I verified any claims about compiler/platform behavior?

### After Implementing
- [ ] Did I test one change at a time?
- [ ] Did the fix work on ALL affected platforms?
- [ ] If things got worse, did I immediately revert and re-analyze?

## Hypothesis-Driven Debugging

State hypotheses explicitly and test them:

1. **Hypothesis**: "I believe X happens because Y"
2. **Test**: "We can verify by doing Z"
3. **Result**: Do Z, observe result
4. **Conclusion**: Confirm or revise hypothesis

**Example**:
- **Hypothesis**: "Windows fails because clang-cl defines `_MSC_VER`, triggering AVX2 code paths"
- **Test**: "Check if `_MSC_VER` is defined by examining the preprocessor output"
- **Result**: Confirmed - clang-cl defines `_MSC_VER=1900`
- **Conclusion**: Hypothesis confirmed, fix should target the AVX2 code path on Windows

## When Things Go Wrong

### If a Fix Makes Things Worse
1. **Stop immediately**
2. **Revert the change**
3. **Re-analyze** - your mental model is wrong
4. **Don't pile more fixes on top** of a broken fix

### If You're Going in Circles
1. **Step back** and re-read all the evidence
2. **Create a fresh tracking table**
3. **Ask clarifying questions** - maybe context is missing
4. **Request artifacts** (build logs, binaries) to verify assumptions

---

## Native Library Debugging (Linux)

### Investigating `undefined symbol` Errors

When you see `undefined symbol: xxx` errors, the symbol is missing from the linked libraries.

#### Step 1: Compare DT_NEEDED between working and broken builds

```bash
# Compare linked libraries between platforms
docker run --rm -v $(pwd):/work debian:bookworm-slim bash -c \
  "apt-get update -qq && apt-get install -y -qq binutils >/dev/null && \
   echo '=== x64 ===' && readelf -d /work/output/native/linux/x64/libSkiaSharp.so | grep NEEDED && \
   echo && echo '=== ARM64 ===' && readelf -d /work/output/native/linux/arm64/libSkiaSharp.so | grep NEEDED"
```

**If a library appears in one but not the other, that's your root cause.**

#### Step 2: Check if the linker is silently failing

The ninja file may have `-lfoo` but the linker silently skips it if it can't find the library:

```bash
# Check ninja file for expected libraries
grep "libs = " externals/skia/out/linux/arm64/obj/SkiaSharp.ninja

# Check if library exists in cross-compile sysroot
docker run --rm skiasharp-linux-gnu-cross-arm64 bash -c \
  "ls -la /usr/aarch64-linux-gnu/lib/libfontconfig*"
```

**Common issue:** The `-dev` package provides a broken symlink (`libfoo.so -> libfoo.so.1.2.3`)
but the actual `.so.1.2.3` file is in the runtime package (`libfoo1`), not the dev package.

#### Step 3: Fix location depends on root cause

| Root Cause | Fix Location |
|------------|--------------|
| Library missing from linker flags | `native/linux/build.cake` or `externals/skia/third_party/BUILD.gn` |
| Library missing from cross-compile sysroot | `scripts/Docker/debian/clang-cross/*/Dockerfile` |
| Indirect dependency (A→B→C missing) | Fix B's linkage or add C explicitly |

### Real Example: ARM64 fontconfig issue (#3369)

**Symptom:** `undefined symbol: uuid_generate_random` on ARM64 only

**Investigation:**
- x64 had `libfontconfig.so.1` in DT_NEEDED
- ARM64 was missing `libfontconfig.so.1` in DT_NEEDED  
- But ninja file had `-lfontconfig` for BOTH builds

**Root cause:** Cross-compile Docker only had `libfontconfig1-dev` which provides a broken symlink.
The actual shared library is in `libfontconfig1` (runtime package).

**Fix:** Download both `-dev` (headers) AND runtime (actual .so) packages in the Dockerfile.

---

## Summary

| Do | Don't |
|----|-------|
| Establish baseline first | Jump to fixing immediately |
| Track changes and effects | Lose track of what changed when |
| Trace conditional code completely | Skim for keywords |
| Use platform differences as clues | Ignore success patterns |
| Make one change at a time | Batch multiple changes |
| Verify claims with evidence | State assumptions as facts |
| Explain why errors are safe to ignore | Dismiss errors without explanation |
| Revert when fixes make things worse | Pile more fixes on top |
