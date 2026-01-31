# Security Audit Best Practices Checklist

Use this checklist to avoid common mistakes when conducting security audits of SkiaSharp's native dependencies.

## Pre-Audit Setup

- [ ] Review [documentation/dependencies.md](../../../../documentation/dependencies.md) for current versions
- [ ] Check cgmanifest.json for tracked dependency versions
- [ ] Identify which dependencies are security-relevant (process untrusted input)

---

## Step 1: Issue/PR Search

**Check BOTH open AND recently closed items:**

- [ ] Search mono/SkiaSharp **open** issues for CVE keywords
- [ ] Search mono/SkiaSharp **closed** issues from last 30 days
- [ ] Search mono/SkiaSharp **open** PRs for dependency updates
- [ ] Search mono/SkiaSharp **closed/merged** PRs from last 30 days
- [ ] Check mono/skia repository for related PRs
- [ ] Use GitHub API to verify PR merge status and dates

**Why?** PRs may be merged during your audit. Always check recent activity.

---

## Step 2: CVE Research - Source Priority

**CRITICAL: Always use this source hierarchy:**

### Tier 1: Authoritative Sources (ALWAYS CHECK FIRST)

- [ ] **NVD (nvd.nist.gov)** - Search for `CVE-YYYY-NNNNN` or dependency name
- [ ] **Red Hat Security Advisory** - Check access.redhat.com/security
- [ ] **OpenCVE (app.opencve.io)** - Cross-reference CVE details
- [ ] **Vendor security pages** - Check official project security advisories

**Record findings:** Note the source, affected versions, and fix version from EACH source.

### Tier 2: Secondary Sources (Verify Against Tier 1)

- [ ] Security blogs (markaicode.com, vulert.com, datacipher.com, etc.)
- [ ] News aggregators (The Hacker News, etc.)
- [ ] Community posts (Stack Overflow, forums)

**⚠️ WARNING:** If Tier 2 sources conflict with Tier 1, **ALWAYS trust Tier 1 (NVD/Red Hat)**.

### Search Queries to Use

```
site:nvd.nist.gov "libpng" CVE-2025
site:access.redhat.com/security "freetype" CVE-2025
"CVE-2025-27363" site:nvd.nist.gov
```

---

## Step 3: Verification Process

For each CVE found:

- [ ] **Record affected versions** from NVD (e.g., "≤ 2.13.0")
- [ ] **Record fix version** from NVD (e.g., "2.13.4")
- [ ] **Check SkiaSharp's current version** in cgmanifest.json
- [ ] **Compare versions:** Is current version in affected range?
- [ ] **Cross-reference** with Red Hat and OpenCVE (should match NVD)
- [ ] If sources conflict, document the discrepancy and use NVD as authority

### If Using Git Commit Verification

- [ ] Verify the fix commit hash is mentioned in CVE or vendor advisory
- [ ] Check if commit is ancestor of current version
- [ ] Document the commit verification results

### Example Verification Table

| Source | Affected Versions | Fix Version | Notes |
|--------|-------------------|-------------|-------|
| NVD | ≤ 2.13.0 | 2.13.4 | Primary source |
| Red Hat | ≤ 2.13.0 | 2.13.4 | Matches NVD ✓ |
| OpenCVE | ≤ 2.13.0 | 2.13.4 | Matches NVD ✓ |
| Blog X | ≤ 2.13.3 | 2.13.4 | **CONFLICTS with NVD** ❌ |

**Decision:** Trust NVD/Red Hat. Current version 2.13.3 is NOT vulnerable.

---

## Step 4: Severity Classification

Use this classification ONLY after verification:

- [ ] **🔴 CRITICAL** - Current version IS vulnerable per NVD + actively exploited
- [ ] **🔴 HIGH** - Current version IS vulnerable per NVD (not actively exploited)
- [ ] **🟡 MEDIUM** - Current version MIGHT be vulnerable (uncertain/conflicting info) OR update recommended for defense in depth
- [ ] **🟢 LOW** - Current version NOT vulnerable, but future update recommended
- [ ] **⚪ FALSE POSITIVE** - CVE doesn't apply (e.g., MiniZip not compiled)

**NEVER classify as CRITICAL without NVD/Red Hat confirmation.**

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
