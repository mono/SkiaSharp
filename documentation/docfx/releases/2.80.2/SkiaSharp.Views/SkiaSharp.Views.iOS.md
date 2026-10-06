# API diff: SkiaSharp.Views.iOS.dll

## SkiaSharp.Views.iOS.dll

### Namespace SkiaSharp.Views.iOS

#### Type Changed: SkiaSharp.Views.iOS.AppleExtensions

Added methods:

```csharp
public static CoreGraphics.CGColor ToCGColor (this SkiaSharp.SKColorF color);
public static CoreImage.CIColor ToCIColor (this SkiaSharp.SKColorF color);
public static SkiaSharp.SKColorF ToSKColorF (this CoreGraphics.CGColor color);
public static SkiaSharp.SKColorF ToSKColorF (this CoreImage.CIColor color);
```


#### Type Changed: SkiaSharp.Views.iOS.iOSExtensions

Added methods:

```csharp
public static SkiaSharp.SKColorF ToSKColorF (this UIKit.UIColor color);
public static UIKit.UIColor ToUIColor (this SkiaSharp.SKColorF color);
```



