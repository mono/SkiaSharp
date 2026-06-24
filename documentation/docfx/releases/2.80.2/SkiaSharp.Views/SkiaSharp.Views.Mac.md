# API diff: SkiaSharp.Views.Mac.dll

## SkiaSharp.Views.Mac.dll

### Namespace SkiaSharp.Views.Mac

#### Type Changed: SkiaSharp.Views.Mac.AppleExtensions

Added methods:

```csharp
public static CoreGraphics.CGColor ToCGColor (this SkiaSharp.SKColorF color);
public static CoreImage.CIColor ToCIColor (this SkiaSharp.SKColorF color);
public static SkiaSharp.SKColorF ToSKColorF (this CoreGraphics.CGColor color);
public static SkiaSharp.SKColorF ToSKColorF (this CoreImage.CIColor color);
```


#### Type Changed: SkiaSharp.Views.Mac.MacExtensions

Added methods:

```csharp
public static AppKit.NSColor ToNSColor (this SkiaSharp.SKColorF color);
public static SkiaSharp.SKColorF ToSKColorF (this AppKit.NSColor color);
```



