# Security Audit Report Schema

JSON schema for the security audit report. The AI generates this JSON as structured output from the audit, then `render-security-audit.py` injects it into `viewer.html` to produce a standalone HTML report.

## Top-Level Structure

```json
{
  "meta": {
    "date": "2026-04-10",
    "schemaVersion": "1.0",
    "skiaSubmoduleCommit": "8c99e432ff06e61c42cf99aa8f2cbe248d301b9a",
    "skiaUpstreamCommit": "9ab7c2064b2b1ab22f856a7f0a8c3b3ae4cb89c7",
    "skiaMilestone": 132,
    "upstreamVerified": true
  },
  "summary": {
    "needsAttention": 3,
    "undiscovered": 2,
    "falsePositive": 4,
    "clean": 5,
    "totalCves": 12,
    "highestSeverity": "HIGH"
  },
  "versionVerification": [ ... ],
  "findings": [ ... ],
  "cgAlerts": { ... },
  "chromeReleases": { ... },
  "nextSteps": [ ... ]
}
```

## `meta` — Audit Metadata

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `date` | string | Yes | ISO date of the audit |
| `schemaVersion` | string | Yes | Always `"1.0"` |
| `skiaSubmoduleCommit` | string | Yes | mono/skia fork commit from `git submodule status` |
| `skiaUpstreamCommit` | string | Yes | google/skia chrome/mNNN branch tip (independently verified) |
| `skiaMilestone` | integer | Yes | Verified from SkMilestone.h |
| `upstreamVerified` | boolean | Yes | Whether `git merge-base --is-ancestor` confirmed upstream ancestry |

## `versionVerification` — Dependency Version Checks

Array of objects, one per dependency:

