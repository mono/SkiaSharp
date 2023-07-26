# API diff: SkiaSharp.dll

## SkiaSharp.dll

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRContext

Added method:

```csharp
public void Flush (GRContextFlushBits flagsBitfield);
```


#### Type Changed: SkiaSharp.GRGlInterface

Obsoleted methods:

```diff
 [Obsolete ()]
 public static GRGlInterface CreateNativeInterface ();
```

Added methods:

```csharp
public static GRGlInterface AssembleAngleInterface (GRGlGetProcDelegate get);
public static GRGlInterface AssembleAngleInterface (object context, GRGlGetProcDelegate get);
public static GRGlInterface CreateNativeAngleInterface ();
public static GRGlInterface CreateNativeGlInterface ();
```


#### Type Changed: SkiaSharp.SKMatrix

Removed fields:

```csharp
public float Persp0;
public float Persp1;
public float Persp2;
public float ScaleX;
public float ScaleY;
public float SkewX;
public float SkewY;
public float TransX;
public float TransY;
```

Added properties:

```csharp
public float Persp0 { get; set; }
public float Persp1 { get; set; }
public float Persp2 { get; set; }
public float ScaleX { get; set; }
public float ScaleY { get; set; }
public float SkewX { get; set; }
public float SkewY { get; set; }
public float TransX { get; set; }
public float TransY { get; set; }
public float[] Values { get; set; }
```

Added method:

```csharp
public void GetValues (float[] values);
```


#### Type Changed: SkiaSharp.SKObject

Modified base type:

```diff
-System.Object
+SkiaSharp.SKNativeObject
```

Modified properties:

```diff
 public ---override--- IntPtr Handle { get; set; }
```

Removed methods:

```csharp
public virtual void Dispose ();
protected override void ~SKObject ();
```

Modified methods:

```diff
-protected virtual void Dispose (bool disposing)
+protected override void Dispose (bool disposing)
```


#### Type Changed: SkiaSharp.SKPath

Added properties:

```csharp
public SKRect Bounds { get; }
public SKPathConvexity Convexity { get; set; }
public bool IsConcave { get; }
public bool IsConvex { get; }
public SKRect TightBounds { get; }
```

Modified methods:

```diff
-public void AddCircle (float x, float y, float radius, SKPathDirection dir = 0)
+public void AddCircle (float x, float y, float radius, SKPathDirection dir)
-public void AddOval (SKRect rect, SKPathDirection direction = 0)
+public void AddOval (SKRect rect, SKPathDirection direction)
-public void AddRect (SKRect rect, SKPathDirection direction = 0)
+public void AddRect (SKRect rect, SKPathDirection direction)
-public void AddRoundedRect (SKRect rect, float rx, float ry, SKPathDirection dir = 0)
+public void AddRoundedRect (SKRect rect, float rx, float ry, SKPathDirection dir)
```

Added methods:

```csharp
public bool GetTightBounds (out SKRect result);
public bool Op (SKPath other, SKPathOp op, SKPath result);
public static SKPath ParseSvgPathData (string svgPath);
public bool Simplify (SKPath result);
public string ToSvgPathData ();
```

#### Type Changed: SkiaSharp.SKPath.Iterator

Modified base type:

```diff
-System.Object
+SkiaSharp.SKNativeObject
```

Removed methods:

```csharp
public virtual void Dispose ();
protected override void ~Iterator ();
```

Added method:

```csharp
protected override void Dispose (bool disposing);
```


#### Type Changed: SkiaSharp.SKPath.RawIterator

Modified base type:

```diff
-System.Object
+SkiaSharp.SKNativeObject
```

Removed methods:

```csharp
public virtual void Dispose ();
protected override void ~RawIterator ();
```

Added method:

```csharp
protected override void Dispose (bool disposing);
```


#### New Type: SkiaSharp.SKPath.OpBuilder

```csharp
public class OpBuilder : SkiaSharp.SKNativeObject, System.IDisposable {
	// constructors
	public SKPath.OpBuilder ();
	// methods
	public void Add (SKPath path, SKPathOp op);
	protected override void Dispose (bool disposing);
	public bool Resolve (SKPath result);
}
```


#### New Type: SkiaSharp.GRContextFlushBits

```csharp
[Serializable]
public enum GRContextFlushBits {
	Discard = 2,
	None = 0,
}
```

#### New Type: SkiaSharp.SKNativeObject

```csharp
public class SKNativeObject : System.IDisposable {
	// properties
	protected virtual IntPtr Handle { get; set; }
	// methods
	public virtual void Dispose ();
	protected virtual void Dispose (bool disposing);
	protected override void ~SKNativeObject ();
}
```

#### New Type: SkiaSharp.SKPathConvexity

```csharp
[Serializable]
public enum SKPathConvexity {
	Concave = 2,
	Convex = 1,
	Unknown = 0,
}
```

#### New Type: SkiaSharp.SKPathOp

```csharp
[Serializable]
public enum SKPathOp {
	Difference = 0,
	Intersect = 1,
	ReverseDifference = 4,
	Union = 2,
	Xor = 3,
}
```


