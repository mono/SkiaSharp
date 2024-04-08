using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D12;
using Vortice.DXGI;

namespace SkiaSharp
{
	public class GRDirect3DTextureInfo
	{
		public ID3D12Resource Resource { get; set; }
		public ResourceStates ResourceState { get; set; }
		public Format Format { get; set; }
		public uint SampleCount { get; set; }
		public uint LevelCount { get; set; }
		public uint SampleQualityPattern { get; set; }
		public bool Protected { get; set; }

		public static implicit operator GRD3dTextureinfo(GRDirect3DTextureInfo textureInfo)
		{
			return new GRD3dTextureinfo
			{
				Resource = textureInfo.Resource.NativePointer,
				ResourceState = (uint)textureInfo.ResourceState,
				Format = (uint)textureInfo.Format,
				SampleCount = textureInfo.SampleCount,
				LevelCount = textureInfo.LevelCount,
				SampleQualityPattern = textureInfo.SampleQualityPattern,
				Protected = textureInfo.Protected
			};
		}
	}
}
