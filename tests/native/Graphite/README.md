# Graphite native smoke tests

Two standalone harnesses validate Skia's Graphite backend at the native layer before any C# binding work is exercised:

- `cpp_smoke/` — exercises Skia Graphite directly through `skgpu::graphite::*` C++. Pass criterion proves that Skia + the chosen GPU backend (Vulkan via Lavapipe in CI / WSL2 dev) work in this environment.
- `c_smoke/` — exercises the same scenario through the new `sk_graphite_*` C API in `externals/skia/include/c/`. Pass criterion isolates bugs in the SkiaSharp C shim from bugs in Skia or the build.

Both harnesses render a 256×256 white surface with a red rounded rectangle, snap → insert → submit (sync), read pixels back, and assert pixel `(128, 128)` is red.

See `specs/002-graphite-backend-support/quickstart.md` for the full three-layer flow (these two layers plus the C# `[SkippableFact]` smoke), the bisection guide, and the canonical build commands.
