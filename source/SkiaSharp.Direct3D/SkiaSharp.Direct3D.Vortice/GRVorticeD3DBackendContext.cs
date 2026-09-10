using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D12;
using Vortice.DXGI;

namespace SkiaSharp
{
	/// <summary>Represents a Direct3D 12 backend context using Vortice types for creating a GPU-backed <see cref="T:SkiaSharp.GRContext" />.</summary>
	/// <remarks />
	public class GRVorticeD3DBackendContext : GRD3DBackendContext
	{
		private IDXGIAdapter1? _adapter;
		private ID3D12Device2? _device;
		private ID3D12CommandQueue? _queue;

		/// <summary>Gets or sets the Vortice DXGI adapter.</summary>
		/// <value>The Vortice DXGI adapter, or <see langword="null" />.</value>
		/// <remarks />
		public new IDXGIAdapter1? Adapter
		{
			get => _adapter;
			set
			{
				_adapter = value;
				base.Adapter = value?.NativePointer ?? default;
			}
		}

		/// <summary>Gets or sets the Vortice Direct3D 12 device.</summary>
		/// <value>The Vortice Direct3D 12 device, or <see langword="null" />.</value>
		/// <remarks />
		public new ID3D12Device2? Device
		{
			get => _device;
			set
			{
				_device = value;
				base.Device = value?.NativePointer ?? default;
			}
		}

		/// <summary>Gets or sets the Vortice Direct3D 12 command queue.</summary>
		/// <value>The Vortice Direct3D 12 command queue, or <see langword="null" />.</value>
		/// <remarks />
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
