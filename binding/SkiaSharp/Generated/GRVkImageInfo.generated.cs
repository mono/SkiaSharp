using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// gr_vk_imageinfo_t
	/// <summary>Represents information about a Vulkan image for use with Skia's GPU backend.</summary>
	/// <remarks />
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct GRVkImageInfo : IEquatable<GRVkImageInfo> {
		// public uint64_t fImage
		private UInt64 fImage;
		/// <summary>Gets or sets the Vulkan image handle.</summary>
		/// <value>The VkImage handle as an unsigned 64-bit integer.</value>
		/// <remarks />
		public UInt64 Image {
			readonly get => fImage;
			set => fImage = value;
		}

		// public gr_vk_alloc_t fAlloc
		private GRVkAlloc fAlloc;
		/// <summary>Gets or sets the memory allocation information for the Vulkan image.</summary>
		/// <value>The <see cref="T:SkiaSharp.GRVkAlloc" /> containing memory allocation details.</value>
		/// <remarks />
		public GRVkAlloc Alloc {
			readonly get => fAlloc;
			set => fAlloc = value;
		}

		// public uint32_t fImageTiling
		private UInt32 fImageTiling;
		/// <summary>Gets or sets the tiling arrangement of the Vulkan image.</summary>
		/// <value>The VkImageTiling value as an unsigned integer.</value>
		/// <remarks />
		public UInt32 ImageTiling {
			readonly get => fImageTiling;
			set => fImageTiling = value;
		}

		// public uint32_t fImageLayout
		private UInt32 fImageLayout;
		/// <summary>Gets or sets the current layout of the Vulkan image.</summary>
		/// <value>The VkImageLayout value as an unsigned integer.</value>
		/// <remarks />
		public UInt32 ImageLayout {
			readonly get => fImageLayout;
			set => fImageLayout = value;
		}

		// public uint32_t fFormat
		private UInt32 fFormat;
		/// <summary>Gets or sets the Vulkan format of the image.</summary>
		/// <value>The VkFormat value as an unsigned integer.</value>
		/// <remarks />
		public UInt32 Format {
			readonly get => fFormat;
			set => fFormat = value;
		}

		// public uint32_t fImageUsageFlags
		private UInt32 fImageUsageFlags;
		/// <summary>Gets or sets the usage flags for the Vulkan image.</summary>
		/// <value>The VkImageUsageFlags value as an unsigned integer.</value>
		/// <remarks />
		public UInt32 ImageUsageFlags {
			readonly get => fImageUsageFlags;
			set => fImageUsageFlags = value;
		}

		// public uint32_t fSampleCount
		private UInt32 fSampleCount;
		/// <summary>Gets or sets the number of samples per pixel for the Vulkan image.</summary>
		/// <value>The sample count.</value>
		/// <remarks />
		public UInt32 SampleCount {
			readonly get => fSampleCount;
			set => fSampleCount = value;
		}

		// public uint32_t fLevelCount
		private UInt32 fLevelCount;
		/// <summary>Gets or sets the number of mipmap levels in the Vulkan image.</summary>
		/// <value>The number of mipmap levels.</value>
		/// <remarks />
		public UInt32 LevelCount {
			readonly get => fLevelCount;
			set => fLevelCount = value;
		}

		// public uint32_t fCurrentQueueFamily
		private UInt32 fCurrentQueueFamily;
		/// <summary>Gets or sets the current queue family index that owns the image.</summary>
		/// <value>The queue family index.</value>
		/// <remarks />
		public UInt32 CurrentQueueFamily {
			readonly get => fCurrentQueueFamily;
			set => fCurrentQueueFamily = value;
		}

		// public bool fProtected
		private Byte fProtected;
		/// <summary>Gets or sets a value indicating whether the Vulkan image is protected.</summary>
		/// <value><see langword="true" /> if the image is protected; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool Protected {
			readonly get => fProtected > 0;
			set => fProtected = value ? (byte)1 : (byte)0;
		}

		// public gr_vk_ycbcrconversioninfo_t fYcbcrConversionInfo
		private GRVkYcbcrConversionInfo fYcbcrConversionInfo;
		/// <summary>Gets or sets the YCbCr conversion information for the Vulkan image.</summary>
		/// <value>The <see cref="T:SkiaSharp.GRVkYcbcrConversionInfo" /> containing YCbCr conversion details.</value>
		/// <remarks />
		public GRVkYcbcrConversionInfo YcbcrConversionInfo {
			readonly get => fYcbcrConversionInfo;
			set => fYcbcrConversionInfo = value;
		}

		// public uint32_t fSharingMode
		private UInt32 fSharingMode;
		/// <summary>Gets or sets the sharing mode of the Vulkan image across queue families.</summary>
		/// <value>The VkSharingMode value as an unsigned integer.</value>
		/// <remarks />
		public UInt32 SharingMode {
			readonly get => fSharingMode;
			set => fSharingMode = value;
		}

		/// <summary>Determines whether the specified <see cref="T:SkiaSharp.GRVkImageInfo" /> is equal to the current instance.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.GRVkImageInfo" /> to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (GRVkImageInfo obj) =>
#pragma warning disable CS8909
			fImage == obj.fImage && fAlloc == obj.fAlloc && fImageTiling == obj.fImageTiling && fImageLayout == obj.fImageLayout && fFormat == obj.fFormat && fImageUsageFlags == obj.fImageUsageFlags && fSampleCount == obj.fSampleCount && fLevelCount == obj.fLevelCount && fCurrentQueueFamily == obj.fCurrentQueueFamily && fProtected == obj.fProtected && fYcbcrConversionInfo == obj.fYcbcrConversionInfo && fSharingMode == obj.fSharingMode;
#pragma warning restore CS8909

		/// <summary>Determines whether the specified object is equal to the current instance.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is GRVkImageInfo f && Equals (f);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.GRVkImageInfo" /> instances are equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.GRVkImageInfo" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.GRVkImageInfo" /> to compare.</param>
		/// <returns><see langword="true" /> if the two instances are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (GRVkImageInfo left, GRVkImageInfo right) =>
			left.Equals (right);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.GRVkImageInfo" /> instances are not equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.GRVkImageInfo" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.GRVkImageInfo" /> to compare.</param>
		/// <returns><see langword="true" /> if the two instances are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (GRVkImageInfo left, GRVkImageInfo right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		/// <remarks />
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
