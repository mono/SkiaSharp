# Issue Triage Report — #2357

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-04T10:47:53Z |
| Type | type/question (0.97 (97%)) |
| Area | area/SkiaSharp (0.80 (80%)) |
| Suggested action | close-as-not-a-bug (0.88 (88%)) |

**Issue Summary:** User asks how to handle GPL/LGPL license components from HarfBuzz's dependency chain (specifically Ragel) in a commercial product using SkiaSharp.

**Analysis:** The reporter believes Ragel (GPL) is a runtime dependency of HarfBuzz, making SkiaSharp incompatible with commercial closed-source products. In reality, Ragel is a code generation build tool — it generates C/C++ source files that are compiled into HarfBuzz. The generated code itself is not governed by Ragel's GPL license; only the Ragel tool binary is. HarfBuzz (and thus SkiaSharp) ships pre-generated C/C++ files, not the Ragel tool. This is the same pattern as using flex/bison at build time. The question is answerable without a code fix.

**Recommendations:** **close-as-not-a-bug** — Licensing question answerable with clarification: Ragel is a build-time code generator not redistributed to users; LGPL components allow commercial linking. No code change needed.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

**Environment:** Commercial product, no specific platform stated

## Analysis

### Technical Summary

The reporter believes Ragel (GPL) is a runtime dependency of HarfBuzz, making SkiaSharp incompatible with commercial closed-source products. In reality, Ragel is a code generation build tool — it generates C/C++ source files that are compiled into HarfBuzz. The generated code itself is not governed by Ragel's GPL license; only the Ragel tool binary is. HarfBuzz (and thus SkiaSharp) ships pre-generated C/C++ files, not the Ragel tool. This is the same pattern as using flex/bison at build time. The question is answerable without a code fix.

### Rationale

