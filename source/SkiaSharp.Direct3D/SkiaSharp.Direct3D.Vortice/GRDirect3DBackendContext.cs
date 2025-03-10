using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D12;
using Vortice.DXGI;

namespace SkiaSharp
{
	public class GRDirect3DBackendContext : GRD3DBackendContext
	{
		private IDXGIAdapter1? _adapter;
		private ID3D12Device2? _device;
		private ID3D12CommandQueue? _queue;

		public new IDXGIAdapter1? Adapter
		{
			get => _adapter;
			set
			{
				_adapter = value;
				base.Adapter = value?.NativePointer ?? default;
			}
		}

		public new ID3D12Device2? Device
		{
			get => _device;
			set
			{
				_device = value;
				base.Device = value?.NativePointer ?? default;
			}
		}

		public new ID3D12CommandQueue? Queue
		{
			get => _queue;
			set
			{
				_queue = value;
				base.Queue = value?.NativePointer ?? default;
			}
		}
	}
}
