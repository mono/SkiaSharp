#if ENABLE_VORTICE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D;
using Vortice.Direct3D12;
using Vortice.DXGI;

namespace SkiaSharp.Direct3D.Tests;

public class VorticeDirect3DContext : Direct3DContext
{
	private readonly IDXGIFactory4 _factory;
	private readonly ID3D12Device2 _device;
	private readonly IDXGIAdapter1 _adapter;
	private ID3D12CommandQueue _queue;
	private bool _disposed;

	public VorticeDirect3DContext()
	{
		if (!D3D12.IsSupported(Vortice.Direct3D.FeatureLevel.Level_11_0))
			throw new NotSupportedException("Current platform doesn't support Direct3D 11.");

		var factory = DXGI.CreateDXGIFactory2<IDXGIFactory4>(true);

		ID3D12Device2 device = default;
		IDXGIAdapter1 adapter = default;
		using (var factory6 = factory.QueryInterfaceOrNull<IDXGIFactory6>())
		{
			if (factory6 is null)
			{
				for (var i = 0; factory6.EnumAdapterByGpuPreference(i, GpuPreference.HighPerformance, out adapter).Success; i++)
				{
					if (D3D12.D3D12CreateDevice(adapter, FeatureLevel.Level_11_0, out device).Success)
						break;
				}
			}
			else
			{
				for (var i = 0; factory.EnumAdapters1(i, out adapter).Success; i++)
				{
					if (D3D12.D3D12CreateDevice(adapter, FeatureLevel.Level_11_0, out device).Success)
						break;
				}
			}
		}

		_factory = factory;
		_device = device ?? throw new NotSupportedException("Current platform unable to create Direct3D device.");
		_adapter = adapter;
	}

	public IDXGIFactory4 Factory => _factory;

	public ID3D12Device2 Device => _device;

	public IDXGIAdapter1 Adapter => _adapter;

	public ID3D12CommandQueue Queue =>
		_queue ??= _device.CreateCommandQueue(new CommandQueueDescription
		{
			Flags = CommandQueueFlags.None,
			Type = CommandListType.Direct
		});

	public override GRD3DBackendContext CreateBackendContext() =>
		new GRVorticeD3DBackendContext
		{
			Adapter = Adapter,
			Device = Device,
			Queue = Queue,
		};

	public override void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;

		_queue?.Dispose();
		_device.Dispose();
		_adapter.Dispose();
		_factory.Dispose();
	}
}
#endif