Issue title and body clearly mark this as a question ('I'd like to know', 'Some tips are welcome'). There is no bug report or reproducible error. The licensing concern stems from a misunderstanding of build-time vs runtime dependencies. Ragel is only a build-time code generator; SkiaSharp/HarfBuzz users do not redistribute Ragel. The distributed binary contains Ragel-generated output, which is the user's own code by standard interpretation. LGPL-2.1 components (Cairo, FreeType2, GLib) have their own implications but LGPL allows linking in commercial products as long as the user can relink with a modified version.

### Key Signals

- "I'd like to know if you are aware and if you know how to deal with the following" — **issue body** (Explicit question seeking guidance, not reporting a defect.)
- "Ragel, is GNU GPL" — **issue body** (Reporter believes the GPL Ragel tool propagates its license to SkiaSharp users.)
- "In a commercial product how do you deal with this situation?" — **issue body** (Asking for advice on commercial usage, not reporting a bug.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `LICENSE.md` | — | context | SkiaSharp itself is MIT licensed. License file does not enumerate transitive build tool dependencies like Ragel. |
| `External-Dependency-Info.txt` | — | direct | File documents external dependencies. Ragel is not listed as a shipped runtime dependency — it is a build-time code generator used only when regenerating HarfBuzz's Unicode shaping tables. |

### Workarounds

- Ragel is a build-time code generator — its GPL license applies only to the tool itself, not to the C/C++ code it generates. HarfBuzz ships pre-generated source files so end users never use the Ragel tool.
- LGPL-2.1 components (FreeType2, GLib) allow commercial linking as long as the end user can re-link with a modified version of those libraries. Shipping SkiaSharp as a dynamically-linked native library satisfies this requirement.
- Review the HarfBuzz project license (Old-MIT/permissive) directly at https://github.com/harfbuzz/harfbuzz/blob/main/COPYING for authoritative guidance.

### Resolution Proposals

**Hypothesis:** Reporter conflates build-time tool dependency (Ragel) with a runtime/distributed dependency. No code change is needed in SkiaSharp — this is a clarification question.

1. **Clarify Ragel as build-time tool only** — workaround, confidence 0.90 (90%), cost/xs, validated=untested
   - Explain that Ragel is only used at build time to generate C source files for HarfBuzz. The generated output is included in HarfBuzz's source distribution, so users of SkiaSharp never invoke or redistribute Ragel. The GPL does not propagate to generated code.
2. **Redirect to HarfBuzz licensing FAQ or legal counsel** — alternative, confidence 0.85 (85%), cost/xs, validated=untested
   - For authoritative guidance on LGPL linking requirements (FreeType2, GLib), suggest the reporter consult HarfBuzz's license file and seek legal counsel for their specific commercial context.

**Recommended proposal:** Clarify Ragel as build-time tool only

**Why:** Directly addresses the root misconception. Ragel is not a runtime dependency; the GPL concern is a non-issue for SkiaSharp users.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.88 (88%) |
| Reason | Licensing question answerable with clarification: Ragel is a build-time code generator not redistributed to users; LGPL components allow commercial linking. No code change needed. |
| Suggested repro platform | — |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply question and compatibility tenet labels | labels=type/question, area/SkiaSharp, tenet/compatibility |
| add-comment | high | 0.85 (85%) | Explain Ragel is a build-time tool and LGPL implications | — |
| close-issue | medium | 0.85 (85%) | Close as answered question | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the question!

**Ragel (GPL):** Ragel is a *build-time code generator* — it is used only when regenerating HarfBuzz's internal Unicode shaping tables from `.rl` grammar files. The pre-generated C/C++ output is already included in HarfBuzz's source distribution. Users of SkiaSharp **never invoke or redistribute the Ragel tool**, so the GPL does not propagate to your application or your users. This is the same principle as using `flex` or `bison` (also GPL) as a build-time lexer/parser generator.

**LGPL-2.1 components (FreeType2, GLib, Cairo):** LGPL-2.1 allows commercial use and linking in closed-source products, provided end users can re-link the application with a modified version of the LGPL library. In practice, distributing the LGPL libraries as separate shared libraries (`.so`/`.dll`) satisfies this requirement. Skia/HarfBuzz typically links these statically in the pre-built SkiaSharp native binaries — if you are using the official NuGet packages, check the SkiaSharp licensing documentation or consult with a legal professional about your specific distribution model.

**HarfBuzz itself** uses the Old-MIT (permissive) license — see https://github.com/harfbuzz/harfbuzz/blob/main/COPYING.

For authoritative advice on your specific commercial context, consulting a legal professional familiar with open-source licensing is recommended. We're closing this as a general Q&A question — please feel free to open a new issue if there is a specific SkiaSharp licensing document you feel is missing.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2357,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-04T10:47:53Z"
  },
  "summary": "User asks how to handle GPL/LGPL license components from HarfBuzz's dependency chain (specifically Ragel) in a commercial product using SkiaSharp.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.8
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "Commercial product, no specific platform stated"
    }
  },
  "analysis": {
    "summary": "The reporter believes Ragel (GPL) is a runtime dependency of HarfBuzz, making SkiaSharp incompatible with commercial closed-source products. In reality, Ragel is a code generation build tool — it generates C/C++ source files that are compiled into HarfBuzz. The generated code itself is not governed by Ragel's GPL license; only the Ragel tool binary is. HarfBuzz (and thus SkiaSharp) ships pre-generated C/C++ files, not the Ragel tool. This is the same pattern as using flex/bison at build time. The question is answerable without a code fix.",
    "rationale": "Issue title and body clearly mark this as a question ('I'd like to know', 'Some tips are welcome'). There is no bug report or reproducible error. The licensing concern stems from a misunderstanding of build-time vs runtime dependencies. Ragel is only a build-time code generator; SkiaSharp/HarfBuzz users do not redistribute Ragel. The distributed binary contains Ragel-generated output, which is the user's own code by standard interpretation. LGPL-2.1 components (Cairo, FreeType2, GLib) have their own implications but LGPL allows linking in commercial products as long as the user can relink with a modified version.",
    "keySignals": [
      {
        "text": "I'd like to know if you are aware and if you know how to deal with the following",
        "source": "issue body",
        "interpretation": "Explicit question seeking guidance, not reporting a defect."
      },
      {
        "text": "Ragel, is GNU GPL",
        "source": "issue body",
        "interpretation": "Reporter believes the GPL Ragel tool propagates its license to SkiaSharp users."
      },
      {
        "text": "In a commercial product how do you deal with this situation?",
        "source": "issue body",
        "interpretation": "Asking for advice on commercial usage, not reporting a bug."
      }
    ],
    "codeInvestigation": [
      {
        "file": "LICENSE.md",
        "finding": "SkiaSharp itself is MIT licensed. License file does not enumerate transitive build tool dependencies like Ragel.",
        "relevance": "context"
      },
      {
        "file": "External-Dependency-Info.txt",
        "finding": "File documents external dependencies. Ragel is not listed as a shipped runtime dependency — it is a build-time code generator used only when regenerating HarfBuzz's Unicode shaping tables.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Ragel is a build-time code generator — its GPL license applies only to the tool itself, not to the C/C++ code it generates. HarfBuzz ships pre-generated source files so end users never use the Ragel tool.",
      "LGPL-2.1 components (FreeType2, GLib) allow commercial linking as long as the end user can re-link with a modified version of those libraries. Shipping SkiaSharp as a dynamically-linked native library satisfies this requirement.",
      "Review the HarfBuzz project license (Old-MIT/permissive) directly at https://github.com/harfbuzz/harfbuzz/blob/main/COPYING for authoritative guidance."
    ],
    "resolution": {
      "hypothesis": "Reporter conflates build-time tool dependency (Ragel) with a runtime/distributed dependency. No code change is needed in SkiaSharp — this is a clarification question.",
      "proposals": [
        {
          "title": "Clarify Ragel as build-time tool only",
          "description": "Explain that Ragel is only used at build time to generate C source files for HarfBuzz. The generated output is included in HarfBuzz's source distribution, so users of SkiaSharp never invoke or redistribute Ragel. The GPL does not propagate to generated code.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Redirect to HarfBuzz licensing FAQ or legal counsel",
          "description": "For authoritative guidance on LGPL linking requirements (FreeType2, GLib), suggest the reporter consult HarfBuzz's license file and seek legal counsel for their specific commercial context.",
          "category": "alternative",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Clarify Ragel as build-time tool only",
      "recommendedReason": "Directly addresses the root misconception. Ragel is not a runtime dependency; the GPL concern is a non-issue for SkiaSharp users."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.88,
      "reason": "Licensing question answerable with clarification: Ragel is a build-time code generator not redistributed to users; LGPL components allow commercial linking. No code change needed."
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question and compatibility tenet labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/question",
          "area/SkiaSharp",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain Ragel is a build-time tool and LGPL implications",
        "risk": "high",
        "confidence": 0.85,
        "comment": "Thanks for the question!\n\n**Ragel (GPL):** Ragel is a *build-time code generator* — it is used only when regenerating HarfBuzz's internal Unicode shaping tables from `.rl` grammar files. The pre-generated C/C++ output is already included in HarfBuzz's source distribution. Users of SkiaSharp **never invoke or redistribute the Ragel tool**, so the GPL does not propagate to your application or your users. This is the same principle as using `flex` or `bison` (also GPL) as a build-time lexer/parser generator.\n\n**LGPL-2.1 components (FreeType2, GLib, Cairo):** LGPL-2.1 allows commercial use and linking in closed-source products, provided end users can re-link the application with a modified version of the LGPL library. In practice, distributing the LGPL libraries as separate shared libraries (`.so`/`.dll`) satisfies this requirement. Skia/HarfBuzz typically links these statically in the pre-built SkiaSharp native binaries — if you are using the official NuGet packages, check the SkiaSharp licensing documentation or consult with a legal professional about your specific distribution model.\n\n**HarfBuzz itself** uses the Old-MIT (permissive) license — see https://github.com/harfbuzz/harfbuzz/blob/main/COPYING.\n\nFor authoritative advice on your specific commercial context, consulting a legal professional familiar with open-source licensing is recommended. We're closing this as a general Q&A question — please feel free to open a new issue if there is a specific SkiaSharp licensing document you feel is missing."
      },
      {
        "type": "close-issue",
        "description": "Close as answered question",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
