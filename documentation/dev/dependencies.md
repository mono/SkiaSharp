# SkiaSharp Native Dependencies

Single source of truth for native dependencies: what's used, what's not, and how to track security vulnerabilities.

## Contents

- [Active Dependencies](#active-dependencies) — What SkiaSharp actually compiles
- [Build Source Reports](#build-source-reports) — Runtime evidence for compliance review
- [cgmanifest.json](#cgmanifestjson) — CVE detection setup
- [Known False Positives](#known-false-positives) — CVEs that don't affect SkiaSharp

---

## Active Dependencies

SkiaSharp uses only a subset of Skia's dependencies. Unused dependencies are commented out in `externals/skia/DEPS` to reduce attack surface.

### Security-Relevant (process untrusted input)

| Dependency | Purpose | CVE Name | Platforms |
|------------|---------|----------|-----------|
| **skia** | 2D graphics engine (core) | skia (via NVD keyword search) | All |
| **libpng** | PNG codec | libpng | All |
| **zlib** | Compression | zlib | All |
| **libjpeg-turbo** | JPEG codec | libjpeg-turbo | All |
| **libwebp** | WebP codec | libwebp | All |
| **freetype** | Font rendering | freetype | Android, Linux, WASM |
| **harfbuzz** | Text shaping | harfbuzz | All (disabled in SkiaSharp) |
| **expat** | XML parsing | libexpat | All |
| **brotli** | WOFF2 fonts | brotli | All |
| **wuffs** | GIF codec | wuffs | All |
| **dng_sdk** | RAW images | dng_sdk | Windows |

### GPU/Graphics

| Dependency | Purpose | Platforms |
|------------|---------|-----------|
| **vulkanmemoryallocator** | Vulkan memory | Android, Linux, Windows |
| **d3d12allocator** | Direct3D memory | Windows |
| **spirv-cross** | Shader translation | Vulkan/Metal |
| **vulkan-headers** | Vulkan API | Vulkan builds |

### Supporting

| Dependency | Purpose | Platforms |
|------------|---------|-----------|
| **piex** | RAW preview | All except Windows, WASM |
| **buildtools** | Compiler toolchain | All |

---

## Build Source Reports

Every SkiaSharp build, test, package, signing, and analysis job in Azure Pipelines enables Git
Trace2 before checkout and publishes a `source_dependencies_<job>_<attempt>` artifact at the end
of the job, including failed jobs. The artifact contains:

- `source-dependencies.json` for aggregation and compliance tooling.
- `source-dependencies.md` for human review.

The report combines three sources so it does not depend on one manifest being complete:

1. Runtime Git events, including Git processes started indirectly by Cake, `git-sync-deps`, and
   `gclient`.
2. Remotes and exact revisions from checked-out repositories that remain in the workspace.
3. Declared repository URLs from `.gitmodules`, `DEPS`, `cgmanifest.json`, Cake files, build
   scripts, Dockerfiles, and pipeline YAML.

Runtime events are marked `observed`; static-only entries are marked `declared`. The latter expose
source paths that exist in another platform or target but were not exercised by the current job.
Container bootstrapper runs write Trace2 data into the mounted workspace so they are included in
the same report.

The raw Trace2 events can contain command-line details, so they remain in the agent temporary
directory and are deleted after the sanitized report is generated. Credentials, URL user info,
queries, and fragments are never copied to the report.

This report intentionally covers source repositories and repository-backed source downloads. It
does not inventory package feeds, SDK installers, operating-system packages, or arbitrary tool
downloads; those need a separate software-bill-of-materials or network-egress report.

---

## cgmanifest.json

Enables Microsoft Component Governance CVE detection.

**Problem:** Skia mirrors dependencies from chromium.googlesource.com, but CVE databases use upstream names.

**Solution:** Use `type: "other"` with canonical names:

```json
{
  "component": {
    "type": "other",
    "other": {
      "name": "libpng",
      "version": "1.6.44",
      "downloadUrl": "https://github.com/glennrp/libpng"
    }
  }
}
```

### Name Mapping

| DEPS Name | cgmanifest Name | Upstream |
|-----------|-----------------|----------|
| skia (core) | `skia` | github.com/google/skia |
| libpng | `libpng` | github.com/glennrp/libpng |
| zlib | `zlib` | github.com/madler/zlib |
| libjpeg-turbo | `libjpeg-turbo` | github.com/libjpeg-turbo/libjpeg-turbo |
| libwebp | `libwebp` | github.com/webmproject/libwebp |
| freetype | `freetype` | gitlab.freedesktop.org/freetype/freetype |
| harfbuzz | `harfbuzz` | github.com/harfbuzz/harfbuzz |
| expat | `libexpat` | github.com/libexpat/libexpat |
| brotli | `brotli` | github.com/google/brotli |
| wuffs | `wuffs` | github.com/google/wuffs-mirror-release-c |
| dng_sdk | `dng_sdk` | android.googlesource.com/.../dng_sdk |

### Skia DEPS Identity Signals

Registrations backed by an enabled `externals/skia/DEPS` entry include a
`skia_dependency` object:

```json
{
  "skia_dependency": {
    "name": "vulkanmemoryallocator",
    "revision": "c788c52156f3ef7bc7ab769cb03c110a53ac8fcb",
    "version_reviewed_identity": "https://chromium.googlesource.com/...@c788c521..."
  }
}
```

`.agents/skills/update-skia/scripts/update_versions.py` synchronizes `revision` mechanically from
final DEPS.
Every tracked registration records the authoritative version evidence in `version_source`. When
the URL or revision changes, the Skia update must re-read checked-out source, update the registration
if needed, advance `version_reviewed_identity` to the final `URL@revision`, and refresh that
evidence. The helper blocks publication when evidence is missing or a changed tracked dependency
has not been reviewed. It also rejects a manifest version change when the corresponding DEPS
identity did not change.

---

## Skia — Special CVE Tracking Notes

Skia is the core dependency and has its own CVEs (integer overflows, heap buffer overflows, etc.). Unlike third-party dependencies, Skia CVEs require special tracking because they are **invisible to Component Governance**:

1. **No standalone CPE** — Skia CVEs are filed under `cpe:2.3:a:google:chrome`, not `skia`
2. **Fork URL mismatch** — cgmanifest references `mono/skia.git`, not upstream
3. **No package ecosystem** — GitHub Advisory DB has no Skia package mapping

### How to Query Skia CVEs

Query the **NVD API** directly (same as any other dependency's CVE lookup):

```
GET https://services.nvd.nist.gov/rest/json/cves/2.0?keywordSearch=Skia
```

This returns all CVEs mentioning "Skia" in their description. To determine which affect us:

1. Extract the Chrome `versionEndExcluding` from each CVE's CPE configuration
2. Map Chrome major version to Skia milestone (Chrome 132.x = m132)
3. Compare against SkiaSharp's `chrome_milestone` from cgmanifest.json
4. Flag CVEs where the fix milestone > our milestone as **potentially affected**
5. CVEs without Chrome version info (e.g., Android-specific) are flagged for **manual review**

### cgmanifest.json Fields

The Skia entry in cgmanifest.json includes custom fields for version tracking:

```json
{
  "component": {
    "type": "other",
    "other": {
      "name": "skia",
      "version": "chrome/m119",
      "downloadUrl": "https://github.com/google/skia"
    }
  },
  "chrome_milestone": 119,
  "upstream_merge_commit": "fcb55886b914028a99f35fb0ba28e66ff82027e3"
}
```

| Field | Purpose |
|-------|---------|
| `chrome_milestone` | Integer milestone number — used to filter NVD results |
| `upstream_merge_commit` | SHA of the upstream `chrome/mNNN` branch tip that was merged into the fork |

The [`auto-skia-submodule-sync`](../../.github/workflows/auto-skia-submodule-sync.yml)
workflow runs daily to advance `externals/skia` to the `mono/skia` `skiasharp`
branch and derive the Component Governance git registration's `commitHash` from
the checked-out submodule. It opens a PR when either value changes, which also
repairs manifest drift. The milestone fields above remain owned by the full Skia
upstream update workflow. Scheduled runs sync `mono/skia`'s `skiasharp` branch
into SkiaSharp's `main`; manual runs can set `skia_branch` and `target_branch`
independently.

### When to Update

Update these fields whenever merging new upstream Skia code:

```bash
# After merging upstream/chrome/m125 into the fork:
# 1. Update cgmanifest.json chrome_milestone to 125
# 2. Update upstream_merge_commit to the tip of upstream/chrome/m125
# 3. Update version to "chrome/m125"
```

### Skia-Specific False Positives

- **Not all CVEs are exploitable through SkiaSharp's API surface.** Chrome exposes Skia via HTML Canvas, SVG, etc. SkiaSharp exposes a different subset. Each CVE needs manual assessment of whether the vulnerable code path is reachable.
- **Android-specific CVEs** (e.g., in `SkiaRenderEngine.cpp`) generally don't affect SkiaSharp.
- **NVD enrichment can lag** — CVEs may appear days after the Chrome release.

---

## Known False Positives

Some CVEs flagged against dependencies **don't affect SkiaSharp** because the vulnerable component isn't compiled.

### MiniZip (in zlib) — NOT USED

**Status:** ❌ Not compiled, not linked

MiniZip is bundled in `zlib/contrib/minizip/` but Skia's BUILD.gn excludes it. CVEs mentioning `unzip.c`, `zip.c`, `ioapi.c`, or functions like `unzOpen`/`zipOpen` are false positives.

**Evidence:**
- `externals/skia/third_party/zlib/BUILD.gn` lists only core zlib sources
- No MiniZip includes: `grep -r "minizip\|unzip\.h" externals/skia/src/` returns nothing

**Core zlib IS used** — CVEs affecting deflate/inflate/adler32/crc32 DO apply.

### FreeType's Bundled zlib

FreeType has its own zlib copy at `freetype/src/gzip/`. When checking zlib CVEs:
- Check if it affects FreeType's bundled copy (different version)
- Core Skia zlib and FreeType zlib are separate

---

## Related Skills

- **[security-audit](../../.agents/skills/security-audit/SKILL.md)** — Find CVEs, verify fixes, generate reports
- **[native-dependency-update](../../.agents/skills/native-dependency-update/SKILL.md)** — Update dependencies, create PRs
