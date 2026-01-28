# DocsSamplesApp Warnings Inventory

This document catalogs all compiler warnings in the DocsSamplesApp project after the migration to .NET 10 and SkiaSharp 3.119.1.

**Last Updated**: January 2026

---

## Summary

| Category | Status | Files | Instances | Notes |
|----------|--------|-------|-----------|-------|
| CS8622 - Event handler nullability | ✅ FIXED | 97 | 107 | Changed `object sender` to `object? sender` |
| CS0618 - Application.MainPage obsolete | ✅ FIXED | 1 | 1 | Migrated to AppShell with CreateWindow |
| CS0618 - SKPaint text APIs obsolete | ⏳ PENDING | ~40 | ~200 | Requires SKFont usage patterns |
| CS0618 - Device.StartTimer obsolete | ⏳ PENDING | 15 | 30 | Use `Dispatcher.StartTimer()` |
| CS0612 - Device class obsolete | ⏳ PENDING | 15 | 32 | Same files as StartTimer |

**Current Build Status**: 234 warnings, 0 errors

---

## ✅ Completed Fixes

### 1. CS8622: Event Handler Nullability (FIXED)

All event handlers updated to use nullable sender:
- `object sender` → `object? sender`
- Fixed in 97 code files and 45 documentation files
- Commit: "Fix event handler nullability (CS8622)"

### 2. CS0618: Application.MainPage (FIXED)

Migrated to modern AppShell pattern:
- App.xaml.cs now uses `CreateWindow()` with `AppShell`
- Deleted unused `MainPage.xaml` and `MainPage.xaml.cs`
- Commit: "Migrate to AppShell pattern"

---

## ⏳ Remaining Warnings

### 3. CS0618/CS0612: Device.StartTimer Obsolete (15 files)

**Description**: `Device.StartTimer` is obsolete in .NET MAUI. The `Device` class itself is also marked obsolete.

**Fix Pattern**:
```csharp
// Before (Xamarin.Forms)
Device.StartTimer(TimeSpan.FromMilliseconds(16), () =>
{
    canvasView.InvalidateSurface();
    return true;
});

// After (.NET MAUI)
Dispatcher.StartTimer(TimeSpan.FromMilliseconds(16), () =>
{
    canvasView.InvalidateSurface();
    return true;
});
```

**Affected Files**:
- Basics/CodeMoreCodePage.cs
- Basics/ExpandingCirclesPage.cs
- Bitmaps/AnimatedGifPage.xaml.cs
- Curves/AnimatedDottedTextPage.cs
- Curves/ConveyorBeltPage.cs
- Curves/DotDashMorphPage.cs
- Curves/PrettyAnalogClockPage.cs
- Curves/SquaringTheCirclePage.cs
- Curves/UnicycleHalfPipePage.cs
- Effects/AnimatedBitmapTilePage.cs
- Effects/GradientAnimationPage.cs
- Effects/InfinityColorsPage.cs
- Paths/AnimatedSpiralPage.cs
- Transforms/HendecagramAnimationPage.cs
- Transforms/UglyAnalogClockPage.cs

---

### 4. CS0618: Obsolete SKPaint Text APIs (~40 files, ~200 instances)

**Description**: Text-related properties on SKPaint are obsolete in SkiaSharp 3.x. Use SKFont instead.

### Obsolete APIs and Replacements

| Obsolete API | Replacement |
|--------------|-------------|
| `SKPaint.TextSize` | `SKFont.Size` |
| `SKPaint.Typeface` | `SKFont.Typeface` |
| `SKPaint.MeasureText()` | `SKFont.MeasureText()` |
| `SKPaint.FontSpacing` | `SKFont.Spacing` |
| `SKPaint.FakeBoldText` | `SKFont.Embolden` |
| `SKPaint.TextAlign` | Pass `SKTextAlign` to `DrawText()` |
| `SKPaint.GetTextPath()` | `SKFont.GetTextPath()` |
| `SKCanvas.DrawText(string, float, float, SKPaint)` | `SKCanvas.DrawText(string, float, float, SKTextAlign, SKFont, SKPaint)` |

### Fix Pattern
```csharp
// Before (SkiaSharp 2.x)
using (var paint = new SKPaint())
{
    paint.TextSize = 40;
    paint.FakeBoldText = true;
    float width = paint.MeasureText("Hello");
    canvas.DrawText("Hello", x, y, paint);
}

// After (SkiaSharp 3.x)
using (var paint = new SKPaint())
using (var font = new SKFont { Size = 40, Embolden = true })
{
    float width = font.MeasureText("Hello");
    canvas.DrawText("Hello", x, y, SKTextAlign.Left, font, paint);
}
```

### For Text Path Operations
```csharp
// Before (SkiaSharp 2.x)
using (var paint = new SKPaint { TextSize = 100 })
{
    SKPath textPath = paint.GetTextPath("Hello", 0, 0);
    canvas.DrawPath(textPath, paint);
}

// After (SkiaSharp 3.x)
using (var paint = new SKPaint())
using (var font = new SKFont { Size = 100 })
{
    SKPath textPath = font.GetTextPath("Hello", 0, 0);
    canvas.DrawPath(textPath, paint);
}
```

---

## Recommended Fix Order

1. ~~**CS8622** (Nullability) - ✅ DONE~~
2. ~~**CS0618** (Application.MainPage) - ✅ DONE~~
3. **CS0618** (Device.StartTimer) - Simple replacement in 15 files
4. **CS0618** (SKPaint text) - Larger refactor, ~40 files

---

## Temporary Suppression Option

If you want to defer the SKPaint text warnings, add to csproj:

```xml
<PropertyGroup>
    <!-- Suppress SKPaint text API warnings until full migration -->
    <NoWarn>$(NoWarn);CS0618</NoWarn>
</PropertyGroup>
```

---

## Commands to Verify Fixes

```bash
# Build and count warnings
cd samples/DocsSamplesApp
dotnet build -f net10.0-maccatalyst 2>&1 | grep -c "warning CS"

# Build with ALL warnings as errors (after all fixes)
dotnet build -f net10.0-maccatalyst /p:TreatWarningsAsErrors=true
```
