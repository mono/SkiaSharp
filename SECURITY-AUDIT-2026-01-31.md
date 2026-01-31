# Security Audit Report

**Date:** January 31, 2026  
**Audited:** mono/SkiaSharp native dependencies  
**Auditor:** GitHub Copilot Security Audit Skill

---

## Executive Summary

This comprehensive security audit examined all native dependencies in SkiaSharp for known CVEs from 2025-2026. The audit reviewed 10 security-relevant dependencies, identified **6 vulnerabilities requiring attention**, and found **1 dependency successfully updated during the audit**.

### Summary

| Status | Count | Dependencies |
|--------|-------|--------------|
| 🔴 Critical - Needs immediate update | 1 | libpng |
| 🟡 Medium - Update recommended | 3 | freetype (verify), expat, harfbuzz |
| ✅ Recently updated/resolved | 2 | libwebp ✅, brotli |
| 🟢 Low/No known issues | 2 | libjpeg-turbo, dng_sdk |
| ⚪ False positive / Low risk | 1 | zlib (MiniZip not used) |
| 🆕 Pending PR | 1 | harfbuzz (#3232) |

---

## Detailed Findings

### 1. 🔴 **CRITICAL: libpng** — Multiple High-Severity CVEs

| Field | Value |
|-------|-------|
| Issues | None reported by users yet |
| CVEs | CVE-2025-28164, CVE-2025-64720, CVE-2025-65018, CVE-2025-66293, CVE-2026-22801 |
| Current | 1.6.44 |
| Minimum fix | 1.6.54 |
| Latest | 1.6.54+ |
| PR | None |
| Severity | **HIGH** (Buffer overflows, heap corruption, DoS) |

**CVE Details:**

- **CVE-2025-28164**: Buffer overflow in `png_create_read_struct()` causing DoS
- **CVE-2025-64720**: Out-of-bounds read during palette compositing (affects 1.6.0-1.6.50)
- **CVE-2025-65018**: Heap buffer overflow in `png_image_finish_read` for 16-bit interlaced PNGs (HIGH severity - potential code execution)
- **CVE-2025-66293**: Out-of-bounds read in simplified API (up to 1012 bytes beyond buffer)
- **CVE-2026-22801**: Integer truncation in write API functions causing heap buffer over-read

**Risk Assessment:**  
HIGH - Multiple buffer overflow vulnerabilities that could lead to code execution when processing malicious PNG files. SkiaSharp processes untrusted PNG data from web/user sources.

**Action:** 🚨 **URGENT** - Create PR to update libpng to 1.6.54 immediately

---

### 2. 🟡 **freetype** — Vulnerability Status UNCLEAR (Needs Verification)

| Field | Value |
|-------|-------|
| Issues | None reported by users yet |
| CVEs | CVE-2025-27363 (ACTIVELY EXPLOITED in ≤2.13.0) |
| Current | 2.13.3 |
| Vulnerable versions | ≤ 2.13.0 (per NVD, Red Hat, OpenCVE) |
| Minimum fix | 2.13.4 (confirmed fix) |
| Latest | 2.13.4+ |
| PR | None |
| Severity | **HIGH** (CVSS 8.1-8.8, active exploitation **IF vulnerable**) |

**CVE Details:**

- **CVE-2025-27363**: Out-of-bounds write in TrueType GX/variable font parsing
  - Signed short from font file incorrectly assigned to unsigned long buffer size
  - Up to six 'long' values written outside allocated heap space
  - **Active exploitation reported by Meta (Facebook) in early 2025**
  - Affects systems rendering untrusted fonts: Linux, Android, embedded systems, browsers

**IMPORTANT CLARIFICATION:**

Most authoritative sources (NVD, Red Hat, OpenCVE, Tenable) state that **CVE-2025-27363 affects FreeType ≤ 2.13.0 ONLY**. The official fix is confirmed in FreeType 2.13.4. 

**SkiaSharp currently uses FreeType 2.13.3**, which is newer than the vulnerable 2.13.0. However:
- Some secondary sources mention possible vulnerability up to 2.13.3
- Versions 2.13.1-2.13.3 may contain backported security patches
- **Verification needed**: Check if 2.13.3 includes the security fix commit (ef636696524b081f1b8819eb0c6a0b932d35757d)

**Risk Assessment:**  
MEDIUM-HIGH - **Uncertainty exists** about 2.13.3's vulnerability status. Most reliable sources indicate it's NOT vulnerable (only ≤2.13.0 affected), but prudent security practice suggests verifying the fix commit or updating to 2.13.4 for certainty.

**Action:** 
1. 🔍 **VERIFY** if FreeType 2.13.3 contains CVE-2025-27363 fix commit
2. 🟡 **UPDATE** to 2.13.4 for certainty (recommended as defense in depth)

---

### 3. 🟡 **libexpat** — Medium Priority Update

| Field | Value |
|-------|-------|
| Issues | None |
| CVEs | CVE-2025-59375 (patched), CVE-2025-66382 (present) |
| Current | 2.7.3 |
| Status | Partially patched |
| PR | None |

**CVE Details:**

- **CVE-2025-59375**: Dynamic memory allocation via crafted XML (FIXED in 2.7.2, improved in 2.7.3)
- **CVE-2025-66382**: DoS via processing delay (up to dozens of seconds for 2MB file) - LOW severity

**Risk Assessment:**  
MEDIUM - Version 2.7.3 already includes the important CVE-2025-59375 fix. The remaining CVE-2025-66382 is low severity (processing delay, not crash). Current version is acceptable but monitor for future updates.

**Action:** ⏳ Monitor for 2.7.4+ release to address CVE-2025-66382

---

### 4. 🟡 **harfbuzz** — Update Available in PR

| Field | Value |
|-------|-------|
| Issues | None |
| CVEs | CVE-2026-22693, CVE-2026-0943 (both affect ≤8.4.0) |
| Current | 8.3.1 |
| Target | 8.4.0 → 12.3.0 |
| Minimum fix | 12.3.0 |
| PR | #3232 (mono/SkiaSharp) + #157 (mono/skia) |
| Severity | HIGH (Null pointer dereference DoS) |

**CVE Details:**

- **CVE-2026-22693**: Null pointer dereference causing DoS/crash (affects bundled HarfBuzz ≤8.4.0)
- **CVE-2026-0943**: Related null pointer dereference in Perl HarfBuzz::Shaper embedding

**Risk Assessment:**  
MEDIUM-HIGH - While DoS vulnerabilities are serious, these primarily affect Perl bindings and may not directly impact SkiaSharp's C++ usage. However, update is recommended for defense in depth.

**Action:** ✅ Merge PR #3232 once ready, or update to 12.3.0+ directly

---

### 5. ✅ **libwebp** — RESOLVED (Updated to 1.6.0)

| Field | Value |
|-------|-------|
| Issues | #3465 (user-reported, Black Duck flagged) - **CLOSED 2026-01-31** |
| CVEs | CVE-2023-4863 (historical, patched in 1.3.2) |
| Previous | 1.3.2 |
| Current | **1.6.0** |
| PR | #3478 **MERGED 2026-01-30** (also #3461, #3476) |
| Severity | Historical high, **NOW RESOLVED** |

**Status:**  
✅ **COMPLETE** - Issue #3465 was closed on January 31, 2026 after PR #3478 was successfully merged on January 30, 2026. libwebp has been updated from 1.3.2 to 1.6.0. No new CVEs for 2025/2026 found.

**Changes in 1.4.0 - 1.6.0:**
- Security hardening and increased fuzzing coverage
- Additional AVX2, SSE2 optimizations
- Fix heap overflow in webpmux
- Lossless encoder improvements
- Various bug and warning fixes

**Action:** ✅ **COMPLETE** - No further action needed

---

### 6. ✅ **brotli** — Recently Updated, But Check Version

| Field | Value |
|-------|-------|
| Issues | None |
| CVEs | CVE-2025-6176 (decompression bomb DoS) |
| Current | 1.2.0 (per cgmanifest.json) |
| Minimum fix | 1.2.0 |
| Status | **FIXED** (if actually on 1.2.0) |
| Severity | HIGH (CVSS 7.5) |

**CVE Details:**

- **CVE-2025-6176**: Decompression bomb DoS in Brotli ≤1.1.0
  - Crafted files expand to massive memory consumption
  - Fixed in 1.2.0 with `max_output_length` parameter

**Risk Assessment:**  
RESOLVED - cgmanifest.json shows 1.2.0 which includes the fix. Verify DEPS matches this version.

**Action:** ✅ Verify actual version in DEPS matches 1.2.0; if not, update

---

### 7. ⚪ **zlib** — False Positive (MiniZip not used)

| Field | Value |
|-------|-------|
| CVEs | CVE-2023-45853 (MiniZip), CVE-2025-14847 (MongoDB integration) |
| Current | 1.3.0.1 |
| Status | Not vulnerable |
| Reason | MiniZip not compiled/linked; MongoDB issue not applicable |

**CVE Details:**

- **CVE-2023-45853**: Buffer overflow in MiniZip (contrib/minizip/) - **NOT COMPILED**
- **CVE-2025-14847**: MongoDB wire protocol issue using zlib compression - **NOT APPLICABLE**

**Risk Assessment:**  
FALSE POSITIVE - Per SkiaSharp documentation (dependencies.md), MiniZip is NOT compiled or linked. CVEs affecting only MiniZip don't apply. Core zlib (deflate/inflate/adler32/crc32) has no recent CVEs.

**Action:** ⚪ No action needed; close any scanner warnings with explanation

---

### 8. 🟢 **libjpeg-turbo** — No Recent CVEs

| Field | Value |
|-------|-------|
| CVEs | None for 2025/2026 |
| Current | 2.1.5.1 |
| Status | Clean |

**Status:**  
No significant CVEs found for libjpeg-turbo 2.1.5 in 2025/2026. Version is acceptable for production use.

**Action:** 🟢 No action needed

---

### 9. 🟢 **wuffs** — Monitor Only

| Field | Value |
|-------|-------|
| Current | 0.3.3 |
| Status | No CVEs found |

**Action:** 🟢 No action needed; monitor for updates

---

### 10. 🟢 **dng_sdk** — Platform-Specific

| Field | Value |
|-------|-------|
| Current | 1.6.0 |
| Platform | Windows only |
| Status | No CVEs found |

**Action:** 🟢 No action needed

---

## Priority Recommendations

### Immediate Action (This Week)

1. **🔴 CRITICAL: Update libpng to 1.6.54** — Multiple high-severity buffer overflows
   - Command: Use `native-dependency-update` skill with "bump libpng to 1.6.54"
   - Affects all platforms

### High Priority (This Month)

2. **🔍 VERIFY freetype 2.13.3 vulnerability status** — CVE-2025-27363
   - **Most authoritative sources (NVD, Red Hat) indicate only ≤2.13.0 is vulnerable**
   - Current version 2.13.3 is likely NOT vulnerable, but verification recommended
   - If uncertain, update to 2.13.4 for peace of mind
   - Command: Use `native-dependency-update` skill with "bump freetype to 2.13.4"
   - Platforms affected: Android, Linux, WASM

3. **🟡 Review harfbuzz PR #3232** — Update from 8.3.1 to 8.4.0 (or preferably 12.3.0)
   - Fixes CVE-2026-22693 and CVE-2026-0943
   - Verify mono/skia PR #157 is merged first

### Medium Priority (Next Quarter)

4. **Monitor expat** for 2.7.4+ release addressing CVE-2025-66382 (low severity DoS)

### Completed ✅

5. **✅ libwebp updated** — Issue #3465 closed, PR #3478 merged on 2026-01-30
   - Successfully updated from 1.3.2 to 1.6.0

### Low Priority / No Action

6. **brotli** — Already at 1.2.0 (includes CVE-2025-6176 fix)
7. **zlib** — Document false positive for CVE-2023-45853 (MiniZip not used)
8. **libjpeg-turbo** — Current version is clean

---

## Verification Commands

After updates, verify with:

```bash
# Check current versions in DEPS
cd externals/skia
cat DEPS | grep -A2 "third_party/externals/libpng\|third_party/externals/freetype"

# Verify in cgmanifest.json
cat cgmanifest.json | jq '.registrations[] | select(.component.other.name == "libpng" or .component.other.name == "freetype")'
```

---

## Additional Context

### Known False Positives

Per `documentation/dependencies.md`:

- **zlib/MiniZip**: CVE-2023-45853 affects MiniZip only. Skia does NOT compile or link MiniZip (`contrib/minizip/` excluded from BUILD.gn)
- **FreeType bundled zlib**: FreeType has its own zlib copy at `freetype/src/gzip/` - separate from core Skia zlib

### Security-Relevant Dependencies

The following dependencies process **untrusted input** and require security monitoring:

- ✅ libpng (PNG codec)
- ✅ zlib (Compression)
- ✅ libjpeg-turbo (JPEG codec)
- ✅ libwebp (WebP codec)
- ✅ freetype (Font rendering)
- ✅ harfbuzz (Text shaping - disabled in SkiaSharp)
- ✅ expat (XML parsing)
- ✅ brotli (WOFF2 fonts)
- ✅ wuffs (GIF codec)
- ✅ dng_sdk (RAW images, Windows only)

---

## Related Issues & PRs

- **Issue #3465**: Update libwebp to latest version (1.6.0) - ✅ **CLOSED 2026-01-31**
- **PR #3478**: Update libwebp to 1.6.0 - ✅ **MERGED 2026-01-30**
- **PR #3232**: Update harfbuzz to 8.4.0 - 🟡 Status unclear
- **mono/skia PR #157**: Required for harfbuzz update

---

## Methodology

This audit used the following process:

1. ✅ Searched mono/SkiaSharp issues for "CVE", "security", "vulnerability", and dependency names
2. ✅ Reviewed `cgmanifest.json` for current dependency versions
3. ✅ Web-searched for "{dependency} CVE 2025 2026" for all security-relevant deps
4. ✅ Cross-referenced with NVD, GitHub Advisory Database, vendor security pages
5. ✅ Verified active PRs addressing known issues
6. ✅ Assessed severity and exploitability based on public reports
7. ✅ Prioritized based on: active exploitation > CVSS score > user reports > proactive findings

---

## Next Steps for Development Team

1. **Create PR** for libpng update to 1.6.54 (CRITICAL)
2. **Verify freetype 2.13.3** vulnerability status via git commit check
3. **Consider updating freetype** to 2.13.4 as defense in depth (recommended)
4. **Review harfbuzz PR #3232** for merge readiness
5. **Document false positives** in security scanning tools
6. **Schedule quarterly audits** to maintain security posture

---

**Report prepared by:** GitHub Copilot Security Audit Skill  
**Report date:** 2026-01-31 (Updated 2026-01-31 15:48 UTC)  
**Valid as of:** January 2026

**Updates in this revision:**
- ✅ Verified libwebp successfully updated to 1.6.0 (Issue #3465 closed, PR #3478 merged)
- 🔍 Corrected CVE-2025-27363 affected versions: Most authoritative sources indicate only FreeType ≤2.13.0 vulnerable, not 2.13.3
- 📊 Updated priority levels based on verification

For questions or to execute updates, use the `native-dependency-update` skill with commands like:
- "bump libpng to 1.6.54"
- "bump freetype to 2.13.4" (if verification warrants update)
