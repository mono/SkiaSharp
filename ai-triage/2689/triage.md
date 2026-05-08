# Issue Triage Report — #2689

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T23:54:38Z |
| Type | type/feature-request (0.97 (97%)) |
| Area | area/SkiaSharp (0.82 (82%)) |
| Suggested action | keep-open (0.85 (85%)) |

**Issue Summary:** Feature request to expose SkiaSharp bindings for Skia's SkSVGDOM API, enabling loading and rendering SVG documents from files or streams onto an SKCanvas.

**Analysis:** SkiaSharp currently provides SVG output only (SKSvgCanvas wrapping sk_svgcanvas_create_with_stream), but has no bindings for reading/rendering SVG files via SkSVGDOM. Skia's SVG module (SkSVGDOM::Builder) became non-experimental in m88 and is used widely (e.g., Compose Multiplatform), but requires enabling skia_enable_svg=true at build time and pulls in the text shaper which adds HarfBuzz/unicode dependencies. A dev/experimental/svg-dom branch exists in the repo per community comments. The maintainer has expressed concern about binary size and prefers improving the Svg.Skia ecosystem, but left the issue open with a help-wanted label.

**Recommendations:** **keep-open** — Valid well-specified feature request with community support (6 upvotes, willing contributor). Maintainer left it open with help-wanted. Implementation complexity requires native build changes and a new optional package — not a quick fix but clearly scoped.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | backend/SVG |
| Tenets | — |
| Partner | — |
| Current labels | status/help-wanted, type/feature-request |

## Evidence

### Reproduction

**Environment:** SkiaSharp 2.88.x / 3.x, cross-platform

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2689 — Original feature request with C++ API reference from rust-skia
- https://github.com/kyamagu/skia-python/blob/e0b030c14e33f70880cfcc502eaeed898a77fc3c/src/skia/SVGDOM.cpp#L13 — skia-python SkSVGDOM::MakeFromStream implementation from m87 (before m88 non-experimental)
- https://github.com/mono/SkiaSharp/pull/1987 — Skottie (skottie_animation_t) PR referenced by maintainer as size-comparable precedent

## Analysis

### Technical Summary

SkiaSharp currently provides SVG output only (SKSvgCanvas wrapping sk_svgcanvas_create_with_stream), but has no bindings for reading/rendering SVG files via SkSVGDOM. Skia's SVG module (SkSVGDOM::Builder) became non-experimental in m88 and is used widely (e.g., Compose Multiplatform), but requires enabling skia_enable_svg=true at build time and pulls in the text shaper which adds HarfBuzz/unicode dependencies. A dev/experimental/svg-dom branch exists in the repo per community comments. The maintainer has expressed concern about binary size and prefers improving the Svg.Skia ecosystem, but left the issue open with a help-wanted label.

### Rationale

This is clearly a feature request — the API does not exist yet in SkiaSharp. The request is well-specified and the reporter provides a concrete C++ API reference (from rust-skia). The maintainer has engaged but not committed, citing binary size and maintenance overhead. The issue has community traction (6 upvotes, a community contributor asking to help). Keeping it open as a tracked enhancement is appropriate. A separate optional binding package (analogous to SkiaSharp.Skottie) would be the right architectural fit.

### Key Signals

