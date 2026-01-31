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

> **Why check closed items?** PRs may merge during your audit.
> 
> **Example:** During Jan 2026 audit, libwebp PR #3478 was merged on 2026-01-30 but initial search only found open issues. Checking closed PRs revealed the issue was already resolved.

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

**Real example (CVE-2025-27363):**
- **Blog posts claimed:** FreeType ≤2.13.3 affected
- **NVD/Red Hat showed:** Only ≤2.13.0 affected, fix in 2.13.4
- **Conclusion:** SkiaSharp's 2.13.3 was NOT vulnerable
- **Lesson:** Always trust NVD/Red Hat over blogs

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

## Step 5: False Positive Check

Before flagging, verify CVE affects SkiaSharp:

- [ ] Check [dependencies.md#known-false-positives](../../../../documentation/dependencies.md#known-false-positives)
- [ ] For zlib: Is it MiniZip-related? (Not compiled in SkiaSharp)
- [ ] For multi-component packages: Is vulnerable component used?

**Known false positives:**
- zlib CVE-2023-45853 (MiniZip only)
- FreeType's bundled zlib (separate from Skia's zlib)

---

## Step 6: Generate Report

- [ ] Use [report-template.md](report-template.md) for formatting
- [ ] Follow priority order: User-reported → PR ready → Undiscovered → False positives
- [ ] Document verification methodology (which sources consulted)
- [ ] Include source references (NVD, Red Hat, OpenCVE)
- [ ] Create corrections document if initial findings changed

---

## Step 7: Quality Assurance

Before finalizing:

- [ ] All CVE numbers correct
- [ ] All version numbers match cgmanifest.json
- [ ] All PR/issue numbers and statuses current
- [ ] Check if any PRs merged during audit (re-verify status)
- [ ] NVD/Red Hat consulted for CRITICAL/HIGH items
- [ ] Document corrections if needed

---

## Common Mistakes (From Past Audits)

### Mistake 1: Trusted Secondary Sources
**Example:** Reported freetype CVE-2025-27363 as CRITICAL affecting 2.13.3 based on blogs. NVD showed only ≤2.13.0 vulnerable.

**Prevention:** Always check NVD/Red Hat FIRST before trusting security blogs.

### Mistake 2: Missed Recently Completed Work
**Example:** Reported libwebp as "in progress" when PR #3478 was already merged during the audit.

**Prevention:** Check closed/merged PRs from last 30 days via GitHub API.

### Mistake 3: Premature CRITICAL Classification
**Example:** Reported CRITICAL based on web search before verifying with NVD.

**Prevention:** Complete NVD verification BEFORE assigning severity.

---

## Post-Audit Actions

- [ ] Store memory about lessons learned
- [ ] Document new false positives discovered
- [ ] Update dependencies.md if new patterns found

---

**Last Updated:** 2026-01-31 (Jan 2026 audit lessons)
