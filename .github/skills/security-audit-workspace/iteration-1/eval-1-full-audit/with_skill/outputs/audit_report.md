# Security Audit Report

**Date:** 2026-04-10
**Audited:** mono/SkiaSharp native dependencies (including Skia core)
**Submodule commit:** `8c99e432ff06e61c42cf99aa8f2cbe248d301b9a`
**Skia milestone:** m132 (verified from SkMilestone.h)

## Summary

| Status | Count |
|--------|-------|
| 🔴 Needs attention | 5 |
| 🆕 Undiscovered | 6 |
| ⚪ False positive | 3 |
| ✅ Already fixed / clean | 5 |

## Version Verification

All dependency versions verified from DEPS commit hashes and upstream header files match cgmanifest.json. **No mismatches detected.**

| Dependency | DEPS-Verified Version | cgmanifest.json | Match |
|------------|----------------------|-----------------|-------|
| libpng | 1.6.54 (from `png.h`) | 1.6.54 | ✅ |
| zlib | 1.3.0.1 (from `zlib.h`) | 1.3.0.1 | ✅ |
| freetype | 2.13.3 (from `freetype.h`) | 2.13.3 | ✅ |
| harfbuzz | 8.3.1 (from `hb-version.h`) | 8.3.1 | ✅ |
| libexpat | 2.7.3 (from `expat.h`) | 2.7.3 | ✅ |
| brotli | 1.2.0 (from `version.h`) | 1.2.0 | ✅ |
| libjpeg-turbo | 2.1.5.1 (from `README.chromium`) | 2.1.5.1 | ✅ |
| libwebp | 1.6.0 (from `NEWS`) | 1.6.0 | ✅ |
| skia | m132 (from `SkMilestone.h`) | chrome/m132 | ✅ |
| skia fork commit | 8c99e432ff... | 8c99e432ff... | ✅ |

---

## Detailed Findings

### 1. 🔴 skia — Needs attention

| Field | Value |
|-------|-------|
| Issues | #2716 (closed), Discussion #3432 |
| CVEs | CVE-2026-3538 (HIGH 8.8), CVE-2026-3931 (HIGH 8.8), CVE-2026-3909 (HIGH 8.8), CVE-2026-4460 (HIGH 8.8), CVE-2025-0444 (MEDIUM 6.3), +2 manual review |
| Current | chrome/m132 |
| Min fix | chrome/m133 (for CVE-2025-0444), chrome/m146 (for all HIGH) |
| NVD query | `keywordSearch=Skia` — 115 total, 21 since 2023, 5 potentially affected, 11 already fixed |

**Chrome-versioned CVEs (fix milestone > m132):**

| CVE | Fixed In | Severity | Description |
|-----|----------|----------|-------------|
| CVE-2026-3538 | m145 | HIGH 8.8 | Integer overflow in Skia |
| CVE-2026-3931 | m146 | HIGH 8.8 | Heap buffer overflow in Skia |
| CVE-2026-3909 | m146 | HIGH 8.8 | Out of bounds write in Skia |
| CVE-2026-4460 | m146 | HIGH 8.8 | Out of bounds read in Skia |
| CVE-2025-0444 | m133 | MEDIUM 6.3 | Use after free in Skia |

**Non-Chrome CVEs (manual review needed):**

| CVE | Severity | Description | Assessment |
|-----|----------|-------------|------------|
| CVE-2025-32318 | HIGH 8.8 | Out of bounds write in Skia due to heap buffer overflow | ⚠️ Needs review — no Chrome milestone, verify code path reachability |
| CVE-2025-54627 | HIGH 8.8 | Out-of-bounds write vulnerability in the skia module | ⚠️ Needs review — may be Android-specific |
| CVE-2024-43768 | HIGH 7.8 | OOB write in SkDeflate.cpp | ⚠️ Likely Android-specific (platform/frameworks) |
| CVE-2025-48630 | HIGH 7.4 | SkiaRenderEngine.cpp GPU cache access | ⚪ Not applicable — Android RenderEngine only |

**Action:** Merge newer upstream Skia milestone (minimum m133 for MEDIUM fix, m146 for all HIGH fixes). This is a major undertaking — the current gap is 14+ milestones.

---

### 2. 🔴 libpng — Needs attention

| Field | Value |
|-------|-------|
| Issues | None |
| CVEs | CVE-2026-25646 (HIGH 8.1), CVE-2026-33416 (HIGH 7.5–8.1), CVE-2026-33636 (HIGH 7.1) |
| Current | 1.6.54 |
| Min fix | 1.6.55 (CVE-2026-25646), 1.6.56 (all) |
| Latest | 1.6.56 |
| PR | None |

**CVE Details:**

| CVE | Severity | Fix Version | Description |
|-----|----------|-------------|-------------|
| CVE-2026-25646 | HIGH 8.1 | 1.6.55 | Heap buffer overflow in `png_set_quantize()` — 30-year-old bug |
| CVE-2026-33416 | HIGH 7.5–8.1 | 1.6.56 | Use-after-free in transparency/palette handling |
| CVE-2026-33636 | HIGH 7.1 | 1.6.56 | OOB read/write in ARM Neon palette expansion (ARM/AArch64 only) |

