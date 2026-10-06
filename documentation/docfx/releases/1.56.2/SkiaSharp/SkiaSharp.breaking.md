# API diff: SkiaSharp.dll

## SkiaSharp.dll

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.SKBitmap

Removed method:

```csharp
public SKColor GetIndex8Color (int x, int y);
```


#### Type Changed: SkiaSharp.SKColorTable

Modified properties:

```diff
-public SKColor[] Colors { get; }
+public SKPMColor[] Colors { get; }
-public SKColor this [int index] { get; }
+public SKPMColor this [int index] { get; }
```


#### Type Changed: SkiaSharp.SKImageInfo

Modified fields:

```diff
-public readonly SKImageInfo Empty;
+public SKImageInfo Empty;
-public readonly SKColorType PlatformColorType;
+public SKColorType PlatformColorType;
```


#### Type Changed: SkiaSharp.SKPath

Modified methods:

```diff
 public void AddPath (SKPath other, SKPath.AddMode mode--- = 0---)
 public void AddPath (SKPath other, ref SKMatrix matrix, SKPath.AddMode mode--- = 0---)
 public void AddPath (SKPath other, float dx, float dy, SKPath.AddMode mode--- = 0---)
```

#### Type Changed: SkiaSharp.SKPath.Iterator

Removed method:

```csharp
public SKPath.Verb Next (SKPoint[] points, bool doConsumeDegenerates, bool exact);
```


#### Type Changed: SkiaSharp.SKPath.RawIterator

Removed methods:

```csharp
public SKPath.Verb Next (SKPoint[] points);
public SKPath.Verb Peek ();
```




