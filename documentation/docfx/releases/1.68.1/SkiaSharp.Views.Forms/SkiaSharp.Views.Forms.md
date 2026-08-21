# API diff: SkiaSharp.Views.Forms.dll

## SkiaSharp.Views.Forms.dll

### Namespace SkiaSharp.Views.Forms

#### Type Changed: SkiaSharp.Views.Forms.SKTouchAction

Added value:

```csharp
WheelChanged = 6,
```


#### Type Changed: SkiaSharp.Views.Forms.SKTouchEventArgs

Added constructor:

```csharp
public SKTouchEventArgs (long id, SKTouchAction type, SKMouseButton mouseButton, SKTouchDeviceType deviceType, SkiaSharp.SKPoint location, bool inContact, int wheelDelta);
```

Added property:

```csharp
public int WheelDelta { get; }
```



