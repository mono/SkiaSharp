# API diff: SkiaSharp.Views.Blazor.dll

## SkiaSharp.Views.Blazor.dll

### Namespace SkiaSharp.Views.Blazor

#### Type Changed: SkiaSharp.Views.Blazor._Imports

Modified base type:

```diff
-Microsoft.AspNetCore.Components.ComponentBase
+System.Object
```

Removed interfaces:

```csharp
Microsoft.AspNetCore.Components.IComponent
Microsoft.AspNetCore.Components.IHandleAfterRender
Microsoft.AspNetCore.Components.IHandleEvent
```

Removed method:

```csharp
protected override void BuildRenderTree (Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder);
```

Added method:

```csharp
protected void Execute ();
```



