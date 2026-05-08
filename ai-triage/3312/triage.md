# Issue Triage Report — #3312

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T17:05:00Z |
| Type | type/question (0.82 (82%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.78 (78%)) |

**Issue Summary:** Reporter is trying to share GPU textures between SkiaSharp and Godot via Vulkan, but GRContext.CreateVulkan keeps returning null; the likely cause is a missing GetProcedureAddress delegate in GRVkBackendContext.

**Analysis:** GRContext.CreateVulkan returns null because the reporter's GRVkBackendContext does not set the required GetProcedureAddress delegate. Without this delegate, Skia cannot load Vulkan function pointers (fGetProc is null in the native struct), so gr_direct_context_make_vulkan fails and returns null. This is a usage issue, not a SkiaSharp bug.

**Recommendations:** **close-as-not-a-bug** — The null return from CreateVulkan is expected behavior when GetProcedureAddress is not set. This is a usage question about how to properly initialize GRVkBackendContext for Vulkan.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | backend/Vulkan |
| Tenets | — |
| Partner | — |
| Current labels | type/bug, backend/Vulkan |

## Evidence

### Reproduction

1. Create a GRVkBackendContext with VkInstance, VkPhysicalDevice, VkDevice, VkQueue set
2. Do NOT set GetProcedureAddress on the context
3. Call GRContext.CreateVulkan(vkBackendContext)
4. Observe null returned

**Environment:** SkiaSharp 3.116.0, Windows, Godot integration via shared Vulkan handles

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The GRVkBackendContext.ToNative() code has not changed in a way that would affect this behavior. |

## Analysis

### Technical Summary

GRContext.CreateVulkan returns null because the reporter's GRVkBackendContext does not set the required GetProcedureAddress delegate. Without this delegate, Skia cannot load Vulkan function pointers (fGetProc is null in the native struct), so gr_direct_context_make_vulkan fails and returns null. This is a usage issue, not a SkiaSharp bug.

### Rationale

Code investigation confirms that GRVkBackendContext.ToNative() sets fGetProc only when getProcContext is not null (i.e., when GetProcedureAddress is set). The reporter's code omits this critical field, causing Skia to silently fail during Vulkan context initialization. The behavior is correct but the requirement is not obviously documented.

### Key Signals

- "When I use GRContext.CreateVulkan it keeps returning null" — **issue body** (The native Skia gr_direct_context_make_vulkan returns null when fGetProc is missing — exactly the behavior seen when GetProcedureAddress is not set)
- "GRVkBackendContext { VkDevice, VkPhysicalDevice, VkInstance, VkQueue, GraphicsQueueIndex }" — **issue body** (The reporter's initialization omits GetProcedureAddress entirely — this is the required Vulkan function pointer loader)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/GRVkBackendContext.cs` | 73-87 | direct | ToNative() sets fGetProc = getProcContext is not null ? DelegateProxies.GRVkGetProcProxy : null — if GetProcedureAddress is not assigned, fGetProc is null and Skia has no way to load Vulkan function pointers, causing context creation to fail |
| `binding/SkiaSharp/GRContext.cs` | 50-63 | direct | CreateVulkan passes backendContext.ToNative() directly to SkiaApi.gr_direct_context_make_vulkan; there is no validation that GetProcedureAddress is set before calling into native code |

### Workarounds

- Set GetProcedureAddress on GRVkBackendContext to a delegate that calls vkGetInstanceProcAddr / vkGetDeviceProcAddr to load Vulkan function pointers by name

### Next Questions

- Can the reporter share how they retrieve VkInstance/VkDevice/VkPhysicalDevice/VkQueue handles from Godot?
- Does Godot's GDNative/GDExtension API expose a way to retrieve the Vulkan function loader?

### Resolution Proposals

**Hypothesis:** The root cause is the missing GetProcedureAddress delegate in GRVkBackendContext. Skia needs this callback to enumerate Vulkan extension functions before it can initialize the GPU context.

1. **Set GetProcedureAddress when building GRVkBackendContext** — workaround, confidence 0.82 (82%), cost/s, validated=untested
   - Assign a delegate to GRVkBackendContext.GetProcedureAddress that calls vkGetInstanceProcAddr (when device is IntPtr.Zero) or vkGetDeviceProcAddr (when device is non-zero) to load each Vulkan function by name. This is required for Skia to initialize a Vulkan GPU context.

**Recommended proposal:** Set GetProcedureAddress when building GRVkBackendContext

**Why:** The missing delegate is the only structural difference between the reporter's code and a working Vulkan context setup. Setting it is the standard requirement per Skia's Vulkan backend design.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.78 (78%) |
| Reason | The null return from CreateVulkan is expected behavior when GetProcedureAddress is not set. This is a usage question about how to properly initialize GRVkBackendContext for Vulkan. |
| Suggested repro platform | windows |

### Missing Info

- How Godot exposes its Vulkan handles (vkInstance, vkDevice, etc.) to .NET interop
- Whether Godot provides a function pointer loader accessible from .NET

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.82 (82%) | Change type/bug to type/question and ensure backend/Vulkan and os/Windows are applied | labels=type/question, area/SkiaSharp, os/Windows-Classic, backend/Vulkan |
| add-comment | high | 0.78 (78%) | Post explanation of missing GetProcedureAddress delegate | — |
| close-issue | medium | 0.72 (72%) | Close as not a bug — the null return is expected when GetProcedureAddress is missing | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report!

The most likely cause of `GRContext.CreateVulkan` returning `null` is that `GRVkBackendContext.GetProcedureAddress` is not set. Skia requires a function pointer loader to enumerate Vulkan extension functions before it can initialize the GPU context — without it, `gr_direct_context_make_vulkan` will always fail silently and return `null`.

You need to assign a delegate to `GetProcedureAddress` that calls the Vulkan `vkGetInstanceProcAddr` / `vkGetDeviceProcAddr` functions to load each Vulkan symbol by name:

```csharp
vkBackendContext.GetProcedureAddress = (name, instance, device) =>
{
    if (device != IntPtr.Zero)
        return VkGetDeviceProcAddr(device, name);
    return VkGetInstanceProcAddr(instance, name);
};
```

You will also want to set `GRVkExtensions` (populated via `GRVkExtensions.Create(...)`) and `MaxAPIVersion` (the Vulkan API version your instance was created with, e.g. `VkMakeVersion(1, 1, 0)`) for best compatibility.

If Godot exposes its Vulkan loader through GDExtension or GDNative, that would be the right source for `vkGetInstanceProcAddr`.

If setting `GetProcedureAddress` doesn't resolve the issue, please share any Vulkan validation layer output and the Godot API you use to retrieve the driver resource handles — that will help narrow things down further.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3312,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T17:05:00Z",
    "currentLabels": [
      "type/bug",
      "backend/Vulkan"
    ]
  },
  "summary": "Reporter is trying to share GPU textures between SkiaSharp and Godot via Vulkan, but GRContext.CreateVulkan keeps returning null; the likely cause is a missing GetProcedureAddress delegate in GRVkBackendContext.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.82
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/Vulkan"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a GRVkBackendContext with VkInstance, VkPhysicalDevice, VkDevice, VkQueue set",
        "Do NOT set GetProcedureAddress on the context",
        "Call GRContext.CreateVulkan(vkBackendContext)",
        "Observe null returned"
      ],
      "environmentDetails": "SkiaSharp 3.116.0, Windows, Godot integration via shared Vulkan handles",
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The GRVkBackendContext.ToNative() code has not changed in a way that would affect this behavior."
    }
  },
  "analysis": {
    "summary": "GRContext.CreateVulkan returns null because the reporter's GRVkBackendContext does not set the required GetProcedureAddress delegate. Without this delegate, Skia cannot load Vulkan function pointers (fGetProc is null in the native struct), so gr_direct_context_make_vulkan fails and returns null. This is a usage issue, not a SkiaSharp bug.",
    "rationale": "Code investigation confirms that GRVkBackendContext.ToNative() sets fGetProc only when getProcContext is not null (i.e., when GetProcedureAddress is set). The reporter's code omits this critical field, causing Skia to silently fail during Vulkan context initialization. The behavior is correct but the requirement is not obviously documented.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/GRVkBackendContext.cs",
        "lines": "73-87",
        "finding": "ToNative() sets fGetProc = getProcContext is not null ? DelegateProxies.GRVkGetProcProxy : null — if GetProcedureAddress is not assigned, fGetProc is null and Skia has no way to load Vulkan function pointers, causing context creation to fail",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/GRContext.cs",
        "lines": "50-63",
        "finding": "CreateVulkan passes backendContext.ToNative() directly to SkiaApi.gr_direct_context_make_vulkan; there is no validation that GetProcedureAddress is set before calling into native code",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "When I use GRContext.CreateVulkan it keeps returning null",
        "source": "issue body",
        "interpretation": "The native Skia gr_direct_context_make_vulkan returns null when fGetProc is missing — exactly the behavior seen when GetProcedureAddress is not set"
      },
      {
        "text": "GRVkBackendContext { VkDevice, VkPhysicalDevice, VkInstance, VkQueue, GraphicsQueueIndex }",
        "source": "issue body",
        "interpretation": "The reporter's initialization omits GetProcedureAddress entirely — this is the required Vulkan function pointer loader"
      }
    ],
    "workarounds": [
      "Set GetProcedureAddress on GRVkBackendContext to a delegate that calls vkGetInstanceProcAddr / vkGetDeviceProcAddr to load Vulkan function pointers by name"
    ],
    "nextQuestions": [
      "Can the reporter share how they retrieve VkInstance/VkDevice/VkPhysicalDevice/VkQueue handles from Godot?",
      "Does Godot's GDNative/GDExtension API expose a way to retrieve the Vulkan function loader?"
    ],
    "resolution": {
      "hypothesis": "The root cause is the missing GetProcedureAddress delegate in GRVkBackendContext. Skia needs this callback to enumerate Vulkan extension functions before it can initialize the GPU context.",
      "proposals": [
        {
          "title": "Set GetProcedureAddress when building GRVkBackendContext",
          "description": "Assign a delegate to GRVkBackendContext.GetProcedureAddress that calls vkGetInstanceProcAddr (when device is IntPtr.Zero) or vkGetDeviceProcAddr (when device is non-zero) to load each Vulkan function by name. This is required for Skia to initialize a Vulkan GPU context.",
          "category": "workaround",
          "confidence": 0.82,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Set GetProcedureAddress when building GRVkBackendContext",
      "recommendedReason": "The missing delegate is the only structural difference between the reporter's code and a working Vulkan context setup. Setting it is the standard requirement per Skia's Vulkan backend design."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.78,
      "reason": "The null return from CreateVulkan is expected behavior when GetProcedureAddress is not set. This is a usage question about how to properly initialize GRVkBackendContext for Vulkan.",
      "suggestedReproPlatform": "windows"
    },
    "missingInfo": [
      "How Godot exposes its Vulkan handles (vkInstance, vkDevice, etc.) to .NET interop",
      "Whether Godot provides a function pointer loader accessible from .NET"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Change type/bug to type/question and ensure backend/Vulkan and os/Windows are applied",
        "risk": "low",
        "confidence": 0.82,
        "labels": [
          "type/question",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "backend/Vulkan"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post explanation of missing GetProcedureAddress delegate",
        "risk": "high",
        "confidence": 0.78,
        "comment": "Thanks for the detailed report!\n\nThe most likely cause of `GRContext.CreateVulkan` returning `null` is that `GRVkBackendContext.GetProcedureAddress` is not set. Skia requires a function pointer loader to enumerate Vulkan extension functions before it can initialize the GPU context — without it, `gr_direct_context_make_vulkan` will always fail silently and return `null`.\n\nYou need to assign a delegate to `GetProcedureAddress` that calls the Vulkan `vkGetInstanceProcAddr` / `vkGetDeviceProcAddr` functions to load each Vulkan symbol by name:\n\n```csharp\nvkBackendContext.GetProcedureAddress = (name, instance, device) =>\n{\n    if (device != IntPtr.Zero)\n        return VkGetDeviceProcAddr(device, name);\n    return VkGetInstanceProcAddr(instance, name);\n};\n```\n\nYou will also want to set `GRVkExtensions` (populated via `GRVkExtensions.Create(...)`) and `MaxAPIVersion` (the Vulkan API version your instance was created with, e.g. `VkMakeVersion(1, 1, 0)`) for best compatibility.\n\nIf Godot exposes its Vulkan loader through GDExtension or GDNative, that would be the right source for `vkGetInstanceProcAddr`.\n\nIf setting `GetProcedureAddress` doesn't resolve the issue, please share any Vulkan validation layer output and the Godot API you use to retrieve the driver resource handles — that will help narrow things down further."
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — the null return is expected when GetProcedureAddress is missing",
        "risk": "medium",
        "confidence": 0.72,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
