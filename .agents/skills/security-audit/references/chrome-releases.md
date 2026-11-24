# Chrome Releases Blog

The [Chrome Releases blog](https://chromereleases.googleblog.com/) is Google's official
announcement channel for Chrome stable, beta, and dev channel updates. Each "Stable Channel
Update for Desktop" post lists **all security fixes** included in the release, with:

- Severity (Critical / High / Medium / Low)
- CVE ID
- Component affected (e.g., "Skia", "ANGLE", "Canvas", "GPU")
- Bug tracker ID (links to `issues.chromium.org`)
- Reporter and date reported
- Bounty amount (if applicable)

## Why This Matters for SkiaSharp

1. **Earlier than NVD:** Chrome discloses CVEs in the blog *immediately* on release. NVD may
   take days or weeks to populate the full entry (CVSS score, references, CPE data).

2. **Explicit component tagging:** The blog names the affected component directly ("Heap buffer
   overflow in **Skia**"). NVD descriptions are often generic ("Google Chrome before X allows...").

3. **Milestone precision:** The blog post's Chrome version (e.g., `147.0.7727.137`) immediately
   tells us the fix milestone (147). This is more reliable than NVD's `versionEndExcluding`.

4. **ANGLE coverage:** ANGLE CVEs appear in the same format, supplementing NVD searches.

## Data Access

- **Atom/RSS feed:** `https://chromereleases.googleblog.com/feeds/posts/default`
- **Pagination:** 25 entries per page, `?start-index=26&max-results=25` for page 2, etc.
- **No authentication required**
- **No documented rate limit** (be polite: 0.5s between pages)

## Script Usage

```bash
# Standard: query last 6 months, cache results
python3 .agents/skills/security-audit/scripts/query-chrome-releases.py \
  --output output/ai/chrome-releases-cache.json

# Verbose: also print extracted CVEs
python3 .agents/skills/security-audit/scripts/query-chrome-releases.py \
  --verbose --output output/ai/chrome-releases-cache.json

# Force re-fetch (ignores 24h cache)
python3 .agents/skills/security-audit/scripts/query-chrome-releases.py \
  --force --output output/ai/chrome-releases-cache.json
```

## Output Structure

The script produces two types of data:

### `structured_cves[]` — Deterministic regex extraction

High-confidence CVE entries extracted from the known blog format. Each has:

| Field | Description |
|-------|-------------|
| `cve_id` | CVE identifier (e.g., `CVE-2026-7353`) |
| `severity` | Critical / High / Medium / Low |
| `component` | Affected component (Skia, ANGLE, Canvas, etc.) |
| `description` | Brief vulnerability description |
| `bug_id` | Chromium issue tracker ID |
| `bug_url` | Full URL to `issues.chromium.org` |
| `chrome_version` | Chrome version that includes the fix |
| `milestone` | Chrome milestone number (e.g., 147) |
| `bounty` | Bug bounty amount (null if N/A) |
| `reporter` | Who reported the vulnerability |
| `date_reported` | When it was reported (YYYY-MM-DD) |
| `blog_post_url` | Link to the blog post |
| `extraction` | Always `"regex"` |

### `posts[]` — Raw text for AI review

Full text content of each keyword-matching post. The AI agent reviews these to find:

- CVEs the regex missed (format variations, line breaks, unusual formatting)
- Indirect Skia references (e.g., "type confusion in Rendering" that actually involves Skia)
- Context about severity and exploitation status ("Google is aware that an exploit exists in the wild")
- Related component CVEs (GPU, Compositing) that may involve Skia code paths

## AI Review Instructions

When the security-audit skill runs this step, the AI agent should:

1. **Read `structured_cves[]` first** — these are the confirmed, high-confidence results.

2. **Scan each post's `text_content`** looking for:
   - Any CVE IDs not already in `structured_cves[]`
   - Mentions of Skia, ANGLE, Canvas, Fonts, GPU that might indicate relevance
   - Wild exploitation notices ("exploit exists in the wild") — these are HIGH PRIORITY
   - Version/milestone information for context

3. **For each additional CVE found by AI review**, add it with `"extraction": "ai_review"`.

4. **Cross-reference with NVD results:**
   - CVE in both Chrome Releases AND NVD → `source: "both"` (most common)
   - CVE in Chrome Releases but NOT NVD → `source: "chrome_releases"` (early disclosure, NVD may be delayed)
   - CVE in NVD but NOT Chrome Releases → `source: "nvd"` (likely vendor bulletin / Android CVE)

5. **Priority boost:** CVEs marked as exploited in the wild get severity boost regardless of CVSS.

## Relevant Components

When filtering for SkiaSharp relevance, these components are important:

| Component | Relevance |
|-----------|-----------|
| **Skia** | Direct — part of our submodule |
| **ANGLE** | Direct — separate native component for WinUI |
| **Canvas** | High — HTML Canvas uses Skia under the hood |
| **Fonts** | High — font rendering uses Skia/FreeType/HarfBuzz |
| **GPU** | Medium — GPU compositing may involve Skia paths |
| **Compositing** | Medium — may involve Skia rendering |
| **WebGL** | Low — WebGL uses ANGLE, potentially relevant |
| **Vulkan** | Low — Vulkan backend in Skia (compiled out in most SkiaSharp builds) |

## Typical Blog Post Format

For AI context, here's the typical structure (may evolve over time):

```
The Stable channel has been updated to VERSION for Windows/Mac and VERSION for Linux...

Security Fixes and Rewards

Note: Access to bug details and links may be kept restricted until a majority of users
are updated with a fix.

This update includes N security fixes.

[$7000][494352590] Critical CVE-2026-7363: Use after free in Canvas. Reported by heapracer on 2026-03-19
[N/A][493221953] Critical CVE-2026-7361: Use after free in iOS. Reported by Google on 2026-03-16
[$16000][493955227] High CVE-2026-7333: Use after free in GPU. Reported by c6eed09fc8b174b0f3eebedcceb1e792 on 2026-03-19
...

We would also like to thank all security researchers that worked with us during the
development cycle to prevent security bugs from ever reaching the stable channel.

As usual, our ongoing internal security work was responsible for a wide range of fixes...
```

**Format notes:**
- Bounty may be `N/A`, `$AMOUNT`, or `[TBD]`
- Bug ID is always numeric (links to issues.chromium.org)
- Severity is always one of: Critical, High, Medium, Low
- Component appears after the colon in the description
- "Reported by Google" means internally found (no bounty)
- Some posts mention "exploit exists in the wild" — flag these immediately
