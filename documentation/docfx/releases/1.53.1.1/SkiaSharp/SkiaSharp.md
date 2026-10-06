# API diff: SkiaSharp.dll

## SkiaSharp.dll

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.SKPaint

Added property:

```csharp
public SKPaintStyle Style { get; set; }
```


#### New Type: SkiaSharp.SKPaintStyle

```csharp
[Serializable]
public enum SKPaintStyle {
	Fill = 0,
	Stroke = 1,
	StrokeAndFill = 2,
}
```


