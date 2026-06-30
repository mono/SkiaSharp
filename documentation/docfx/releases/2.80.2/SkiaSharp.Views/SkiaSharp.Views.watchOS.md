# API diff: SkiaSharp.Views.watchOS.dll

## SkiaSharp.Views.watchOS.dll

### Namespace SkiaSharp.Views.watchOS

#### Type Changed: SkiaSharp.Views.watchOS.AppleExtensions

Added methods:

```csharp
public static CoreGraphics.CGColor ToCGColor (this SkiaSharp.SKColorF color);
public static SkiaSharp.SKColorF ToSKColorF (this CoreGraphics.CGColor color);
```


#### Type Changed: SkiaSharp.Views.watchOS.iOSExtensions

Added methods:

```csharp
public static SkiaSharp.SKColorF ToSKColorF (this UIKit.UIColor color);
public static UIKit.UIColor ToUIColor (this SkiaSharp.SKColorF color);
```



