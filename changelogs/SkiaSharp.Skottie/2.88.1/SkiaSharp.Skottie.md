# API diff: SkiaSharp.Skottie.dll

## SkiaSharp.Skottie.dll

> Assembly Version Changed: 2.88.0.0 vs 0.0.0.0

### New Namespace SkiaSharp.Skottie

#### New Type: SkiaSharp.Skottie.Animation

```csharp
public class Animation : SkiaSharp.SKObject, SkiaSharp.ISKNonVirtualReferenceCounted, SkiaSharp.ISKReferenceCounted, SkiaSharp.ISKSkipObjectRegistration {
	// properties
	public System.TimeSpan Duration { get; }
	public double Fps { get; }
	public double InPoint { get; }
	public double OutPoint { get; }
	public SkiaSharp.SKSize Size { get; }
	public string Version { get; }
	// methods
	public static Animation Create (SkiaSharp.SKData data);
	public static Animation Create (SkiaSharp.SKStream stream);
	public static Animation Create (System.IO.Stream stream);
	public static Animation Create (string path);
	protected override void DisposeNative ();
	public static Animation Parse (string json);
	public void Render (SkiaSharp.SKCanvas canvas, SkiaSharp.SKRect dst);
	public void Render (SkiaSharp.SKCanvas canvas, SkiaSharp.SKRect dst, AnimationRenderFlags flags);
	public void Seek (double percent, SkiaSharp.SceneGraph.InvalidationController ic);
	public void SeekFrame (double frame, SkiaSharp.SceneGraph.InvalidationController ic);
	public void SeekFrameTime (double seconds, SkiaSharp.SceneGraph.InvalidationController ic);
	public void SeekFrameTime (System.TimeSpan time, SkiaSharp.SceneGraph.InvalidationController ic);
	public static bool TryCreate (SkiaSharp.SKData data, out Animation animation);
	public static bool TryCreate (SkiaSharp.SKStream stream, out Animation animation);
	public static bool TryCreate (System.IO.Stream stream, out Animation animation);
	public static bool TryCreate (string path, out Animation animation);
	public static bool TryParse (string json, out Animation animation);
}
```

#### New Type: SkiaSharp.Skottie.AnimationRenderFlags

```csharp
[Serializable]
[Flags]
public enum AnimationRenderFlags {
	DisableTopLevelClipping = 2,
	SkipTopLevelIsolation = 1,
}
```

