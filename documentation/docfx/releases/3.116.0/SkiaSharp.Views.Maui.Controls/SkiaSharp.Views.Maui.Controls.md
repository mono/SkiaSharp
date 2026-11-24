# API diff: SkiaSharp.Views.Maui.Controls.dll

## SkiaSharp.Views.Maui.Controls.dll

> Assembly Version Changed: 3.116.0.0 vs 2.88.0.0

### Namespace SkiaSharp.Views.Maui.Controls

#### Type Changed: SkiaSharp.Views.Maui.Controls.SKCanvasView

Removed interface:

```csharp
ISKCanvasViewController
```

Removed method:

```csharp
protected override Microsoft.Maui.SizeRequest OnMeasure (double widthConstraint, double heightConstraint);
```


#### Type Changed: SkiaSharp.Views.Maui.Controls.SKGLView

Removed interface:

```csharp
ISKGLViewController
```

Added interface:

```csharp
SkiaSharp.Views.Maui.ISKGLView
```

Added field:

```csharp
public static Microsoft.Maui.Controls.BindableProperty IgnorePixelScalingProperty;
```

Added property:

```csharp
public override bool IgnorePixelScaling { get; set; }
```

Removed method:

```csharp
protected override Microsoft.Maui.SizeRequest OnMeasure (double widthConstraint, double heightConstraint);
```


#### Removed Type SkiaSharp.Views.Maui.Controls.ISKCanvasViewController
#### Removed Type SkiaSharp.Views.Maui.Controls.ISKGLViewController

