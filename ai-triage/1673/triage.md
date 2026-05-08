# Issue Triage Report — #1673

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-06T20:01:32Z |
| Type | type/question (0.88 (88%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-not-a-bug (0.88 (88%)) |

**Issue Summary:** Reporter believes SKMatrix.CreateRotationDegrees gives wrong results when a path was already translated, but the behavior is actually correct matrix-order math: rotation around origin applies to the already-translated coordinates.

**Analysis:** The reported behavior is the mathematically correct outcome of matrix transformation order. SKMatrix.CreateRotationDegrees creates a rotation around the coordinate origin (0,0). When a path is first translated away from origin and then a plain rotation matrix is applied, the rotation occurs around origin, not around the path's own center — exactly what the screenshots show. The reporter's working workaround (rotate first, then translate) confirms this: rotating around origin before translating keeps the shape in place. No bug exists in CreateRotationDegrees or CreateTranslation.

**Recommendations:** **close-as-not-a-bug** — CreateRotationDegrees works correctly. The observed behavior is standard affine transform math. The reporter's own workaround confirms it. A pivot overload already exists for rotating around a point other than origin.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | os/macOS |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Create an SKPath around origin (0,0) using MakePath, which also calls path.Transform(SKMatrix.CreateTranslation(offset.X, offset.Y)) before returning
2. After receiving the path, call path.Transform(SKMatrix.CreateRotationDegrees(45.0F))
3. Observe the path is rotated around origin (0,0), causing the translated path to arc away from expected position

**Environment:** SkiaSharp 2.80.2, Visual Studio for Mac, macOS, command-line PNG generation

**Screenshots:**
- https://user-images.githubusercontent.com/17300872/112697779-4c197300-8e56-11eb-8202-8e7541425c9a.png — Custom rotation code — correct result
- https://user-images.githubusercontent.com/17300872/112697845-723f1300-8e56-11eb-89ad-6a1aed89802e.png — CreateRotationDegrees result — path rotated around origin showing unintended translation

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.2 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | SKMatrix rotation logic has not changed; the behavior is by-design. |

## Analysis

### Technical Summary

The reported behavior is the mathematically correct outcome of matrix transformation order. SKMatrix.CreateRotationDegrees creates a rotation around the coordinate origin (0,0). When a path is first translated away from origin and then a plain rotation matrix is applied, the rotation occurs around origin, not around the path's own center — exactly what the screenshots show. The reporter's working workaround (rotate first, then translate) confirms this: rotating around origin before translating keeps the shape in place. No bug exists in CreateRotationDegrees or CreateTranslation.

### Rationale

Issue title says '[BUG]' but the described behavior is the standard, expected result of affine transform ordering. Skia rotation matrices always rotate around origin unless a pivot is supplied. The reporter's own workaround (flip the order) proves the API works correctly. This is a usage/conceptual question, not a defect.

### Key Signals

- "this works: path.Transform(SKMatrix.CreateRotationDegrees(180.0F)); path.Transform(SKMatrix.CreateTranslation(...))" — **issue body** (Reporter confirms order-correct sequence works — demonstrates the API itself is not broken.)
- "Appears to be translating as well back to an X origin of 0" — **issue body** (Classic symptom of rotating an already-translated path around origin (0,0).)
- "Maybe I'm missing some method to clear the matrix first?" — **issue body** (Reporter suspects a usage gap, not a code defect — confirms this is a question.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKMatrix.cs` | 165-196 | direct | CreateRotation(radians) and CreateRotationDegrees(degrees) build a rotation matrix with no translation component — they always rotate around (0,0). A separate pivot-accepting overload CreateRotationDegrees(degrees, pivotX, pivotY) exists for rotation around an arbitrary point. |
| `binding/SkiaSharp/SKPath.cs` | 251-268 | direct | SKPath.Transform(in SKMatrix matrix) applies the given matrix to all path points in place. Successive calls multiply the effects: Transform(T) then Transform(R) is equivalent to applying T first, then R — so points at offset (tx,ty) are rotated around origin. |

### Workarounds

- Apply rotation before translation: path.Transform(SKMatrix.CreateRotationDegrees(angle)); path.Transform(SKMatrix.CreateTranslation(dx, dy));
- Use the pivot overload to rotate around the path center: path.Transform(SKMatrix.CreateRotationDegrees(angle, centerX, centerY));
- Compose both transforms into one matrix: var m = SKMatrix.CreateRotationDegrees(angle).PostConcat(SKMatrix.CreateTranslation(dx, dy)); path.Transform(m);

### Resolution Proposals

**Hypothesis:** Reporter misunderstands affine transform order — rotation is always around origin unless a pivot is specified.

1. **Use the pivot overload** — workaround, confidence 0.92 (92%), cost/xs, validated=yes
   - Call SKMatrix.CreateRotationDegrees(degrees, pivotX, pivotY) with the path's centroid as the pivot to rotate in place.
2. **Compose matrices before applying** — alternative, confidence 0.90 (90%), cost/xs, validated=yes
   - Build the combined matrix via PostConcat/PreConcat and apply once, giving explicit control over transform order.

**Recommended proposal:** Use the pivot overload

**Why:** Single-call, intention-revealing, no order ambiguity.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.88 (88%) |
| Reason | CreateRotationDegrees works correctly. The observed behavior is standard affine transform math. The reporter's own workaround confirms it. A pivot overload already exists for rotating around a point other than origin. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Label as question targeting SkiaSharp core on macOS | labels=type/question, area/SkiaSharp, os/macOS |
| add-comment | high | 0.88 (88%) | Explain transform order and suggest pivot overload / PostConcat pattern | — |
| close-issue | medium | 0.85 (85%) | Close as not a bug — by-design matrix ordering | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for filing this with screenshots — they make the situation clear!

The behavior you're seeing is actually correct affine transform math, not a bug in `CreateRotationDegrees`. When you call `path.Transform(SKMatrix.CreateRotationDegrees(45))`, the rotation is **always around the coordinate origin (0, 0)**. If the path has already been translated away from origin (as your `MakePath` helper does), rotating it spins it around origin — which shifts the position, exactly as your screenshots show.

Your own working snippet confirms this: rotating *before* translating keeps the path centred on origin during the rotation, then moves it to the final position.

**Two clean solutions:**

1. **Use the pivot overload** — rotate around the path's own centre:
```csharp
// compute the centre of the path bounds
var bounds = path.Bounds;
var cx = bounds.MidX;
var cy = bounds.MidY;
path.Transform(SKMatrix.CreateRotationDegrees(45f, cx, cy));
```

2. **Compose the full transform in one step** with `PostConcat`:
```csharp
// rotate around origin, then translate
var m = SKMatrix.CreateRotationDegrees(45f)
               .PostConcat(SKMatrix.CreateTranslation(offset.X, offset.Y));
path.Transform(m);
```

Closing as by-design. Feel free to reopen if you think there's something more going on!
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1673,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-06T20:01:32Z"
  },
  "summary": "Reporter believes SKMatrix.CreateRotationDegrees gives wrong results when a path was already translated, but the behavior is actually correct matrix-order math: rotation around origin applies to the already-translated coordinates.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "platforms": [
      "os/macOS"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an SKPath around origin (0,0) using MakePath, which also calls path.Transform(SKMatrix.CreateTranslation(offset.X, offset.Y)) before returning",
        "After receiving the path, call path.Transform(SKMatrix.CreateRotationDegrees(45.0F))",
        "Observe the path is rotated around origin (0,0), causing the translated path to arc away from expected position"
      ],
      "environmentDetails": "SkiaSharp 2.80.2, Visual Studio for Mac, macOS, command-line PNG generation",
      "screenshots": [
        {
          "url": "https://user-images.githubusercontent.com/17300872/112697779-4c197300-8e56-11eb-8202-8e7541425c9a.png",
          "description": "Custom rotation code — correct result"
        },
        {
          "url": "https://user-images.githubusercontent.com/17300872/112697845-723f1300-8e56-11eb-89ad-6a1aed89802e.png",
          "description": "CreateRotationDegrees result — path rotated around origin showing unintended translation"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.2"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "SKMatrix rotation logic has not changed; the behavior is by-design."
    }
  },
  "analysis": {
    "summary": "The reported behavior is the mathematically correct outcome of matrix transformation order. SKMatrix.CreateRotationDegrees creates a rotation around the coordinate origin (0,0). When a path is first translated away from origin and then a plain rotation matrix is applied, the rotation occurs around origin, not around the path's own center — exactly what the screenshots show. The reporter's working workaround (rotate first, then translate) confirms this: rotating around origin before translating keeps the shape in place. No bug exists in CreateRotationDegrees or CreateTranslation.",
    "rationale": "Issue title says '[BUG]' but the described behavior is the standard, expected result of affine transform ordering. Skia rotation matrices always rotate around origin unless a pivot is supplied. The reporter's own workaround (flip the order) proves the API works correctly. This is a usage/conceptual question, not a defect.",
    "keySignals": [
      {
        "text": "this works: path.Transform(SKMatrix.CreateRotationDegrees(180.0F)); path.Transform(SKMatrix.CreateTranslation(...))",
        "source": "issue body",
        "interpretation": "Reporter confirms order-correct sequence works — demonstrates the API itself is not broken."
      },
      {
        "text": "Appears to be translating as well back to an X origin of 0",
        "source": "issue body",
        "interpretation": "Classic symptom of rotating an already-translated path around origin (0,0)."
      },
      {
        "text": "Maybe I'm missing some method to clear the matrix first?",
        "source": "issue body",
        "interpretation": "Reporter suspects a usage gap, not a code defect — confirms this is a question."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKMatrix.cs",
        "lines": "165-196",
        "finding": "CreateRotation(radians) and CreateRotationDegrees(degrees) build a rotation matrix with no translation component — they always rotate around (0,0). A separate pivot-accepting overload CreateRotationDegrees(degrees, pivotX, pivotY) exists for rotation around an arbitrary point.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPath.cs",
        "lines": "251-268",
        "finding": "SKPath.Transform(in SKMatrix matrix) applies the given matrix to all path points in place. Successive calls multiply the effects: Transform(T) then Transform(R) is equivalent to applying T first, then R — so points at offset (tx,ty) are rotated around origin.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Apply rotation before translation: path.Transform(SKMatrix.CreateRotationDegrees(angle)); path.Transform(SKMatrix.CreateTranslation(dx, dy));",
      "Use the pivot overload to rotate around the path center: path.Transform(SKMatrix.CreateRotationDegrees(angle, centerX, centerY));",
      "Compose both transforms into one matrix: var m = SKMatrix.CreateRotationDegrees(angle).PostConcat(SKMatrix.CreateTranslation(dx, dy)); path.Transform(m);"
    ],
    "resolution": {
      "hypothesis": "Reporter misunderstands affine transform order — rotation is always around origin unless a pivot is specified.",
      "proposals": [
        {
          "title": "Use the pivot overload",
          "description": "Call SKMatrix.CreateRotationDegrees(degrees, pivotX, pivotY) with the path's centroid as the pivot to rotate in place.",
          "category": "workaround",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Compose matrices before applying",
          "description": "Build the combined matrix via PostConcat/PreConcat and apply once, giving explicit control over transform order.",
          "category": "alternative",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Use the pivot overload",
      "recommendedReason": "Single-call, intention-revealing, no order ambiguity."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.88,
      "reason": "CreateRotationDegrees works correctly. The observed behavior is standard affine transform math. The reporter's own workaround confirms it. A pivot overload already exists for rotating around a point other than origin.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Label as question targeting SkiaSharp core on macOS",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/SkiaSharp",
          "os/macOS"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain transform order and suggest pivot overload / PostConcat pattern",
        "risk": "high",
        "confidence": 0.88,
        "comment": "Thanks for filing this with screenshots — they make the situation clear!\n\nThe behavior you're seeing is actually correct affine transform math, not a bug in `CreateRotationDegrees`. When you call `path.Transform(SKMatrix.CreateRotationDegrees(45))`, the rotation is **always around the coordinate origin (0, 0)**. If the path has already been translated away from origin (as your `MakePath` helper does), rotating it spins it around origin — which shifts the position, exactly as your screenshots show.\n\nYour own working snippet confirms this: rotating *before* translating keeps the path centred on origin during the rotation, then moves it to the final position.\n\n**Two clean solutions:**\n\n1. **Use the pivot overload** — rotate around the path's own centre:\n```csharp\n// compute the centre of the path bounds\nvar bounds = path.Bounds;\nvar cx = bounds.MidX;\nvar cy = bounds.MidY;\npath.Transform(SKMatrix.CreateRotationDegrees(45f, cx, cy));\n```\n\n2. **Compose the full transform in one step** with `PostConcat`:\n```csharp\n// rotate around origin, then translate\nvar m = SKMatrix.CreateRotationDegrees(45f)\n               .PostConcat(SKMatrix.CreateTranslation(offset.X, offset.Y));\npath.Transform(m);\n```\n\nClosing as by-design. Feel free to reopen if you think there's something more going on!"
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — by-design matrix ordering",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
