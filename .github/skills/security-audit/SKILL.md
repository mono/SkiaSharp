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
2. Get and VERIFY versions from submodule/DEPS (not just cgmanifest.json)
   ├─ 2a. Verify Skia milestone from SkMilestone.h + find upstream google/skia commit
   ├─ 2b. Verify dep versions from DEPS commit hashes + header files
   └─ 2c. Report any cgmanifest.json mismatches as findings
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

### Step 2: Get and Verify Dependency Versions

> ⚠️ **CRITICAL: Never trust cgmanifest.json blindly.** Always verify versions against the actual submodule and DEPS file. cgmanifest.json is manually maintained and can drift.

#### 2a. Verify Skia milestone and upstream commit

The mono/skia fork contains both upstream google/skia code AND custom SkiaSharp C API commits. Track both:

```bash
# 1. Get the actual submodule commit
git submodule status externals/skia
# Output: 8c99e432... externals/skia (the mono/skia fork commit)

# 2. Read the REAL milestone from the source
cat externals/skia/include/core/SkMilestone.h
# Look for: #define SK_MILESTONE NNN

# 3. Find the upstream google/skia merge point
cd externals/skia
git log --oneline --merges --grep="chrome/m" -5 HEAD
# Find the merge commit that brought in chrome/mNNN

# 4. Add the upstream remote and verify
git remote add upstream https://github.com/google/skia.git 2>/dev/null
git fetch upstream chrome/mNNN --depth=1
git log --format="%H %s" -1 FETCH_HEAD
# This gives the upstream_merge_commit

# 5. Confirm upstream is ancestor of our fork
git merge-base --is-ancestor FETCH_HEAD <merge-parent> && echo "VERIFIED"
```

**Compare against cgmanifest.json and report mismatches:**

| Field | Source of truth | cgmanifest.json field |
|-------|----------------|----------------------|
| Milestone | `SkMilestone.h` in submodule | `chrome_milestone` |
| Fork commit | `git submodule status` | git entry `commitHash` |
| Upstream commit | `git fetch upstream chrome/mNNN` tip | `upstream_merge_commit` |

#### 2b. Verify third-party dependency versions

Read the DEPS file from the actual submodule commit (NOT from cgmanifest.json):

```bash
cat externals/skia/DEPS
# Extract commit hashes for each dependency
```

Then verify actual versions from the Chromium mirror header files. For each dependency, fetch the version header at the pinned commit:

| Dependency | Version file | Version pattern |
|------------|-------------|-----------------|
| libpng | `png.h` line 1-3 | `libpng version X.Y.Z` |
| freetype | `include/freetype/freetype.h` | `FREETYPE_MAJOR`, `FREETYPE_MINOR`, `FREETYPE_PATCH` |
| harfbuzz | `src/hb-version.h` | `HB_VERSION_STRING "X.Y.Z"` |
| libexpat | `expat/lib/expat.h` | `XML_MAJOR_VERSION`, `XML_MINOR_VERSION`, `XML_MICRO_VERSION` |
| brotli | `c/common/version.h` | `BROTLI_VERSION_MAJOR`, `_MINOR`, `_PATCH` |
| zlib | `zlib.h` | `ZLIB_VERSION "X.Y.Z"` |
| libjpeg-turbo | `README.chromium` | `Version: X.Y.Z` |
| libwebp | `NEWS` line 1 | `version X.Y.Z` |

**Googlesource mirror URL pattern:**
```
https://{host}/{path}/+/{commit_sha}/{file}?format=TEXT
```
Response is base64-encoded. Decode with `[System.Convert]::FromBase64String()`.

If submodule externals are initialized, read directly from `externals/skia/third_party/externals/{dep}/` instead.

**Report any mismatches** between cgmanifest.json and actual versions as findings in the audit report.

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

Then classify using the **verified milestone from SkMilestone.h** (not cgmanifest.json):

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
