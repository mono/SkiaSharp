# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 1.56.0.0 vs 1.55.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.SKImageFilter

Removed method:

```csharp
public static SKImageFilter CreateMerge (SKImageFilter[] filters, SKXferMode[] modes, SKImageFilter.CropRect cropRect);
```


#### Type Changed: SkiaSharp.SKPictureRecorder

Modified methods:

```diff
-public SKCanvas BeginRecording (SKRect rect)
+public SKCanvas BeginRecording (SKRect cullRect)
```



