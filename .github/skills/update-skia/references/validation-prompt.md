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
3. Check for changes I may have filtered as "Graphite-only" that actually affect Ganesh
4. Check for removed/moved headers that our C API includes:
   grep -rh '#include' src/c/*.cpp | sort -u
   Then verify each included header still exists at upstream/chrome/m{TARGET}
5. Report: missed items, incorrect classifications, and confirmed items
