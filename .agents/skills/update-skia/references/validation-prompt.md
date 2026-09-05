# Validation Agent Prompt Template

Use this prompt when launching the Phase 3 validation agent. Substitute every placeholder.

---

I'm updating SkiaSharp's Skia submodule from exact upstream commit `{BASE_UPSTREAM_SHA}` to
`{TARGET_UPSTREAM_SHA}`. The exact mono/skia fork base is `{SKIA_BASE_SHA}`.
Here is the breaking-change analysis file: `{ANALYSIS_FILE}`.
Here is the provisional fork-vs-target dependency decision file: `{DEPENDENCY_FILE}`.

Please validate by (run from `externals/skia`):
1. Keep this review read-only. Inspect explicit revisions with `git log`, `git show`, `git diff`,
   `git ls-tree`, and search commands only. Do not checkout, switch, restore, stash, reset, fetch,
   merge, edit, create, delete, or stage repository files. Return an item as unverified when a
   read-only check cannot prove it.
2. Compare the analysis against changed source, public header, module, and build-list paths in
   `{BASE_UPSTREAM_SHA}..{TARGET_UPSTREAM_SHA}`. Report only relevant omissions; do not echo the
   exhaustive file list.
3. For each HIGH/MEDIUM item, verify C API impact in `src/c/` and `include/c/`, including changed
   wrapped factory/context behavior, new null preconditions, required fields, and removed fallbacks.
4. Check shared GPU changes across both shipped families:
   - Search shared GPU headers (`include/gpu/GpuTypes.h`, and `include/gpu/*.h` outside `ganesh/`
     and `graphite/`).
   - For each new type, grep `include/gpu/ganesh/` and `include/gpu/graphite/` to see which consume it.
5. Check removed or moved headers and symbols referenced by the fork-base C API. Verify apparent
   removals against exact target commit `{TARGET_UPSTREAM_SHA}` rather than trusting deleted or
   reordered diff lines.
6. Check every `static_assert(sizeof(...))` in `src/c/sk_structs.cpp`.
   For each asserted C++ struct, compare the target milestone's definition against our
   C API struct in `include/c/sk_types.h`. Flag any struct that gained or lost fields.
7. Recompute the dependency diff from the exact fork base:
   `git diff {SKIA_BASE_SHA}..{TARGET_UPSTREAM_SHA} -- DEPS`.
   Confirm the decision report accounts separately for every revision and enabled/commented-state
   difference. Flag any accept-target decision whose only evidence is that the dependency remains
   active or newer. For compatibility-sensitive rolls that also change consuming source, inspect
   the roll commit and verify the selected revision exposes the required API.
8. Report only missed items, incorrect classifications, unsafe dependency decisions, and one
   concise `PASS` or `UNVERIFIED` checklist entry for checks 2–7 with exact-SHA evidence. Do not
   restate the primary analysis.
