# API diff: SkiaSharp.Views.Forms.dll

## SkiaSharp.Views.Forms.dll

### Namespace SkiaSharp.Views.Forms

#### Type Changed: SkiaSharp.Views.Forms.Extensions

Added methods:

```csharp
public static Xamarin.Forms.Color ToFormsColor (this SkiaSharp.SKColorF color);
public static SkiaSharp.SKColorF ToSKColorF (this Xamarin.Forms.Color color);
```


#### Type Changed: SkiaSharp.Views.Forms.SKCanvasViewRenderer

Added method:

```csharp
protected override SkiaSharp.Views.WPF.SKElement CreateNativeControl ();
```


#### Type Changed: SkiaSharp.Views.Forms.SKGLViewRenderer

Added method:

```csharp
protected override SKHostedGLControl CreateNativeControl ();
```



