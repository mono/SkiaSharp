# Fix Anti-Patterns

Critical rules for the issue-fix skill. Violating any of these produces invalid fixes or broken builds.

## Critical Rules

### #1: ❌ Never skip validation
You MUST run `validate-fix.ps1` (or `.py` fallback) and see ✅ before persisting. Mentally checking fields is NOT validation. If you haven't seen ✅ from the script, the fix JSON is invalid.

### #2: ❌ Never skip or reorder phases
Phases 1→9 are strictly sequential. Each has a gate. Do not proceed past a gate until criteria are met. Do not say "in parallel."

### #3: ❌ Never commit to protected branches
Direct commits to `main` (SkiaSharp) or `main`/`skiasharp` (skia submodule) are policy violations. Always use a feature branch: `dev/issue-NNNN-description`.

### #4: ❌ Never claim "fix works" without running tests
`dotnet build` succeeding is NOT sufficient. Run `dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj` and verify pass. For performance fixes, also verify timing improvement with benchmarks.

### #5: ❌ Never skip writing regression tests
A fix without a regression test is incomplete. The test must fail without the fix and pass with it. Skip ONLY for hardware-dependent scenarios (GPU not available, specific device required).

### #6: ❌ Never confuse workaround with root cause
If your fix ADDS something new to compensate → probably a workaround. If your fix RESTORES something that SHOULD have been there → probably root cause. Ask: "Why does it work elsewhere?" The answer IS the root cause.

### #7: ❌ Never use `externals-download` after C API changes
If you modified any file in `externals/skia/src/c/` or `externals/skia/include/c/`, you MUST rebuild natives with `dotnet cake --target=externals-{platform}`. Downloaded binaries won't contain your new functions → `EntryPointNotFoundException` at runtime.

### #8: ❌ Never conclude "can't reproduce perf issue" from a console app
Performance bugs involving views (SKGLView, SKMetalView, SKCanvasView) CANNOT be reproduced in a console app. Console apps bypass the entire view rendering pipeline. Use the correct platform file from the repro skill.

### #9: ❌ Never record timing with VSync enabled
VSync caps frame rate to display refresh (60fps or 120fps), masking real performance. Always disable VSync before measuring. If all configurations show exactly 60fps or 120fps regardless of scene complexity, VSync is almost certainly enabled.

### #10: ❌ Never investigate without a debugging table
For any non-trivial investigation (especially performance), maintain a table of experiments: what changed → what resulted → what it proves. This prevents circular investigation and makes the process reviewable.

### #11: ❌ Never assert GL state without cross-checking
On macOS, `glGetIntegerv` can return incorrect values for the default framebuffer (e.g., `GL_STENCIL_BITS = 0` when 8 bits are allocated). Always cross-check GL queries against the pixel format or other authoritative source.
