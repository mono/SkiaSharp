---
title: "SkiaSharp effects"
description: "Learn how to alter the normal display of graphics with gradients, bitmap tiling, blend modes, blur and other effects."
ms.service: dotnet-maui
ms.subservice: skiasharp
ms.assetid: B3E06572-8E2A-49FA-90D1-444C394CD516
author: davidbritch
ms.author: dabritch
ms.date: 08/22/2018
no-loc: [.NET MAUI, Microsoft.Maui]
---

# SkiaSharp effects

The SkiaSharp [`SKPaint`](xref:SkiaSharp.SKPaint) class defines six properties that can be classified under the general term of _effects_. These are properties that alter the normal display of graphics in some way. The SkiaSharp effects fall into six categories:

## [Path Effects](../curves/effects.md)

Set the [`PathEffect`](xref:SkiaSharp.SKPaint.PathEffect) property of `SKPaint` to an object of type [`SKPathEffect`](xref:SkiaSharp.SKPathEffect) to display dashed lines, or to stroke or fill an area with a pattern created from paths. The path effect was covered earlier in this series in the article [**Path Effects in SkiaSharp**](../curves/effects.md).

## [Shaders](shaders/index.md)

Set the [`Shader`](xref:SkiaSharp.SKPaint.Shader) property of `SKPaint` to an object of type [`SKShader`](xref:SkiaSharp.SKShader) to display linear or circular gradients, tiled bitmaps, and Perlin noise patterns.

## [Blend Modes](blend-modes/index.md)

Set the [`BlendMode`](xref:SkiaSharp.SKPaint.BlendMode) property of `SKPaint` to a member of the [`SKBlendMode`](xref:SkiaSharp.SKBlendMode) enumeration to govern what happens when a source graphic is displayed on a destination. SkiaSharp supports all the CSS compositing and blend modes, including the Porter-Duff modes, separable blend modes, and non-separable blend modes.

## [Mask Filters](mask-filters.md)

Set the [`MaskFilter`](xref:SkiaSharp.SKPaint.MaskFilter) property of `SKPaint` to an object of type [`SKMaskFilter`](xref:SkiaSharp.SKMaskFilter) for blurs and other alpha effects.

## [Image Filters](image-filters.md)

Set the [`ImageFilter`](xref:SkiaSharp.SKPaint.ImageFilter) property of `SKPaint` to an object of type [`SKImageFilter`](xref:SkiaSharp.SKImageFilter) for blurring bitmaps and creating drop shadows, embossing, or engraving effects.

## [Color Filters](color-filters.md)

Set the [`ColorFilter`](xref:SkiaSharp.SKPaint.ColorFilter) property of `SKPaint` to an object of type [`SKColorFilter`](xref:SkiaSharp.SKColorFilter) to alter colors using tables or matrix transforms.

All the sample code for these articles are in the sample. From the home page, select **SkiaSharp Effects**.

## Related links

- [SkiaSharp APIs](/dotnet/api/skiasharp)
