# Issue Triage Report — #4601

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Type | type/enhancement (0.98) |
| Area | area/SkiaSharp (0.93) |
| Backend | backend/Vulkan |
| Tenet | tenet/reliability |
| Suggested action | ready-to-fix (0.95) |

**Summary:** The current `GRVkBackendContext` wrapper and its generated native projection
do not contain the optional Skia device-lost callback fields, so managed applications cannot
receive Skia-originated Vulkan device-loss diagnostics.

The requested change is ABI-safe and additive. `GRVkBackendContext.cs` currently retains and
marshals only `GetProcedureAddress`; `GRVkBackendContextNative.generated.cs` consequently has
no device-lost callback/context fields. The existing delegate-proxy and Vulkan context tests
provide the relevant implementation and verification patterns.

An implementation is available in [#4655](https://github.com/mono/SkiaSharp/pull/4655), which
requires the companion [`mono/skia#338`](https://github.com/mono/skia/pull/338). Both remain
open, so the issue should remain open until those changes land.

**Recommendation:** Complete and merge the additive device-lost binding. Applications can
temporarily monitor their own Vulkan device loss outside SkiaSharp, but cannot receive the
Skia-originated callback until the binding ships.
