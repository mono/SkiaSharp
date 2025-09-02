# API diff: SkiaSharp.Views.Mac.dll

## SkiaSharp.Views.Mac.dll

> Assembly Version Changed: 2.88.0.0 vs 2.80.0.0

### Namespace SkiaSharp.Views.Mac

#### Type Changed: SkiaSharp.Views.Mac.SKCanvasLayer

Added interfaces:

```csharp
CoreAnimation.ICAMediaTiming
Foundation.INSCoding
Foundation.INSObjectProtocol
Foundation.INSSecureCoding
ObjCRuntime.INativeObject
System.IDisposable
System.IEquatable<Foundation.NSObject>
```


#### Type Changed: SkiaSharp.Views.Mac.SKCanvasView

Added interfaces:

```csharp
AppKit.INSAccessibility
AppKit.INSAccessibilityElementProtocol
AppKit.INSAppearanceCustomization
AppKit.INSDraggingDestination
AppKit.INSTouchBarProvider
AppKit.INSUserActivityRestoring
AppKit.INSUserInterfaceItemIdentification
Foundation.INSCoding
Foundation.INSObjectProtocol
ObjCRuntime.INativeObject
System.IDisposable
System.IEquatable<Foundation.NSObject>
```


#### Type Changed: SkiaSharp.Views.Mac.SKGLLayer

Added interfaces:

```csharp
CoreAnimation.ICAMediaTiming
Foundation.INSCoding
Foundation.INSObjectProtocol
Foundation.INSSecureCoding
ObjCRuntime.INativeObject
System.IDisposable
System.IEquatable<Foundation.NSObject>
```


#### Type Changed: SkiaSharp.Views.Mac.SKGLView

Added interfaces:

```csharp
AppKit.INSAccessibility
AppKit.INSAccessibilityElementProtocol
AppKit.INSAppearanceCustomization
AppKit.INSDraggingDestination
AppKit.INSTouchBarProvider
AppKit.INSUserActivityRestoring
AppKit.INSUserInterfaceItemIdentification
Foundation.INSCoding
Foundation.INSObjectProtocol
ObjCRuntime.INativeObject
System.IDisposable
System.IEquatable<Foundation.NSObject>
```


#### Type Changed: SkiaSharp.Views.Mac.SKMetalView

Added interfaces:

```csharp
AppKit.INSAccessibility
AppKit.INSAccessibilityElementProtocol
AppKit.INSAppearanceCustomization
AppKit.INSDraggingDestination
AppKit.INSTouchBarProvider
AppKit.INSUserActivityRestoring
AppKit.INSUserInterfaceItemIdentification
CoreAnimation.ICALayerDelegate
Foundation.INSCoding
Foundation.INSObjectProtocol
System.IEquatable<Foundation.NSObject>
```


#### Type Changed: SkiaSharp.Views.Mac.SKPaintGLSurfaceEventArgs

Obsoleted constructors:

```diff
 [Obsolete ()]
 public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKColorType colorType, SkiaSharp.GRGlFramebufferInfo glInfo);
```

Added constructors:

```csharp
public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKImageInfo info);
public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKImageInfo info, SkiaSharp.SKImageInfo rawInfo);
```

Added properties:

```csharp
public SkiaSharp.SKImageInfo Info { get; }
public SkiaSharp.SKImageInfo RawInfo { get; }
```


#### Type Changed: SkiaSharp.Views.Mac.SKPaintSurfaceEventArgs

Added constructor:

```csharp
public SKPaintSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info, SkiaSharp.SKImageInfo rawInfo);
```

Added property:

```csharp
public SkiaSharp.SKImageInfo RawInfo { get; }
```



