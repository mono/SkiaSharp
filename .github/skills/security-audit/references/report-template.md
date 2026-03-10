# Security Audit Report Template

Templates for formatting security audit findings.

## Summary Section

```markdown
## Security Audit Report

**Date:** {date}
**Audited:** mono/SkiaSharp native dependencies

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

Use **separate tables per item** to avoid terminal wrapping issues:

```markdown
### 1. ✅ libexpat — Ready to merge

| Field | Value |
|-------|-------|
| Issues | #3389, #3425 |
| CVEs | CVE-2025-59375 (HIGH 7.5), CVE-2024-50602 (Medium 5.9) |
| Current | 2.7.3 |
| Latest | 2.7.3 |
| PR | #3458 (CI passing) |

**Action:** Merge when CI passes

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

### 3. 🆕 brotli — Undiscovered CVE

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

### 4. ⚪ zlib (MiniZip) — False positive

| Field | Value |
|-------|-------|
| Issue | #3285 |
| CVE | CVE-2023-45853 |
| Status | Not vulnerable |

**Reason:** CVE affects MiniZip only. Skia does not compile or link MiniZip.

**Action:** Close issue with explanation
```

## Status Legend

| Status | Meaning | Action |
|--------|---------|--------|
| ✅ Ready to merge | PR exists, fixes CVE, CI passing | Merge |
| 🟡 In progress | PR exists but needs work (outdated, CI failing) | Update PR |
| 🔴 Needs attention | User-reported issue, no PR | Create PR |
| 🆕 Undiscovered | CVE found proactively, no issue/PR | Create issue + PR |
| ⚪ False positive | CVE doesn't affect SkiaSharp | Close with explanation |

## Priority Order

Report findings in this order:

1. **🔴 User-reported + no PR** → Users are waiting
2. **✅ User-reported + PR ready** → Quick wins
3. **🟡 User-reported + PR needs work** → Unblock these
4. **🆕 Undiscovered CVEs** → Proactive fixes
5. **⚪ False positives** → Close with explanation

## Recommendations Section

End the report with specific next steps:

```markdown
### Next Steps

1. **Merge PR #3458** — libexpat update is ready
2. **Update libwebp PR** — Current targets 1.4.0, latest is 1.6.0
3. **Create PR for libpng** — Run: `bump libpng to 1.6.44`
4. **Close #3285** — zlib/MiniZip CVE is false positive
```
