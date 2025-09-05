---
title: "SkiaSharp Lines and Paths"
description: "This article explains how to use SkiaSharp to draw lines and graphics paths in .NET MAUI applications, and demonstrates this with sample code."
ms.service: dotnet-maui
ms.assetid: 316A15FE-383D-4D06-8641-BAC7EE7474CA
ms.subservice: skiasharp
author: davidbritch
ms.author: dabritch
ms.date: 03/10/2017
no-loc: [.NET MAUI, Microsoft.Maui]
---

# SkiaSharp Lines and Paths

_Use SkiaSharp to draw lines and graphics paths_

The [previous section](~/xamarin-forms/user-interface/graphics/skiasharp/basics/index.md) demonstrated that the SkiaSharp `SKCanvas` class includes several methods to draw circles, ovals, rectangles, rounded rectangles, text, and bitmaps. This section and later sections cover the various classes connected with creating and rendering *graphics paths*.

The graphics path is the most generalized approach to drawing lines and curves in SkiaSharp. This section covers using an [`SKPath`](xref:SkiaSharp.SKPath) object to draw straight lines, and to use a collection of tiny straight lines (called a *polyline*) to draw curves that you can define algorithmically. A later section on [**SkiaSharp Curves and Paths**](../curves/index.md) discusses the various sorts of curves supported by `SKPath`.

All the sample programs in this section appear under the heading **Lines and Paths** in the home page of the sample program, and in the [**Paths**](https://github.com/xamarin/xamarin-forms-samples/tree/master/SkiaSharpForms/Demos/Demos/SkiaSharpFormsDemos/Paths) folder of that solution.

## [Lines and Stroke Caps](lines.md)

Learn how to use SkiaSharp to draw lines with different stroke caps.

## [Path Basics](paths.md)

Explore the SkiaSharp `SKPath` object for combining lines and curves.

## [The Path Fill Types](fill-types.md)

Discover the different effects possible with SkiaSharp path fill types.

## [Polylines and Parametric Equations](polylines.md)

Use SkiaSharp to render any line you can define with parametric equations.

## [Dots and Dashes](dots.md)

Master the intricacies of drawing dotted and dashed lines in SkiaSharp.

## [Finger Painting](finger-paint.md)

Use your fingers to paint on the canvas.

## Related Links

- [SkiaSharp APIs](/dotnet/api/skiasharp)
