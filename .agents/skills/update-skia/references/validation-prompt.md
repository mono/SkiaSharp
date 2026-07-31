# Validation Agent Prompt Template

Use this prompt when launching the Phase 3 validation agent. Substitute the milestone
numbers and paste your breaking change analysis table.

---

I'm updating SkiaSharp's Skia submodule over this authoritative upstream range:
`{DIFF_RANGE}`.
Here is the breaking-change analysis file: `{ANALYSIS_FILE}`.
Here is the provisional fork-vs-target dependency decision file: `{DEPENDENCY_FILE}`.

Please validate by (run from `externals/skia`):
1. Keep this review read-only. Inspect explicit revisions with `git log`, `git show`, `git diff`,
   `git ls-tree`, and search commands only. Do not checkout, switch, restore, stash, reset, fetch,
   merge, edit, create, delete, or stage repository files. Return an item as unverified when a
   read-only check cannot prove it.
2. Run: `git diff {DIFF_RANGE} --stat -- src/ include/`
   Count the files changed and compare to my analysis — did I miss any?
3. For each HIGH/MEDIUM item I identified, verify the C API impact by grepping `src/c/` and
   `include/c/`.
4. Check GPU changes across both shipped families — Ganesh and Graphite:
   - Search shared GPU headers (`include/gpu/GpuTypes.h`, and `include/gpu/*.h` outside `ganesh/`
     and `graphite/`).
   - For each new type, grep `include/gpu/ganesh/` and `include/gpu/graphite/` to see which consume it.
5. Check for removed/moved headers that our C API includes:
   `grep -rh '#include' src/c/*.cpp | sort -u`
   Then verify each included header still exists at the target commit
6. **Struct size audit**: Check every `static_assert(sizeof(...))` in `src/c/sk_structs.cpp`.
   For each asserted C++ struct, compare the target milestone's definition against our
   C API struct in `include/c/sk_types.h`. Flag any struct that gained or lost fields.
7. **Deleted file audit**: For each file deleted between milestones
   (`git diff --diff-filter=D --name-only`), check if our C API references it (`#include`
   or uses its types). For referenced deletions, search the target branch for where the
   content moved (`git ls-tree -r {TARGET_UPSTREAM_REF} --name-only | grep STEM`).
8. **Removal verification**: For any symbol claimed "removed" in the analysis, verify it
   is truly absent from the target branch (not just moved within the file):
   `git show {TARGET_UPSTREAM_REF}:PATH | grep SYMBOL`
9. For wrapped factory/context-creation paths, inspect implementation changes and identify
   new null-return preconditions, required context fields, or removed default/fallback behavior.
   An unchanged public signature is not proof of unchanged behavior.
10. Recompute the dependency diff from the fork base, not from the previous upstream milestone:
   `git diff origin/{SKIA_BASE_BRANCH}..{TARGET_UPSTREAM_REF} -- DEPS`.
   Confirm the decision report accounts for every revision and enabled/commented-state difference.
11. For every compatibility-sensitive decision, and every dependency roll in `{DIFF_RANGE}` that
   also changes consuming source, inspect the roll commit with
   `git log -S "<target revision>" {DIFF_RANGE} -- DEPS`. Verify the chosen revision exposes the
   fields/functions used by target source; list any decision that cannot be proven read-only.
12. Report only missed items, incorrect classifications, unsafe dependency decisions, and a
    concise confirmation checklist. Do not restate the primary analysis.
