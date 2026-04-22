# Validation Agent Prompt Template

Use this prompt when launching the Phase 3 validation agent. Substitute the milestone
numbers and paste your breaking change analysis table.

---

I'm updating SkiaSharp's Skia submodule from m{CURRENT} to m{TARGET}.
Here is the breaking change analysis I produced: [paste analysis table]

Please validate by (run from externals/skia):
1. Run: git diff upstream/chrome/m{CURRENT}..upstream/chrome/m{TARGET} --stat -- src/ include/
   Count the files changed and compare to my analysis — did I miss any?
2. For each HIGH/MEDIUM item I identified, verify the C API impact by grepping src/c/ include/c/
3. Check for changes I may have filtered as "Graphite-only" that actually affect Ganesh:
   - Search shared GPU headers (include/gpu/GpuTypes.h, include/gpu/*.h outside ganesh/ and graphite/)
   - For each new type, grep include/gpu/ganesh/ to see if Ganesh consumes it
4. Check for removed/moved headers that our C API includes:
   grep -rh '#include' src/c/*.cpp | sort -u
   Then verify each included header still exists at upstream/chrome/m{TARGET}
5. **Struct size audit**: Check every `static_assert(sizeof(...))` in src/c/sk_structs.cpp.
   For each asserted C++ struct, compare the target milestone's definition against our
   C API struct in include/c/sk_types.h. Flag any struct that gained or lost fields.
6. **Deleted file audit**: For each file deleted between milestones
   (git diff --diff-filter=D --name-only), check if our C API references it (#include
   or uses its types). For referenced deletions, search the target branch for where the
   content moved (git ls-tree -r upstream/chrome/m{TARGET} --name-only | grep STEM).
7. **Removal verification**: For any symbol claimed "removed" in the analysis, verify it
   is truly absent from the target branch (not just moved within the file):
   git show upstream/chrome/m{TARGET}:PATH | grep SYMBOL
8. Report: missed items, incorrect classifications, and confirmed items
