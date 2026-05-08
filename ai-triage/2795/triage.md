# Issue Triage Report — #2795

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T20:51:48Z |
| Type | type/documentation (0.82 (82%)) |
| Area | area/Docs (0.85 (85%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** SKSL custom shaders that compiled successfully on SkiaSharp 2.88.x fail with 23 errors on SkiaSharp 3.x because the upstream Skia SkSL language removed `fragmentProcessor`, `sample()`, and `discard` in favor of `shader`, `.eval()`, and early-return patterns respectively; the reporter asks for migration documentation.

**Analysis:** Upstream Skia deprecated and then removed several SkSL language features between the milestones used in SkiaSharp 2.88.x and 3.x: `fragmentProcessor` was replaced by `shader`, `sample(fp, coords)` was replaced by `fp.eval(coords)`, and `discard` is no longer allowed in runtime effect shaders. A custom `smoothstep` redefinition also conflicts with the now-built-in implementation. No SkiaSharp-level fix is possible; migration documentation is needed.

**Recommendations:** **needs-investigation** — Migration documentation is missing. The issue is valid and affects all users who wrote SkSL shaders with 2.88.x syntax. Needs a documentation PR and a comment with the migration steps.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/documentation |
| Area | area/Docs |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Take a SKSL shader that uses `in fragmentProcessor`, `sample()`, and `discard` keywords
2. Compile it with SkiaSharp 3.x via SKRuntimeEffect.CreateShader()
3. Observe 23 compilation errors

**Environment:** SkiaSharp 3.x (Alpha); SkiaSharp 2.88.6 worked. Windows 11, Visual Studio.

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1319 — Related earlier question about SkRuntimeEffect and fragmentProcessor (closed/resolved, but used the same old SkSL syntax)

**Code snippets:**

```csharp
in fragmentProcessor elev_map;
...
return sample(elev_map, fc).xyz;
...
discard;
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.6, 2.88.2, 3.0.0-preview |
| Worked in | 2.88.6 |
| Broke in | 3.0.0-preview |
| Current relevance | likely |
| Relevance reason | SkiaSharp 3.x ships a significantly newer Skia milestone whose SkSL compiler removed these deprecated language features entirely. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.90 (90%) |
| Reason | Shader compiled and ran correctly on 2.88.6; broken on 3.x preview. The SkSL language changed between Skia milestones. |
| Worked in version | 2.88.6 |
| Broke in version | 3.0.0-preview |

## Analysis

### Technical Summary

Upstream Skia deprecated and then removed several SkSL language features between the milestones used in SkiaSharp 2.88.x and 3.x: `fragmentProcessor` was replaced by `shader`, `sample(fp, coords)` was replaced by `fp.eval(coords)`, and `discard` is no longer allowed in runtime effect shaders. A custom `smoothstep` redefinition also conflicts with the now-built-in implementation. No SkiaSharp-level fix is possible; migration documentation is needed.

### Rationale

The reporter's shader used deprecated SkSL syntax (`fragmentProcessor`, `sample()`, `discard`) that was valid in Skia m87/m88 but was removed in subsequent Skia milestones bundled with SkiaSharp 3.x. There is no SkiaSharp bug to fix — the SkSL compiler correctly rejects the old syntax. However, SkiaSharp 3.x introduced a significant breaking change in the shader authoring API that requires documentation so users can migrate. Classified as `type/documentation` because the primary gap is a missing migration guide for the SkSL language changes between SkiaSharp major versions.

### Key Signals

- "in fragmentProcessor elev_map; ... error: 2: no type named 'fragmentProcessor'" — **issue body** (Upstream Skia SkSL removed the `fragmentProcessor` type; it must be replaced with `shader`.)
- "return sample(elev_map, fc).xyz; ... error: 42: unknown identifier 'sample'" — **issue body** (`sample(fp, coords)` is no longer valid; the new syntax is `elev_map.eval(coords)`.)
- "discard statement is only permitted in fragment shaders" — **issue body** (SkSL runtime effects no longer support `discard`; early return via `return half4(0,0,0,0)` or transparent pixel is the replacement.)
- "What I am requesting here is documentation on the changes made to SKGL, to guide us in porting our custom shaders to 3.0.0." — **issue body** (Reporter explicitly requests migration docs, not a code fix.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKRuntimeEffect.cs` | 22-53 | direct | SKRuntimeEffect.CreateShader / CreateColorFilter / CreateBlender are correctly exposed, delegating to the native Skia SkSL compiler. The errors originate in the SkSL parser, not the C# wrapper. The API surface is correct. |
| `tests/Tests/SkiaSharp/SKRuntimeEffectTest.cs` | 140-153 | direct | Tests confirm that 'in' variable declarations (including 'in fragmentProcessor') now generate compiler errors with messages containing "'in'". This validates that the new SkSL compiler actively rejects the old syntax. |

### Workarounds

- Replace `in fragmentProcessor myTex;` with `uniform shader myTex;`
- Replace `sample(myTex, coords)` with `myTex.eval(coords)`
- Replace `discard;` with `return half4(0.0, 0.0, 0.0, 0.0);` (transparent pixel)
- Remove custom `smoothstep` definition — it is now a built-in SkSL function and cannot be redeclared
- Replace `vec2` with `float2` (SkSL uses HLSL-style type names, not GLSL)

### Next Questions

- Which Skia milestone introduced the removal of fragmentProcessor?
- Are there other deprecated SkSL constructs that should be documented in the same guide?
- Should SkiaSharp provide a compatibility shim or pre-processor for old SkSL shaders?

### Resolution Proposals

**Hypothesis:** All 23 errors stem from three SkSL language removals introduced by upstream Skia between milestones 87/88 and the milestone used in SkiaSharp 3.x. A migration guide mapping old constructs to new ones would resolve the issue for all affected users.

1. **Write SkSL migration guide in docs** — fix, confidence 0.88 (88%), cost/s, validated=untested
   - Add a migration section to the SkiaSharp documentation covering: fragmentProcessor→shader, sample()→.eval(), discard→transparent return, smoothstep built-in conflict, and vec2→float2.
2. **Provide inline shader migration workaround** — workaround, confidence 0.92 (92%), cost/xs, validated=untested
   - Post a comment with the exact translation of the three breaking patterns, so the reporter can immediately port their shader.

```csharp
// OLD (SkiaSharp 2.88.x SkSL syntax)
// in fragmentProcessor elev_map;
// return sample(elev_map, fc).xyz;
// discard;

// NEW (SkiaSharp 3.x SkSL syntax)
uniform shader elev_map;
// ...
return elev_map.eval(fc).xyz;
// Instead of discard, return transparent:
return half4(0.0, 0.0, 0.0, 0.0);

// Remove your custom smoothstep() — it is now built-in.
// Replace vec2 with float2 if needed.
```

**Recommended proposal:** Provide inline shader migration workaround

**Why:** Actionable immediately for the reporter. A full docs PR can follow separately.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Migration documentation is missing. The issue is valid and affects all users who wrote SkSL shaders with 2.88.x syntax. Needs a documentation PR and a comment with the migration steps. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.85 (85%) | Correct label to type/documentation, add area/Docs, os/Windows-Classic, tenet/compatibility | labels=type/documentation, area/Docs, os/Windows-Classic, tenet/compatibility |
| add-comment | medium | 0.88 (88%) | Provide SkSL migration mapping for the three breaking changes | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for reporting this! The errors are caused by **breaking changes in the SkSL shading language** introduced by the newer Skia version bundled in SkiaSharp 3.x. Here is how to migrate your shader:

**1. `fragmentProcessor` → `uniform shader`**
```
// Before (2.88.x)
in fragmentProcessor elev_map;

// After (3.x)
uniform shader elev_map;
```

**2. `sample(fp, coords)` → `fp.eval(coords)`**
```
// Before
return sample(elev_map, fc).xyz;

// After
return elev_map.eval(fc).xyz;
```

**3. `discard` → return a transparent pixel**
```
// Before
discard;

// After
return half4(0.0, 0.0, 0.0, 0.0);
```

**4. Remove your custom `smoothstep()` definition** — it is now a built-in SkSL function and cannot be redeclared.

These four changes should resolve all 23 errors. We will track adding formal migration documentation as well.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2795,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T20:51:48Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SKSL custom shaders that compiled successfully on SkiaSharp 2.88.x fail with 23 errors on SkiaSharp 3.x because the upstream Skia SkSL language removed `fragmentProcessor`, `sample()`, and `discard` in favor of `shader`, `.eval()`, and early-return patterns respectively; the reporter asks for migration documentation.",
  "classification": {
    "type": {
      "value": "type/documentation",
      "confidence": 0.82
    },
    "area": {
      "value": "area/Docs",
      "confidence": 0.85
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Take a SKSL shader that uses `in fragmentProcessor`, `sample()`, and `discard` keywords",
        "Compile it with SkiaSharp 3.x via SKRuntimeEffect.CreateShader()",
        "Observe 23 compilation errors"
      ],
      "codeSnippets": [
        "in fragmentProcessor elev_map;\n...\nreturn sample(elev_map, fc).xyz;\n...\ndiscard;"
      ],
      "environmentDetails": "SkiaSharp 3.x (Alpha); SkiaSharp 2.88.6 worked. Windows 11, Visual Studio.",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1319",
          "description": "Related earlier question about SkRuntimeEffect and fragmentProcessor (closed/resolved, but used the same old SkSL syntax)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.6",
        "2.88.2",
        "3.0.0-preview"
      ],
      "workedIn": "2.88.6",
      "brokeIn": "3.0.0-preview",
      "currentRelevance": "likely",
      "relevanceReason": "SkiaSharp 3.x ships a significantly newer Skia milestone whose SkSL compiler removed these deprecated language features entirely."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.9,
      "reason": "Shader compiled and ran correctly on 2.88.6; broken on 3.x preview. The SkSL language changed between Skia milestones.",
      "workedInVersion": "2.88.6",
      "brokeInVersion": "3.0.0-preview"
    }
  },
  "analysis": {
    "summary": "Upstream Skia deprecated and then removed several SkSL language features between the milestones used in SkiaSharp 2.88.x and 3.x: `fragmentProcessor` was replaced by `shader`, `sample(fp, coords)` was replaced by `fp.eval(coords)`, and `discard` is no longer allowed in runtime effect shaders. A custom `smoothstep` redefinition also conflicts with the now-built-in implementation. No SkiaSharp-level fix is possible; migration documentation is needed.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKRuntimeEffect.cs",
        "lines": "22-53",
        "finding": "SKRuntimeEffect.CreateShader / CreateColorFilter / CreateBlender are correctly exposed, delegating to the native Skia SkSL compiler. The errors originate in the SkSL parser, not the C# wrapper. The API surface is correct.",
        "relevance": "direct"
      },
      {
        "file": "tests/Tests/SkiaSharp/SKRuntimeEffectTest.cs",
        "lines": "140-153",
        "finding": "Tests confirm that 'in' variable declarations (including 'in fragmentProcessor') now generate compiler errors with messages containing \"'in'\". This validates that the new SkSL compiler actively rejects the old syntax.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "in fragmentProcessor elev_map; ... error: 2: no type named 'fragmentProcessor'",
        "source": "issue body",
        "interpretation": "Upstream Skia SkSL removed the `fragmentProcessor` type; it must be replaced with `shader`."
      },
      {
        "text": "return sample(elev_map, fc).xyz; ... error: 42: unknown identifier 'sample'",
        "source": "issue body",
        "interpretation": "`sample(fp, coords)` is no longer valid; the new syntax is `elev_map.eval(coords)`."
      },
      {
        "text": "discard statement is only permitted in fragment shaders",
        "source": "issue body",
        "interpretation": "SkSL runtime effects no longer support `discard`; early return via `return half4(0,0,0,0)` or transparent pixel is the replacement."
      },
      {
        "text": "What I am requesting here is documentation on the changes made to SKGL, to guide us in porting our custom shaders to 3.0.0.",
        "source": "issue body",
        "interpretation": "Reporter explicitly requests migration docs, not a code fix."
      }
    ],
    "rationale": "The reporter's shader used deprecated SkSL syntax (`fragmentProcessor`, `sample()`, `discard`) that was valid in Skia m87/m88 but was removed in subsequent Skia milestones bundled with SkiaSharp 3.x. There is no SkiaSharp bug to fix — the SkSL compiler correctly rejects the old syntax. However, SkiaSharp 3.x introduced a significant breaking change in the shader authoring API that requires documentation so users can migrate. Classified as `type/documentation` because the primary gap is a missing migration guide for the SkSL language changes between SkiaSharp major versions.",
    "workarounds": [
      "Replace `in fragmentProcessor myTex;` with `uniform shader myTex;`",
      "Replace `sample(myTex, coords)` with `myTex.eval(coords)`",
      "Replace `discard;` with `return half4(0.0, 0.0, 0.0, 0.0);` (transparent pixel)",
      "Remove custom `smoothstep` definition — it is now a built-in SkSL function and cannot be redeclared",
      "Replace `vec2` with `float2` (SkSL uses HLSL-style type names, not GLSL)"
    ],
    "resolution": {
      "hypothesis": "All 23 errors stem from three SkSL language removals introduced by upstream Skia between milestones 87/88 and the milestone used in SkiaSharp 3.x. A migration guide mapping old constructs to new ones would resolve the issue for all affected users.",
      "proposals": [
        {
          "title": "Write SkSL migration guide in docs",
          "description": "Add a migration section to the SkiaSharp documentation covering: fragmentProcessor→shader, sample()→.eval(), discard→transparent return, smoothstep built-in conflict, and vec2→float2.",
          "category": "fix",
          "confidence": 0.88,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Provide inline shader migration workaround",
          "description": "Post a comment with the exact translation of the three breaking patterns, so the reporter can immediately port their shader.",
          "category": "workaround",
          "confidence": 0.92,
          "effort": "cost/xs",
          "codeSnippet": "// OLD (SkiaSharp 2.88.x SkSL syntax)\n// in fragmentProcessor elev_map;\n// return sample(elev_map, fc).xyz;\n// discard;\n\n// NEW (SkiaSharp 3.x SkSL syntax)\nuniform shader elev_map;\n// ...\nreturn elev_map.eval(fc).xyz;\n// Instead of discard, return transparent:\nreturn half4(0.0, 0.0, 0.0, 0.0);\n\n// Remove your custom smoothstep() — it is now built-in.\n// Replace vec2 with float2 if needed.",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Provide inline shader migration workaround",
      "recommendedReason": "Actionable immediately for the reporter. A full docs PR can follow separately."
    },
    "nextQuestions": [
      "Which Skia milestone introduced the removal of fragmentProcessor?",
      "Are there other deprecated SkSL constructs that should be documented in the same guide?",
      "Should SkiaSharp provide a compatibility shim or pre-processor for old SkSL shaders?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Migration documentation is missing. The issue is valid and affects all users who wrote SkSL shaders with 2.88.x syntax. Needs a documentation PR and a comment with the migration steps.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct label to type/documentation, add area/Docs, os/Windows-Classic, tenet/compatibility",
        "risk": "low",
        "confidence": 0.85,
        "labels": [
          "type/documentation",
          "area/Docs",
          "os/Windows-Classic",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Provide SkSL migration mapping for the three breaking changes",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for reporting this! The errors are caused by **breaking changes in the SkSL shading language** introduced by the newer Skia version bundled in SkiaSharp 3.x. Here is how to migrate your shader:\n\n**1. `fragmentProcessor` → `uniform shader`**\n```\n// Before (2.88.x)\nin fragmentProcessor elev_map;\n\n// After (3.x)\nuniform shader elev_map;\n```\n\n**2. `sample(fp, coords)` → `fp.eval(coords)`**\n```\n// Before\nreturn sample(elev_map, fc).xyz;\n\n// After\nreturn elev_map.eval(fc).xyz;\n```\n\n**3. `discard` → return a transparent pixel**\n```\n// Before\ndiscard;\n\n// After\nreturn half4(0.0, 0.0, 0.0, 0.0);\n```\n\n**4. Remove your custom `smoothstep()` definition** — it is now a built-in SkSL function and cannot be redeclared.\n\nThese four changes should resolve all 23 errors. We will track adding formal migration documentation as well."
      }
    ]
  }
}
```

</details>
