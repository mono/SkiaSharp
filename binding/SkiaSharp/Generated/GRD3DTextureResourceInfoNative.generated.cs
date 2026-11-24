using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// gr_d3d_textureresourceinfo_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct GRD3DTextureResourceInfoNative : IEquatable<GRD3DTextureResourceInfoNative> {
		// public d3d_d12_resource_t* fResource
		public d3d_d12_resource_t fResource;

		// public d3d_alloc_t* fAlloc
		public d3d_alloc_t fAlloc;

		// public uint32_t fResourceState
		public UInt32 fResourceState;

		// public uint32_t fFormat
		public UInt32 fFormat;

		// public uint32_t fSampleCount
		public UInt32 fSampleCount;

		// public uint32_t fLevelCount
		public UInt32 fLevelCount;

		// public unsigned int fSampleQualityPattern
		public UInt32 fSampleQualityPattern;

		// public bool fProtected
		public Byte fProtected;

		public readonly bool Equals (GRD3DTextureResourceInfoNative obj) =>
#pragma warning disable CS8909
			fResource == obj.fResource && fAlloc == obj.fAlloc && fResourceState == obj.fResourceState && fFormat == obj.fFormat && fSampleCount == obj.fSampleCount && fLevelCount == obj.fLevelCount && fSampleQualityPattern == obj.fSampleQualityPattern && fProtected == obj.fProtected;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is GRD3DTextureResourceInfoNative f && Equals (f);

		public static bool operator == (GRD3DTextureResourceInfoNative left, GRD3DTextureResourceInfoNative right) =>
			left.Equals (right);

		public static bool operator != (GRD3DTextureResourceInfoNative left, GRD3DTextureResourceInfoNative right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fResource);
			hash.Add (fAlloc);
			hash.Add (fResourceState);
			hash.Add (fFormat);
			hash.Add (fSampleCount);
			hash.Add (fLevelCount);
			hash.Add (fSampleQualityPattern);
			hash.Add (fProtected);
			return hash.ToHashCode ();
		}

	}
}