```json
{
  "name": "libpng",
  "depsCommit": "02f2b4f4699f0ef9111a6534f093b53732df4452",
  "verifiedVersion": "1.6.54",
  "cgmanifestVersion": "1.6.54",
  "match": true,
  "verificationMethod": "png.h header via googlesource mirror"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | string | Yes | Dependency name |
| `source` | string | Yes | Where this dep comes from: `"Skia DEPS"`, `"ANGLE"`, `"ANGLE submodule"` |
| `depsCommit` | string | Yes | Commit SHA from DEPS file (or ANGLE branch tag) |
| `verifiedVersion` | string | Yes | Version found in header file |
| `latestVersion` | string | No | Latest available version upstream (for update recommendations) |
| `cgmanifestVersion` | string | No | Version in cgmanifest.json (null/empty if missing) |
| `match` | boolean | Yes | Whether they agree (false if cgmanifest entry is missing) |
| `verificationMethod` | string | Yes | How the version was verified |

## `findings` — Individual Dependency Findings

Array of finding objects, sorted by priority then severity.

> 🛑 **ONE finding per dependency.** Every dependency (e.g., "skia", "libpng", "freetype")
> must appear as exactly ONE object in this array. All CVEs for that dependency — regardless
> of their status (affected, already_fixed, false_positive) — go inside that single finding's
> `cves[]` array. Use each CVE object's `assessment` field to distinguish affected vs. fixed
> vs. false positive. **Do NOT create multiple finding objects for the same dependency split
> by status, milestone, or category.** The validator enforces this as an error.

```json
{
  "id": 1,
  "dependency": "skia",
  "status": "needs_attention",
  "currentVersion": "chrome/m132",
  "fixVersion": "chrome/m147",
  "latestVersion": "chrome/m147",
  "cves": [
    {
      "id": "CVE-2026-3931",
      "severity": "HIGH",
      "cvss": 8.8,
      "fixedIn": "m146",
      "description": "Heap buffer overflow in Skia",
      "source": "NVD (Chrome CPE)",
      "assessment": "affected",
      "evidence": "Fix milestone m146 > our m132"
    }
  ],
  "nonChromeCves": [
    {
      "id": "CVE-2025-32318",
      "severity": "HIGH",
      "cvss": 8.8,
      "description": "OOB write due to heap buffer overflow",
      "source": "Android 16 Security Bulletin",
      "assessment": "needs_review",
      "codePathCheck": "No specific file/function identified publicly",
      "evidence": "Android bulletin, shared Skia code — needs manual verification"
    }
  ],
  "issues": [],
  "prs": [],
  "action": "Merge upstream Skia to m147",
  "notes": "Recommended target: m147 for all CVEs. Minimum: m146 for HIGH CVEs."
}
```

### Finding `status` Values

| Value | Meaning |
|-------|---------|
| `needs_attention` | 🔴 User-reported, no PR |
| `ready_to_merge` | ✅ PR exists, CI passing |
| `in_progress` | 🟡 PR exists, needs work |
| `undiscovered` | 🆕 Proactively found, no user issue |
| `false_positive` | ⚪ Does not affect SkiaSharp |
| `clean` | ✅ No known CVEs |

### CVE `assessment` Values

| Value | Meaning |
|-------|---------|
| `affected` | CVE applies to our version |
| `already_fixed` | Fix is in our version |
| `false_positive` | Does not affect SkiaSharp (reason in `evidence`) |
| `needs_review` | Cannot determine without manual analysis |
| `nvd_range_error` | NVD version range is wrong; fix commit already in our tree |

### CVE Fields — "Value OR Note" Principle

> **Every essential CVE field must be either a concrete value OR a `*Note` field explaining
> why it's missing.** Silent nulls ("we couldn't find it so we left it blank") are forbidden
> and are rejected by the validator. This forces every CVE to be fully investigated.

Examples:

| ✅ Valid | ❌ Invalid |
|---------|------------|
| `"bugId": "496526419"` | `"bugId": null` |
| `"bugId": null, "bugIdNote": "CVE has no issues.chromium.org reference URL in NVD"` | (no note) |
| `"fixCommit": "4320748a..."` | `"fixCommit": null` |
| `"fixCommit": null, "fixCommitNote": "Bug NNN has no matching commit on chrome/m147 or chrome/m148 branches after full fetch"` | (no note) |
| `"severity": "HIGH", "cvss": 8.3` | `"severity": null` (severity is always required) |
| `"severity": "HIGH", "cvss": null, "severityNote": "CVSS not yet assigned by NVD; using Chromium 'High' rating"` | `"cvss": null` (no note) |

### CVE Object Fields

| Field | Type | Required | Note Fallback | Description |
|-------|------|----------|---------------|-------------|
| `id` | string | ✅ Always | — | CVE ID, e.g., `CVE-2026-8579` |
| `severity` | enum | ✅ Always | — | `CRITICAL` / `HIGH` / `MEDIUM` / `LOW`. Use Chromium rating if NVD CVSS pending. |
| `description` | string | ✅ Always | — | What the vulnerability is |
| `source` | string | ✅ Always | — | Where this CVE came from |
| `assessment` | enum | ✅ Always | — | See assessment table above |
| `bugId` | string\|null | Value or note | `bugIdNote` | Chromium bug ID |
| `bugUrl` | string\|null | Required when `bugId` set | — | `https://issues.chromium.org/issues/{bugId}` |
| `fixCommit` | string\|null | Value or note | `fixCommitNote` | Upstream Skia commit SHA that fixes the CVE |
| `fixCommitTitle` | string\|null | Optional | — | Subject of the fix commit |
| `fixMilestone` | string\|null | Optional | — | Chrome milestone, e.g., `m148` |
| `cvss` | number\|null | Value or note | `severityNote` | CVSS score |
| `filesModified` | string[]\|null | Optional | — | Files changed by the fix commit |
| `onUpstreamMilestone` | bool\|null | Value or note (when `affected`) | `branchNote` | Is fix on `upstream/chrome/m{OUR}`? |
| `inOurTree` | bool\|null | ✅ When `affected` (never null) | — | Is fix in our fork's HEAD? |
| `cherryPicksCleanly` | bool\|null | Value or note (when `affected`) | `cherryPickNote` | Did `git cherry-pick --no-commit` succeed? |
| `reachability` | enum\|null | Value or note (when `affected`) | `reachabilityNote` | `REACHABLE` / `COMPILED_NOT_EXPOSED` / `NOT_REACHABLE` |
| `evidence` | string | Optional | — | Freeform summary/evidence |

When `assessment == "affected"`, the resolution fields (`cherryPicksCleanly`, `reachability`, `onUpstreamMilestone`, `inOurTree`) are strictly enforced — each requires either a value or its note counterpart (with `inOurTree` always requiring a boolean).

### CVE `source` Values

Examples: `"NVD (Chrome CPE)"`, `"NVD web search"`, `"Android Security Bulletin"`, `"Huawei HarmonyOS Bulletin"`, `"Chromium severity rating (CVSS pending)"`

