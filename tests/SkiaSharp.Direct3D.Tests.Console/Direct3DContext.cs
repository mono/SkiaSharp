using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D;
using Vortice.Direct3D12;
using Vortice.DXGI;

namespace SkiaSharp.Direct3D.Tests
{
	public class Direct3DContext : IDisposable
	{
		private readonly IDXGIFactory4 _factory;
		private readonly ID3D12Device2 _device;
		private readonly IDXGIAdapter1 _adapter;
		private ID3D12CommandQueue _queue;
		private bool _disposed;

		public Direct3DContext()
		{
			if (!D3D12.IsSupported(Vortice.Direct3D.FeatureLevel.Level_11_0))
				throw new NotSupportedException("Current platform doesn't support Direct3D 11.");

			var factory = DXGI.CreateDXGIFactory2<IDXGIFactory4>(true);

			ID3D12Device2 device = default;
			IDXGIAdapter1 adapter = null;
			using (IDXGIFactory6 factory6 = factory.QueryInterfaceOrNull<IDXGIFactory6>())
			{
				if (factory6 != null)
				{
					for (int adapterIndex = 0; factory6.EnumAdapterByGpuPreference(adapterIndex, GpuPreference.HighPerformance, out adapter).Success; adapterIndex++)
					{
						AdapterDescription1 desc = adapter!.Description1;
						if (D3D12.D3D12CreateDevice(adapter, FeatureLevel.Level_11_0, out device).Success)
							break;
					}
				}
				else
				{
					for (int adapterIndex = 0;
						factory.EnumAdapters1(adapterIndex, out adapter).Success;
						adapterIndex++)
					{
						AdapterDescription1 desc = adapter.Description1;
						if (D3D12.D3D12CreateDevice(adapter, FeatureLevel.Level_11_0, out device).Success)
							break;
					}
				}
			}

			if (device == null)
				throw new NotSupportedException("Current platform unable to create Direct3D device.");

			_factory = factory;
			_device = device;
			_adapter = adapter;
		}

		public IDXGIFactory4 Factory => _factory;

		public ID3D12Device2 Device => _device;

		public IDXGIAdapter1 Adapter => _adapter;

		public ID3D12CommandQueue Queue
		{
			get
			{
				if (_queue == null)
				{
					_queue = _device.CreateCommandQueue(new CommandQueueDescription
					{
						Flags = CommandQueueFlags.None,
						Type = CommandListType.Direct
					});
				}
				return _queue;
			}
		}

		public void Dispose()
		{
			if (_disposed)
				return;
			_disposed = true;
			if (_queue != null)
			{
				_queue.Dispose();
				_queue = null;
			}
			_device.Dispose();
			_adapter.Dispose();
			_factory.Dispose();
		}
	}
}
