# Component Governance (CG) Alerts

CG scans Docker container images and build-time dependencies used by the
combined `skiasharp-package` Azure DevOps pipeline. It flags CVEs in OS
packages, npm dependencies, Rust crates, and NuGet packages used during the
build.

> ⚠️ **MANDATORY:** The audit MUST include CG alerts from
> **skiasharp-package** (pipeline 1642). It builds native and managed assets,
> performs real signing, and registers the shipped packages in BAR.

## Why This Matters

CG alerts are **not visible** from GitHub Issues or NVD searches alone. They come from the
internal Azure DevOps pipeline and flag vulnerabilities in:

- **Docker base images** (Debian packages: dpkg, libcap2, sed, rsync)
- **Cross-compilation sysroots** (Alpine packages: busybox, file, binutils, zlib, freetype, gmp)
- **Build toolchain dependencies** (npm: minimatch, path-to-regexp, ws, express; Rust: hashbrown, zerovec, time)
- **NuGet build dependencies** (Microsoft.WindowsAppSDK)

> ⚠️ **Do NOT editorialize about whether CG alerts "ship" or not.**
> A vulnerable build chain means a potentially compromised build artifact.
> Present all CG alerts at the same importance level as other findings.
> HIGH severity CG alerts are `needs_attention` just like any other HIGH CVE.

## Run the Script (ONCE per audit)

> 🛑 **CRITICAL — SAVE TO FILE:** This script queries tens of build logs and takes 2–3 minutes.
> Save the output to a **file** (not a shell variable) so it persists across tool calls.
> Run it ONCE, save the JSON, then read from that file for the rest of the audit.
> **NEVER run this script more than once per audit session.**

> 🛑 **TAKES 5–7 MINUTES — YOU MUST WAIT:** This script queries 60+ CG jobs across 8+ builds.
> It is NORMAL for it to take 5–7 minutes with no intermediate output. You MUST use a long
> timeout (initial_wait of 600 seconds or more). Do NOT:
> - Give up after 30 seconds and write empty/fake data
> - Fabricate a `queriedAt` timestamp without actually running the script
> - Write `"totalAlerts": 0` because you didn't wait
> - Skip CG because "it's taking too long"
>
> **Empty CG results with `pipelines: []` will fail validation.** This is a security audit —
> every step is mandatory regardless of how long it takes.

```bash
# Run ONCE and save — this is your CG data for the entire audit
# ⚠️ Takes 5-7 minutes! Use initial_wait: 600 (or mode: sync with 600s wait)
# Using --output writes JSON to file while progress prints to stdout (agent sees activity)
mkdir -p output/ai
python3 .agents/skills/security-audit/scripts/query-cg-alerts.py \
  --output output/ai/cg-alerts-cache.json

# Then read from the file (fast, no API calls):
python3 -c "import json; d=json.load(open('output/ai/cg-alerts-cache.json')); print(f'{d[\"totalAlerts\"]} alerts')"
```

The output includes a `queriedAt` ISO timestamp so you can verify freshness.

### Additional Flags

```bash
# Also print human-readable text summary alongside JSON
python3 .agents/skills/security-audit/scripts/query-cg-alerts.py \
  --text --output output/ai/cg-alerts-cache.json

# With per-job verbose progress (shows each CG log being parsed)
python3 .agents/skills/security-audit/scripts/query-cg-alerts.py \
  --verbose --output output/ai/cg-alerts-cache.json

# Query only a specific branch
python3 .agents/skills/security-audit/scripts/query-cg-alerts.py \
  --branch main --output output/ai/cg-alerts-cache.json

# Query a specific build
python3 .agents/skills/security-audit/scripts/query-cg-alerts.py \
  --build-id 14176611 --output output/ai/cg-alerts-cache.json
```

## CG Portal

**Alerts:** https://dev.azure.com/dnceng/internal/_componentGovernance/dotnet-SkiaSharp

The Component Governance registration, alerts, repository identity, and scan
pipeline live in the dnceng `internal` project.

### What the Script Does

1. Discovers the latest completed Build from `main` and every active `release/*` branch.
2. Identifies ALL CG logs in each build (every job, no sampling — this is security).
3. Parses and deduplicates all CVEs across builds, pipelines, and jobs.
4. Sorts by severity and reports which branches and pipelines are affected.

> **Note:** There is no documented build-independent CG alerts API. The script
> enumerates every Component Governance log in the latest Build of every active
> branch (no sampling) and reports all alerts observed by those scans.

## Manual Approach (for Debugging)

