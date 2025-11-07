---
applyTo: "samples/**/*.cs"
---

# Sample Code Instructions

You are working on sample/example code.

> **📚 Documentation:**
> - **Quick Start:** [design/QUICKSTART.md](../../design/QUICKSTART.md) - See best practices
> - **API Guide:** [design/adding-new-apis.md](../../design/adding-new-apis.md)

## Sample Code Standards

- Demonstrate **best practices** (always use `using` statements)
- Include **error handling**
- Show **complete, working examples**
- Keep code **simple and educational**
- **Comment** complex or non-obvious parts

## Memory Management in Samples

### Always Use Using Statements
```csharp
// ✅ Correct
using (var surface = SKSurface.Create(info))
using (var canvas = surface.Canvas)
using (var paint = new SKPaint())
{
    // Use objects
}
```

### Make Self-Contained
```csharp
using System;
using System.IO;
using SkiaSharp;

public static void DrawRectangleSample()
{
    var info = new SKImageInfo(256, 256);
    
    using (var surface = SKSurface.Create(info))
    using (var canvas = surface.Canvas)
    using (var paint = new SKPaint { Color = SKColors.Blue })
    {
        canvas.Clear(SKColors.White);
        canvas.DrawRect(new SKRect(50, 50, 200, 200), paint);
        
        // Save
        using (var image = surface.Snapshot())
        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
        using (var stream = File.OpenWrite("output.png"))
        {
            data.SaveTo(stream);
        }
    }
}
```

## What NOT to Do

❌ **Don't skip disposal**
❌ **Don't show bad patterns**
❌ **Don't leave code incomplete**
❌ **Don't skip error handling**
