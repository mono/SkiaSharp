# Validation Agent Prompt Template

Use this prompt when launching the Phase 3 validation agent. Substitute the milestone
numbers and paste your breaking change analysis table.

---

I'm updating SkiaSharp's Skia submodule over this authoritative upstream range:
`{DIFF_RANGE}`.
Here is the breaking-change analysis file: `{ANALYSIS_FILE}`.

Please validate by (run from externals/skia):
1. Run: git diff {DIFF_RANGE} --stat -- src/ include/
   Count the files changed and compare to my analysis — did I miss any?
2. For each HIGH/MEDIUM item I identified, verify the C API impact by grepping src/c/ include/c/
3. Check for changes I may have incorrectly treated as backend-local that also affect Ganesh:
   - Search shared GPU headers (include/gpu/GpuTypes.h, include/gpu/*.h outside ganesh/ and graphite/)
   - For each new type, grep include/gpu/ganesh/ to see if Ganesh consumes it
4. Check for removed/moved headers that our C API includes:
   grep -rh '#include' src/c/*.cpp | sort -u
   Then verify each included header still exists at the target commit
5. **Struct size audit**: Check every `static_assert(sizeof(...))` in src/c/sk_structs.cpp.
   For each asserted C++ struct, compare the target milestone's definition against our
   C API struct in include/c/sk_types.h. Flag any struct that gained or lost fields.
6. **Deleted file audit**: For each file deleted between milestones
   (git diff --diff-filter=D --name-only), check if our C API references it (#include
   or uses its types). For referenced deletions, search the target branch for where the
   content moved (git ls-tree -r {TARGET_UPSTREAM_REF} --name-only | grep STEM).
7. **Removal verification**: For any symbol claimed "removed" in the analysis, verify it
   is truly absent from the target branch (not just moved within the file):
   git show {TARGET_UPSTREAM_REF}:PATH | grep SYMBOL
8. Recompute the dependency diff from the fork base, not from the previous upstream milestone:
   `git diff origin/{SKIA_BASE_BRANCH}..{TARGET_UPSTREAM_REF} -- DEPS`.
   Confirm the decision report accounts for every revision and enabled/commented-state difference.
9. For every accepted or preserved revision, find the upstream roll commit with
   `git log -S "<target revision>" {DIFF_RANGE} -- DEPS`, inspect source changes in that
   commit, and verify the chosen dependency revision exposes every field/function the target
   source uses. A pin roll coupled to wrapper-source changes is compatibility-sensitive.
10. Report: missed items, incorrect classifications, dependency decisions, and confirmed items.
