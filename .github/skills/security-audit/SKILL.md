---
name: security-audit
description: >
  Audit SkiaSharp's native dependencies for security vulnerabilities and CVEs.
  Read-only investigation that produces a status report with recommendations.

  Use when user asks to:
  - Audit security issues or CVEs
  - Check CVE status across dependencies
  - Find security-related issues and their PR coverage
  - Get an overview of open vulnerabilities
  - See what security work is pending

  Triggers: "security audit", "audit CVEs", "CVE status", "what security issues are open",
  "check vulnerability status", "security overview", "what CVEs need fixing".

  This skill is READ-ONLY. To actually fix issues, use the `native-dependency-update` skill.
---

# Security Audit Skill

Investigate security status of SkiaSharp's native dependencies. Skia core is a dependency just like libpng or freetype — all are audited together in a unified report.

> ℹ️ This skill is **read-only**. To create PRs and fix issues, use the `native-dependency-update` skill.

## Key References

- **[documentation/dev/dependencies.md](../../../documentation/dev/dependencies.md)** — Which dependencies to audit, cgmanifest format, known false positives, Skia-specific tracking notes
- **[references/report-template.md](references/report-template.md)** — Report format templates

## Workflow

```
1. Search issues/PRs (all deps including Skia)
2. Get versions from cgmanifest.json / DEPS
3. Query CVE databases for ALL dependencies
   ├─ Third-party deps: web search "{dep} CVE {year}"
   └─ Skia core: NVD API keywordSearch=Skia
4. Verify fix commits for each CVE
   ├─ Fixed? → Mark clean
   └─ Not fixed? → Flag for action
5. Check false positives
6. Generate unified report (all deps together, sorted by priority/severity)
```

### Step 1: Search Issues & PRs

Search mono/SkiaSharp open issues for:
- CVE numbers (e.g., "CVE-2024")
- Keywords: "security", "vulnerability"
- Dependency names: skia, libpng, expat, zlib, webp, harfbuzz, freetype

Search PRs in both mono/SkiaSharp and mono/skia for dependency updates.

### Step 2: Get Dependency Versions

For third-party dependencies:

```bash
cd externals/skia/third_party/externals/{dep}
git describe --tags --always
```

For Skia core, read `cgmanifest.json` and find the entry with `"name": "skia"`. Extract:
- `chrome_milestone` — the integer milestone number (e.g., `119`)
- `upstream_merge_commit` — the SHA of the upstream merge point

If these fields are missing, determine the milestone from the Skia submodule:

```bash
cd externals/skia
git log --oneline --merges --grep="upstream/chrome" -1
```

Only audit **security-relevant** dependencies (see [dependencies.md](../../../documentation/dev/dependencies.md#security-relevant-process-untrusted-input)).

### Step 3: Query CVE Databases

Query CVEs for **all** dependencies in parallel — Skia core and third-party alike.

#### Third-party dependencies

```
"{dependency} CVE {current year}"
"{dependency} security vulnerability"
```

#### Skia core

> ⚠️ **Skia CVEs are invisible to Component Governance.** The NVD query is the ONLY way to detect them.

```bash
curl -s "https://services.nvd.nist.gov/rest/json/cves/2.0?keywordSearch=Skia&resultsPerPage=200"
```

> **Note:** No API key is required but rate limit is 5 requests per 30 seconds without one.

For each Skia CVE returned, extract:
1. **CVE ID** and **description**
2. **Severity** (CVSS score from `metrics.cvssMetricV31`)
3. **Chrome fix version** from `configurations[].nodes[].cpeMatch[]` where `criteria` contains `chrome` and `versionEndExcluding` exists
4. **Chrome fix milestone** = major version number from `versionEndExcluding` (e.g., `132.0.6834.83` → `132`)

Then classify:

| Condition | Classification |
|-----------|---------------|
| Fix milestone > our milestone | 🔴 **Potentially affected** — CVE was fixed after our fork point |
| Fix milestone < our milestone | ✅ **Already fixed** — fix is included in our fork's upstream base |
| Fix milestone == our milestone | ⚠️ **Manual review needed** — fix may have been backported to the milestone branch AFTER our merge point. Verify with commit-level check against `upstream_merge_commit` |
| No Chrome version (Android-specific, mentions `SkiaRenderEngine`) | ⚪ **Not applicable** — Android render engine, not in SkiaSharp |
| No Chrome version (other Skia code) | ⚠️ **Manual review needed** — check if affected code path exists in fork |

### Step 4: Verify Fix Commits (CRITICAL)

> ⚠️ **CVE databases often have WRONG version ranges.** Always verify.

```bash
cd externals/skia/third_party/externals/{dependency}

# Check if fix commit is ancestor of current HEAD
git merge-base --is-ancestor {fix_commit} HEAD && echo "FIXED" || echo "VULNERABLE"
```

**Example:** CVE-2025-27363 claimed FreeType ≤2.13.3 was affected, fix in 2.13.4. Verification showed the fix commit was in 2.13.1 — SkiaSharp's 2.13.3 was already patched.

### Step 5: Check False Positives

Before flagging, verify the CVE actually affects SkiaSharp:
- **MiniZip** (in zlib) — Not compiled, not vulnerable
- **FreeType's bundled zlib** — Separate from Skia's zlib
- **Android SkiaRenderEngine** — Not part of SkiaSharp
- **Chrome-specific rendering paths** (HTML Canvas, SVG in browser) — May not be reachable through SkiaSharp's C API

See [dependencies.md](../../../documentation/dev/dependencies.md#known-false-positives) for details.

### Step 6: Generate Report

Use [references/report-template.md](references/report-template.md).

The report is a **single unified list** of all dependencies sorted by priority and severity. Skia core appears alongside libpng, freetype, etc. — not in a separate section.

**Priority order (applies equally to Skia core and third-party deps):**
1. 🔴 User-reported + no PR
2. ✅ User-reported + PR ready  
3. 🟡 User-reported + PR needs work
4. 🆕 Undiscovered CVEs
5. ⚪ False positives

Within each priority level, sort by severity (CRITICAL > HIGH > MEDIUM > LOW).

## Handoff

After audit, use `native-dependency-update` skill:
- "Merge PR #3458"
- "Update libwebp to 1.6.0"
- "Bump libpng to fix CVE-2024-XXXXX"

For Skia core CVEs, the fix requires merging a newer upstream milestone into the fork.
This is a significant undertaking — flag it in the report with the milestone gap.
