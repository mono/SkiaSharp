using System;
using System.Collections.Generic;
using System.Text;

namespace SkiaSharp
{
	public class GRD3DTextureResourceInfo
	{
		public nint Resource { get; set; }
		public uint ResourceState { get; set; }
		public uint Format { get; set; }
		public uint SampleCount { get; set; }
		public uint LevelCount { get; set; }
		public uint SampleQualityPattern { get; set; }
		public bool Protected { get; set; }

		internal GrD3DTextureResourceInfoNative ToNative ()
		{
			return new GrD3DTextureResourceInfoNative {
				fResource = Resource,
				fResourceState = ResourceState,
				fFormat = Format,
				fSampleCount = SampleCount,
				fLevelCount = LevelCount,
				fSampleQualityPattern = SampleQualityPattern,
				fProtected = Protected ? (byte)1 : (byte)0
			};
		}
	}
}
