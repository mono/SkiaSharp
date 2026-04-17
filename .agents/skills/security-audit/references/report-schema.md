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

Array of finding objects, sorted by priority then severity:

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
