# Security Audit Execution Checklist

Actionable checklist for conducting security audits. **See [SKILL.md](../SKILL.md) for detailed rationale and examples.**

## Pre-Audit Setup

- [ ] Review [documentation/dependencies.md](../../../../documentation/dependencies.md) for current versions
- [ ] Check cgmanifest.json for tracked dependency versions
- [ ] Identify security-relevant dependencies (process untrusted input)

---

## Step 1: Issue/PR Search

**Check BOTH open AND recently closed items (last 30 days):**

- [ ] Search mono/SkiaSharp **open** issues for CVE keywords
- [ ] Search mono/SkiaSharp **closed** issues from last 30 days
- [ ] Search mono/SkiaSharp **open** PRs for dependency updates
- [ ] Search mono/SkiaSharp **closed/merged** PRs from last 30 days
- [ ] Check mono/skia repository for related PRs
- [ ] Use GitHub API to verify PR merge status and dates

> **Why?** PRs may merge during your audit. See SKILL.md Step 1 for examples.

---

## Step 2: CVE Research

**Use authoritative sources FIRST (see SKILL.md Step 3 for complete hierarchy):**

- [ ] **NVD (nvd.nist.gov)** - Search for CVE or dependency name
- [ ] **Red Hat Security Advisory** - Cross-reference findings
- [ ] **OpenCVE (app.opencve.io)** - Verify version ranges
- [ ] Document source, affected versions, fix version from EACH source

**Search queries:**
```
site:nvd.nist.gov "{dependency}" CVE-{year}
site:access.redhat.com/security "{dependency}" CVE-{year}
```

> **Rule:** If sources conflict, NVD/Red Hat are authoritative. See SKILL.md for CVE-2025-27363 example.

---

## Step 3: Verification

For each CVE:

- [ ] Record affected versions from NVD (e.g., "≤ 2.13.0")
- [ ] Record fix version from NVD (e.g., "2.13.4")
- [ ] Check SkiaSharp's current version in cgmanifest.json
- [ ] Compare: Is current version in affected range?
- [ ] Cross-reference with Red Hat/OpenCVE (should match NVD)
- [ ] If sources conflict, document and use NVD as authority

**Example verification table:**

| Source | Affected | Fix | Match? |
|--------|----------|-----|--------|
| NVD | ≤ 2.13.0 | 2.13.4 | Primary |
| Red Hat | ≤ 2.13.0 | 2.13.4 | ✓ |
| Blog X | ≤ 2.13.3 | 2.13.4 | ❌ Ignore |

**Git commit verification (if applicable):**
- [ ] Verify fix commit hash from CVE/vendor advisory
- [ ] Check if commit is ancestor: `git merge-base --is-ancestor {commit} HEAD`

---

## Step 4: Severity Classification

**Classify ONLY after NVD verification:**

- [ ] **🔴 CRITICAL** - Vulnerable per NVD + actively exploited
- [ ] **🔴 HIGH** - Vulnerable per NVD (not actively exploited)
- [ ] **🟡 MEDIUM** - Uncertain OR update recommended for defense in depth
- [ ] **🟢 LOW** - Not vulnerable, but update recommended
- [ ] **⚪ FALSE POSITIVE** - CVE doesn't apply (e.g., MiniZip not compiled)

> **Rule:** NEVER classify as CRITICAL without NVD/Red Hat confirmation.

---

## Step 5: False Positive Check

Before flagging, verify CVE affects SkiaSharp:

