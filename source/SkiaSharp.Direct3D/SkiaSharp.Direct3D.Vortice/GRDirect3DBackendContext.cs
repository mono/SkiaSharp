using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D12;
using Vortice.DXGI;

namespace SkiaSharp
{
	public class GRDirect3DBackendContext
	{
		public IDXGIAdapter1 Adapter { get; set; }

		public ID3D12Device2 Device { get; set; }

		public ID3D12CommandQueue Queue { get; set; }

		public bool ProtectedContext { get; set; }

		public static implicit operator GRD3dBackendcontext(GRDirect3DBackendContext context)
		{
			return new GRD3dBackendcontext
			{
				Adapter = context.Adapter.NativePointer,
				Device = context.Device.NativePointer,
				Queue = context.Queue.NativePointer,
				ProtectedContext = context.ProtectedContext
			};
		}
	}
}
