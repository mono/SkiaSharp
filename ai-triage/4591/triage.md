# Issue Triage Report — #4591

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-08-02T05:14:47Z |
| Type | type/enhancement (0.92 (92%)) |
| Area | area/Build (0.88 (88%)) |
| Suggested action | keep-open (0.95 (95%)) |

**Issue Summary:** Umbrella tracking issue to re-enable GPU backend CI coverage (OpenGL, Metal, Dawn) that was explicitly opted out after #4575 made GPU backends required rather than silently skipped — currently there is no OpenGL or Metal coverage anywhere in CI.

**Analysis:** 11 CI legs have GPU backends opted out via SKIASHARP_TEST_SKIP_GPU, leaving the project with zero OpenGL coverage and zero Metal coverage. The opt-outs were made explicit by #4575 (which was the correct move); this issue tracks closing them. Five sub-issues cover the distinct root causes: Linux GLX concurrency (#4590), macOS Metal hang (#4598), Mac Catalyst GPU family (#4599), iOS gradient compile failure (#4555), and Windows/container OpenGL (no ICD — Mesa mirroring needed). WASM Dawn requires a browser-flag passthrough in DeviceRunners upstream.

**Recommendations:** **keep-open** — Active umbrella tracking issue with well-scoped sub-items and ongoing incremental progress. Each GPU coverage gap has a dedicated sub-issue and a clear remediation path.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/Build |
| Platforms | os/Windows-Classic, os/macOS, os/Linux, os/iOS, os/WASM |
| Backends | backend/OpenGL, backend/Metal |
| Tenets | — |
| Perf | — |
| Partner | — |
| Current labels | type/enhancement, area/Tests |

## Evidence

### Reproduction

1. Run CI on any of the 11 affected legs listed in the tracking table
2. Observe SKIASHARP_TEST_SKIP_GPU suppressing ganesh-gl, ganesh-metal, graphite-metal, or graphite-dawn

**Environment:** Azure DevOps dnceng-public, hosted agents and container images (Linux Azure Linux, Alpine, Windows, macOS x64)

**Related issues:** #4575, #4590, #4598, #4599, #4555

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/4575 — Made GPU backends required — introduced the opt-out declarations
- https://github.com/mono/SkiaSharp/issues/4590 — Linux GLX concurrency crash blocking ganesh-gl on Linux
- https://github.com/mono/SkiaSharp/issues/4598 — macOS: virtualized Metal hangs test host on shutdown
- https://github.com/mono/SkiaSharp/issues/4599 — Mac Catalyst: MTLDevice reports no usable MTLGPUFamily
- https://github.com/mono/SkiaSharp/issues/4555 — iOS simulator: Graphite gradient pipeline fails to compile

## Analysis

### Technical Summary

11 CI legs have GPU backends opted out via SKIASHARP_TEST_SKIP_GPU, leaving the project with zero OpenGL coverage and zero Metal coverage. The opt-outs were made explicit by #4575 (which was the correct move); this issue tracks closing them. Five sub-issues cover the distinct root causes: Linux GLX concurrency (#4590), macOS Metal hang (#4598), Mac Catalyst GPU family (#4599), iOS gradient compile failure (#4555), and Windows/container OpenGL (no ICD — Mesa mirroring needed). WASM Dawn requires a browser-flag passthrough in DeviceRunners upstream.

### Rationale

Classified as type/enhancement because CI coverage gaps are improvements to the build/test infrastructure, not bugs in SkiaSharp's public API. The area is area/Build as the work is entirely in CI scripts and test plumbing. Backends backend/OpenGL and backend/Metal are labeled because those are the specific uncovered backends. suggestedAction is keep-open because this is an active umbrella tracking issue with well-defined sub-items and ongoing progress.

### Key Signals

