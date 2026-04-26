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
- **[references/report-template.md](references/report-template.md)** — Report format templates (markdown)
- **[references/report-schema.md](references/report-schema.md)** — JSON schema for structured output
- **[scripts/render-security-audit.py](scripts/render-security-audit.py)** — Renders JSON → standalone HTML
- **[scripts/viewer.html](scripts/viewer.html)** — HTML template (Bootstrap 5)

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
6. Assemble structured JSON report (per report-schema.md)
7. Render HTML from JSON (render-security-audit.py)
8. Present markdown summary to user
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

> 🛑 **MANDATORY:** Steps 3–5 below (fetching the upstream google/skia branch) are **required**, not optional.
> Adding a git remote and fetching is a read-only operation — it does not modify any tracked files.
> Without independent verification of the upstream merge point, the audit would be trusting
> cgmanifest.json circularly, which defeats the purpose of verification.

The mono/skia fork contains both upstream google/skia code AND custom SkiaSharp C API commits. Track both:

```bash
# 1. Get the actual submodule commit
git submodule status externals/skia
# Output: 8c99e432... externals/skia (the mono/skia fork commit)

# 2. Read the REAL milestone from the source
cat externals/skia/include/core/SkMilestone.h
# Look for: #define SK_MILESTONE NNN

# 3. Find the upstream google/skia merge point (MANDATORY)
cd externals/skia
git log --oneline --merges --grep="chrome/m" -5 HEAD
# Find the merge commit that brought in chrome/mNNN

# 4. Add the upstream remote and fetch (MANDATORY — this is read-only)
git remote add upstream https://github.com/google/skia.git 2>/dev/null
git fetch upstream chrome/mNNN --depth=1
git log --format="%H %s" -1 FETCH_HEAD
# This gives the independently-verified upstream_merge_commit

# 5. Confirm upstream is ancestor of our fork (MANDATORY)
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

**Skia DEPS dependencies:**

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

#### 2c. Verify ANGLE dependencies

ANGLE is a **separate** native component (Windows-only, for WinUI). It is NOT part of the Skia submodule — it's cloned separately from `https://github.com/google/angle.git`.

Get the ANGLE version from `scripts/VERSIONS.txt`:
```bash
grep ANGLE scripts/VERSIONS.txt
# Output: ANGLE    release    chromium/NNNN
```