## `nextSteps` — Prioritized Actions

Array of action objects:

```json
{
  "priority": 1,
  "severity": "HIGH",
  "action": "Merge upstream Skia to m147",
  "dependency": "skia",
  "reason": "Fixes 6 CVEs including 4× HIGH 8.8",
  "command": null
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `priority` | integer | Yes | 1 = highest |
| `severity` | string | Yes | Highest CVE severity driving this action |
| `action` | string | Yes | What to do |
| `dependency` | string | Yes | Which dependency |
| `reason` | string | Yes | Why (cite CVE count/severity) |
| `command` | string | No | CLI command if applicable (e.g., `bump libpng to 1.6.56`) |

## `chromeReleases` — Chrome Releases Blog Data (Optional)

When the Chrome Releases blog was queried (Step 1.5), include this section for traceability.
This section is optional — omit it only if the blog query was skipped or failed.

```json
{
  "queriedAt": "2026-04-10T14:30:00Z",
  "monthsQueried": 6,
  "postsReviewed": 12,
  "totalCvesExtracted": 45,
  "skiaRelevantCves": 8,
  "structuredCves": [
    {
      "cveId": "CVE-2026-8510",
      "severity": "Critical",
      "component": "Skia",
      "description": "Integer overflow in Skia",
      "bugId": "502636904",
      "milestone": 148,
      "blogPostUrl": "https://chromereleases.googleblog.com/...",
      "inNvd": false,
      "extraction": "regex"
    }
  ],
  "earlyDisclosures": [],
  "cacheFile": "output/ai/chrome-releases-cache.json"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `queriedAt` | string | Yes | ISO timestamp of the query |
| `monthsQueried` | integer | Yes | How many months back were queried |
| `postsReviewed` | integer | Yes | Number of posts that matched keywords |
| `totalCvesExtracted` | integer | Yes | Total CVEs found across all posts (all components) |
| `skiaRelevantCves` | integer | Yes | CVEs in Skia-relevant components |
| `structuredCves` | array | Yes | **All** Skia-relevant CVEs from the blog (the primary rendered array). Each has camelCase fields: `cveId`, `severity`, `component`, `milestone`, `bugId`, `blogPostUrl` |
| `earlyDisclosures` | array | No | Subset: CVEs found in blog but NOT yet in NVD (may be empty). Same schema as `structuredCves` items. |
| `cacheFile` | string | Yes | Path to the cached JSON from the script |

### Field Name Mapping (Script → Report JSON)

The `query-chrome-releases.py` script outputs **snake_case** fields. When building the report
JSON, transform to **camelCase**:

| Script field (`structured_cves[]`) | Report field (`structuredCves[]`) |
|------------------------------------|-----------------------------------|
| `cve_id` | `cveId` |
| `bug_id` | `bugId` |
| `blog_post_url` | `blogPostUrl` |
| `severity` | `severity` (unchanged) |
| `component` | `component` (unchanged) |
| `milestone` | `milestone` (unchanged) |

### Setting `blogPostUrl` on Findings CVEs

When a CVE in `findings[].cves[]` also appears in `structuredCves[]`, copy the `blogPostUrl`
onto the finding's CVE object. This enables the HTML viewer to link directly to the source
blog post.

### CVE `source` Field (Updated)

The `source` field on individual CVE objects in `findings[].cves[]` should indicate provenance:

| Value | Meaning |
|-------|---------|
| `"NVD (Chrome CPE)"` | Found via NVD query with Chrome CPE match |
| `"NVD web search"` | Found via NVD keyword search |
| `"Chrome Releases blog"` | Found in Chrome Releases blog only (early disclosure) |
| `"NVD + Chrome Releases"` | Found in both sources (most common) |
| `"Android Security Bulletin"` | Vendor bulletin — Android |
| `"Huawei HarmonyOS Bulletin"` | Vendor bulletin — Huawei |
| `"Chromium severity rating (CVSS pending)"` | Severity from Chromium, NVD CVSS not yet published |

### CVE `extraction` Field (New, Optional)

For CVEs that came from Chrome Releases data, indicate how they were extracted:

| Value | Meaning |
|-------|---------|
| `"regex"` | Deterministically extracted by the script's regex parser |
| `"ai_review"` | Found by AI reviewing the raw post text (format variation) |

This helps track reliability and identify if the regex needs updating.
