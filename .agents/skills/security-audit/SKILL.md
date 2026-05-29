---
name: security-audit
description: >
  Audit SkiaSharp's native dependencies for security vulnerabilities and CVEs,
  including Component Governance (CG) alerts from the SkiaSharp-Native and SkiaSharp Azure DevOps pipelines.
  Read-only investigation that produces a status report with recommendations.

  Use when user asks to:
  - Audit security issues or CVEs
  - Check CVE status across dependencies
  - Find security-related issues and their PR coverage
  - Get an overview of open vulnerabilities
  - See what security work is pending
  - Check Component Governance alerts
  - Review CG alerts from the native build pipeline

  Triggers: "security audit", "audit CVEs", "CVE status", "what security issues are open",
  "check vulnerability status", "security overview", "what CVEs need fixing",
  "CG alerts", "component governance", "check container CVEs".

  This skill is READ-ONLY. To actually fix issues, use the `native-dependency-update` skill.
---

# Security Audit Skill

Investigate the security status of SkiaSharp's native dependencies. Skia core is treated as
the product itself (not just a dependency) and gets a deeper, commit-level resolution
process. Third-party deps and Component Governance alerts are audited alongside it and
combined into a single unified report.

> ℹ️ This skill is **read-only**. To create PRs and fix issues, use the `native-dependency-update` skill.

## Key References

- **[references/chrome-releases.md](references/chrome-releases.md)** — Chrome Releases blog: RSS query, two-pass extraction (regex + AI review), cross-referencing with NVD
- **[references/skia-cve-resolution.md](references/skia-cve-resolution.md)** — Skia core CVE pipeline (NVD → Bug ID → Commit → Branch → Cherry-pick → Reachability). **The Skia process is fine-grained — read this before auditing Skia.**
- **[references/third-party-deps.md](references/third-party-deps.md)** — Third-party CVE process (libpng, freetype, harfbuzz, etc.): version verification, fix-commit ancestry, known false positives
- **[references/cg-alerts.md](references/cg-alerts.md)** — Component Governance alerts: ADO pipeline queries, Docker container CVEs, fix locations
- **[documentation/dev/dependencies.md](../../../documentation/dev/dependencies.md)** — Dependency list, cgmanifest format, Skia-specific tracking notes
- **[references/report-template.md](references/report-template.md)** — Markdown format guide (used by `render-security-audit-md.py`)
- **[references/report-schema.md](references/report-schema.md)** — JSON schema for structured output
- **[references/security-audit-schema.json](references/security-audit-schema.json)** — Machine-readable JSON Schema (Draft 2020-12)
- **[scripts/validate-security-audit.py](scripts/validate-security-audit.py)** — Validates report JSON against schema + semantic checks
- **[scripts/render-security-audit.py](scripts/render-security-audit.py)** — Renders JSON → standalone HTML
- **[scripts/render-security-audit-md.py](scripts/render-security-audit-md.py)** — Renders JSON → Markdown (for AI consumption)
- **[scripts/viewer.html](scripts/viewer.html)** — HTML template (Bootstrap 5)

## Workflow

1. Search GitHub issues/PRs (all deps including Skia)
2. Query Chrome Releases blog (`query-chrome-releases.py`) — see [Chrome Releases](references/chrome-releases.md)
3. Verify dependency versions from submodule/DEPS/headers (NOT cgmanifest.json)
4. Audit Skia core CVEs — see [Skia CVE Resolution](references/skia-cve-resolution.md)
5. Audit third-party dependency CVEs — see [Third-Party Deps](references/third-party-deps.md)
6. Query Component Governance alerts — see [CG Alerts](references/cg-alerts.md)
7. Check false positives
8. Assemble structured JSON report
9. Validate report (`validate-security-audit.py`)
10. Render HTML (`render-security-audit.py`)
11. Present markdown summary to user

---

### Step 1: Search Issues & PRs

Search mono/SkiaSharp open issues for:

- CVE numbers (e.g., "CVE-2024")
- Keywords: "security", "vulnerability"
- Dependency names: skia, libpng, expat, zlib, webp, harfbuzz, freetype

Search PRs in both `mono/SkiaSharp` and `mono/skia` for dependency updates already in flight.

---

### Step 1.5: Query Chrome Releases Blog

> 🔍 The Chrome Releases blog often discloses Skia CVEs **before NVD** processes them.
> This step provides early detection and cross-validation.

**See [references/chrome-releases.md](references/chrome-releases.md)** for full details on the
data source, script usage, and AI review instructions.

#### Run the script

