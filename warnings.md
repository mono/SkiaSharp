# DocsSamplesApp Warnings Inventory

This document catalogs all compiler warnings in the DocsSamplesApp project after the migration to .NET 10 and SkiaSharp 3.119.1.

---

## Summary

| Category | Files | Instances | Difficulty | Notes |
|----------|-------|-----------|------------|-------|
| CS8622 - Event handler nullability | 97 | 107 | Easy | Change `object sender` to `object? sender` |
| CS0618 - SKPaint text APIs obsolete | 40 | ~165 | Medium | Requires SKFont usage patterns |
| CS0618 - Device.StartTimer obsolete | 16 | 22 | Easy | Use `Dispatcher.StartTimer()` |
| CS0618 - Application.MainPage obsolete | 1 | 1 | Easy | Override `CreateWindow` |
| CS0612 - Device class obsolete | 11 | 11 | Easy | Same as StartTimer fix |
| CS8625 - Null literal to non-nullable | 1 | 2 | Easy | Fix specific null assignments |
| CS8602 - Possible null dereference | 1 | 1 | Easy | Add null check |

---

## 1. CS8622: Event Handler Nullability Mismatch (97 files)

**Description**: `'void Page.OnCanvasViewPaintSurface(object sender, ...)' doesn't match delegate 'EventHandler<...>'`

**Fix Pattern**:
```csharp
// Before
void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)

// After  
void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
```

Also applies to:
- `OnTouch(object sender, SKTouchEventArgs args)` → `OnTouch(object? sender, SKTouchEventArgs args)`
- `OnSliderValueChanged(object sender, ValueChangedEventArgs args)` → `OnSliderValueChanged(object? sender, ...)`
- `OnPickerSelectedIndexChanged(object sender, EventArgs args)` → `OnPickerSelectedIndexChanged(object? sender, ...)`
- `OnStepperValueChanged(object sender, ValueChangedEventArgs args)` → `OnStepperValueChanged(object? sender, ...)`

**Affected Files**:
- Basics/BasicBitmapsPage.cs
- Basics/CodeMoreCodePage.cs
- Basics/ExpandingCirclesPage.cs
- Basics/FramedTextPage.cs
- Basics/OutlinedTextPage.cs
- Basics/SimpleCirclePage.cs
- Basics/SurfaceSizePage.cs
- Bitmaps/FillRectanglePage.cs
- Bitmaps/GradientBitmapPage.cs
- Bitmaps/HelloBitmapPage.cs
- Bitmaps/LatticeDisplayPage.cs
- Bitmaps/LatticeNinePatchPage.cs
- Bitmaps/MonkeyMoustachePage.cs
- Bitmaps/NinePatchDisplayPage.cs
- Bitmaps/PhotoCropperCanvasView.cs
- Bitmaps/PhotoCroppingPage.xaml.cs
- Bitmaps/PixelDimensionsPage.cs
- Bitmaps/PixelizedImagePage.cs
- Bitmaps/PosterizePage.cs
- Bitmaps/RainbowSinePage.cs
- Bitmaps/UniformScalingPage.cs
- Curves/AnimatedDottedTextPage.cs
- Curves/AnotherRoundedHeptagonPage.cs
- Curves/ArcInfinityPage.cs
- Curves/BezierInfinityPage.cs
- Curves/CatsInFramePage.cs
- Curves/CharacterOutlineOutlinesPage.cs
- Curves/CircularTextPage.cs
- Curves/ClipOperationsPage.cs
- Curves/ClippingTextPage.cs
- Curves/ConveyorBeltPage.cs
- Curves/DashedHatchLinesPage.cs
- Curves/DotDashMorphPage.cs
- Curves/ExplodedPieChartPage.cs
- Curves/FourCircleIntersectClipPage.cs
- Curves/FourLeafCloverPage.cs
- Curves/GlobularTextPage.cs
- Curves/HatchFillPage.cs
- Curves/JitterTextPage.cs
- Curves/LinkedChainPage.cs
- Curves/MonkeyThroughKeyholePage.cs
- Curves/PathDataCatPage.cs
- Curves/PathDataHelloPage.cs
- Curves/PathTileFillPage.cs
- Curves/PrettyAnalogClockPage.cs
- Curves/RegionOperationsPage.cs
- Curves/RegionPaintPage.cs
- Curves/RoundedHeptagonPage.cs
- Curves/SquaringTheCirclePage.cs
- Curves/TextPathEffectPage.cs
- Curves/UnicycleHalfPipePage.cs
- DocsSamplesApp.csproj
- Effects/AlgorithmicBrickWallPage.cs
- Effects/AnimatedBitmapTilePage.cs
- Effects/BlueBananaPage.cs
- Effects/BlurryReflectionPage.cs
- Effects/CenteredTilesPage.cs
- Effects/ChainLinkFencePage.cs
- Effects/CompositingMaskPage.cs
- Effects/ConicalSpecularHighlightPage.cs
- Effects/CornerToCornerGradientPage.cs
- Effects/GradientAnimationPage.cs
- Effects/GradientTextPage.cs
- Effects/GrayScaleMatrixPage.cs
- Effects/InfinityColorsPage.cs
- Effects/PastelMatrixPage.cs
- Effects/PhotographicBrickWallPage.cs
- Effects/PosterizeTablePage.cs
- Effects/PrimaryColorsPage.cs
- Effects/RadialGradientMaskPage.cs
- Effects/RadialSpecularHighlightPage.cs
- Effects/RainbowArcGradientPage.cs
- Effects/RainbowGradientPage.cs
- Effects/ReflectionGradientPage.cs
- Effects/StoneWallPage.cs
- Effects/SweepGradientPage.cs
- Effects/TileAlignmentPage.cs
- Paths/AnimatedSpiralPage.cs
- Paths/ArchimedeanSpiralPage.cs
- Paths/OverlappingCirclesPage.cs
- Paths/StrokeCapsPage.cs
- Paths/StrokeJoinsPage.cs
- Paths/TwoTriangleContoursPage.cs
- Transforms/AccumulatedTranslatePage.cs
- Transforms/AnimatedRotation3DPage.cs
- Transforms/AnisotropicScalingPage.cs
- Transforms/AnisotropicTextPage.cs
- Transforms/HendecagramAnimationPage.cs
- Transforms/HendecagramArrayPage.cs
- Transforms/IsotropicScalingPage.cs
- Transforms/ObliqueTextPage.cs
- Transforms/PathTransformPage.cs
- Transforms/RotateAndRevolvePage.cs
- Transforms/RotatedTextPage.cs
- Transforms/SkewShadowTextPage.cs
- Transforms/TranslateTextEffectsPage.cs
- Transforms/UglyAnalogClockPage.cs

