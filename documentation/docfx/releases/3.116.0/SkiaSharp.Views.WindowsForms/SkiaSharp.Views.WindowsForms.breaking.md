# API diff: SkiaSharp.Views.WindowsForms.dll

## SkiaSharp.Views.WindowsForms.dll

> Assembly Version Changed: 3.116.0.0 vs 2.88.0.0

### Namespace SkiaSharp.Views.Desktop

#### Type Changed: SkiaSharp.Views.Desktop.SKGLControl

Modified base type:

```diff
-OpenTK.GLControl
+OpenTK.GLControl.GLControl
```

Removed constructors:

```csharp
public SKGLControl (OpenTK.Graphics.GraphicsMode mode);
public SKGLControl (OpenTK.Graphics.GraphicsMode mode, int major, int minor, OpenTK.Graphics.GraphicsContextFlags flags);
```