**Action:** Create PR to update libpng to 1.6.56. CVE-2026-25646 is exploitable via crafted PNG files.

---

### 3. 🔴 zlib — Needs attention

| Field | Value |
|-------|-------|
| Issues | Discussion #3432 (MiniZip — false positive) |
| CVEs | CVE-2026-27171 (DoS via crc32_combine) |
| Current | 1.3.0.1 |
| Min fix | 1.3.2 |
| Latest | 1.3.2 |
| PR | None |

**CVE Details:**

| CVE | Severity | Fix Version | Description |
|-----|----------|-------------|-------------|
| CVE-2026-27171 | MEDIUM | 1.3.2 | Infinite loop in `crc32_combine64` — DoS via CPU exhaustion |

**Action:** Create PR to update zlib to 1.3.2.

---

### 4. 🔴 freetype — Needs attention

| Field | Value |
|-------|-------|
| Issues | None |
| CVEs | CVE-2026-23865 (MEDIUM 5.3) |
| Current | 2.13.3 |
| Min fix | 2.14.2 |
| Latest | 2.14.2 |
| PR | None |

**Note:** CVE-2025-27363 (HIGH 8.1, actively exploited) affects FreeType ≤ 2.13.0, but the fix was backported and is present in 2.13.1+. Our version 2.13.3 is **already patched** for this CVE. ✅

| CVE | Severity | Fix Version | Description |
|-----|----------|-------------|-------------|
| CVE-2026-23865 | MEDIUM 5.3 | 2.14.2 | Integer overflow/OOB read in variable font parsing (HVAR/VVAR/MVAR) |

**Action:** Create PR to update freetype to 2.14.2 (or monitor for next bump).

---

### 5. 🔴 libexpat — Needs attention

| Field | Value |
|-------|-------|
| Issues | None |
| CVEs | CVE-2026-25210, CVE-2026-24515 |
| Current | 2.7.3 |
| Min fix | 2.7.4 |
| Latest | 2.7.4 |
| PR | None |

**Note:** CVE-2024-8176 (HIGH 7.5), CVE-2024-45491 (HIGH), and CVE-2025-59375 (HIGH 7.5) are all already fixed in our version 2.7.3. ✅

| CVE | Severity | Fix Version | Description |
|-----|----------|-------------|-------------|
| CVE-2026-25210 | LOW–MEDIUM | 2.7.4 | Integer overflow in `doContent` |
| CVE-2026-24515 | LOW–MEDIUM | 2.7.4 | Missing copy of unknown encoding handler user data |

**Action:** Create PR to update libexpat to 2.7.4 (minor update).

---

### 6. 🆕 harfbuzz — Undiscovered CVE

| Field | Value |
|-------|-------|
| Issues | None |
| CVEs | CVE-2026-22693 (MEDIUM 5.3) |
| Current | 8.3.1 |
| Min fix | 12.3.0 |
| Latest | 12.3.0+ |
| PR | mono/skia #157 (updates to 8.4.0 — does NOT fix this CVE) |

**Note:** CVE-2024-56732 (heap buffer overflow, affects 8.5.0–10.0.1) does **NOT** affect our version 8.3.1 since it's below the affected range. ✅

| CVE | Severity | Fix Version | Description |
|-----|----------|-------------|-------------|
| CVE-2026-22693 | MEDIUM 5.3 | 12.3.0 | Null pointer dereference in `SubtableUnicodesCache::create` — DoS in low-memory conditions |

**Note:** HarfBuzz text shaping is disabled in SkiaSharp by default. This CVE has lower practical impact. The version gap (8.3.1 → 12.3.0) is very large and may require a Skia milestone update.

**Action:** Create issue to track. Low priority given disabled status and large version gap.

---

### 7. 🆕 libjpeg-turbo — Undiscovered, needs review

| Field | Value |
|-------|-------|
| Issues | None |
| CVEs | CVE-2023-2804 (needs review) |
| Current | 2.1.5.1 |
| Min fix | 3.0+ for all fixes |
| Latest | 3.1.0 |
| PR | None |

| CVE | Severity | Description | Assessment |
|-----|----------|-------------|------------|
| CVE-2023-2804 | MEDIUM | Heap buffer overflow with 12-bit lossless JPEG | ⚠️ SkiaSharp typically uses 8-bit mode; 12-bit lossless is uncommon but code path may exist |

**Note:** Most libjpeg-turbo CVEs affect older versions or specific tools (tjLoadImage, tjbench). The version 2.1.5.1 is a Chromium-patched fork with its own backports. Major version gap to latest (3.1.0) would require careful testing.

**Action:** Review whether 12-bit JPEG support is compiled in. Monitor for Chromium mirror updates.

---

### 8. ✅ libwebp — Clean

| Field | Value |
|-------|-------|
| Issues | None |
| CVEs | None active |
| Current | 1.6.0 |
| Status | Up to date |

