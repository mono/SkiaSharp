---
applyTo: "tests/**/*.cs,**/*Tests.cs,**/*Test.cs"
---

# Test Code Instructions

You are working on test code for SkiaSharp.

> **📚 Documentation:**
> - **Quick Start:** [design/QUICKSTART.md](../../design/QUICKSTART.md)
> - **Memory Management:** [design/memory-management.md](../../design/memory-management.md)
> - **Error Handling:** [design/error-handling.md](../../design/error-handling.md)

## Testing Focus Areas

1. **Memory Management** - Verify no leaks, proper disposal, ref counting
2. **Error Handling** - Test invalid inputs, failure cases, exceptions
3. **Object Lifecycle** - Test create → use → dispose pattern
4. **Threading** - Test thread-safety where documented

## Test Patterns

### Always Use Using Statements
```csharp
[Fact]
public void DrawRectWorksCorrectly()
{
    using (var bitmap = new SKBitmap(100, 100))
    using (var canvas = new SKCanvas(bitmap))
    using (var paint = new SKPaint { Color = SKColors.Red })
    {
        canvas.DrawRect(new SKRect(10, 10, 90, 90), paint);
        Assert.NotEqual(SKColors.White, bitmap.GetPixel(50, 50));
    }
}
```

### Test Disposal
```csharp
[Fact]
public void DisposedObjectThrows()
{
    var paint = new SKPaint();
    paint.Dispose();
    Assert.Throws<ObjectDisposedException>(() => paint.Color = SKColors.Red);
}
```

### Test Error Cases
```csharp
[Fact]
public void NullParameterThrows()
{
    using (var canvas = new SKCanvas(bitmap))
    {
        Assert.Throws<ArgumentNullException>(() => 
            canvas.DrawRect(rect, null));
    }
}
```

## What to Test

✅ Test both success and failure paths
✅ Test edge cases (empty, null, zero, negative, max)
✅ Verify exception types and messages
✅ Test complete lifecycle
✅ Test memory management (no leaks)

## What NOT to Do

❌ Leave objects undisposed in tests
❌ Ignore exception types
❌ Test only happy path
❌ Assume GC will clean up