---

## 2. CS0618: Obsolete SKPaint Text APIs (40 files, ~165 instances)

**Description**: Text-related properties on SKPaint are obsolete in SkiaSharp 3.x. Use SKFont instead.

### Obsolete APIs and Replacements

| Obsolete API | Replacement |
|--------------|-------------|
| `SKPaint.TextSize` | `SKFont.Size` |
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

**Affected Files**:
- Basics/BasicBitmapsPage.cs
- Basics/CodeMoreCodePage.cs
- Basics/FramedTextPage.cs
- Basics/OutlinedTextPage.cs
- Basics/SurfaceSizePage.cs
- Bitmaps/GradientBitmapPage.cs
- Bitmaps/HelloBitmapPage.cs
- Curves/AnimatedDottedTextPage.cs
- Curves/CharacterOutlineOutlinesPage.cs
- Curves/CircularTextPage.cs
- Curves/ClipOperationsPage.cs
- Curves/ClippingTextPage.cs
- Curves/GlobularTextPage.cs
- Curves/JitterTextPage.cs
- Curves/PathLengthPage.xaml.cs
- Curves/RegionOperationsPage.cs
- Curves/TextPathEffectPage.cs
- DocsSamplesApp.csproj
- Effects/BlurryReflectionPage.cs
- Effects/DistanceLightExperimentPage.xaml.cs
- Effects/DropShadowExperimentPage.xaml.cs
- Effects/GradientTextPage.cs
- Effects/ImageBlurExperimentPage.xaml.cs
- Effects/MaskBlurExperimentPage.xaml.cs
- Effects/ReflectionGradientPage.cs
- MatrixDisplay.cs
- Paths/StrokeCapsPage.cs
- Paths/StrokeJoinsPage.cs
- Transforms/AnimatedRotation3DPage.cs
- Transforms/AnisotropicTextPage.cs
- Transforms/BasicRotatePage.xaml.cs
- Transforms/BasicScalePage.xaml.cs
- Transforms/CenteredRotatePage.xaml.cs
- Transforms/CenteredScalePage.xaml.cs
- Transforms/ObliqueTextPage.cs
- Transforms/RotatedTextPage.cs
- Transforms/SkewAngleExperimentPage.xaml.cs
- Transforms/SkewExperimentPage.xaml.cs
- Transforms/SkewShadowTextPage.cs
- Transforms/TranslateTextEffectsPage.cs

