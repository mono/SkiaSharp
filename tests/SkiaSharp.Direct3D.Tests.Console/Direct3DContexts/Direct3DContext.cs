using System;

namespace SkiaSharp.Direct3D.Tests;

public abstract class Direct3DContext : IDisposable
{
	public abstract GRD3DBackendContext CreateBackendContext();

	public abstract void Dispose();
}
