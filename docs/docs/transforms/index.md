---
title: "SkiaSharp Transforms"
description: "This article explores transforms for displaying SkiaSharp graphics in .NET MAUI applications, and demonstrates this with sample code."
ms.service: dotnet-maui
ms.assetid: E9BE322E-ECB3-4395-AFE4-4474A0F25551
author: davidbritch
ms.author: dabritch
ms.date: 03/10/2017
---

# SkiaSharp Transforms

_Learn about transforms for displaying SkiaSharp graphics_

SkiaSharp supports traditional graphics transforms that are implemented as methods of the [`SKCanvas`](xref:SkiaSharp.SKCanvas) object. Mathematically, transforms alter the coordinates and sizes that you specify in `SKCanvas` drawing functions as the graphical objects are rendered. Transforms are often convenient for drawing repetitive graphics or for animation. Some techniques &mdash; such as rotating bitmaps or text &mdash; are not possible without the use of transforms.

SkiaSharp transforms support the following operations:

- *Translate* to shift coordinates from one location to another
- *Scale* to increase or decrease coordinates and sizes
- *Rotate* to rotate coordinates around a point
- *Skew* to shift coordinates horizontally or vertically so that a rectangle becomes a parallelogram

These are known as *affine* transforms. Affine transforms always preserve parallel lines and never cause a coordinate or size to become infinite. A square is never transformed into anything other than a parallelogram, and a circle is never transformed into anything other than an ellipse.

SkiaSharp also supports non-affine transforms (also called *projective* or *perspective* transforms) based on a standard 3-by-3 transform matrix. A non-affine transform allows a square to be transformed into any convex quadrilateral, which is a four-sided figure with all interior angles less than 180 degrees. Non-affine transforms can cause coordinates or sizes to become infinite, but they are vital for 3D effects.

## Differences between SkiaSharp and .NET MAUI Transforms

.NET MAUI also supports transforms that are similar to those in SkiaSharp. The .NET MAUI [`VisualElement`](xref:Microsoft.Maui.Controls.VisualElement) class defines the following transform properties:

- [`TranslationX`](xref:Microsoft.Maui.Controls.VisualElement.TranslationX) and [`TranslationY`](xref:Microsoft.Maui.Controls.VisualElement.TranslationY)
- [`Scale`](xref:Microsoft.Maui.Controls.VisualElement.Scale)
- [`Rotation`](xref:Microsoft.Maui.Controls.VisualElement.Rotation), [`RotationX`](xref:Microsoft.Maui.Controls.VisualElement.RotationX), and [`RotationY`](xref:Microsoft.Maui.Controls.VisualElement.RotationY)

The `RotationX` and `RotationY` properties are perspective transforms that create quasi-3D effects.

There are several crucial differences between SkiaSharp transforms and .NET MAUI transforms:

The first difference is that SkiaSharp transforms are applied to the entire `SKCanvas` object while the .NET MAUI transforms are applied to individual `VisualElement` derivatives. (You can apply the .NET MAUI transforms to the `SKCanvasView` object itself, because `SKCanvasView` derives from `VisualElement`, but within that `SKCanvasView`, the SkiaSkarp transforms apply.)

The SkiaSharp transforms are relative to the upper-left corner of the `SKCanvas` while .NET MAUI transforms are relative to the upper-left corner of the `VisualElement` to which they are applied. This difference is important when applying scaling and rotation transforms because these transforms are always relative to a particular point.

The really big difference is that SKiaSharp transforms are *methods* while the .NET MAUI transforms are *properties*. This is a semantic difference beyond the syntactical difference: SkiaSharp transforms perform an operation while .NET MAUI transforms set a state. SkiaSharp transforms apply to subsequently drawn graphics objects, but not to graphics objects that are drawn before the transform is applied. In contrast, a .NET MAUI transform applies to a previously rendered element as soon as the property is set. SkiaSharp transforms are cumulative as the methods are called; .NET MAUI transforms are replaced when the property is set with another value.

All the sample programs in this section appear in the **SkiaSharp Transforms** section of the sample program. Source code can be found in the [**Transforms**](https://github.com/xamarin/xamarin-forms-samples/tree/master/SkiaSharpForms/Demos/Demos/SkiaSharpFormsDemos/Transforms) folder of the solution.

## [The Translate Transform](translate.md)

Learn how to use the translate transform to shift SkiaSharp graphics.

## [The Scale Transform](scale.md)

Discover the SkiaSharp scale transform for scaling objects to various sizes.

## [The Rotate Transform](rotate.md)

Explore the effects and animations possible with the SkiaSharp rotate transform.

## [The Skew Transform](skew.md)

See how the skew transform can create tilted graphical object.

## [Matrix Transforms](matrix.md)

Dive deeper into SkiaSharp transforms with the versatile transform matrix.

## [Touch Manipulations](touch.md)

Use matrix transforms to implement touch manipulations for dragging, scaling, and rotation.

## [Non-Affine Transforms](non-affine.md)

Go beyond the oridinary with non-affine transform effects.

## [3D Rotation](3d-rotation.md)

Use non-affine transforms to rotate 2D objects in 3D space.

## Related Links

- [SkiaSharp APIs](/dotnet/api/skiasharp)