- "Skia's own frontend support for the SVG format is no longer experimental. Please expose an API around SkSVGDOM::Builder()" — **issue body** (Clear feature request for SkSVGDOM reading/rendering bindings, not a bug report.)
- "One downside of SVG support is that it pulls in the shaper, which pulls in the harfbuzz and unicode modules. This might make the small API balloon significantly." — **maintainer comment (mattleibow)** (Maintainer's primary concern is binary size; suggests optional module design may be needed before proceeding.)
- "Hi @mattleibow is this branch (dev/experimental/svg-dom) supposed to cover this feature? If anyone can guide me I can try to add SkSVGDOM." — **community comment (fcallejon)** (A dev/experimental/svg-dom branch exists; community contributor willing to implement.)
- "Argh, the svgdom code was based on m57. Skia itself is m131 now..." — **community comment (HinTak)** (Any existing experimental work is very outdated relative to current Skia milestone.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKSVG.cs` | — | direct | Only SKSvgCanvas exists — wraps sk_svgcanvas_create_with_stream for writing SVG output. No SkSVGDOM reader/renderer binding exists. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | 15571-15589 | direct | C API surface for SVG only includes sk_svgcanvas_create_with_stream (SVG output). No sk_svgdom_* functions are present — confirming the requested feature is entirely absent. |
| `binding/SkiaSharp.Skottie/` | — | related | Skottie is packaged as a separate optional binding project (SkiaSharp.Skottie.csproj) with its own generated C# API and native dependency (libSkiaSharp.Skottie.json). This is the architectural precedent for adding SkSVGDOM as an optional module. |

### Next Questions

- Is skia_enable_svg=true currently set in the native build scripts?
- What is the binary size increase when enabling SkSVGDOM + shaper?
- Is the dev/experimental/svg-dom branch still active or abandoned?
- Would an SkiaSharp.SVG separate NuGet package be acceptable to the maintainer?

### Resolution Proposals

**Hypothesis:** SkSVGDOM bindings can be added as a separate optional package (SkiaSharp.Svg or similar, analogous to SkiaSharp.Skottie), with a new C API shim exposing SkSVGDOM::Builder and render APIs, built with skia_enable_svg=true.

1. **Add SkSVGDOM as optional separate package** — fix, confidence 0.72 (72%), cost/xl, validated=untested
   - Create a new binding project (e.g., SkiaSharp.Svg) mirroring the SkiaSharp.Skottie structure. Add C API shim functions for sk_svgdom_make_from_stream, sk_svgdom_render, and sk_svgdom_get_container_size. Enable skia_enable_svg=true in native build. This keeps core SkiaSharp lean while enabling SVG loading for users who need it.
2. **Use Svg.Skia as current workaround** — workaround, confidence 0.80 (80%), cost/xs, validated=untested
   - Svg.Skia (https://github.com/wieslawsoltes/Svg.Skia) provides SVG rendering using SkiaSharp. While the reporter noted PDF output quality issues with this approach, it is a functional SVG rendering solution today.

**Recommended proposal:** Add SkSVGDOM as optional separate package

**Why:** Addresses the root ask while respecting the maintainer's binary size concerns through an opt-in separate package, consistent with the existing Skottie precedent.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.85 (85%) |
| Reason | Valid well-specified feature request with community support (6 upvotes, willing contributor). Maintainer left it open with help-wanted. Implementation complexity requires native build changes and a new optional package — not a quick fix but clearly scoped. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply feature-request, area/SkiaSharp, and backend/SVG labels | labels=type/feature-request, area/SkiaSharp, backend/SVG |
| add-comment | medium | 0.85 (85%) | Acknowledge request, confirm architectural path (optional package like Skottie), note dev/experimental/svg-dom branch, invite community contribution | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed request! To summarize the current state:

**What exists today:**
- `SKSvgCanvas` — writes SVG *output* (wraps `SkSVGCanvas`)
- Path SVG helpers — `SKPath.ParseSvgPathData` / `ToSvgPathData`
- No SkSVGDOM bindings for reading/rendering SVG *input* documents

**Architecture note:** The cleanest path for this would be a separate optional NuGet package (e.g., `SkiaSharp.Svg`) modeled on [`SkiaSharp.Skottie`](https://github.com/mono/SkiaSharp/tree/main/binding/SkiaSharp.Skottie), which already shows how to add optional Skia modules with their own C API shim and native build flags. This avoids inflating the core `SkiaSharp` package with the HarfBuzz shaper dependency that `skia_enable_svg=true` pulls in.

**For anyone wanting to contribute:** The `dev/experimental/svg-dom` branch is a starting point (though likely outdated against current Skia milestone). The key steps would be:
1. Add C API functions (`sk_svgdom_make_from_stream`, `sk_svgdom_render`, `sk_svgdom_get_container_size`) to the Skia fork
2. Enable `skia_enable_svg=true` in the native build
3. Create a new `SkiaSharp.Svg` binding project with generated C# wrappers

**In the meantime**, [Svg.Skia](https://github.com/wieslawsoltes/Svg.Skia) provides SVG rendering using SkiaSharp today.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2689,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T23:54:38Z",
    "currentLabels": [
      "status/help-wanted",
      "type/feature-request"
    ]
  },
  "summary": "Feature request to expose SkiaSharp bindings for Skia's SkSVGDOM API, enabling loading and rendering SVG documents from files or streams onto an SKCanvas.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.82
    },
    "backends": [
      "backend/SVG"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "SkiaSharp 2.88.x / 3.x, cross-platform",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2689",
          "description": "Original feature request with C++ API reference from rust-skia"
        },
        {
          "url": "https://github.com/kyamagu/skia-python/blob/e0b030c14e33f70880cfcc502eaeed898a77fc3c/src/skia/SVGDOM.cpp#L13",
          "description": "skia-python SkSVGDOM::MakeFromStream implementation from m87 (before m88 non-experimental)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/1987",
          "description": "Skottie (skottie_animation_t) PR referenced by maintainer as size-comparable precedent"
        }
      ]
    }
  },
  "analysis": {
    "summary": "SkiaSharp currently provides SVG output only (SKSvgCanvas wrapping sk_svgcanvas_create_with_stream), but has no bindings for reading/rendering SVG files via SkSVGDOM. Skia's SVG module (SkSVGDOM::Builder) became non-experimental in m88 and is used widely (e.g., Compose Multiplatform), but requires enabling skia_enable_svg=true at build time and pulls in the text shaper which adds HarfBuzz/unicode dependencies. A dev/experimental/svg-dom branch exists in the repo per community comments. The maintainer has expressed concern about binary size and prefers improving the Svg.Skia ecosystem, but left the issue open with a help-wanted label.",
    "rationale": "This is clearly a feature request — the API does not exist yet in SkiaSharp. The request is well-specified and the reporter provides a concrete C++ API reference (from rust-skia). The maintainer has engaged but not committed, citing binary size and maintenance overhead. The issue has community traction (6 upvotes, a community contributor asking to help). Keeping it open as a tracked enhancement is appropriate. A separate optional binding package (analogous to SkiaSharp.Skottie) would be the right architectural fit.",
    "keySignals": [
      {
        "text": "Skia's own frontend support for the SVG format is no longer experimental. Please expose an API around SkSVGDOM::Builder()",
        "source": "issue body",
        "interpretation": "Clear feature request for SkSVGDOM reading/rendering bindings, not a bug report."
      },
      {
        "text": "One downside of SVG support is that it pulls in the shaper, which pulls in the harfbuzz and unicode modules. This might make the small API balloon significantly.",
        "source": "maintainer comment (mattleibow)",
        "interpretation": "Maintainer's primary concern is binary size; suggests optional module design may be needed before proceeding."
      },
      {
        "text": "Hi @mattleibow is this branch (dev/experimental/svg-dom) supposed to cover this feature? If anyone can guide me I can try to add SkSVGDOM.",
        "source": "community comment (fcallejon)",
        "interpretation": "A dev/experimental/svg-dom branch exists; community contributor willing to implement."
      },
      {
        "text": "Argh, the svgdom code was based on m57. Skia itself is m131 now...",
        "source": "community comment (HinTak)",
        "interpretation": "Any existing experimental work is very outdated relative to current Skia milestone."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKSVG.cs",
        "finding": "Only SKSvgCanvas exists — wraps sk_svgcanvas_create_with_stream for writing SVG output. No SkSVGDOM reader/renderer binding exists.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "15571-15589",
        "finding": "C API surface for SVG only includes sk_svgcanvas_create_with_stream (SVG output). No sk_svgdom_* functions are present — confirming the requested feature is entirely absent.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp.Skottie/",
        "finding": "Skottie is packaged as a separate optional binding project (SkiaSharp.Skottie.csproj) with its own generated C# API and native dependency (libSkiaSharp.Skottie.json). This is the architectural precedent for adding SkSVGDOM as an optional module.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "Is skia_enable_svg=true currently set in the native build scripts?",
      "What is the binary size increase when enabling SkSVGDOM + shaper?",
      "Is the dev/experimental/svg-dom branch still active or abandoned?",
      "Would an SkiaSharp.SVG separate NuGet package be acceptable to the maintainer?"
    ],
    "resolution": {
      "hypothesis": "SkSVGDOM bindings can be added as a separate optional package (SkiaSharp.Svg or similar, analogous to SkiaSharp.Skottie), with a new C API shim exposing SkSVGDOM::Builder and render APIs, built with skia_enable_svg=true.",
      "proposals": [
        {
          "title": "Add SkSVGDOM as optional separate package",
          "description": "Create a new binding project (e.g., SkiaSharp.Svg) mirroring the SkiaSharp.Skottie structure. Add C API shim functions for sk_svgdom_make_from_stream, sk_svgdom_render, and sk_svgdom_get_container_size. Enable skia_enable_svg=true in native build. This keeps core SkiaSharp lean while enabling SVG loading for users who need it.",
          "category": "fix",
          "confidence": 0.72,
          "effort": "cost/xl",
          "validated": "untested"
        },
        {
          "title": "Use Svg.Skia as current workaround",
          "description": "Svg.Skia (https://github.com/wieslawsoltes/Svg.Skia) provides SVG rendering using SkiaSharp. While the reporter noted PDF output quality issues with this approach, it is a functional SVG rendering solution today.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add SkSVGDOM as optional separate package",
      "recommendedReason": "Addresses the root ask while respecting the maintainer's binary size concerns through an opt-in separate package, consistent with the existing Skottie precedent."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.85,
      "reason": "Valid well-specified feature request with community support (6 upvotes, willing contributor). Maintainer left it open with help-wanted. Implementation complexity requires native build changes and a new optional package — not a quick fix but clearly scoped.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request, area/SkiaSharp, and backend/SVG labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp",
          "backend/SVG"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge request, confirm architectural path (optional package like Skottie), note dev/experimental/svg-dom branch, invite community contribution",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed request! To summarize the current state:\n\n**What exists today:**\n- `SKSvgCanvas` — writes SVG *output* (wraps `SkSVGCanvas`)\n- Path SVG helpers — `SKPath.ParseSvgPathData` / `ToSvgPathData`\n- No SkSVGDOM bindings for reading/rendering SVG *input* documents\n\n**Architecture note:** The cleanest path for this would be a separate optional NuGet package (e.g., `SkiaSharp.Svg`) modeled on [`SkiaSharp.Skottie`](https://github.com/mono/SkiaSharp/tree/main/binding/SkiaSharp.Skottie), which already shows how to add optional Skia modules with their own C API shim and native build flags. This avoids inflating the core `SkiaSharp` package with the HarfBuzz shaper dependency that `skia_enable_svg=true` pulls in.\n\n**For anyone wanting to contribute:** The `dev/experimental/svg-dom` branch is a starting point (though likely outdated against current Skia milestone). The key steps would be:\n1. Add C API functions (`sk_svgdom_make_from_stream`, `sk_svgdom_render`, `sk_svgdom_get_container_size`) to the Skia fork\n2. Enable `skia_enable_svg=true` in the native build\n3. Create a new `SkiaSharp.Svg` binding project with generated C# wrappers\n\n**In the meantime**, [Svg.Skia](https://github.com/wieslawsoltes/Svg.Skia) provides SVG rendering using SkiaSharp today."
      }
    ]
  }
}
```

</details>
