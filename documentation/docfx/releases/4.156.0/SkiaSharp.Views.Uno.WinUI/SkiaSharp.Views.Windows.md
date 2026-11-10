# API diff: SkiaSharp.Views.Windows.dll

## SkiaSharp.Views.Windows.dll

> Assembly Version Changed: 4.156.0.0 vs 4.155.0.0

### Namespace SkiaSharp.Views.Windows

#### Type Changed: SkiaSharp.Views.Windows.SKSwapChainPanel

Modified base type:

```diff
-Microsoft.UI.Xaml.FrameworkElement
+Microsoft.UI.Xaml.Controls.Grid
```

Removed property:

```csharp
public static bool RaiseOnUnsupported { get; set; }
```
