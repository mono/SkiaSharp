# API diff: SkiaSharp.Direct3D.Vortice.dll

## SkiaSharp.Direct3D.Vortice.dll

> Assembly Version Changed: 3.119.0.0 vs 0.0.0.0

### New Namespace SkiaSharp

#### New Type: SkiaSharp.GRVorticeD3DBackendContext

```csharp
public class GRVorticeD3DBackendContext : SkiaSharp.GRD3DBackendContext {
	// constructors
	public GRVorticeD3DBackendContext ();
	// properties
	public Vortice.DXGI.IDXGIAdapter1 Adapter { get; set; }
	public Vortice.Direct3D12.ID3D12Device2 Device { get; set; }
	public Vortice.Direct3D12.ID3D12CommandQueue Queue { get; set; }
}
```

#### New Type: SkiaSharp.GRVorticeD3DTextureResourceInfo

```csharp
public class GRVorticeD3DTextureResourceInfo : SkiaSharp.GRD3DTextureResourceInfo {
	// constructors
	public GRVorticeD3DTextureResourceInfo ();
	// properties
	public Vortice.DXGI.Format Format { get; set; }
	public Vortice.Direct3D12.ID3D12Resource Resource { get; set; }
	public Vortice.Direct3D12.ResourceStates ResourceState { get; set; }
}
```

