# API diff: SkiaSharp.Views.Forms.dll

## SkiaSharp.Views.Forms.dll

> Assembly Version Changed: 2.88.0.0 vs 2.80.0.0

### Namespace SkiaSharp.Views.Forms

#### Type Changed: SkiaSharp.Views.Forms.SKCanvasViewRenderer

Removed constructor:

```csharp
[Obsolete ("This constructor is obsolete as of version 2.5. Please use SKCanvasViewRenderer(Context) instead.")]
public SKCanvasViewRenderer ();
```


#### Type Changed: SkiaSharp.Views.Forms.SKCanvasViewRendererBase`2

Removed constructor:

```csharp
[Obsolete ("This constructor is obsolete as of version 2.5. Please use SKCanvasViewRendererBase(Context) instead.")]
protected SKCanvasViewRendererBase`2 ();
```


#### Type Changed: SkiaSharp.Views.Forms.SKGLViewRenderer

Removed constructor:

```csharp
[Obsolete ("This constructor is obsolete as of version 2.5. Please use SKGLViewRenderer(Context) instead.")]
public SKGLViewRenderer ();
```


#### Type Changed: SkiaSharp.Views.Forms.SKGLViewRendererBase`2

Removed constructor:

```csharp
[Obsolete ("This constructor is obsolete as of version 2.5. Please use SKGLViewRendererBase(Context) instead.")]
protected SKGLViewRendererBase`2 ();
```


#### Type Changed: SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs

Added constructor:

```csharp
public SKPaintSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info, SkiaSharp.SKImageInfo rawInfo);
```

Added property:

```csharp
public SkiaSharp.SKImageInfo RawInfo { get; }
```



