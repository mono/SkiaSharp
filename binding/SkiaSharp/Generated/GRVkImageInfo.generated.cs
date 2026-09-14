using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// gr_vk_imageinfo_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct GRVkImageInfo : IEquatable<GRVkImageInfo> {
		// public uint64_t fImage
		private UInt64 fImage;
		public UInt64 Image {
			readonly get => fImage;
			set => fImage = value;
		}

		// public gr_vk_alloc_t fAlloc
		private GRVkAlloc fAlloc;
		public GRVkAlloc Alloc {
			readonly get => fAlloc;
			set => fAlloc = value;
		}

		// public uint32_t fImageTiling
		private UInt32 fImageTiling;
		public UInt32 ImageTiling {
			readonly get => fImageTiling;
			set => fImageTiling = value;
		}

		// public uint32_t fImageLayout
		private UInt32 fImageLayout;
		public UInt32 ImageLayout {
			readonly get => fImageLayout;
			set => fImageLayout = value;
		}

		// public uint32_t fFormat
		private UInt32 fFormat;
		public UInt32 Format {
			readonly get => fFormat;
			set => fFormat = value;
		}

		// public uint32_t fImageUsageFlags
		private UInt32 fImageUsageFlags;
		public UInt32 ImageUsageFlags {
			readonly get => fImageUsageFlags;
			set => fImageUsageFlags = value;
		}

		// public uint32_t fSampleCount
		private UInt32 fSampleCount;
		public UInt32 SampleCount {
			readonly get => fSampleCount;
			set => fSampleCount = value;
		}

		// public uint32_t fLevelCount
		private UInt32 fLevelCount;
		public UInt32 LevelCount {
			readonly get => fLevelCount;
			set => fLevelCount = value;
		}

		// public uint32_t fCurrentQueueFamily
		private UInt32 fCurrentQueueFamily;
		public UInt32 CurrentQueueFamily {
			readonly get => fCurrentQueueFamily;
			set => fCurrentQueueFamily = value;
		}

		// public bool fProtected
		private Byte fProtected;
		public bool Protected {
			readonly get => fProtected > 0;
			set => fProtected = value ? (byte)1 : (byte)0;
		}

		// public gr_vk_ycbcrconversioninfo_t fYcbcrConversionInfo
		private GRVkYcbcrConversionInfo fYcbcrConversionInfo;
		public GRVkYcbcrConversionInfo YcbcrConversionInfo {
			readonly get => fYcbcrConversionInfo;
			set => fYcbcrConversionInfo = value;
		}

		// public uint32_t fSharingMode
		private UInt32 fSharingMode;
		public UInt32 SharingMode {
			readonly get => fSharingMode;
			set => fSharingMode = value;
		}

		public readonly bool Equals (GRVkImageInfo obj) =>
#pragma warning disable CS8909
			fImage == obj.fImage && fAlloc == obj.fAlloc && fImageTiling == obj.fImageTiling && fImageLayout == obj.fImageLayout && fFormat == obj.fFormat && fImageUsageFlags == obj.fImageUsageFlags && fSampleCount == obj.fSampleCount && fLevelCount == obj.fLevelCount && fCurrentQueueFamily == obj.fCurrentQueueFamily && fProtected == obj.fProtected && fYcbcrConversionInfo == obj.fYcbcrConversionInfo && fSharingMode == obj.fSharingMode;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is GRVkImageInfo f && Equals (f);

		public static bool operator == (GRVkImageInfo left, GRVkImageInfo right) =>
			left.Equals (right);

		public static bool operator != (GRVkImageInfo left, GRVkImageInfo right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fImage);
			hash.Add (fAlloc);
			hash.Add (fImageTiling);
			hash.Add (fImageLayout);
			hash.Add (fFormat);
			hash.Add (fImageUsageFlags);
			hash.Add (fSampleCount);
			hash.Add (fLevelCount);
			hash.Add (fCurrentQueueFamily);
			hash.Add (fProtected);
			hash.Add (fYcbcrConversionInfo);
			hash.Add (fSharingMode);
			return hash.ToHashCode ();
		}

	}
}
