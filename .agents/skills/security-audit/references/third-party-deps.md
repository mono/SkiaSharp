# Third-Party Dependency CVEs

Third-party dependencies (libpng, freetype, harfbuzz, libexpat, brotli, zlib, libjpeg-turbo,
libwebp, ANGLE submodules, etc.) are vendored into Skia via `externals/skia/DEPS`. Their CVE
process is much simpler than Skia core: verify the version, check whether the fix commit is in
our pinned tree, and recommend a DEPS bump if not.

## Query CVE Databases

Search the web for each dependency in parallel:

```
"{dependency} CVE {current year}"
"{dependency} security vulnerability"
```

## Verify Versions From Source

> ⚠️ **Never trust cgmanifest.json blindly.** Always verify against the actual DEPS file and
> source headers. cgmanifest.json is manually maintained and drifts.

### Read the Pinned Commit From DEPS

```bash
cat externals/skia/DEPS
# Extract commit hashes for each dependency
```

### Read the Real Version From a Header File

Each dependency exposes its version in a header. Fetch the header at the pinned commit from
the Chromium git mirror:

| Dependency | Version file | Version pattern |
|------------|-------------|-----------------|
| libpng | `png.h` lines 1–3 | `libpng version X.Y.Z` |
| freetype | `include/freetype/freetype.h` | `FREETYPE_MAJOR`, `FREETYPE_MINOR`, `FREETYPE_PATCH` |
| harfbuzz | `src/hb-version.h` | `HB_VERSION_STRING "X.Y.Z"` |
| libexpat | `expat/lib/expat.h` | `XML_MAJOR_VERSION`, `XML_MINOR_VERSION`, `XML_MICRO_VERSION` |
| brotli | `c/common/version.h` | `BROTLI_VERSION_MAJOR`, `_MINOR`, `_PATCH` |
| zlib | `zlib.h` | `ZLIB_VERSION "X.Y.Z"` |
| libjpeg-turbo | `README.chromium` | `Version: X.Y.Z` |
| libwebp | `NEWS` line 1 | `version X.Y.Z` |

### Googlesource Mirror URL Pattern

```
https://{host}/{path}/+/{commit_sha}/{file}?format=TEXT
```

Response is base64-encoded. Decode with:

- Python: `base64.b64decode(text)`
- PowerShell: `[System.Convert]::FromBase64String($text)`

If the submodule externals are initialized locally, you can read directly from
`externals/skia/third_party/externals/{dep}/` instead.

## Verify Fix Commits

> ⚠️ **CVE databases often have WRONG version ranges.** Always verify with the actual commit
> in the vendored tree.

```bash
cd externals/skia/third_party/externals/{dependency}

# Check if the fix commit is ancestor of current HEAD
git merge-base --is-ancestor {fix_commit} HEAD && echo "FIXED" || echo "VULNERABLE"
```

### Example: CVE-2025-27363 (FreeType)

CVE-2025-27363 claimed FreeType ≤ 2.13.3 was affected, with the fix in 2.13.4. Verification
showed the fix commit (`ef636696...`) was actually merged into 2.13.1 — SkiaSharp's pinned
2.13.3 was already patched. The published NVD version range was simply wrong.

When this happens, classify the CVE as **⚪ False positive (NVD version range incorrect)** in
the false positive section, citing the actual fix commit as evidence. Do NOT place it in
findings with a "but it's actually fixed" note.

## Known Third-Party False Positives

| Pattern | Why |
|---------|-----|
| **MiniZip** (inside zlib) | Not compiled by Skia, not linked |
| **FreeType's bundled zlib** | Separate copy from Skia's zlib — Skia's zlib build excludes it |
| **NVD version range errors** | NVD ranges are sometimes wrong (see CVE-2025-27363 above). Verify with `git merge-base --is-ancestor`. |

## ANGLE and Its Submodules

ANGLE is a **separate** native component (Windows-only, for WinUI). It is **not** part of the
Skia submodule — it's cloned separately from `https://github.com/google/angle.git`.

```bash
# Read the ANGLE version pin
grep ANGLE scripts/VERSIONS.txt
# Output: ANGLE    release    chromium/NNNN
```

ANGLE has its own submodules that must also be tracked:

- `third_party/zlib` (separate from Skia's zlib)
- `third_party/jsoncpp`
- `third_party/vulkan-deps`
- `third_party/astc-encoder/src`

Check that all of these are in `cgmanifest.json`. If missing, flag as a coverage gap.

### ANGLE in Chrome Releases Blog

ANGLE CVEs also appear in the Chrome Releases blog with the component name "ANGLE" (e.g.,
"Use after free in ANGLE"). The `query-chrome-releases.py` script captures these alongside
Skia CVEs.

When cross-referencing, ANGLE CVEs from Chrome Releases provide:
- The exact Chrome milestone where the fix shipped
- The bug ID for commit resolution
- Severity rating before NVD publishes CVSS

This supplements the NVD + web search approach for ANGLE vulnerabilities.

## Reporting Third-Party Dep Findings

For each third-party CVE, include:

| Field | Source |
|-------|--------|
| CVE ID, description, CVSS | NVD / vendor advisory |
| Affected dependency | Dependency name |
| Affected version range | NVD (verify against actual!) |
| Fix commit / version | Vendor changelog or commit log |
| Our pinned commit | DEPS file |
| Our pinned version | Header file (verified) |
| Fix included? | `git merge-base --is-ancestor` |
| Recommended action | Bump DEPS commit to ≥ {fix_version} |

> Status priority for third-party CVEs:
> - ✅ Already fixed (fix commit ancestor of HEAD)
> - 🔴 Needs attention (fix commit not in HEAD; recommend DEPS bump)
> - ⚪ False positive (NVD range error, code not compiled, etc.)