```bash
python3 .agents/skills/security-audit/scripts/query-chrome-releases.py \
  --verbose --output output/ai/chrome-releases-cache.json
```

This takes ~10-30 seconds (fetches RSS feed pages). Cache is reused if < 24 hours old.

#### Two-pass review

1. **Deterministic (regex):** Read `structured_cves[]` from the JSON output. These are
   high-confidence CVEs extracted from the known blog format. Each has a CVE ID, severity,
   component, bug ID, and milestone already parsed.

2. **AI review (broad):** Scan `posts[].text_content` for anything the regex missed:
   - CVE mentions not captured by regex (format variations, line breaks)
   - Indirect Skia references ("type confusion in Rendering")
   - Wild exploitation notices (highest priority!)
   - Related component CVEs (GPU, Compositing) that may involve Skia code

#### Cross-reference with NVD (Step 3)

After the NVD query in Step 3, compare results:

| Chrome Releases | NVD | Interpretation |
|-----------------|-----|----------------|
| ✅ Found | ✅ Found | Normal — use NVD CVSS, Chrome Releases for milestone |
| ✅ Found | ❌ Not found | **Early disclosure** — NVD may be delayed. Use Chrome severity. |
| ❌ Not found | ✅ Found | Vendor bulletin CVE (Android/Huawei) — not in Chrome stable |

Set the `source` field on each CVE object: `"both"`, `"chrome_releases"`, or `"nvd"`.

---

### Step 3: Verify Dependency Versions

> ⚠️ **CRITICAL: Never trust `cgmanifest.json` blindly.** Always verify versions against the
> actual submodule, DEPS file, and source headers. cgmanifest.json is manually maintained
> and can drift. Report any mismatches as findings.

#### 2.1 Verify Skia milestone and upstream commit

> 🛑 **MANDATORY:** Fetching the upstream `google/skia` branch is **required**, not optional.
> Adding a git remote and fetching is read-only — it does not modify any tracked files.
> Without independent verification of the upstream merge point, the audit would trust
> cgmanifest.json circularly, defeating the purpose of verification.

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

# 4. Add the upstream remote and fetch (read-only)
git remote add upstream https://github.com/google/skia.git 2>/dev/null || \
  git remote set-url upstream https://github.com/google/skia.git
git fetch upstream chrome/mNNN
git log --format="%H %s" -1 FETCH_HEAD
# This gives the independently-verified upstream_merge_commit