- [ ] Check [dependencies.md#known-false-positives](../../../../documentation/dependencies.md#known-false-positives)
- [ ] For zlib: Is it MiniZip-related? (Not compiled in SkiaSharp)
- [ ] For multi-component packages: Is vulnerable component used?

---

## Step 6: Generate Report

- [ ] Use [report-template.md](report-template.md) for formatting
- [ ] Follow priority order: User-reported → PR ready → Undiscovered → False positives
- [ ] Document verification methodology
- [ ] Include source references (NVD, Red Hat, OpenCVE)

---

## Quality Assurance

Before finalizing:

- [ ] All CVE numbers correct
- [ ] All version numbers match cgmanifest.json
- [ ] All PR/issue numbers and statuses current
- [ ] Check if any PRs merged during audit (re-verify status)
- [ ] NVD/Red Hat consulted for CRITICAL/HIGH items
- [ ] Document corrections if initial findings changed

---

## Common Mistakes

**See SKILL.md "Lessons Learned" section for detailed examples.**

- ❌ **Trusting blogs first** - Always verify with NVD/Red Hat
- ❌ **Missing recent PRs** - Check closed items from last 30 days
- ❌ **Premature CRITICAL** - Complete verification BEFORE classification

---

## Step 5: False Positive Check

Before flagging a CVE, verify it actually affects SkiaSharp:

- [ ] Check [dependencies.md#known-false-positives](../../../../documentation/dependencies.md#known-false-positives)
- [ ] For zlib CVEs: Check if MiniZip-related (not compiled in SkiaSharp)
- [ ] For multi-component packages: Verify the vulnerable component is actually used

**Known False Positives:**
- zlib CVE-2023-45853 (MiniZip only)
- FreeType's bundled zlib vulnerabilities (separate from Skia's zlib)

---

## Step 6: Report Generation

- [ ] Use [report-template.md](report-template.md) for formatting
- [ ] Document your verification methodology
- [ ] Include evidence table showing source verification
- [ ] List authoritative sources consulted (NVD, Red Hat, OpenCVE)
- [ ] If corrections needed, create separate corrections document

### Priority Order for Reporting

1. 🔴 User-reported + no PR (immediate action needed)
2. ✅ User-reported + PR ready (quick win)
3. 🟡 User-reported + PR needs work
4. 🆕 Undiscovered CVEs
5. ⚪ False positives (document why)

---

## Step 7: Quality Assurance

Before finalizing report:

- [ ] Double-check all CVE numbers are correct
- [ ] Verify all version numbers match cgmanifest.json
- [ ] Confirm all PR/issue numbers and statuses are current
- [ ] Check if any PRs merged during audit (re-verify status)
- [ ] Ensure authoritative sources (NVD, Red Hat) were consulted for CRITICAL/HIGH items
- [ ] Document any corrections made during verification

---

## Common Mistakes to Avoid

### ❌ Mistake 1: Trusting Secondary Sources
**Example:** Reporting "CVE affects version 2.13.3" because a blog post said so, when NVD says "≤ 2.13.0"

**Solution:** Always verify with NVD/Red Hat first. Blog posts often extrapolate incorrectly.

### ❌ Mistake 2: Missing Recent Activity
**Example:** Reporting dependency as "needs update" when PR was merged yesterday

**Solution:** Check closed/merged PRs from last 30 days via GitHub API.

### ❌ Mistake 3: Premature CRITICAL Classification
**Example:** Reporting CRITICAL based on web search before verifying with NVD

**Solution:** Complete NVD verification BEFORE assigning severity.

### ❌ Mistake 4: Not Documenting Corrections
**Example:** Silently changing report without noting what was corrected

**Solution:** If verification reveals errors, create corrections document showing before/after.

---

## Post-Audit

- [ ] Store memory about verification lessons learned
- [ ] Document any new false positives discovered
- [ ] Update dependencies.md if new patterns found
- [ ] Share corrections methodology if applicable

---

## Quick Reference: Source Trust Levels

| Trust Level | Sources | Use For |
|-------------|---------|---------|
| **PRIMARY** | NVD, Red Hat, OpenCVE, Tenable | Final determination of vulnerability |
| **SECONDARY** | Vendor advisories, GitHub Security | Supporting information |
| **TERTIARY** | Security blogs, news sites | Context only, always verify |

**Golden Rule:** When in doubt, trust NVD over everything else.

---

**Last Updated:** 2026-01-31 (Based on Jan 2026 audit lessons learned)
