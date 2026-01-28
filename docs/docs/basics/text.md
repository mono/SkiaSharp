---
title: "Integrating Text and Graphics"
description: "This article explains how to determine the size of rendered text string to integrate text with SkiaSharp graphics into .NET MAUI applications, and demonstrates this with sample code."
ms.service: dotnet-maui
ms.assetid: A0B5AC82-7736-4AD8-AA16-FE43E18D203C
author: davidbritch
ms.author: dabritch
ms.date: 03/10/2017
---

# Integrating Text and Graphics

_See how to determine the size of rendered text string to integrate text with SkiaSharp graphics_

This article demonstrates how to measure text, scale the text to a particular size, and integrate text with other graphics:

![Text surrounded by rectangles](text-images/textandgraphicsexample.png)

That image also includes a rounded rectangle. The SkiaSharp `Canvas` class includes [`DrawRect`](xref:SkiaSharp.SKCanvas.DrawRect*) methods to draw a rectangle and [`DrawRoundRect`](xref:SkiaSharp.SKCanvas.DrawRoundRect*) methods to draw a rectangle with rounded corners. These methods allow the rectangle to be defined as an `SKRect` value or in other ways.

The **Framed Text** page centers a short text string on the page and surrounds it with a frame composed of a pair of rounded rectangles. The [`FramedTextPage`](https://github.com/mono/SkiaSharp/blob/docs/samples/Demos/Demos/SkiaSharpFormsDemos/Basics/FramedTextPage.cs) class shows how it's done.

In SkiaSharp, you use the `SKPaint` class to set text and font attributes, but you can also use it to obtain the rendered size of text. The beginning of the following `PaintSurface` event handler calls two different `MeasureText` methods. The first [`MeasureText`](xref:SkiaSharp.SKPaint.MeasureText(System.String)) call has a simple `string` argument and returns the pixel width of the text based on the current font attributes. The program then calculates a new `TextSize` property of the `SKPaint` object based on that rendered width, the current `TextSize` property, and the width of the display area. This calculation is intended to set `TextSize` so that the text string to be rendered at 90% of the width of the screen:

```csharp
void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
{
    SKImageInfo info = args.Info;
    SKSurface surface = args.Surface;
    SKCanvas canvas = surface.Canvas;

    canvas.Clear();

    string str = "Hello SkiaSharp!";

    // Create an SKPaint object to display the text
    SKPaint textPaint = new SKPaint
    {
        Color = SKColors.Chocolate
    };

    // Adjust TextSize property so text is 90% of screen width
    float textWidth = textPaint.MeasureText(str);
    textPaint.TextSize = 0.9f * info.Width * textPaint.TextSize / textWidth;

    // Find the text bounds
    SKRect textBounds = new SKRect();
    textPaint.MeasureText(str, ref textBounds);
    ...
}
```

The second [`MeasureText`](xref:SkiaSharp.SKPaint.MeasureText(System.String,SkiaSharp.SKRect@)) call has an `SKRect` argument, so it obtains both a width and height of the rendered text. The `Height` property of this `SKRect` value depends on the presence of capital letters, ascenders, and descenders in the text string. Different `Height` values are reported for the text strings "mom", "cat", and "dog", for example.

The `Left` and `Top` properties of the `SKRect` structure indicate the coordinates of the upper-left corner of the rendered text if the text is displayed by a `DrawText` call with X and Y positions of 0. For example, when this program is running on an iPhone 7 simulator, `TextSize` is assigned the value 90.6254 as a result of the calculation following the first call to `MeasureText`. The `SKRect` value obtained from the second call to `MeasureText` has the following property values:

- `Left` = 6
- `Top` = &ndash;68
- `Width` = 664.8214
- `Height` = 88;

Keep in mind that the X and Y coordinates you pass to the `DrawText` method specify the left side of the text at the baseline. The `Top` value indicates that the text extends 68 pixels above that baseline and (subtracting 68 it from 88) 20 pixels below the baseline. The `Left` value of 6 indicates that the text begins six pixels to the right of the X value in the `DrawText` call. This allows for normal inter-character spacing. If you want to display the text snugly in the upper-left corner of the display, pass the negatives of these `Left` and `Top` values as the X and Y coordinates of `DrawText`, in this example, &ndash;6 and 68.

The `SKRect` structure defines several handy properties and methods, some of which are used in the remainder of the `PaintSurface` handler. The `MidX` and `MidY` values indicate the coordinates of the center of the rectangle. (In the iPhone 7 example, those values are 338.4107 and &ndash;24.) The following code uses these values for the easiest calculation of coordinates to center text on the display:

```csharp
void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
{
    ...
    // Calculate offsets to center the text on the screen
    float xText = info.Width / 2 - textBounds.MidX;
    float yText = info.Height / 2 - textBounds.MidY;

    // And draw the text
    canvas.DrawText(str, xText, yText, textPaint);
    ...
}
```

The `SKImageInfo` info structure also defines a [`Rect`](xref:SkiaSharp.SKImageInfo.Rect) property of type `SKRect`, so you can also calculate `xText` and `yText` like this:

```csharp
float xText = info.Rect.MidX - textBounds.MidX;
float yText = info.Rect.MidY - textBounds.MidY;
```

The `PaintSurface` handler concludes with two calls to `DrawRoundRect`, both of which require arguments of `SKRect`. This `SKRect` value is based on the `SKRect` value obtained from the `MeasureText` method, but it can't be the same. First, it needs to be a little larger so that the rounded rectangle doesn't draw over edges of the text. Secondly, it needs to be shifted in space so that the `Left` and `Top` values correspond to the upper-left corner where the rectangle is to be positioned. These two jobs are accomplished by the [`Offset`](xref:SkiaSharp.SKRect.Offset*) and [`Inflate`](xref:SkiaSharp.SKRect.Inflate*) methods defined by `SKRect`:

```csharp
void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
{
    ...
    // Create a new SKRect object for the frame around the text
    SKRect frameRect = textBounds;
    frameRect.Offset(xText, yText);
    frameRect.Inflate(10, 10);

    // Create an SKPaint object to display the frame
    SKPaint framePaint = new SKPaint
    {
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 5,
        Color = SKColors.Blue
    };

    // Draw one frame
    canvas.DrawRoundRect(frameRect, 20, 20, framePaint);

    // Inflate the frameRect and draw another
    frameRect.Inflate(10, 10);
    framePaint.Color = SKColors.DarkBlue;
    canvas.DrawRoundRect(frameRect, 30, 30, framePaint);
}
```

Following that, the remainder of the method is straight-forward. It creates another `SKPaint` object for the borders and calls `DrawRoundRect` twice. The second call uses a rectangle inflated by another 10 pixels. The first call specifies a corner radius of 20 pixels. The second has a corner radius of 30 pixels, so they seem to be parallel:

 [![Triple screenshot of the Framed Text page](text-images/framedtext-small.png)](text-images/framedtext-large.png#lightbox "Triple screenshot of the Framed Text page")

You can turn your phone or simulator sideways to see the text and frame increase in size.

If you only need to center some text on the screen, you can do it approximately without measuring the text. Instead, set the [`TextAlign`](xref:SkiaSharp.SKPaint.TextAlign) property of `SKPaint` to the enumeration member [`SKTextAlign.Center`](xref:SkiaSharp.SKTextAlign). The X coordinate you specify in the `DrawText` method then indicates where the horizontal center of the text is positioned. If you pass the midpoint of the screen to the `DrawText` method, the text will be horizontally centered and *nearly* vertically centered because the baseline will be vertically centered.

Text can be treated much like any other graphical object. One simple option is to display the outline of the text characters:

[![Triple screen shot of the Outlined Text page](text-images/outlinedtext-small.png)](text-images/outlinedtext-large.png#lightbox "Triple screenshot of the Outlined Text page")

This is accomplished simply by changing the normal `Style` property of the `SKPaint` object from its default setting of `SKPaintStyle.Fill` to `SKPaintStyle.Stroke`, and by specifying a stroke width. The `PaintSurface` handler of the **Outlined Text** page shows how it's done:

```csharp
void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
{
    SKImageInfo info = args.Info;
    SKSurface surface = args.Surface;
    SKCanvas canvas = surface.Canvas;

    canvas.Clear();

    string text = "OUTLINE";

    // Create an SKPaint object to display the text
    SKPaint textPaint = new SKPaint
    {
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 1,
        FakeBoldText = true,
        Color = SKColors.Blue
    };

    // Adjust TextSize property so text is 95% of screen width
    float textWidth = textPaint.MeasureText(text);
    textPaint.TextSize = 0.95f * info.Width * textPaint.TextSize / textWidth;

    // Find the text bounds
    SKRect textBounds = new SKRect();
    textPaint.MeasureText(text, ref textBounds);

    // Calculate offsets to center the text on the screen
    float xText = info.Width / 2 - textBounds.MidX;
    float yText = info.Height / 2 - textBounds.MidY;

    // And draw the text
    canvas.DrawText(text, xText, yText, textPaint);
}
```

Another common graphical object is the bitmap. That's a large topic covered in depth in the section [**SkiaSharp Bitmaps**](../bitmaps/index.md), but the next article, [**Bitmap Basics in SkiaSharp**](bitmaps.md), provides a briefer introduction.

## Related Links

- [SkiaSharp APIs](/dotnet/api/skiasharp)
