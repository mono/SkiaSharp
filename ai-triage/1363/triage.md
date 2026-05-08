# Issue Triage Report — #1363

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T18:08:23Z |
| Type | type/feature-request (0.92 (92%)) |
| Area | area/Build (0.95 (95%)) |
| Suggested action | keep-open (0.80 (80%)) |

**Issue Summary:** Feature request to move Linux native builds away from Cake to a bash script or Makefile, removing the Mono dependency for Linux build contributors.

**Analysis:** The reporter (a project maintainer) requests that Linux native builds be moved out of Cake into a simpler bash script or Makefile, citing Mono as an unnecessary extra requirement for Linux build contributors. As of triage, the native/linux/build.cake file still exists and Cake is still used for Linux builds via build.sh invoking dotnet cake.

**Recommendations:** **keep-open** — Valid build infrastructure feature request filed by a maintainer. The Linux build still uses Cake as of triage. No blocking urgency but worth pursuing to simplify CI contributor setup.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/Build |
| Platforms | os/Linux |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

**Environment:** Linux native build pipeline, Cake/Mono dependency

## Analysis

### Technical Summary

The reporter (a project maintainer) requests that Linux native builds be moved out of Cake into a simpler bash script or Makefile, citing Mono as an unnecessary extra requirement for Linux build contributors. As of triage, the native/linux/build.cake file still exists and Cake is still used for Linux builds via build.sh invoking dotnet cake.

### Rationale

The issue title uses [FEATURE] and the body describes a new capability (replacing Cake with a bash/Makefile alternative for Linux). This is a feature request to change the build tooling, not a bug in SkiaSharp functionality. The area is Build since it concerns the build infrastructure. The feature has not yet been implemented — native/linux/build.cake still exists and build.sh still calls dotnet cake.

### Key Signals

- "Building SkiaSharp does not actually use it, and we can maybe have a bash script or Makefile to do this." — **issue body** (Explicit feature request to replace Cake with simpler tooling for Linux builds.)
- "Cake requires mono, and although it is pretty cross-platform, it is an additional requirement." — **issue body** (The motivation is removing the Mono/Cake dependency from the Linux build path.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `native/linux/build.cake` | — | direct | Linux native build is defined in a Cake script — the Cake dependency for Linux builds is confirmed still present. |
| `build.sh` | — | direct | The top-level build.sh still calls `dotnet cake $@`, meaning all targets including Linux go through Cake. No bash/Makefile alternative exists for Linux. |

### Resolution Proposals

**Hypothesis:** Replace native/linux/build.cake with a bash script or Makefile that invokes GN/ninja directly to build libSkiaSharp and libHarfBuzzSharp for Linux, allowing CI to build without requiring dotnet/Cake on Linux builders.

1. **Add bash build script for Linux** — fix, confidence 0.75 (75%), cost/l, validated=untested
   - Create a build.sh in native/linux/ that directly invokes GN configuration and ninja build, mirroring what build.cake does but without requiring Mono or Cake. The main build.sh can delegate to this for Linux targets.
2. **Keep Cake but document Mono requirement clearly** — investigation, confidence 0.60 (60%), cost/xs, validated=untested
   - As a lower-effort alternative, document clearly that Linux builds require dotnet (not mono) since Cake now runs on dotnet. This may already be addressed if the project moved to dotnet-based Cake.

**Recommended proposal:** Add bash build script for Linux

**Why:** Directly addresses the stated goal of removing the Cake/Mono dependency from Linux builds.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.80 (80%) |
| Reason | Valid build infrastructure feature request filed by a maintainer. The Linux build still uses Cake as of triage. No blocking urgency but worth pursuing to simplify CI contributor setup. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply feature-request, Build, and Linux labels | labels=type/feature-request, area/Build, os/Linux |
