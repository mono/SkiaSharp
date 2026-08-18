# API diff: SkiaSharp.dll

## SkiaSharp.dll

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.SKCanvas

Added constructor:

```csharp
public SKCanvas (SKBitmap bitmap);
```


#### Type Changed: SkiaSharp.SKColor

Added methods:

```csharp
public static uint op_Explicit (SKColor color);
public static SKColor op_Implicit (uint color);
```


#### Type Changed: SkiaSharp.SKPaint

Added properties:

```csharp
public bool DeviceKerningEnabled { get; set; }
public bool FakeBoldText { get; set; }
public SKPaintHinting HintingLevel { get; set; }
public bool IsAutohinted { get; set; }
public bool IsEmbeddedBitmapText { get; set; }
public bool IsLinearText { get; set; }
public bool LcdRenderText { get; set; }
public bool StrikeThruText { get; set; }
public bool SubpixelText { get; set; }
public bool UnderlineText { get; set; }
```


#### Type Changed: SkiaSharp.SKPath

Added properties:

```csharp
public SKPoint Item { get; }
public int PointCount { get; }
public SKPoint[] Points { get; }
```

Added methods:

```csharp
public SKPoint GetPoint (int index);
public SKPoint[] GetPoints (int max);
public int GetPoints (SKPoint[] points, int max);
```


#### New Type: SkiaSharp.SKPaintHinting

```csharp
[Serializable]
public enum SKPaintHinting {
	Full = 3,
	NoHinting = 0,
	Normal = 2,
	Slight = 1,
}
```


