# Component Governance (CG) Alerts

CG scans Docker container images and build-time dependencies used by the SkiaSharp-Native
and SkiaSharp Azure DevOps pipelines. It flags CVEs in OS packages, npm dependencies, Rust
crates, and NuGet packages used during the build.

> ⚠️ **MANDATORY:** The audit MUST include CG alerts from BOTH the **SkiaSharp-Native**
> (pipeline 26493) and **SkiaSharp** (pipeline 10789) pipelines — together they make up the
> shipped build.

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
mkdir -p output/ai
python3 .agents/skills/security-audit/scripts/query-cg-alerts.py \
  > output/ai/cg-alerts-cache.json

# Then read from the file (fast, no API calls):
cat output/ai/cg-alerts-cache.json | \
  python3 -c "import sys,json; d=json.loads(sys.stdin.read()); print(d['totalAlerts'])"
```

The output includes a `queriedAt` ISO timestamp so you can verify freshness.

### Additional Flags

```bash
# Human-readable text output (nothing truncated, all CVEs listed)
python3 .agents/skills/security-audit/scripts/query-cg-alerts.py --text

# Query only a specific branch
python3 .agents/skills/security-audit/scripts/query-cg-alerts.py --branch main

# Query only the native pipeline
python3 .agents/skills/security-audit/scripts/query-cg-alerts.py --pipeline native

# Query only the managed pipeline
python3 .agents/skills/security-audit/scripts/query-cg-alerts.py --pipeline managed

# Query a specific build
python3 .agents/skills/security-audit/scripts/query-cg-alerts.py --build-id 14176611
```

### What the Script Does

1. Discovers the latest completed build from `main` AND all active `release/*` branches for BOTH pipelines.
2. Identifies ALL CG logs in each build (every job, no sampling — this is security).
3. Parses and deduplicates all CVEs across builds, pipelines, and jobs.
4. Sorts by severity and reports which branches and pipelines are affected.

> **Note:** There is no build-independent CG REST API. The `governance.visualstudio.com`
> service does not expose alert data through any documented endpoint. The CG portal UI
> aggregates from build results internally. Our script achieves the same result by
> enumerating every CG log in the latest build of every active branch (no sampling), which
> reports ALL active registration-level alerts.

## Manual Approach (for Debugging)

```bash
# 1. Get latest build ID (native pipeline)
BUILD_ID=$(az pipelines runs list --pipeline-id 26493 \
  --org https://devdiv.visualstudio.com --project DevDiv \
  --top 1 --query "[0].id" -o tsv)

# For managed pipeline, use --pipeline-id 10789 instead

# 2. Get timeline to find CG log IDs
az devops invoke --area build --resource timeline \
  --route-parameters project=DevDiv buildId=$BUILD_ID \
  --org https://devdiv.visualstudio.com -o json

# 3. Parse CVEs from a specific CG log
az devops invoke --area build --resource logs \
  --route-parameters project=DevDiv buildId=$BUILD_ID logId={LOG_ID} \
  --org https://devdiv.visualstudio.com -o json
```

## CG Alert Categories

> These categories are reference context for understanding where alerts come from and how to
> fix them. They are **NOT** part of the report JSON — the viewer groups by component
> automatically.

| Category | Source | Fix Mechanism |
|----------|--------|---------------|
| Alpine sysroot packages | `apk add` in alpine Dockerfile | Bump `DISTRO_VERSION` in Dockerfile |
| Debian base image packages | `apt-get` / base image | Update base image or wait for Debian patches |
| npm build tooling | .NET SDK / Cake dependencies | Update .NET SDK or pin versions |
| Rust crate deps | .NET SDK internals | Update .NET SDK |
| NuGet build deps | Build-time references | Update package version |

## Key Files for CG Fixes

| File | Controls |
|------|----------|
| `scripts/infra/native/linux/docker/alpine/Dockerfile` (lines 43–47) | Alpine sysroot version (`DISTRO_VERSION`) |
| `scripts/infra/native/linux/docker/debian/11/Dockerfile` | Debian 11 base image (EOL June 2026) |
| `scripts/infra/native/linux/docker/debian/13/Dockerfile` | Debian 13 base image |
| `scripts/infra/native/linux/docker/bionic/Dockerfile` | Bionic/Android cross-compile |
| `scripts/infra/native/wasm/docker/Dockerfile` | WASM build container |

## Including CG Data in the Report

> 🛑 **CRITICAL:** Include the **complete `alerts` array** from the script output in the
> report. Do NOT summarize or truncate. The viewer needs every individual alert to render
> correctly. Copy the entire JSON output from `output/ai/cg-alerts-cache.json` as the
> `cgAlerts` value.

```bash
# The cgAlerts section of your report MUST be the raw script output:
cat output/ai/cg-alerts-cache.json
# Copy this entire JSON object as the value of "cgAlerts" in the report.
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
        "pipelines": ["SkiaSharp-Native"],
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

## CG Portal Links

- **Registration:** https://devdiv.visualstudio.com/DevDiv/_componentGovernance/113321
- **Native pipeline:** https://dev.azure.com/devdiv/DevDiv/_build?definitionId=26493
- **Managed pipeline:** https://dev.azure.com/devdiv/DevDiv/_build?definitionId=10789
- **Alert type filter:** Append `?_a=alerts&typeId={typeId}&alerts-view-option=active` to the registration URL
