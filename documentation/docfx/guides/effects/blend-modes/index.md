---
title: "SkiaSharp blend modes"
description: "Use blend modes to define what happens when graphical objects are stacked on one another."
ms.service: dotnet-maui
ms.assetid: CE1B222E-A2D0-4016-A532-EC1E59EE3D6B
author: davidbritch
ms.author: dabritch
ms.date: 08/23/2018
---

# SkiaSharp blend modes

These articles focus on the [`BlendMode`](xref:SkiaSharp.SKPaint.BlendMode) property of [`SKPaint`](xref:SkiaSharp.SKPaint). The `BlendMode` property is of type [`SKBlendMode`](xref:SkiaSharp.SKBlendMode), an enumeration with 29 members.

The `BlendMode` property determines what happens when a graphical object (often called the _source_) is rendered on top of existing graphical objects (called the _destination_). Normally, we expect the new graphical object to obscure the objects underneath it. But that happens only because the default blend mode is `SKBlendMode.SrcOver`, which means that the source is drawn _over_ the destination. The other 28 members of `SKBlendMode` cause other effects. In graphics programming, the technique of combining graphical objects in various ways is known as _compositing_.

## The SKBlendModes enumeration

The SkiaSharp blend modes correspond closely to those described in the W3C [**Compositing and Blending Level 1**](https://www.w3.org/TR/compositing-1/) specification. The Skia [**SkBlendMode Overview**](https://skia.org/docs/user/api/skblendmode_overview/) also provides helpful background information. For a general introduction to blend modes, the [**Blend modes**](https://en.wikipedia.org/wiki/Blend_modes) article in Wikipedia is a good start. Blend modes are supported in Adobe Photoshop, so there is much additional online information about blend modes in that context.

The 29 members of the `SKBlendMode` enumeration can be divided into three categories:

| Porter-Duff | Separable    | Non-Separable |
| ----------- | ------------ | ------------- |
| `Clear`     | `Modulate`   | `Hue`         |
| `Src`       | `Screen`     | `Saturation`  |
| `Dst`       | `Overlay`    | `Color`       |
| `SrcOver`   | `Darken`     | `Luminosity`  |
| `DstOver`   | `Lighten`    |               |
| `SrcIn`     | `ColorDodge` |               |
| `DstIn`     | `ColorBurn`  |               |
| `SrcOut`    | `HardLight`  |               |
| `DstOut`    | `SoftLight`  |               |
| `SrcATop`   | `Difference` |               |
| `DstATop`   | `Exclusion`  |               |
| `Xor`       | `Multiply`   |               |
| `Plus`      |              |               |

The names of these three categories will take on more meaning in the discussions that follow. The order that the members are listed here is the same as in the definition of the `SKBlendMode` enumeration. The 13 enumeration members in the first column have the integer values 0 to 12. The second column are enumeration members that correspond to integers 13 to 24, and the members in the third column have values of 25 to 28.

These blend modes are discussed in _approximately_ the same order in the W3C **Compositing and Blending Level 1** document, but there are a few differences: The `Src` mode is called _Copy_ in the W3C document, and `Plus` is called _Lighter_. The W3C document defines a _Normal_ blend mode that isn't included in `SKBlendModes` because it would be the same as `SrcOver`. The `Modulate` blend mode (at the top of the second column) isn't included in the W3C document, and discussion of the `Multiply` mode precedes `Screen`.

Because the `Modulate` blend mode is unique to Skia, it will be discussed as an additional Porter-Duff mode and as a separable mode.

## The importance of transparency

Historically, compositing was developed in conjunction with the concept of the _alpha channel_. In a display surface such as the `SKCanvas` object and a full-color bitmap, each pixel consists of 4 bytes: 1 byte each for the red, green, and blue components, and an additional byte for transparency. This alpha component is 0 for full transparency and 0xFF for full opacity, with different levels of transparency between those values.

Many of the blend modes rely on transparency. Usually, when an `SKCanvas` is first obtained in a `PaintSurface` handler, or when an `SKCanvas` is created to draw on a bitmap, the first step is this call:

```csharp
canvas.Clear();
```

This method replaces all the pixels of the canvas with transparent black pixels, equivalent to `new SKColor(0, 0, 0, 0)` or the integer 0x00000000. All the bytes of all the pixels are initialized to zero.

The drawing surface of an `SKCanvas` that is obtained in a `PaintSurface` handler might appear to have a white background, but that's only because the `SKCanvasView` itself has a transparent background, and the page has a white background. You can demonstrate this fact to yourself by setting the .NET MAUI `BackgroundColor` property of `SKCanvasView` to a .NET MAUI color:

```csharp
canvasView.BackgroundColor = Colors.Red;
```

Or, in a class that derives from `ContentPage`, you can set the page background color:

```csharp
BackgroundColor = Colors.Red;
```

You'll see this red background behind your SkiaSharp graphics because the SkiaSharp canvas itself is transparent.

The article [**SkiaSharp Transparency**](../../basics/transparency.md) showed some basic techniques in using transparency to arrange multiple graphics in a composite image. The blend modes go beyond that, but transparency remains crucial to the blend modes.

## [SkiaSharp Porter-Duff blend modes](porter-duff.md)

Use the Porter-Duff blend modes to compose scenes based on source and destination images.

## [SkiaSharp separable blend modes](separable.md)

Use the separable blend modes to alter red, green, and blue colors.

## [SkiaSharp non-separable blend modes](non-separable.md)

Use the non-separable blend modes to alter hue, saturation, or luminosity.

## Related links

- [SkiaSharp APIs](/dotnet/api/skiasharp)
