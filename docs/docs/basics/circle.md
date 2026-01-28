---
title: "Drawing a Simple Circle in SkiaSharp"
description: "This article explains the basics of SkiaSharp drawing, including canvases and paint objects, in .NET MAUI applications, and demonstrates this with sample code."
ms.service: dotnet-maui
ms.assetid: E3A4E373-F65D-45C8-8E77-577A804AC3F8
author: davidbritch
ms.author: dabritch
ms.date: 03/10/2017
---

# Drawing a Simple Circle in SkiaSharp

_Learn the basics of SkiaSharp drawing, including canvases and paint objects_

This article introduces the concepts of drawing graphics in .NET MAUI using SkiaSharp, including creating an `SKCanvasView` object to host the graphics, handling the `PaintSurface` event, and using a `SKPaint` object to specify color and other drawing attributes.

The sample program contains all the sample code for this series of SkiaSharp articles. The first page is entitled **Simple Circle** and invokes the page class [`SimpleCirclePage`](https://github.com/mono/SkiaSharp/blob/docs/samples/DocsSamplesApp/DocsSamplesApp/Basics/SimpleCirclePage.cs). This code shows how to draw a circle in the center of the page with a radius of 100 pixels. The outline of the circle is red, and the interior of the circle is blue.

![A blue circle outlined in red](circle-images/circleexample.png)

The [`SimpleCirclePage`](https://github.com/mono/SkiaSharp/blob/docs/samples/DocsSamplesApp/DocsSamplesApp/Basics/SimpleCirclePage.cs) page class derives from `ContentPage` and contains two `using` directives for the SkiaSharp namespaces:

```csharp
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
```

The following constructor of the class creates an [`SKCanvasView`](xref:SkiaSharp.Views.Maui.Controls.SKCanvasView) object, attaches a handler for the [`PaintSurface`](xref:SkiaSharp.Views.Maui.Controls.SKCanvasView.PaintSurface) event, and sets the `SKCanvasView` object as the content of the page:

```csharp
public SimpleCirclePage()
{
    Title = "Simple Circle";

    SKCanvasView canvasView = new SKCanvasView();
    canvasView.PaintSurface += OnCanvasViewPaintSurface;
    Content = canvasView;
}
```

The `SKCanvasView` occupies the entire content area of the page. You can alternatively combine an `SKCanvasView` with other .NET MAUI `View` derivatives, as you'll see in other examples.

The `PaintSurface` event handler is where you do all your drawing. This method can be called multiple times while your program is running, so it should maintain all the information necessary to recreate the graphics display:

```csharp
void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
{
    ...
}

```

The [`SKPaintSurfaceEventArgs`](xref:SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs) object that accompanies the event has two properties:

- [`Info`](xref:SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs.Info) of type [`SKImageInfo`](xref:SkiaSharp.SKImageInfo)
- [`Surface`](xref:SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs.Surface) of type [`SKSurface`](xref:SkiaSharp.SKSurface)

The `SKImageInfo` structure contains information about the drawing surface, most importantly, its width and height in pixels. The `SKSurface` object represents the drawing surface itself. In this program, the drawing surface is a video display, but in other programs an `SKSurface` object can also represent a bitmap that you use SkiaSharp to draw on.

The most important property of `SKSurface` is [`Canvas`](xref:SkiaSharp.SKSurface.Canvas) of type [`SKCanvas`](xref:SkiaSharp.SKCanvas). This class is a graphics drawing context that you use to perform the actual drawing. The `SKCanvas` object encapsulates a graphics state, which includes graphics transforms and clipping.

Here's a typical start of a `PaintSurface` event handler:

```csharp
void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
{
    SKImageInfo info = args.Info;
    SKSurface surface = args.Surface;
    SKCanvas canvas = surface.Canvas;

    canvas.Clear();
    ...
}

```

The [`Clear`](xref:SkiaSharp.SKCanvas.Clear) method clears the canvas with a transparent color. An overload lets you specify a background color for the canvas.

The goal here is to draw a red circle filled with blue. Because this particular graphic image contains two different colors, the job needs to be done in two steps. The first step is to draw the outline of the circle. To specify the color and other characteristic of the line, you create and initialize an [`SKPaint`](xref:SkiaSharp.SKPaint) object:

```csharp
void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
{
    ...
    SKPaint paint = new SKPaint
    {
        Style = SKPaintStyle.Stroke,
        Color = Colors.Red.ToSKColor(),
        StrokeWidth = 25
    };
    ...
}
```

The [`Style`](xref:SkiaSharp.SKPaint.Style) property indicates that you want to *stroke* a line (in this case the outline of the circle) rather than *fill* the interior. The three members of the [`SKPaintStyle`](xref:SkiaSharp.SKPaintStyle) enumeration are as follows:

- [`Fill`](xref:SkiaSharp.SKPaintStyle.Fill)
- [`Stroke`](xref:SkiaSharp.SKPaintStyle.Stroke)
- [`StrokeAndFill`](xref:SkiaSharp.SKPaintStyle.StrokeAndFill)

The default is `Fill`. Use the third option to stroke the line and fill the interior with the same color.

Set the [`Color`](xref:SkiaSharp.SKPaint.Color) property to a value of type [`SKColor`](xref:SkiaSharp.SKColor). One way to get an `SKColor` value is by converting a .NET MAUI `Color` value to an `SKColor` value using the extension method [`ToSKColor`](xref:SkiaSharp.Views.Maui.Extensions.ToSKColor*). The [`Extensions`](xref:SkiaSharp.Views.Maui.Extensions) class in the `SkiaSharp.Views.Maui.Controls` namespace includes other methods that convert between .NET MAUI values and SkiaSharp values.

The [`StrokeWidth`](xref:SkiaSharp.SKPaint.StrokeWidth) property indicates the thickness of the line. Here it's set to 25 pixels.

You use that `SKPaint` object to draw the circle:

```csharp
void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
{
    ...
    canvas.DrawCircle(info.Width / 2, info.Height / 2, 100, paint);
    ...
}
```

Coordinates are specified relative to the upper-left corner of the display surface. X coordinates increase to the right and Y coordinates increase going down. In discussion about graphics, often the mathematical notation (x, y) is used to denote a point. The point (0, 0) is the upper-left corner of the display surface and is often called the *origin*.

The first two arguments of `DrawCircle` indicate the X and Y coordinates of the center of the circle. These are assigned to half the width and height of the display surface to put the center of the circle in the center of the display surface. The third argument specifies the circle's radius, and the last argument is the `SKPaint` object.

To fill the interior of the circle, you can alter two properties of the `SKPaint` object and call `DrawCircle` again. This code also shows an alternative way to get an `SKColor` value from one of the many fields of the [`SKColors`](xref:SkiaSharp.SKColors) structure:

```csharp
void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
{
    ...
    paint.Style = SKPaintStyle.Fill;
    paint.Color = SKColors.Blue;
    canvas.DrawCircle(args.Info.Width / 2, args.Info.Height / 2, 100, paint);
}
```

This time, the `DrawCircle` call fills the circle using the new properties of the `SKPaint` object.

Here's the program running on iOS and Android:

[![Triple screenshot of the Simple Circle page](circle-images/simplecircle-small.png)](circle-images/simplecircle-large.png#lightbox "Triple screenshot of the Simple Circle page")

When running the program yourself, you can turn the phone or simulator sideways to see how the graphic is redrawn. Each time the graphic needs to be redrawn, the `PaintSurface` event handler is called again.

It's also possible to color graphical objects with gradients or bitmap tiles. These options are discussed in the section on [**SkiaSharp shaders**](../effects/shaders/index.md).

An `SKPaint` object is little more than a collection of graphics drawing properties. These objects are lightweight. You can reuse `SKPaint` objects as this program does, or you can create multiple `SKPaint` objects for various combinations of drawing properties. You can create and initialize these objects outside of the `PaintSurface` event handler, and you can save them as fields in your page class.

> [!NOTE]
> The `SKPaint` class defines an [`IsAntialias`](xref:SkiaSharp.SKPaint.IsAntialias) to enable anti-aliasing in the rendering of your graphics. Anti-aliasing generally results in visually smoother edges, so you'll probably want to set this property to `true` in most of your `SKPaint` objects. For purposes of simplicity, this property is _not_ set in most of the sample pages.

Although the width of the circle's outline is specified as 25 pixels &mdash; or one-quarter of the radius of the circle &mdash; it appears to be thinner, and there's a good reason for that: Half the width of the line is obscured by the blue circle. The arguments to the `DrawCircle` method define the abstract geometric coordinates of a circle. The blue interior is sized to that dimension to the nearest pixel, but the 25-pixel-wide outline straddles the geometric circle &mdash; half on the inside and half on the outside.

The next sample in the [Integrating with .NET MAUI](integration.md) article demonstrates this visually.

## Related Links

- [SkiaSharp APIs](/dotnet/api/skiasharp)
