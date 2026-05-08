# Issue Triage Report — #3005

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T18:16:35Z |
| Type | type/feature-request (0.92 (92%)) |
| Area | area/SkiaSharp.Views (0.90 (90%)) |
| Suggested action | keep-open (0.88 (88%)) |

**Issue Summary:** Feature request to add XAML namespace schema URIs (XmlnsDefinition) to SkiaSharp.Views.WPF assembly so users can reference it with a short URI instead of the verbose CLR namespace declaration.

**Analysis:** SkiaSharp.Views.WPF does not define any XmlnsDefinition assembly attributes, so there is no XAML namespace schema URI shorthand. Users must use the verbose CLR namespace declaration. The maintainer has explicitly endorsed adding a schema.

**Recommendations:** **keep-open** — Valid, endorsed feature request. Maintainer confirmed interest. Implementation is clear but the schema URI string must be decided before work begins.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Try to use a short XAML namespace URI for SkiaSharp WPF (like xmlns:skia="http://schemas.skiasharp.com/wpf")
2. Observe that no such schema exists — must use the verbose CLR namespace form: xmlns:skia="clr-namespace:SkiaSharp.Views.WPF;assembly=SkiaSharp.Views.WPF"

**Environment:** SkiaSharp.Views.WPF, WPF, Windows

**Repository links:**
- https://github.com/microsoft/XamlBehaviorsWpf/blob/1a72a7608536fcae884c1ed318b18ce6411974c5/src/Microsoft.Xaml.Behaviors/AssemblyInfo.cs#L33 — Reference implementation: Microsoft.Xaml.Behaviors uses XmlnsDefinition attribute
- https://github.com/dotnet/DataGridExtensions/blob/a8b0e4aee70bbedcd1831b5b28eee3abd9a7ae83/src/DataGridExtensions/Properties/AssemblyInfo.cs#L12 — Reference implementation: DataGridExtensions uses XmlnsDefinition attribute
- https://github.com/punker76/gong-wpf-dragdrop/blob/de75a24239a950ae381bac602c3ce3867f6cd2f6/src/GongSolutions.WPF.DragDrop/Properties/AssemblyInfo.cs#L7 — Reference implementation: gong-wpf-dragdrop uses XmlnsDefinition attribute

## Analysis

### Technical Summary

SkiaSharp.Views.WPF does not define any XmlnsDefinition assembly attributes, so there is no XAML namespace schema URI shorthand. Users must use the verbose CLR namespace declaration. The maintainer has explicitly endorsed adding a schema.

### Rationale

Confirmed absence of XmlnsDefinition attributes in both the shared assembly info and the WPF project files. The maintainer's comment 'I think skia needs its own xaml schema :)' confirms this is a desired enhancement. A community commenter provided three concrete reference implementations from Microsoft and other popular WPF libraries. This is squarely a feature request — adding new QoL functionality that currently doesn't exist.

### Key Signals