---

## 3. CS0618/CS0612: Device.StartTimer Obsolete (16 files)

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
- DocsSamplesApp.csproj
- Effects/AnimatedBitmapTilePage.cs
- Effects/GradientAnimationPage.cs
- Effects/InfinityColorsPage.cs
- Paths/AnimatedSpiralPage.cs
- Transforms/HendecagramAnimationPage.cs
- Transforms/UglyAnalogClockPage.cs

---

## 4. CS0618: Application.MainPage Obsolete (1 file)

**Description**: Setting `MainPage` directly is obsolete in .NET MAUI.

**Fix Pattern**:
```csharp
// Before
public App()
{
    MainPage = new NavigationPage(new MainPage());
}

// After - Option 1: Override CreateWindow
public App()
{
    // Empty or InitializeComponent only
}

protected override Window CreateWindow(IActivationState? activationState)
{
    return new Window(new NavigationPage(new MainPage()));
}

// After - Option 2: Continue using MainPage (suppress warning)
#pragma warning disable CS0618
    MainPage = new NavigationPage(new MainPage());
#pragma warning restore CS0618
```

**File**: `App.xaml.cs`

---

## 5. CS8625: Null Literal to Non-Nullable Type (1 file, 2 instances)

**Description**: `Cannot convert null literal to non-nullable reference type`

**Fix**: Make the receiving type nullable or use `null!` if you're sure null is intentional.

**File**: `BitmapExtensions.cs` (lines 59, 94)

```csharp
// The issue is likely returning null where the return type is non-nullable
// Fix by making return type nullable: SKBitmap? instead of SKBitmap
```

---

## 6. CS8602: Possible Null Dereference (1 file)

**Description**: `Dereference of a possibly null reference`

**Fix**: Add null check or use null-conditional operator.

**File**: `Basics/TapToggleFillPage.xaml.cs` (line 25)

---

## Recommended Fix Order

### Phase 1: Quick Fixes (< 30 minutes)
1. **CS0618** (Device.StartTimer) - Find/replace `Device.StartTimer` → `Dispatcher.StartTimer` in 16 files
2. **CS0618** (Application.MainPage) - Single file fix in App.xaml.cs  
3. **CS8625/CS8602** (Null issues) - 3 specific manual fixes

### Phase 2: Nullability (1-2 hours)
4. **CS8622** (Nullability) - Change `object sender` to `object? sender` in 97 files
   - Can use find/replace: `object sender,` → `object? sender,`

### Phase 3: SKPaint Text Migration (4-8 hours)
5. **CS0618** (SKPaint text) - Larger refactor affecting 40 files
   - Each file needs SKFont introduced alongside SKPaint
   - Text measuring, sizing, and drawing all need updates
   - Consider doing this in batches or suppressing temporarily

---

## Temporary Suppression Option

If you want to defer the SKPaint text warnings, add to csproj:

```xml
<PropertyGroup>
    <!-- Suppress SKPaint text API warnings until full migration -->
    <NoWarn>$(NoWarn);CS0618</NoWarn>
</PropertyGroup>
```

Or use more targeted pragma in individual files:

```csharp
#pragma warning disable CS0618 // SKPaint text APIs
// ... code using obsolete text APIs ...
#pragma warning restore CS0618
```

---

## Documentation Impact

After fixing these warnings, update the corresponding documentation:

| Warning Category | Documentation to Update |
|-----------------|------------------------|
| Device.StartTimer | docs/docs/basics/animation.md |
| SKPaint text APIs | docs/docs/basics/text.md, plus any page showing text rendering |
| Application.MainPage | docs/docs/install.md (if applicable) |

---

## Commands to Verify Fixes

After making fixes, verify with:

```bash
# Build with nullable warnings as errors
cd samples/DocsSamplesApp
dotnet build -f net10.0-maccatalyst /p:WarningsAsErrors="nullable"

# Build with ALL warnings as errors (after all fixes)
dotnet build -f net10.0-maccatalyst /p:TreatWarningsAsErrors=true
```