# 5. Confirm upstream is ancestor of our fork
git merge-base --is-ancestor FETCH_HEAD <merge-parent> && echo "VERIFIED"
```

Compare against cgmanifest.json and report mismatches:

| Field | Source of truth | cgmanifest.json field |
|-------|----------------|----------------------|
| Milestone | `SkMilestone.h` in submodule | `chrome_milestone` |
| Fork commit | `git submodule status` | git entry `commitHash` |
| Upstream commit | `git fetch upstream chrome/mNNN` tip | `upstream_merge_commit` |

#### 2.2 Verify third-party dependency versions

See **[references/third-party-deps.md](references/third-party-deps.md)** for the full table of
header files and the googlesource mirror URL pattern. In short: read pinned commit hashes
from `externals/skia/DEPS`, then fetch each dependency's version header at that commit and
parse the version string.

#### 2.3 Verify ANGLE and its submodules

ANGLE is a **separate** native component (Windows-only, for WinUI). It is NOT part of the
Skia submodule.

```bash
grep ANGLE scripts/VERSIONS.txt
# Output: ANGLE    release    chromium/NNNN
```

ANGLE has its own submodules (`third_party/zlib`, `jsoncpp`, `vulkan-deps`,
`astc-encoder/src`) that must also be tracked. See
[references/third-party-deps.md](references/third-party-deps.md#angle-and-its-submodules)
for details. Flag any missing from cgmanifest.json as a coverage gap.

#### 2.4 Build the dependency overview

The `versionVerification` array in the JSON report must include **ALL** dependencies from
ALL sources:

| Source | What to include |
|--------|----------------|
| `"Skia DEPS"` | All deps from `externals/skia/DEPS` + Skia itself |
| `"ANGLE"` | ANGLE itself (version from `VERSIONS.txt`) |
| `"ANGLE submodule"` | ANGLE's submodules (zlib, jsoncpp, vulkan-deps, astc-encoder) |
| `"GPU/Graphics"` | VulkanMemoryAllocator, SPIRV-Cross, D3D12Allocator from DEPS |
| `"Supporting"` | piex, wuffs, dng_sdk, buildtools from DEPS |

Each entry must have a `source` field and a `cgmanifestVersion` field (null if missing).
Report mismatches as findings.

---

### Step 4: Audit Skia Core CVEs

> 🛑 Skia is the product, not just a dependency. Every Skia CVE must be resolved to a
> specific fix commit, branch, cherry-pick test, and reachability assessment. Classification
> by milestone alone is INCOMPLETE.

**See [references/skia-cve-resolution.md](references/skia-cve-resolution.md) for the full
process**, including:

- NVD query (`keywordSearch=Skia`)
- Bug ID extraction from `issues.chromium.org/issues/NNNNN` references
- `git fetch upstream chrome/mNNN` + `git log --grep=<bug_id>` to find fix commits
- Branch ancestry verification (our milestone? our tree?)
- Cherry-pick feasibility test
- Reachability through SkiaSharp C API
- Non-Chrome CVEs (Android / vendor bulletins)
- Required output fields per CVE

---

### Step 5: Audit Third-Party Dependency CVEs

For libpng, freetype, harfbuzz, libexpat, brotli, zlib, libjpeg-turbo, libwebp, ANGLE
submodules, etc.

**See [references/third-party-deps.md](references/third-party-deps.md)** for:

- Web/NVD search queries
- Version verification (DEPS commit + header files)
- Fix-commit ancestry check (`git merge-base --is-ancestor`)
- NVD version range errors (e.g., CVE-2025-27363 / FreeType)
- Known false positives (MiniZip, FreeType's bundled zlib)

---

### Step 6: Query Component Governance Alerts

CG scans Docker container images and build-time deps from both ADO pipelines. CG alerts are
invisible to GitHub Issues and NVD searches alone.

> 🛑 **THIS STEP TAKES 5–7 MINUTES.** The CG script queries 60+ jobs across 8+ builds. This
> is NORMAL and NON-NEGOTIABLE. Use `initial_wait: 600` (or higher). Do NOT skip, fabricate
> empty results, or write placeholder data because it's "taking too long." The validator will
> reject reports with empty `pipelines` or fabricated timestamps.

**See [references/cg-alerts.md](references/cg-alerts.md)** for:

- The one-shot query script (`scripts/query-cg-alerts.py`) — run ONCE, cache to file
- Manual `az devops` approach for debugging
- Alert categories (Alpine, Debian, npm, Rust, NuGet)
- Key Dockerfiles for fixes
- How to embed the raw `alerts` array in the report (do NOT summarize)
- Portal links

---

### Step 7: Check False Positives

Before flagging anything, verify the CVE actually affects SkiaSharp.

**General false positives** (apply to any dependency):

- **NVD version range errors** — When a CVE claims version X is affected but the fix commit
  is already in version X's tree, classify as false positive and cite the fix commit.
- **Chrome-only rendering paths** (HTML Canvas, SVG in browser) — May not be reachable through
  the SkiaSharp C API.

**Dependency-specific false positives:**

- Skia core → see [references/skia-cve-resolution.md](references/skia-cve-resolution.md#skia-specific-false-positives)
- Third-party deps → see [references/third-party-deps.md](references/third-party-deps.md#known-third-party-false-positives)
- Full reference: [dependencies.md](../../../documentation/dev/dependencies.md#known-false-positives)

---

### Step 8: Assemble Structured JSON Report

> 🛑 **MANDATORY:** The audit MUST produce a JSON file conforming to
> [references/report-schema.md](references/report-schema.md). This is the machine-readable
> output used by dashboards and CI.

Build the JSON object with these top-level keys:

1. **`meta`** — Date, schema version, Skia commit hashes, milestone, upstream verification status
2. **`summary`** — Counts by status category, total CVEs, highest severity
3. **`versionVerification`** — One entry per dependency with DEPS commit, verified version, cgmanifest version, match boolean
4. **`findings`** — Array of finding objects sorted by priority then severity. **ONE object per dependency** (e.g., one "skia" finding containing ALL Skia CVEs regardless of status). Each has `dependency`, `status`, `cves[]`, `nonChromeCves[]`, `action`, `notes`. The `status` reflects the WORST-case status among the CVEs.
5. **`cgAlerts`** — The complete raw JSON from `query-cg-alerts.py` (full `alerts` array, do not summarize)
6. **`nextSteps`** — Prioritized action items with severity, command, and reason

> 🛑 **COMPLETENESS REQUIREMENT:** The `findings` array MUST include **every CVE returned
> by the NVD query** (Step 3 of skia-cve-resolution.md). CVEs that are verified as already
> fixed in our tree are classified as `"already_fixed"` or `"false_positive"` — they are
> NOT dropped from the report. An audit that finds 15 CVEs in NVD but only reports 7 in the
> JSON is INCOMPLETE and will fail review. The total CVE count in `summary.totalCves` must
> match the number of CVE objects across all findings.

> 🛑 **ONE FINDING PER DEPENDENCY:** Do NOT create multiple finding objects for the same
> dependency. All CVEs for "skia" go in ONE finding. All CVEs for "libpng" go in ONE
> finding. Use each CVE's `assessment` field to distinguish affected/fixed/false_positive.
> The finding's top-level `status` reflects the worst-case among its CVEs (e.g., if 3 CVEs
> are already_fixed but 2 are needs_attention, the finding status is `"needs_attention"`).

Save as `output/ai/security-audit-{date}.json`.

---

### Step 9: Validate Report

> 🛑 **MANDATORY:** Always validate before rendering. Fix any errors reported.

```bash
python3 .agents/skills/security-audit/scripts/validate-security-audit.py \
  output/ai/security-audit-{date}.json