- "xmlns:skia="clr-namespace:SkiaSharp.Views.WPF;assembly=SkiaSharp.Views.WPF"" — **issue body** (Current verbose CLR namespace form required by users; reporter wants a short schema URI equivalent.)
- "I think skia needs its own xaml schema :)" — **comment by mattleibow (maintainer)** (Maintainer explicitly endorses the feature request.)
- "Links to XmlnsDefinition examples in Microsoft.Xaml.Behaviors, DataGridExtensions, and gong-wpf-dragdrop" — **comment by myd7349** (Community has identified the standard WPF pattern: add [assembly: XmlnsDefinition(...)] in AssemblyInfo.cs. Implementation path is clear.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.Shared/Properties/SkiaSharpViewsAssemblyInfo.cs` | — | direct | Shared assembly info file contains only AssemblyTitle, AssemblyDescription, AssemblyCompany, AssemblyProduct, AssemblyCopyright, and NeutralResourcesLanguage attributes. No XmlnsDefinition attribute is present. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SkiaSharp.Views.WPF.csproj` | — | direct | Project file sets RootNamespace=SkiaSharp.Views.WPF and includes the shared AssemblyInfo. No XmlnsDefinition-related properties or attributes are configured. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKElement.cs` | — | context | SKElement is declared in namespace SkiaSharp.Views.WPF. No assembly-level XmlnsDefinition attribute. Users must reference the full CLR namespace in XAML. |

### Workarounds

- Continue using the verbose CLR namespace form: xmlns:skia="clr-namespace:SkiaSharp.Views.WPF;assembly=SkiaSharp.Views.WPF"
- Define a local alias in each XAML file using the CLR namespace syntax

### Resolution Proposals

**Hypothesis:** Adding [assembly: XmlnsDefinition("http://schemas.skiasharp.com/wpf", "SkiaSharp.Views.WPF")] to the WPF assembly info (or a dedicated WPF-specific AssemblyInfo.cs) would enable the short xmlns form. The chosen URI needs to be decided by maintainers.

1. **Add XmlnsDefinition attribute to SkiaSharp.Views.WPF assembly** — fix, confidence 0.85 (85%), cost/xs, validated=untested
   - Add one or more [assembly: XmlnsDefinition(...)] attributes to a WPF-specific AssemblyInfo.cs file in the SkiaSharp.Views.WPF project, following the pattern used by Microsoft.Xaml.Behaviors and other libraries. The URI value (e.g. http://schemas.skiasharp.com/wpf) needs to be decided by maintainers.
2. **Consider extending to WinUI and other XAML platforms** — alternative, confidence 0.70 (70%), cost/s, validated=untested
   - If WPF gets a schema URI, consider also adding XmlnsDefinition to SkiaSharp.Views.WinUI for consistency across XAML platforms.

**Recommended proposal:** Add XmlnsDefinition attribute to SkiaSharp.Views.WPF assembly

**Why:** Minimal effort (single attribute), well-understood pattern with multiple reference implementations already provided by the community. The main open question is the schema URI string, which the maintainer needs to decide.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.88 (88%) |
| Reason | Valid, endorsed feature request. Maintainer confirmed interest. Implementation is clear but the schema URI string must be decided before work begins. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply feature-request, views, and Windows-Classic labels | labels=type/feature-request, area/SkiaSharp.Views, os/Windows-Classic |
| add-comment | medium | 0.85 (85%) | Acknowledge the request, confirm it's tracked, and explain what is needed before implementation can start | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for raising this! Adding `XmlnsDefinition` attributes to `SkiaSharp.Views.WPF` is definitely the right approach — libraries like [Microsoft.Xaml.Behaviors](https://github.com/microsoft/XamlBehaviorsWpf/blob/1a72a7608536fcae884c1ed318b18ce6411974c5/src/Microsoft.Xaml.Behaviors/AssemblyInfo.cs#L33) use exactly this pattern.

The main open question is what URI to use (e.g., `http://schemas.skiasharp.com/wpf`). Once that's decided the implementation would be a one-line addition to the assembly info. Tracking this as a feature request until we settle on the URI.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3005,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T18:16:35Z"
  },
  "summary": "Feature request to add XAML namespace schema URIs (XmlnsDefinition) to SkiaSharp.Views.WPF assembly so users can reference it with a short URI instead of the verbose CLR namespace declaration.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Try to use a short XAML namespace URI for SkiaSharp WPF (like xmlns:skia=\"http://schemas.skiasharp.com/wpf\")",
        "Observe that no such schema exists — must use the verbose CLR namespace form: xmlns:skia=\"clr-namespace:SkiaSharp.Views.WPF;assembly=SkiaSharp.Views.WPF\""
      ],
      "environmentDetails": "SkiaSharp.Views.WPF, WPF, Windows",
      "repoLinks": [
        {
          "url": "https://github.com/microsoft/XamlBehaviorsWpf/blob/1a72a7608536fcae884c1ed318b18ce6411974c5/src/Microsoft.Xaml.Behaviors/AssemblyInfo.cs#L33",
          "description": "Reference implementation: Microsoft.Xaml.Behaviors uses XmlnsDefinition attribute"
        },
        {
          "url": "https://github.com/dotnet/DataGridExtensions/blob/a8b0e4aee70bbedcd1831b5b28eee3abd9a7ae83/src/DataGridExtensions/Properties/AssemblyInfo.cs#L12",
          "description": "Reference implementation: DataGridExtensions uses XmlnsDefinition attribute"
        },
        {
          "url": "https://github.com/punker76/gong-wpf-dragdrop/blob/de75a24239a950ae381bac602c3ce3867f6cd2f6/src/GongSolutions.WPF.DragDrop/Properties/AssemblyInfo.cs#L7",
          "description": "Reference implementation: gong-wpf-dragdrop uses XmlnsDefinition attribute"
        }
      ]
    }
  },
  "analysis": {
    "summary": "SkiaSharp.Views.WPF does not define any XmlnsDefinition assembly attributes, so there is no XAML namespace schema URI shorthand. Users must use the verbose CLR namespace declaration. The maintainer has explicitly endorsed adding a schema.",
    "rationale": "Confirmed absence of XmlnsDefinition attributes in both the shared assembly info and the WPF project files. The maintainer's comment 'I think skia needs its own xaml schema :)' confirms this is a desired enhancement. A community commenter provided three concrete reference implementations from Microsoft and other popular WPF libraries. This is squarely a feature request — adding new QoL functionality that currently doesn't exist.",
    "keySignals": [
      {
        "text": "xmlns:skia=\"clr-namespace:SkiaSharp.Views.WPF;assembly=SkiaSharp.Views.WPF\"",
        "source": "issue body",
        "interpretation": "Current verbose CLR namespace form required by users; reporter wants a short schema URI equivalent."
      },
      {
        "text": "I think skia needs its own xaml schema :)",
        "source": "comment by mattleibow (maintainer)",
        "interpretation": "Maintainer explicitly endorses the feature request."
      },
      {
        "text": "Links to XmlnsDefinition examples in Microsoft.Xaml.Behaviors, DataGridExtensions, and gong-wpf-dragdrop",
        "source": "comment by myd7349",
        "interpretation": "Community has identified the standard WPF pattern: add [assembly: XmlnsDefinition(...)] in AssemblyInfo.cs. Implementation path is clear."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Shared/Properties/SkiaSharpViewsAssemblyInfo.cs",
        "finding": "Shared assembly info file contains only AssemblyTitle, AssemblyDescription, AssemblyCompany, AssemblyProduct, AssemblyCopyright, and NeutralResourcesLanguage attributes. No XmlnsDefinition attribute is present.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WPF/SkiaSharp.Views.WPF.csproj",
        "finding": "Project file sets RootNamespace=SkiaSharp.Views.WPF and includes the shared AssemblyInfo. No XmlnsDefinition-related properties or attributes are configured.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKElement.cs",
        "finding": "SKElement is declared in namespace SkiaSharp.Views.WPF. No assembly-level XmlnsDefinition attribute. Users must reference the full CLR namespace in XAML.",
        "relevance": "context"
      }
    ],
    "workarounds": [
      "Continue using the verbose CLR namespace form: xmlns:skia=\"clr-namespace:SkiaSharp.Views.WPF;assembly=SkiaSharp.Views.WPF\"",
      "Define a local alias in each XAML file using the CLR namespace syntax"
    ],
    "resolution": {
      "hypothesis": "Adding [assembly: XmlnsDefinition(\"http://schemas.skiasharp.com/wpf\", \"SkiaSharp.Views.WPF\")] to the WPF assembly info (or a dedicated WPF-specific AssemblyInfo.cs) would enable the short xmlns form. The chosen URI needs to be decided by maintainers.",
      "proposals": [
        {
          "title": "Add XmlnsDefinition attribute to SkiaSharp.Views.WPF assembly",
          "description": "Add one or more [assembly: XmlnsDefinition(...)] attributes to a WPF-specific AssemblyInfo.cs file in the SkiaSharp.Views.WPF project, following the pattern used by Microsoft.Xaml.Behaviors and other libraries. The URI value (e.g. http://schemas.skiasharp.com/wpf) needs to be decided by maintainers.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Consider extending to WinUI and other XAML platforms",
          "description": "If WPF gets a schema URI, consider also adding XmlnsDefinition to SkiaSharp.Views.WinUI for consistency across XAML platforms.",
          "category": "alternative",
          "confidence": 0.7,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add XmlnsDefinition attribute to SkiaSharp.Views.WPF assembly",
      "recommendedReason": "Minimal effort (single attribute), well-understood pattern with multiple reference implementations already provided by the community. The main open question is the schema URI string, which the maintainer needs to decide."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.88,
      "reason": "Valid, endorsed feature request. Maintainer confirmed interest. Implementation is clear but the schema URI string must be decided before work begins.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request, views, and Windows-Classic labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp.Views",
          "os/Windows-Classic"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the request, confirm it's tracked, and explain what is needed before implementation can start",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for raising this! Adding `XmlnsDefinition` attributes to `SkiaSharp.Views.WPF` is definitely the right approach — libraries like [Microsoft.Xaml.Behaviors](https://github.com/microsoft/XamlBehaviorsWpf/blob/1a72a7608536fcae884c1ed318b18ce6411974c5/src/Microsoft.Xaml.Behaviors/AssemblyInfo.cs#L33) use exactly this pattern.\n\nThe main open question is what URI to use (e.g., `http://schemas.skiasharp.com/wpf`). Once that's decided the implementation would be a one-line addition to the assembly info. Tracking this as a feature request until we settle on the URI."
      }
    ]
  }
}
```

</details>
