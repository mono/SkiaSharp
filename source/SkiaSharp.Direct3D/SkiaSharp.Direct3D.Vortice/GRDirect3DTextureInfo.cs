using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D12;
using Vortice.DXGI;

namespace SkiaSharp
{
	public class GRDirect3DTextureInfo : GRD3DTextureResourceInfo
	{
		private ID3D12Resource? _resource;

		public new ID3D12Resource? Resource
		{
			get => _resource;
			set
			{
				_resource = value;
				base.Resource = value?.NativePointer ?? default;
			}
		}

		public new ResourceStates ResourceState
		{
			get => (ResourceStates)base.ResourceState;
			set => base.ResourceState = (uint)value;
		}

		public new Format Format
		{
			get => (Format)base.Format;
			set => base.Format = (uint)value;
		}
	}
}