ANGLE has its own submodules that must also be tracked:
- `third_party/zlib` (separate from Skia's zlib)
- `third_party/jsoncpp`
- `third_party/vulkan-deps`
- `third_party/astc-encoder/src`

Check if these are in cgmanifest.json. If missing, flag as a coverage gap.

#### 2d. Build the Dependency Overview

The `versionVerification` array in the JSON must include **ALL** dependencies from ALL sources:

| Source | What to include |
|--------|----------------|
| `"Skia DEPS"` | All deps from `externals/skia/DEPS` + Skia itself |
| `"ANGLE"` | ANGLE itself (version from VERSIONS.txt) |
| `"ANGLE submodule"` | ANGLE's submodules (zlib, jsoncpp, vulkan-deps, astc-encoder) |
| `"GPU/Graphics"` | VulkanMemoryAllocator, SPIRV-Cross, D3D12Allocator from DEPS |
| `"Supporting"` | piex, wuffs, dng_sdk, buildtools from DEPS |

Each entry must have a `source` field and a `cgmanifestVersion` field (null if missing from cgmanifest.json).

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
| Fix milestone ≤ our milestone | ✅ **Already fixed** — fix is included in our fork's upstream base |
| No Chrome version (mentions `SkiaRenderEngine`) | ⚪ **Not applicable** — Android render engine infrastructure, not part of Skia library |
| No Chrome version (reported via Android/Huawei/other vendor bulletin) | ⚠️ **Code-path verification needed** — see below |
| No Chrome version (other) | ⚠️ **Manual review needed** — check if affected code path exists in fork |
| CVSS score not yet published | Use Chromium severity rating (High → treat as HIGH 8.8) |

> ⚠️ **Newly published CVEs may lack a CVSS score.** If NVD hasn't assigned one yet, check
> the Chromium severity rating from the Chrome release notes or cvedetails.com. Treat
> Chromium "High" as HIGH (~8.8) for prioritization purposes.

#### Non-Chrome Skia CVEs (Android/Huawei/vendor bulletins)

CVEs reported through Android Security Bulletins or vendor bulletins (Huawei HarmonyOS, etc.)
reference Skia code that **may also exist in upstream google/skia and therefore in our fork**.
These forks all diverged from the same upstream, so shared code paths are common.

**Do NOT dismiss a CVE just because it was reported through a vendor bulletin.**

Instead, verify whether the vulnerable code exists in our fork:

```bash
cd externals/skia

# 1. Check if the vulnerable file exists
find . -name "SkDeflate.cpp" -o -name "TheVulnerableFile.cpp"

# 2. Check if the vulnerable function exists
git grep "vulnerable_function_name"

# 3. If a fix commit is referenced (e.g., from AOSP), check if the
#    vulnerable code pre-fix exists in our fork
```

| If... | Then... |
|-------|---------|
| Vulnerable file/function does NOT exist in our fork | ⚪ False positive — vendor-specific code |
| Vulnerable file/function EXISTS in our fork + fix commit is ancestor of HEAD | ✅ Already fixed |
| Vulnerable file/function EXISTS in our fork + NOT fixed | 🔴 Needs attention |

### Step 4: Verify Fix Commits (CRITICAL)

> ⚠️ **CVE databases often have WRONG version ranges.** Always verify with the actual commit.

```bash
cd externals/skia/third_party/externals/{dependency}

# Check if fix commit is ancestor of current HEAD
git merge-base --is-ancestor {fix_commit} HEAD && echo "FIXED" || echo "VULNERABLE"
```

**Example:** CVE-2025-27363 claimed FreeType ≤2.13.3 was affected, fix in 2.13.4. Verification
showed the fix commit (`ef636696...`) was actually in 2.13.1 — SkiaSharp's 2.13.3 was already
patched. The NVD version range was wrong.

When a CVE's version range says our version is affected but the fix commit is already in our tree,
classify it as **⚪ False positive (NVD version range incorrect)** — not as a finding. Place it
in the false positive section with an explanation of why the NVD range is wrong and cite the
actual fix commit as evidence.

### Step 5: Check False Positives

Before flagging, verify the CVE actually affects SkiaSharp:

- **MiniZip** (in zlib) — Not compiled by Skia, not linked
- **FreeType's bundled zlib** — Separate from Skia's zlib copy
- **Android SkiaRenderEngine** (`SkiaRenderEngine.cpp`) — Android OS rendering infrastructure, not part of the Skia library itself. Always a false positive.
- **Android/vendor Skia forks** — CVEs from Android Security Bulletins or Huawei/Samsung bulletins may reference code in `platform/external/skia` (AOSP's fork) that doesn't exist in upstream `google/skia`. **Verify by checking if the affected file/function exists in our fork** (see Step 3 above). Don't dismiss based solely on which vendor reported it — the code could be shared.
- **Chrome-specific rendering paths** (HTML Canvas, SVG in browser) — May not be reachable through SkiaSharp's C API
- **NVD version range errors** — When a CVE claims version X is affected but the fix commit is already in version X's tree, classify as false positive and cite the fix commit (see Step 4)

See [dependencies.md](../../../documentation/dev/dependencies.md#known-false-positives) for details.

### Step 6: Assemble Structured JSON Report

> 🛑 **MANDATORY:** The audit MUST produce a JSON file conforming to [references/report-schema.md](references/report-schema.md). This is the machine-readable output used by dashboards and CI.

Build the JSON object with these top-level keys:

1. **`meta`** — Date, schema version, Skia commit hashes, milestone, upstream verification status
2. **`summary`** — Counts by status category, total CVEs, highest severity
3. **`versionVerification`** — One entry per dependency with DEPS commit, verified version, cgmanifest version, match boolean
4. **`findings`** — Array of finding objects sorted by priority then severity. Each has `dependency`, `status`, `cves[]`, `nonChromeCves[]`, `action`, `notes`
5. **`nextSteps`** — Prioritized action items with severity, command, and reason

Save as `output/ai/security-audit-{date}.json` in the repo (same pattern as other AI outputs).

### Step 7: Render HTML Report

> 🛑 **MANDATORY:** Always generate the HTML report. The human needs a readable dashboard.

```bash
python3 .claude/skills/security-audit/scripts/render-security-audit.py \
  output/ai/security-audit-{date}.json
```

This produces a self-contained HTML file (Bootstrap 5, no external dependencies except CDN CSS) alongside the JSON. The HTML renders:
- Summary cards with status counts
- Collapsible findings with CVE tables, severity badges, and NVD links
- Version verification table with match/mismatch indicators
- Skia upstream verification details with commit links
- Prioritized next steps with severity-coded borders

Present the output path to the user:
```
✅ security-audit-2026-04-10.html (45 KB)
   m132 • 2026-04-10 • 12 CVEs • Highest: HIGH
   🔴 3 attention · 🆕 2 undiscovered · ⚪ 4 FP · ✅ 5 clean
```

### Step 8: Present Markdown Summary

After generating JSON and HTML, present a concise markdown summary to the user in the conversation (using the report-template.md format). This is in ADDITION to the JSON+HTML files, not instead of them.

**Priority order (applies equally to Skia core and third-party deps):**
1. 🔴 User-reported + no PR
2. ✅ User-reported + PR ready  
3. 🟡 User-reported + PR needs work
4. 🆕 Undiscovered CVEs (proactively found, no user-filed issue)
5. ⚪ False positives

Within each priority level, sort by severity (CRITICAL > HIGH > MEDIUM > LOW).

#### Report quality rules

1. **Skia bump recommendations must target the highest-severity CVE**, not the lowest. If there are HIGH CVEs at m146 and a MEDIUM at m133, recommend m146 as the target, not m133.

2. **Don't include already-closed GitHub issues** in the report unless they are directly relevant to an open vulnerability. If a CVE is fixed and the tracking issue is closed, omit it.

3. **CVEs with NVD version range errors** go in the ⚪ false positive section with the fix commit as evidence — not in the findings section with a "but it's actually fixed" note.

4. **CVEs without a CVSS score** should use the vendor severity rating (e.g., Chromium "High" → HIGH ~8.8) and note that the official CVSS is pending.

5. **"Undiscovered"** means a CVE found proactively by the audit (via NVD/web search) that has no corresponding user-filed GitHub issue. It does NOT mean the CVE is unknown to the world.

## Handoff

After audit, use `native-dependency-update` skill:
- "Merge PR #3458"
- "Update libwebp to 1.6.0"
- "Bump libpng to fix CVE-2024-XXXXX"

For Skia core CVEs, the fix requires merging a newer upstream milestone into the fork.
This is a significant undertaking — flag it in the report with the milestone gap.