```bash
# 1. Get latest combined Build ID
BUILD_ID=$(az pipelines runs list --pipeline-id 1642 \
  --org https://dev.azure.com/dnceng --project internal \
  --top 1 --query "[0].id" -o tsv)

# 2. Get timeline to find CG log IDs
az devops invoke --area build --resource timeline \
  --route-parameters project=internal buildId=$BUILD_ID \
  --org https://dev.azure.com/dnceng -o json

# 3. Parse CVEs from a specific CG log
az devops invoke --area build --resource logs \
  --route-parameters project=internal buildId=$BUILD_ID logId={LOG_ID} \
  --org https://dev.azure.com/dnceng -o json
```

## CG Alert Categories

> These categories are reference context for understanding where alerts come from and how to
> fix them. They are **NOT** part of the report JSON — the viewer groups by component
> automatically.

| Category | Source | Fix Mechanism |
|----------|--------|---------------|
| Alpine sysroot packages | `apk.static add` in alpine Dockerfile (fontconfig) | Bump `ALPINE_VERSION` in `alpine/Dockerfile` |
| Ubuntu/Debian .deb packages | SHA-pinned fontconfig .debs in glibc Dockerfiles | Bump `FC_VERSION` + new SHA-256 in `glibc/Dockerfile` or `glibc-x86/Dockerfile` or `glibc-ppc64le/Dockerfile` |
| .NET cross base image | `mcr.microsoft.com/dotnet-buildtools/prereqs:azurelinux-3.0-net10.0-cross-*` | Bump the .NET image tag (controlled by .NET infra team) |
| npm build tooling | .NET SDK / Cake dependencies | Update .NET SDK or pin versions |
| Rust crate deps | .NET SDK internals | Update .NET SDK |
| NuGet build deps | Build-time references | Update package version |

## Key Files for CG Fixes

| File | Controls |
|------|----------|
| `scripts/infra/native/linux/docker/glibc/Dockerfile` | Glibc cross images (arm, arm64, x64, riscv64, loongarch64) — pinned fontconfig .debs + .NET SDK version |
| `scripts/infra/native/linux/docker/glibc-x86/Dockerfile` | x86 self-contained build (libc++ stage + fontconfig) |
| `scripts/infra/native/linux/docker/glibc-ppc64le/Dockerfile` | ppc64le self-contained build (libc++ stage + fontconfig) |
| `scripts/infra/native/linux/docker/alpine/Dockerfile` | Musl/Alpine cross images — `ALPINE_VERSION` (look for `ARG ALPINE_VERSION`) controls fontconfig source |
| `scripts/infra/native/linux/docker/bionic/Dockerfile` | Bionic/Android cross-compile (NDK detected dynamically) |
| `scripts/infra/native/wasm/docker/Dockerfile` | WASM build container |

## Including CG Data in the Report

> 🛑 **CRITICAL:** Include the **complete `alerts` array** from the script output in the
> report. Do NOT summarize or truncate. The viewer needs every individual alert to render
> correctly. Read the JSON from `output/ai/cg-alerts-cache.json` and embed it as the
> `cgAlerts` value in your report.

```bash
# Read the cached CG data (the script already wrote it with --output):
python3 -c "import json; d=json.load(open('output/ai/cg-alerts-cache.json')); print(f'{d[\"totalAlerts\"]} alerts, {len(d[\"pipelines\"])} pipelines')"
# Use the entire contents of this file as the "cgAlerts" value in the report JSON.
```

The script output has this structure (include ALL fields as-is):

```json
{
  "cgAlerts": {
    "queriedAt": "2026-05-24T12:34:56+00:00",
    "pipelines": [...],
    "builds": [...],
    "totalAlerts": 121,
    "bySeverity": {"High": 7, "Medium": 110, "Low": 4},
    "alerts": [
      {
        "id": "CVE-2024-XXXXX",
        "component": "busybox 1.35.0-r31",
        "severity": "Medium",
        "sources": ["Alpine 3.17"],
        "branches": ["main"],
        "pipelines": ["skiasharp-package"],
        "paths": ["/some/path/to/manifest"]
      }
    ]
  }
}
```

**Do NOT:**

- Summarize alerts into categories (the viewer does grouping itself)
- Omit the `alerts` array
- Replace `alerts` with `uniqueCVEs` or `categories`
- Write `totalAlerts: N` without including the actual N alerts

## CG Build Link

- **Combined Build pipeline:** https://dev.azure.com/dnceng/internal/_build?definitionId=1642

Use the exact Build run URL emitted by the script for audit evidence. Do not
substitute a latest branch build after collection.
