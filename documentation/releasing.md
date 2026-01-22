# Release Guide

How to release SkiaSharp: create branch → bump main → wait for CI/publish → tag → finalize.

---

## Release Checklist

### 1. Create the Release Branch

| Type | Base Branch | New Branch | Example |
|------|-------------|------------|---------|
| Preview | `main` | `release/{version}-preview.{N}` | `release/3.116.0-preview.1` |
| Stable | `release/{version}-preview.{N}` | `release/{version}` | `release/3.116.0` |
| Hotfix | `v{version}` (tag) | `release/{version}.{fix}-preview.{N}` | `release/3.116.0.1-preview.1` |

> ⚠️ Hotfix releases should only be done for security issues or critical crashes.


### 2. Update Version on Release Branch

**On the release branch**, edit [`scripts/azure-templates-variables.yml`](../scripts/azure-templates-variables.yml) and set `PREVIEW_LABEL`:

| Type | PREVIEW_LABEL | Example |
|------|---------------|---------|
| Preview | `preview.{N}` | `preview.1` |
| Stable | `stable` | `stable` |
| Hotfix | `preview.1` | `preview.1` (always starts at 1 for hotfixes) |

### 3. Bump Version on Main (Preview from main only)

**On the main branch** — do this immediately after creating the release branch. Don't wait for CI.

> ⚠️ Skip this step for stable and hotfix releases — only preview releases from main need version bumping.

Update these files via PR (merge immediately):
- [`scripts/azure-templates-variables.yml`](../scripts/azure-templates-variables.yml) — change `SKIASHARP_VERSION`
- [`scripts/VERSIONS.txt`](../scripts/VERSIONS.txt) — update ALL version numbers:
  - `SkiaSharp file` version (e.g., `3.119.3.0`)
  - All SkiaSharp `nuget` versions (e.g., `3.119.3`)
  - `HarfBuzzSharp file` version — increment 4th digit (e.g., `8.3.1.3` → `8.3.1.4`)
  - All HarfBuzzSharp `nuget` versions — same as file version

> **HarfBuzzSharp versioning:** The first 3 digits (`8.3.1`) correspond to the native HarfBuzz version. The 4th digit is incremented with each SkiaSharp release to keep packages in sync, even without HarfBuzz changes. When native HarfBuzz is upgraded, reset to 3-digit version (e.g., `8.4.0`).

### 4. Wait for CI and Verify Build

Wait for Azure Pipelines build to complete and pass.

**For stable releases only:**
- [ ] NuGet packages are produced in artifacts
- [ ] Native assets are 4-6MB per binary
- [ ] All assemblies are strong named
- [ ] NuGet metadata is correct (tags, icons, licenses)
- [ ] Samples build and deploy in Release mode
- [ ] Documentation has no "To be added." placeholders

### 5. Publish Packages

| Release Type | Destination | Action |
|--------------|-------------|--------|
| Preview | Azure DevOps feed | Automatic after CI |
| Preview | NuGet.org (optional) | Run [publish pipeline](https://dev.azure.com/devdiv/DevDiv/_build?definitionId=25298) with preview flag |
| Stable | NuGet.org | Run [publish pipeline](https://dev.azure.com/devdiv/DevDiv/_build?definitionId=25298) with stable flag |

### 6. Verify Packages are Live

- [ ] Packages are accessible on the target feed
- [ ] Version numbers are correct

### 7. Tag the Release

⚠️ **Only tag after packages are verified live** — tags cannot be deleted.

| Type | Tag Format | Example |
|------|------------|---------|
| Preview | `v{version}-preview.{N}.{build}` | `v3.116.0-preview.1.23` |
| Stable | `v{version}` | `v3.116.0` |
| Hotfix Preview | `v{version}.{fix}-preview.{N}.{build}` | `v3.116.0.1-preview.1.5` |
| Hotfix Stable | `v{version}.{fix}` | `v3.116.0.1` |

### 8. Create GitHub Release

1. Go to GitHub Releases → "Draft a new release"
2. Select the tag
3. Title: `Version {version}` (stable) or `Version {version} (Preview {N})` (preview)
4. **Preview only:** Mark as pre-release
5. **Stable only:** Attach `samples.zip`
6. Add release notes
7. Publish

### 9. Close Milestone (Stable releases only)

- [ ] Close the GitHub milestone with link to release notes

---

## Reference

### Main Branches

| Branch | Purpose |
|--------|---------|
| `main` | Latest development (currently 3.x series) |
| `release/2.x` | Maintenance for 2.x series |

### Key Files

| File | What to Change | When |
|------|----------------|------|
| [`scripts/azure-templates-variables.yml`](../scripts/azure-templates-variables.yml) | `PREVIEW_LABEL` | Every release branch |
| [`scripts/azure-templates-variables.yml`](../scripts/azure-templates-variables.yml) | `SKIASHARP_VERSION` | Immediately after creating release branch (on main) |
| [`scripts/VERSIONS.txt`](../scripts/VERSIONS.txt) | File versions, NuGet versions | Immediately after creating release branch (on main) |

### Pipelines

| Pipeline | Purpose |
|----------|---------|
| [Preview Feed Publish](https://dev.azure.com/devdiv/DevDiv/_build?definitionId=27373) | Builds + auto-publishes to preview feed |
| [NuGet.org Publish](https://dev.azure.com/devdiv/DevDiv/_build?definitionId=25298) | Publishes to NuGet.org (manual) |

### Feeds

| Feed | URL |
|------|-----|
| Preview | `https://aka.ms/skiasharp-eap/index.json` |
| Stable | NuGet.org |

### Automatic Preview Builds

Preview packages are automatically published to the Azure DevOps feed on:
- **Every push to `main`** — version: `{version}-preview.0.{build}`
- **Nightly builds** (midnight UTC) — version: `{version}-nightly.{build}`

To use preview builds, add the feed to your `nuget.config`:

```xml
<configuration>
  <packageSources>
    <add key="SkiaSharp Preview" value="https://aka.ms/skiasharp-eap/index.json" />
  </packageSources>
</configuration>
```
