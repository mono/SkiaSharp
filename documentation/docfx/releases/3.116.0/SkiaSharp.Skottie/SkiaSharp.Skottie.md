# API diff: SkiaSharp.Skottie.dll

## SkiaSharp.Skottie.dll

> Assembly Version Changed: 3.116.0.0 vs 2.88.0.0

### Namespace SkiaSharp.Skottie

#### Type Changed: SkiaSharp.Skottie.Animation

Added method:

```csharp
public static AnimationBuilder CreateBuilder (AnimationBuilderFlags flags);
```


#### New Type: SkiaSharp.Skottie.AnimationBuilder

```csharp
public sealed class AnimationBuilder : SkiaSharp.SKObject, SkiaSharp.ISKSkipObjectRegistration {
	// properties
	public AnimationBuilderStats Stats { get; }
	// methods
	public Animation Build (SkiaSharp.SKData data);
	public Animation Build (SkiaSharp.SKStream stream);
	public Animation Build (System.IO.Stream stream);
	public Animation Build (string path);
	protected override void DisposeNative ();
	public AnimationBuilder SetFontManager (SkiaSharp.SKFontManager fontManager);
	public AnimationBuilder SetResourceProvider (SkiaSharp.Resources.ResourceProvider resourceProvider);
}
```

#### New Type: SkiaSharp.Skottie.AnimationBuilderFlags

```csharp
[Serializable]
public enum AnimationBuilderFlags {
	DeferImageLoading = 1,
	None = 0,
	PreferEmbeddedFonts = 2,
}
```

#### New Type: SkiaSharp.Skottie.AnimationBuilderStats

```csharp
public struct AnimationBuilderStats, System.IEquatable<AnimationBuilderStats> {
	// properties
	public int AnimatorCount { get; }
	public System.TimeSpan JsonParseTime { get; }
	public int JsonSize { get; }
	public System.TimeSpan SceneParseTime { get; }
	public System.TimeSpan TotalLoadTime { get; }
	// methods
	public virtual bool Equals (AnimationBuilderStats obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static bool op_Equality (AnimationBuilderStats left, AnimationBuilderStats right);
	public static bool op_Inequality (AnimationBuilderStats left, AnimationBuilderStats right);
}
```


