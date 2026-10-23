---
title: "Testing"
description: "Test rendered images, choose stable references and diagnose visual differences with SkiaSharp."
---

# Testing rendered output

Rendering tests answer a different question from drawing tutorials: **did this
change produce the expected pixels?** This section covers testing tools and
practices, independent of the UI framework or test runner.

## Choose the right test boundary

| Test | Control before comparison |
| --- | --- |
| Render a scene directly | Fonts, image inputs, canvas size, color/alpha representation and animation time |
| Capture a control or page | The above, plus display scale, capture bounds, platform and a stable UI state |
| Validate a package build | The exact managed and native package cohort, rather than locally built substitutes |

Keep reviewed references appropriate to that boundary. A GPU-surface reference,
full-page browser capture and cropped native control are not interchangeable.
An image comparer cannot correct unstable capture timing or a changed layout.

## Compare and investigate

[Compare images](image-comparison.md) introduces `SkiaSharp.Testing`: exact and
tolerant comparisons, measurements, ignore coverage and visual diagnostics.

On failure, retain the reference, actual image, difference image and comparison
settings from the same attempt. Inspect what changed before adjusting tolerance
or exclusions. Baseline acceptance is a separate, explicit review step; missing
references must not silently pass.
