using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_graphite_vk_texture_info_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKGraphiteVkTextureInfo : IEquatable<SKGraphiteVkTextureInfo> {
		// public int32_t fSampleCount
		private Int32 fSampleCount;
		public Int32 SampleCount {
			readonly get => fSampleCount;
			set => fSampleCount = value;
		}

		// public bool fMipmapped
		private Byte fMipmapped;
		public bool Mipmapped {
			readonly get => fMipmapped > 0;
			set => fMipmapped = value ? (byte)1 : (byte)0;
		}

		// public uint32_t fFlags
		private UInt32 fFlags;
		public UInt32 Flags {
			readonly get => fFlags;
			set => fFlags = value;
		}

		// public int32_t fFormat
		private Int32 fFormat;
		public Int32 Format {
			readonly get => fFormat;
			set => fFormat = value;
		}

		// public int32_t fImageTiling
		private Int32 fImageTiling;
		public Int32 ImageTiling {
			readonly get => fImageTiling;
			set => fImageTiling = value;
		}

		// public uint32_t fImageUsageFlags
		private UInt32 fImageUsageFlags;
		public UInt32 ImageUsageFlags {
			readonly get => fImageUsageFlags;
			set => fImageUsageFlags = value;
		}

		// public int32_t fSharingMode
		private Int32 fSharingMode;
		public Int32 SharingMode {
			readonly get => fSharingMode;
			set => fSharingMode = value;
		}

		// public uint32_t fAspectMask
		private UInt32 fAspectMask;
		public UInt32 AspectMask {
			readonly get => fAspectMask;
			set => fAspectMask = value;
		}

		public readonly bool Equals (SKGraphiteVkTextureInfo obj) =>
#pragma warning disable CS8909
			fSampleCount == obj.fSampleCount && fMipmapped == obj.fMipmapped && fFlags == obj.fFlags && fFormat == obj.fFormat && fImageTiling == obj.fImageTiling && fImageUsageFlags == obj.fImageUsageFlags && fSharingMode == obj.fSharingMode && fAspectMask == obj.fAspectMask;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKGraphiteVkTextureInfo f && Equals (f);

		public static bool operator == (SKGraphiteVkTextureInfo left, SKGraphiteVkTextureInfo right) =>
			left.Equals (right);

		public static bool operator != (SKGraphiteVkTextureInfo left, SKGraphiteVkTextureInfo right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fSampleCount);
			hash.Add (fMipmapped);
			hash.Add (fFlags);
			hash.Add (fFormat);
			hash.Add (fImageTiling);
			hash.Add (fImageUsageFlags);
			hash.Add (fSharingMode);
			hash.Add (fAspectMask);
			return hash.ToHashCode ();
		}

	}
}