CVE-2023-4863 (CRITICAL, heap buffer overflow) was fixed in 1.3.2. Our version 1.6.0 is well past the fix. ✅

**Action:** None required.

---

### 9. ✅ brotli — Clean

| Field | Value |
|-------|-------|
| Issues | None |
| CVEs | None active |
| Current | 1.2.0 |
| Status | Up to date |

CVE-2025-6176 (DoS decompression bomb) affects brotli ≤ 1.1.0, fixed in 1.2.0. Our version IS the fix version. ✅

**Action:** None required.

---

### 10. ⚪ zlib (MiniZip) — False positive

| Field | Value |
|-------|-------|
| Issue | Discussion #3432 |
| CVE | CVE-2023-45853 |
| Status | Not vulnerable |

**Reason:** CVE-2023-45853 affects MiniZip only (`contrib/minizip/`). Skia's `BUILD.gn` excludes MiniZip sources. No MiniZip includes exist in the Skia source tree. Binary scanners flag the zlib version but the vulnerable code is not compiled or linked.

**Action:** Respond to Discussion #3432 with explanation that MiniZip is not compiled.

---

### 11. ⚪ skia (SkiaRenderEngine) — False positive

| Field | Value |
|-------|-------|
| Issue | None |
| CVE | CVE-2025-48630 |
| Status | Not applicable |

**Reason:** CVE-2025-48630 affects `SkiaRenderEngine.cpp` in Android's graphics framework. This is Android-specific rendering infrastructure, not part of Skia core or SkiaSharp's API surface.

**Action:** None required. Mark as not applicable.

---

### 12. ⚪ skia (Android platform CVEs) — False positive

| Field | Value |
|-------|-------|
| Issue | None |
| CVE | CVE-2024-43768 |
| Status | Likely not applicable |

**Reason:** CVE-2024-43768 references `SkDeflate.cpp` in `platform/frameworks` — this is Android's framework layer, not Skia's core library. SkiaSharp does not include Android framework code.

**Action:** None required. Mark as not applicable.

---

### 13. ✅ wuffs — Clean

| Field | Value |
|-------|-------|
| Issues | None |
| CVEs | None known |
| Current | 0.3.3 |
| Status | No recent CVEs found |

**Action:** None required.

---

### 14. ✅ dng_sdk — Clean

| Field | Value |
|-------|-------|
| Issues | None |
| CVEs | None recent |
| Current | 1.6.0 |
| Status | No recent CVEs found |

**Action:** None required.

---

### 15. ✅ VulkanMemoryAllocator / SPIRV-Cross / D3D12Allocator — Clean

| Field | Value |
|-------|-------|
| Issues | None |
| CVEs | None recent |
| Status | No recent CVEs found for current versions |

**Action:** None required.

---

## GitHub Issues & PRs Summary

| # | Repo | Type | Title | Status | Relevance |
|---|------|------|-------|--------|-----------|
| #3432 | mono/SkiaSharp | Discussion | zlib CVE-2023-45853 question | Open | ⚪ False positive (MiniZip) |
| #2716 | mono/SkiaSharp | Issue | CVE-2023-6345 in SkiaSharp | Closed | ✅ Already fixed |
| #3166 | mono/SkiaSharp | PR | Address CVE-2024-30105 | Merged | ✅ Already fixed |
| #3656 | mono/SkiaSharp | PR | Fix cgmanifest.json version drift | Open | 🟡 In progress |
| #157 | mono/skia | PR | Update harfbuzz to 8.4.0 | Open | 🟡 Minor version bump |

---

## Next Steps

1. **🔴 Update libpng to 1.6.56** — Fixes 3 HIGH severity CVEs including a 30-year-old heap overflow (CVE-2026-25646). Highest priority third-party update.

2. **🔴 Update zlib to 1.3.2** — Fixes CVE-2026-27171 (DoS via infinite loop in crc32_combine).

3. **🔴 Update libexpat to 2.7.4** — Fixes 2 low-medium CVEs (CVE-2026-25210, CVE-2026-24515). Minor bump from 2.7.3.

4. **🔴 Update freetype to 2.14.2** — Fixes CVE-2026-23865 (MEDIUM, variable font OOB read). Larger version jump from 2.13.3.

5. **🟡 Merge mono/skia PR #157** — Updates harfbuzz to 8.4.0. Does not fix CVE-2026-22693 but keeps dependency current.

6. **🟡 Review Skia milestone gap** — Current m132, latest affected CVE requires m146. This is a 14-milestone gap. Plan a Skia upstream merge to at minimum m133 (fixes CVE-2025-0444 USE-after-free) and ideally m146 (fixes all known HIGH CVEs).

7. **🟡 Review libjpeg-turbo 12-bit JPEG support** — Determine if CVE-2023-2804 is reachable in SkiaSharp's build configuration.

8. **⚪ Close Discussion #3432** — Explain that CVE-2023-45853 (MiniZip) is a false positive; MiniZip is not compiled.

9. **📋 Create tracking issues** — File GitHub issues for undiscovered CVEs in libpng, zlib, freetype, and libexpat to ensure visibility.
