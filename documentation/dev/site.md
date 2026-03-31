# Building the Website

This guide covers how to build, preview, and iterate on the SkiaSharp documentation site at [mono.github.io/SkiaSharp](https://mono.github.io/SkiaSharp/).

## Site Structure

The published site is assembled from three independently-built components:

| URL Path | Source | Build Tool |
|----------|--------|------------|
| `/` | `documentation/site/` | Static HTML/CSS (no build) |
| `/docs/` | `documentation/docfx/` | [DocFX](https://dotnet.github.io/docfx/) |
| `/gallery/` | `samples/Gallery/Blazor/` | Blazor WASM (`dotnet publish`) |

The CI workflow (`build-site.yml`) builds all three in parallel, then assembles and deploys them.

## Prerequisites

```bash
dotnet tool restore   # installs DocFX and other tools
```

You also need `python3` if you want to use the preview server script.

## Building Locally

### Landing Page

The landing page is plain HTML/CSS — no build step. Just edit and refresh:

- `documentation/site/index.html`
- `documentation/site/style.css`
- `documentation/site/404.html`

To preview in a browser, open `documentation/site/index.html` directly. Note that links to `/docs/` and `/gallery/` won't work since those require the full assembled site.

### DocFX Conceptual Docs

Build the DocFX site locally to iterate on content, styling, templates, or configuration:

```bash
dotnet docfx documentation/docfx/docfx.json
```

This outputs to `output/site/docs/`. To preview with live rebuild on changes:

```bash
dotnet docfx documentation/docfx/docfx.json --serve
```

This starts a local server (typically at `http://localhost:8080`) and watches for file changes. This is the fastest way to iterate on:

- Markdown content (`documentation/docfx/basics/`, `paths/`, etc.)
- Table of contents (`documentation/docfx/TOC.yml`)
- DocFX configuration (`documentation/docfx/docfx.json`)
- Templates and styling (see [Customizing DocFX](#customizing-docfx) below)

### Gallery (Blazor WASM)

The gallery requires SkiaSharp NuGet packages from CI. Building locally:

```bash
# 1. Download CI packages
dotnet cake --target=docs-download-output

# 2. Detect the version
#    Look at output/nugets/ for the SkiaSharp package version

# 3. Generate samples (converts ProjectReference → PackageReference)
dotnet cake --target=samples-generate \
  --previewLabel="preview.0" \
  --buildNumber="76"

# 4. Add the local NuGet source
dotnet nuget add source "$(pwd)/output/nugets" --name local-ci-packages

# 5. Publish the Blazor WASM app
dotnet publish "output/samples-preview/Gallery/Blazor/SkiaSharpSample.Blazor.csproj" \
  -c Release \
  -o output/site/gallery
```

> **Tip:** The gallery build is complex. For most docs work, you can skip it and focus on DocFX or the landing page.

## Previewing the Deployed Site

After CI deploys to a branch, use the preview scripts to serve locally with the correct `/SkiaSharp/` path prefix (matching GitHub Pages):

```powershell
# Preview staging (PRs deploy here)
pwsh scripts/serve-site.ps1

# Preview production
pwsh scripts/serve-site.ps1 docs-live
```

```bash
# Same thing on macOS/Linux
./scripts/serve-site.sh              # docs-staging
./scripts/serve-site.sh docs-live    # production
```

Opens at `http://localhost:8080/SkiaSharp/`. The script clones the branch into a temp directory and serves it via `python3 -m http.server`.

## CI Workflow

The `build-site.yml` workflow handles everything:

```
┌─────────────┐  ┌───────────────┐  ┌───────────────┐
│ build-docs  │  │ build-gallery │  │ build-landing │
│  (DocFX)    │  │ (Blazor WASM) │  │ (static copy) │
└──────┬──────┘  └───────┬───────┘  └───────┬───────┘
       │                 │                   │
       └────────────┬────┴───────────────────┘
                    ▼
              ┌──────────┐
              │  deploy  │  Assembles all artifacts
              └──────────┘  and pushes to branch
```

| Trigger | Deploys to |
|---------|------------|
| Push to `main` | `docs-live` (production) |
| PR (same repo) | `docs-staging` (preview) |
| PR (fork) | Build only, no deploy |
| Manual dispatch | Choice of `docs-live` or `docs-staging` |

## Customizing DocFX

DocFX configuration lives in `documentation/docfx/docfx.json`. Key settings:

| Setting | Purpose |
|---------|---------|
| `template` | `["default", "modern"]` — the visual theme |
| `globalMetadata._appTitle` | Site title shown in the header |
| `globalMetadata._appLogoPath` | Logo image path |
| `globalMetadata._enableSearch` | Enable/disable search |
| `dest` | Output directory (currently `../../output/site/docs`) |

### Custom Templates

To customize the look of the DocFX site beyond what configuration offers, you can add a [custom template](https://dotnet.github.io/docfx/docs/template.html):

1. Export the default template: `dotnet docfx template export modern`
2. Copy the files you want to override into a folder (e.g., `documentation/docfx/template/`)
3. Add your template folder to `docfx.json`:
   ```json
   "template": ["default", "modern", "template"]
   ```
4. Rebuild: `dotnet docfx documentation/docfx/docfx.json --serve`

Common customizations:
- **Styles:** Override CSS in `template/public/main.css`
- **Layout:** Override partials in `template/partials/`
- **Header/Footer:** Modify `template/partials/head.tmpl.partial` or `template/partials/footer.tmpl.partial`

## File Reference

```
documentation/
  docfx/                    # DocFX project root (→ /docs/)
    docfx.json              # DocFX configuration
    index.md                # Docs landing page
    TOC.yml                 # Top nav bar (Conceptual, API Reference)
    guides/             # Tutorial content (→ /docs/guides/)
      TOC.yml               # Sidebar navigation tree
      basics/               # Tutorial sections...
      paths/
      transforms/
      curves/
      bitmaps/
      effects/
      images/               # Shared images
    xrefmaps/               # API cross-reference data
  site/                     # Landing page source (→ /)
    index.html
    style.css
    404.html
  dev/                      # Internal dev docs (not published)
    site.md                 # This file
scripts/
  serve-site.sh             # Preview server (bash)
  serve-site.ps1            # Preview server (PowerShell)
.github/workflows/
  build-site.yml            # Unified build + deploy workflow
```


