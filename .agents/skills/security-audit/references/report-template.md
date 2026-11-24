# Security Audit Report Template

Templates for formatting security audit findings. All dependencies — including Skia core — use the same format.

## Summary Section

```markdown
## Security Audit Report

**Date:** {date}
**Audited:** mono/SkiaSharp native dependencies (including Skia core)

### Summary

| Status | Count |
|--------|-------|
| ✅ Ready to merge | N |
| 🟡 In progress | N |
| 🔴 Needs attention | N |
| 🆕 Undiscovered | N |
| ⚪ False positive | N |
```

## Detailed Findings

Use **separate tables per item** to avoid terminal wrapping issues.
All dependencies — Skia core and third-party — are listed together, sorted by priority:

```markdown
### 1. 🔴 skia — Needs attention

| Field | Value |
|-------|-------|
| Issues | None |
| CVEs | CVE-2024-1283 (CRITICAL 9.8), CVE-2025-0436 (HIGH 8.8), +7 more |
| Current | chrome/m119 |
| Min fix | chrome/m121 (for CRITICAL), chrome/m145 (for all) |
| NVD query | `keywordSearch=Skia` — 111 total, 9 potentially affected |

**Chrome-versioned CVEs (fix milestone > m119):**

| CVE | Fixed In | Severity | Description |
|-----|----------|----------|-------------|
| CVE-2024-1283 | m121 | CRITICAL 9.8 | Heap buffer overflow in Skia |
| CVE-2025-0436 | m132 | HIGH 8.8 | Integer overflow in Skia |

**Non-Chrome CVEs (manual review needed):**

| CVE | Severity | Description | Assessment |
|-----|----------|-------------|------------|
| CVE-2025-32318 | HIGH 8.8 | Heap buffer overflow in Skia | ⚠️ Needs review |
| CVE-2025-48630 | N/A | SkiaRenderEngine.cpp | ⚪ Android-only |

**Action:** Merge newer upstream milestone (minimum m121 for CRITICAL fix)

---

### 2. 🔴 libpng — Needs attention

| Field | Value |
|-------|-------|
| Issues | #1234 |
| CVEs | CVE-2024-XXXXX (HIGH 8.1) |
| Current | 1.6.40 |
| Min fix | 1.6.42 |
| Latest | 1.6.44 |
| PR | None |

**Action:** Create PR to update to 1.6.44

---

### 3. ✅ libexpat — Ready to merge

| Field | Value |
|-------|-------|
| Issues | #3389, #3425 |
| CVEs | CVE-2025-59375 (HIGH 7.5), CVE-2024-50602 (Medium 5.9) |
| Current | 2.7.3 |
| Latest | 2.7.3 |
| PR | #3458 (CI passing) |

**Action:** Merge when CI passes

---

### 4. 🆕 brotli — Undiscovered CVE

| Field | Value |
|-------|-------|
| Issues | None |
| CVEs | CVE-2024-YYYYY (Medium 5.5) |
| Current | 1.0.9 |
| Min fix | 1.1.0 |
| Latest | 1.1.0 |
| PR | None |

**Action:** Create issue, then create PR

---

### 5. ⚪ zlib (MiniZip) — False positive

| Field | Value |
|-------|-------|
| Issue | #3285 |
| CVE | CVE-2023-45853 |
| Status | Not vulnerable |

**Reason:** CVE affects MiniZip only. Skia does not compile or link MiniZip.

**Action:** Close issue with explanation
```

### Skia-Specific Formatting Notes

Skia CVEs use the same entry format as other dependencies, with two additions:
- **Chrome-versioned CVEs table** — Shows the milestone gap (fix milestone vs our fork milestone)
- **Non-Chrome CVEs table** — Shows CVEs that need manual review for reachability
- **Assessment guidance for each CVE:**
  - Check if the affected file/function exists in `externals/skia/` at the fork's commit
  - Check if the code path is reachable through SkiaSharp's C API
  - Android `SkiaRenderEngine` CVEs are typically false positives for SkiaSharp

---

## Status Legend

| Status | Meaning | Action |
|--------|---------|--------|
| ✅ Ready to merge | PR exists, fixes CVE, CI passing | Merge |
| 🟡 In progress | PR exists but needs work (outdated, CI failing) | Update PR |
| 🔴 Needs attention | User-reported issue, no PR | Create PR |
| 🆕 Undiscovered | CVE found proactively, no issue/PR | Create issue + PR |
| ⚪ False positive | CVE doesn't affect SkiaSharp | Close with explanation |

## Priority Order

Report findings in this order, regardless of whether the dependency is Skia core or third-party:

1. **🔴 User-reported + no PR** → Users are waiting
2. **✅ User-reported + PR ready** → Quick wins
3. **🟡 User-reported + PR needs work** → Unblock these
4. **🆕 Undiscovered CVEs** → Proactive fixes
5. **⚪ False positives** → Close with explanation

Within each priority level, sort by severity (CRITICAL > HIGH > MEDIUM > LOW).

## Recommendations Section

End the report with specific next steps:

```markdown
### Next Steps

1. **Merge upstream Skia m121** — Fixes CRITICAL CVE-2024-1283
2. **Merge PR #3458** — libexpat update is ready
3. **Update libwebp PR** — Current targets 1.4.0, latest is 1.6.0
4. **Create PR for libpng** — Run: `bump libpng to 1.6.44`
5. **Close #3285** — zlib/MiniZip CVE is false positive
```
