# API diff: SkiaSharp.Views.tvOS.dll

## SkiaSharp.Views.tvOS.dll

### Namespace SkiaSharp.Views.tvOS

#### Type Changed: SkiaSharp.Views.tvOS.AppleExtensions

Added methods:

```csharp
public static CoreGraphics.CGColor ToCGColor (this SkiaSharp.SKColorF color);
public static CoreImage.CIColor ToCIColor (this SkiaSharp.SKColorF color);
public static SkiaSharp.SKColorF ToSKColorF (this CoreGraphics.CGColor color);
public static SkiaSharp.SKColorF ToSKColorF (this CoreImage.CIColor color);
```


#### Type Changed: SkiaSharp.Views.tvOS.iOSExtensions

Added methods:

```csharp
public static SkiaSharp.SKColorF ToSKColorF (this UIKit.UIColor color);
public static UIKit.UIColor ToUIColor (this SkiaSharp.SKColorF color);
```



