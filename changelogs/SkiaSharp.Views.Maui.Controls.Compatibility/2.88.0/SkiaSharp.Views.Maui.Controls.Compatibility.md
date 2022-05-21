# API diff: SkiaSharp.Views.Maui.Controls.Compatibility.dll

## SkiaSharp.Views.Maui.Controls.Compatibility.dll

> Assembly Version Changed: 2.88.0.0 vs 0.0.0.0

### New Namespace SkiaSharp.Views.Maui.Controls.Compatibility

#### New Type: SkiaSharp.Views.Maui.Controls.Compatibility.AppHostBuilderExtensions

```csharp
public static class AppHostBuilderExtensions {
	// methods

	[Obsolete]
public static Microsoft.Maui.Hosting.MauiAppBuilder UseSkiaSharpCompatibilityRenderers (this Microsoft.Maui.Hosting.MauiAppBuilder builder);
}
```

### New Namespace SkiaSharp.Views.Maui.Controls.Hosting

#### New Type: SkiaSharp.Views.Maui.Controls.Hosting.AppHostBuilderExtensions

```csharp
public static class AppHostBuilderExtensions {
	// methods
	public static Microsoft.Maui.Hosting.MauiAppBuilder UseSkiaSharp (this Microsoft.Maui.Hosting.MauiAppBuilder builder, bool registerRenderers, bool replaceHandlers);
}
```