```

Exit codes: `0` = valid, `1` = fixable errors (fix and retry), `2` = fatal.
Warnings are informational — errors must be fixed before proceeding.

---

### Step 10: Render HTML + Markdown Reports

> 🛑 **MANDATORY:** Always generate both reports.

```bash
python3 .agents/skills/security-audit/scripts/render-security-audit.py \
  output/ai/security-audit-{date}.json

python3 .agents/skills/security-audit/scripts/render-security-audit-md.py \
  output/ai/security-audit-{date}.json
```

This produces:
- **HTML** — Self-contained dashboard (Bootstrap 5) for human review
- **Markdown** — Comprehensive report for AI consumption and action suggestions

The HTML renders:

- Summary cards with status counts
- Collapsible findings with CVE tables, severity badges, NVD links
- Chrome Releases blog section with above-milestone CVEs by component
- Version verification table with match/mismatch indicators
- Skia upstream verification details with commit links
- Prioritized next steps with severity-coded borders

Present the output path to the user:

```
✅ security-audit-2026-04-10.html (45 KB)
   m132 • 2026-04-10 • 12 CVEs • Highest: HIGH
   🔴 3 attention · 🆕 2 undiscovered · ⚪ 4 FP · ✅ 5 clean
```

---

### Step 11: Present Summary to User

The Markdown report was already generated in Step 10. Present a brief summary in the
conversation pointing to the generated files:

```
✅ Reports generated:
   • output/ai/security-audit-{date}.json (structured data)
   • output/ai/security-audit-{date}.html (interactive dashboard)
   • output/ai/security-audit-{date}.md  (full markdown for AI review)

   m147 • 2026-05-29 • 102 CVEs • Highest: CRITICAL
   🔴 0 attention · 🆕 0 undiscovered · ⚪ 1 FP · ✅ 6 clean
   📰 Chrome Releases: 146 Skia-relevant CVEs (16 above current milestone)
```

Then highlight the **top actionable items** from the report:
- Any `needs_attention` or `undiscovered` findings
- Chrome Releases CVEs above our current milestone (especially Skia/ANGLE)
- Critical/High CG alerts

#### Report quality rules

These rules apply to the JSON assembly (Step 8) and are enforced by the renderers:

1. **Skia bump recommendations must target the highest-severity CVE**, not the lowest. If
   there are HIGH CVEs at m146 and a MEDIUM at m133, recommend m146 as the target.
2. **Don't include already-closed GitHub issues** unless directly relevant to an open
   vulnerability.
3. **CVEs with NVD version range errors** go in the ⚪ false positive section with the fix
   commit as evidence — not in findings with a "but it's actually fixed" note.
4. **CVEs without a CVSS score** should use the vendor severity rating (e.g., Chromium
   "High" → HIGH ~8.8) and note that the official CVSS is pending.
5. **"Undiscovered"** means a CVE found proactively by the audit (via NVD/web search) that
   has no corresponding user-filed GitHub issue. It does NOT mean the CVE is unknown to the
   world.

---

## Handoff

After audit, use the `native-dependency-update` skill to act on findings:

- "Merge PR #3458"
- "Update libwebp to 1.6.0"
- "Bump libpng to fix CVE-2024-XXXXX"

For Skia core CVEs, the fix typically requires merging a newer upstream milestone into the
fork (or cherry-picking specific fix commits, per the resolution pipeline). This is a
significant undertaking — flag it in the report with the milestone gap and the list of
required commits.

For CG container alerts, the fix is updating Dockerfiles under
`scripts/infra/native/linux/docker/`. This does not require a Skia submodule update — only
Docker image rebuilds.