- "Today CI has no OpenGL coverage anywhere, and no Metal coverage anywhere." — **issue body** (Two major rendering backends have zero test signal — regressions in ganesh-gl and Metal backends would ship silently.)
- "Mesa (mesa-dist-win) works — verified correct per-arch PE headers and the required WGL extensions present." — **issue body** (Windows OpenGL is solvable without test-code changes; the blocker is a trustworthy package mirror for Mesa.)
- "Each row removed is a real gain. The opt-out mechanism is deliberately per-leg so items can be closed one at a time." — **issue body** (Incremental progress is acceptable — the issue is a coordination tracker, not a blocking gate.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `scripts/azure-templates-stages-test.yml` | 475,507,539,571,603 | direct | Five Linux/Windows/NanoServer legs set SKIASHARP_TEST_SKIP_GPU: ganesh-gl, referencing this issue as the tracking issue. Three Apple legs skip ganesh-metal and graphite-metal. Two WASM legs skip graphite-dawn. |
| `tests/Tests/SkiaSharp/Gpu/GpuPolicy.cs` | 46-58 | direct | GpuPolicy.RequiredOn declares ganesh-gl as required on Desktop|NanoServer, ganesh-metal/graphite-metal as required on Apple, and graphite-dawn as required on Browser — confirming that all opted-out backends are within their declared required platforms and the opt-outs are temporary coverage gaps. |
| `tests/Tests/SkiaSharp/Gpu/GpuPolicy.cs` | 64-75 | related | RequireOrSkip() respects SKIASHARP_TEST_SKIP_GPU env var and skips when backend is in the opt-out list, confirming the mechanism works correctly; removing the env var entries is the path to re-enabling coverage. |

### Next Questions

- Is there a trustworthy package feed for mesa-dist-win that would allow mirroring for Windows OpenGL on CI?
- Is an Apple Silicon pool (or real device) on the roadmap for resolving all three Apple Metal gaps at once?
- Is adding Mesa to Linux/Alpine container images considered out-of-scope by design?
- What is the upstream DeviceRunners timeline for adding browser-argument passthrough for WebGPU/Dawn?

### Resolution Proposals

**Hypothesis:** Each of the 11 opt-outs has a distinct root cause; each can be closed independently once its blocking sub-issue is resolved (GLX concurrency fix, Mesa mirror, Apple Silicon pool, DeviceRunners upstream change).

1. **Fix Linux GLX concurrency (#4590)** — fix, confidence 0.88 (88%), cost/s, validated=untested
   - Move xUnit test classes that call SKTest.CreateGlContext() into a single collection to prevent concurrent GLX context creation. This is the most tractable item and most likely to restore real GL coverage.
2. **Mirror Mesa for Windows OpenGL** — fix, confidence 0.75 (75%), cost/m, validated=untested
   - Set up a trustworthy mirror of mesa-dist-win and install opengl32.dll + libgallium_wgl.dll into System32/SysWOW64 following the same pattern as the existing Vulkan ICD installer (scripts/infra/native/windows/install-vulkan-icd.ps1).
3. **Apple Silicon CI pool for Metal** — fix, confidence 0.70 (70%), cost/l, validated=untested
   - Switch macOS/iOS/Catalyst CI to an Apple Silicon pool (or real device) to get non-virtualized Metal — would resolve #4598, #4599, and #4555 in one change.
4. **Upstream DeviceRunners browser-flag passthrough for WASM/Dawn** — fix, confidence 0.65 (65%), cost/l, validated=untested
   - Contribute --enable-unsafe-webgpu --use-webgpu-adapter=swiftshader flag passthrough to the DeviceRunners CLI so WASM tests can use SwiftShader WebGPU on headless CI.

**Recommended proposal:** Fix Linux GLX concurrency (#4590)

**Why:** Most tractable, no external dependencies, restores ganesh-gl coverage on the largest CI matrix (Linux legs).

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.95 (95%) |
| Reason | Active umbrella tracking issue with well-scoped sub-items and ongoing incremental progress. Each GPU coverage gap has a dedicated sub-issue and a clear remediation path. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply correct labels from taxonomy (area/Build replaces non-taxonomy area/Tests; add platform and backend labels) | labels=type/enhancement, area/Build, os/Windows-Classic, os/macOS, os/Linux, os/iOS, os/WASM, backend/OpenGL, backend/Metal |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4591,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-08-02T05:14:47Z",
    "currentLabels": [
      "type/enhancement",
      "area/Tests"
    ]
  },
  "summary": "Umbrella tracking issue to re-enable GPU backend CI coverage (OpenGL, Metal, Dawn) that was explicitly opted out after #4575 made GPU backends required rather than silently skipped — currently there is no OpenGL or Metal coverage anywhere in CI.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.92
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.88
    },
    "platforms": [
      "os/Windows-Classic",
      "os/macOS",
      "os/Linux",
      "os/iOS",
      "os/WASM"
    ],
    "backends": [
      "backend/OpenGL",
      "backend/Metal"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Run CI on any of the 11 affected legs listed in the tracking table",
        "Observe SKIASHARP_TEST_SKIP_GPU suppressing ganesh-gl, ganesh-metal, graphite-metal, or graphite-dawn"
      ],
      "environmentDetails": "Azure DevOps dnceng-public, hosted agents and container images (Linux Azure Linux, Alpine, Windows, macOS x64)",
      "relatedIssues": [
        4575,
        4590,
        4598,
        4599,
        4555
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/4575",
          "description": "Made GPU backends required — introduced the opt-out declarations"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/4590",
          "description": "Linux GLX concurrency crash blocking ganesh-gl on Linux"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/4598",
          "description": "macOS: virtualized Metal hangs test host on shutdown"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/4599",
          "description": "Mac Catalyst: MTLDevice reports no usable MTLGPUFamily"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/4555",
          "description": "iOS simulator: Graphite gradient pipeline fails to compile"
        }
      ]
    }
  },
  "analysis": {
    "summary": "11 CI legs have GPU backends opted out via SKIASHARP_TEST_SKIP_GPU, leaving the project with zero OpenGL coverage and zero Metal coverage. The opt-outs were made explicit by #4575 (which was the correct move); this issue tracks closing them. Five sub-issues cover the distinct root causes: Linux GLX concurrency (#4590), macOS Metal hang (#4598), Mac Catalyst GPU family (#4599), iOS gradient compile failure (#4555), and Windows/container OpenGL (no ICD — Mesa mirroring needed). WASM Dawn requires a browser-flag passthrough in DeviceRunners upstream.",
    "codeInvestigation": [
      {
        "file": "scripts/azure-templates-stages-test.yml",
        "lines": "475,507,539,571,603",
        "finding": "Five Linux/Windows/NanoServer legs set SKIASHARP_TEST_SKIP_GPU: ganesh-gl, referencing this issue as the tracking issue. Three Apple legs skip ganesh-metal and graphite-metal. Two WASM legs skip graphite-dawn.",
        "relevance": "direct"
      },
      {
        "file": "tests/Tests/SkiaSharp/Gpu/GpuPolicy.cs",
        "lines": "46-58",
        "finding": "GpuPolicy.RequiredOn declares ganesh-gl as required on Desktop|NanoServer, ganesh-metal/graphite-metal as required on Apple, and graphite-dawn as required on Browser — confirming that all opted-out backends are within their declared required platforms and the opt-outs are temporary coverage gaps.",
        "relevance": "direct"
      },
      {
        "file": "tests/Tests/SkiaSharp/Gpu/GpuPolicy.cs",
        "lines": "64-75",
        "finding": "RequireOrSkip() respects SKIASHARP_TEST_SKIP_GPU env var and skips when backend is in the opt-out list, confirming the mechanism works correctly; removing the env var entries is the path to re-enabling coverage.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Today CI has no OpenGL coverage anywhere, and no Metal coverage anywhere.",
        "source": "issue body",
        "interpretation": "Two major rendering backends have zero test signal — regressions in ganesh-gl and Metal backends would ship silently."
      },
      {
        "text": "Mesa (mesa-dist-win) works — verified correct per-arch PE headers and the required WGL extensions present.",
        "source": "issue body",
        "interpretation": "Windows OpenGL is solvable without test-code changes; the blocker is a trustworthy package mirror for Mesa."
      },
      {
        "text": "Each row removed is a real gain. The opt-out mechanism is deliberately per-leg so items can be closed one at a time.",
        "source": "issue body",
        "interpretation": "Incremental progress is acceptable — the issue is a coordination tracker, not a blocking gate."
      }
    ],
    "rationale": "Classified as type/enhancement because CI coverage gaps are improvements to the build/test infrastructure, not bugs in SkiaSharp's public API. The area is area/Build as the work is entirely in CI scripts and test plumbing. Backends backend/OpenGL and backend/Metal are labeled because those are the specific uncovered backends. suggestedAction is keep-open because this is an active umbrella tracking issue with well-defined sub-items and ongoing progress.",
    "nextQuestions": [
      "Is there a trustworthy package feed for mesa-dist-win that would allow mirroring for Windows OpenGL on CI?",
      "Is an Apple Silicon pool (or real device) on the roadmap for resolving all three Apple Metal gaps at once?",
      "Is adding Mesa to Linux/Alpine container images considered out-of-scope by design?",
      "What is the upstream DeviceRunners timeline for adding browser-argument passthrough for WebGPU/Dawn?"
    ],
    "resolution": {
      "hypothesis": "Each of the 11 opt-outs has a distinct root cause; each can be closed independently once its blocking sub-issue is resolved (GLX concurrency fix, Mesa mirror, Apple Silicon pool, DeviceRunners upstream change).",
      "proposals": [
        {
          "title": "Fix Linux GLX concurrency (#4590)",
          "description": "Move xUnit test classes that call SKTest.CreateGlContext() into a single collection to prevent concurrent GLX context creation. This is the most tractable item and most likely to restore real GL coverage.",
          "category": "fix",
          "confidence": 0.88,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Mirror Mesa for Windows OpenGL",
          "description": "Set up a trustworthy mirror of mesa-dist-win and install opengl32.dll + libgallium_wgl.dll into System32/SysWOW64 following the same pattern as the existing Vulkan ICD installer (scripts/infra/native/windows/install-vulkan-icd.ps1).",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Apple Silicon CI pool for Metal",
          "description": "Switch macOS/iOS/Catalyst CI to an Apple Silicon pool (or real device) to get non-virtualized Metal — would resolve #4598, #4599, and #4555 in one change.",
          "category": "fix",
          "confidence": 0.7,
          "effort": "cost/l",
          "validated": "untested"
        },
        {
          "title": "Upstream DeviceRunners browser-flag passthrough for WASM/Dawn",
          "description": "Contribute --enable-unsafe-webgpu --use-webgpu-adapter=swiftshader flag passthrough to the DeviceRunners CLI so WASM tests can use SwiftShader WebGPU on headless CI.",
          "category": "fix",
          "confidence": 0.65,
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Fix Linux GLX concurrency (#4590)",
      "recommendedReason": "Most tractable, no external dependencies, restores ganesh-gl coverage on the largest CI matrix (Linux legs)."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.95,
      "reason": "Active umbrella tracking issue with well-scoped sub-items and ongoing incremental progress. Each GPU coverage gap has a dedicated sub-issue and a clear remediation path.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply correct labels from taxonomy (area/Build replaces non-taxonomy area/Tests; add platform and backend labels)",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/enhancement",
          "area/Build",
          "os/Windows-Classic",
          "os/macOS",
          "os/Linux",
          "os/iOS",
          "os/WASM",
          "backend/OpenGL",
          "backend/Metal"
        ]
      }
    ]
  }
}
```

</details>
