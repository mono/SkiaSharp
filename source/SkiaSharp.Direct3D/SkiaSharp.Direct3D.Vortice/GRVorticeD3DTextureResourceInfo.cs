using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D12;
using Vortice.DXGI;

namespace SkiaSharp
{
	/// <summary>Represents information about a Direct3D 12 texture resource using Vortice types.</summary>
	/// <remarks />
	public class GRVorticeD3DTextureResourceInfo : GRD3DTextureResourceInfo
	{
		private ID3D12Resource? _resource;

		/// <summary>Gets or sets the Vortice Direct3D 12 resource.</summary>
		/// <value>The Vortice Direct3D 12 resource, or <see langword="null" />.</value>
		/// <remarks />
		public new ID3D12Resource? Resource
		{
			get => _resource;
			set
			{
				_resource = value;
				base.Resource = value?.NativePointer ?? default;
			}
		}

		/// <summary>Gets or sets the current state of the resource.</summary>
		/// <value>The resource state flags indicating how the resource is currently being used.</value>
		/// <remarks />
		public new ResourceStates ResourceState
		{
			get => (ResourceStates)base.ResourceState;
			set => base.ResourceState = (uint)value;
		}

		/// <summary>Gets or sets the DXGI format of the texture resource.</summary>
		/// <value>The DXGI format of the texture.</value>
		/// <remarks />
		public new Format Format
		{
			get => (Format)base.Format;
			set => base.Format = (uint)value;
		}
	}
}
