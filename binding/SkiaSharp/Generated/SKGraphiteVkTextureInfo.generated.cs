using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_graphite_vk_texture_info_t
	/// <summary>Describes the Vulkan-specific format and usage of a texture for use with Graphite.</summary>
	/// <remarks />
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKGraphiteVkTextureInfo : IEquatable<SKGraphiteVkTextureInfo> {
		// public int32_t fSampleCount
		private Int32 fSampleCount;
		/// <summary>Gets or sets the number of samples per pixel.</summary>
		/// <value>The sample count.</value>
		/// <remarks />
		public Int32 SampleCount {
			readonly get => fSampleCount;
			set => fSampleCount = value;
		}

		// public bool fMipmapped
		private Byte fMipmapped;
		/// <summary>Gets or sets a value indicating whether the texture has mipmaps.</summary>
		/// <value>
		///           <see langword="true" /> if the texture has mipmaps; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool Mipmapped {
			readonly get => fMipmapped > 0;
			set => fMipmapped = value ? (byte)1 : (byte)0;
		}

		// public uint32_t fFlags
		private UInt32 fFlags;
		/// <summary>Gets or sets the Vulkan image creation flags.</summary>
		/// <value>A bitwise combination of Vulkan <c>VkImageCreateFlags</c> values.</value>
		/// <remarks />
		public UInt32 Flags {
			readonly get => fFlags;
			set => fFlags = value;
		}

		// public int32_t fFormat
		private Int32 fFormat;
		/// <summary>Gets or sets the Vulkan image format.</summary>
		/// <value>The Vulkan <c>VkFormat</c> value.</value>
		/// <remarks />
		public Int32 Format {
			readonly get => fFormat;
			set => fFormat = value;
		}

		// public int32_t fImageTiling
		private Int32 fImageTiling;
		/// <summary>Gets or sets the Vulkan image tiling arrangement.</summary>
		/// <value>The Vulkan <c>VkImageTiling</c> value.</value>
		/// <remarks />
		public Int32 ImageTiling {
			readonly get => fImageTiling;
			set => fImageTiling = value;
		}

		// public uint32_t fImageUsageFlags
		private UInt32 fImageUsageFlags;
		/// <summary>Gets or sets the Vulkan image usage flags.</summary>
		/// <value>A bitwise combination of Vulkan <c>VkImageUsageFlags</c> values.</value>
		/// <remarks />
		public UInt32 ImageUsageFlags {
			readonly get => fImageUsageFlags;
			set => fImageUsageFlags = value;
		}

		// public int32_t fSharingMode
		private Int32 fSharingMode;
		/// <summary>Gets or sets the Vulkan image sharing mode.</summary>
		/// <value>The Vulkan <c>VkSharingMode</c> value.</value>
		/// <remarks />
		public Int32 SharingMode {
			readonly get => fSharingMode;
			set => fSharingMode = value;
		}

		// public uint32_t fAspectMask
		private UInt32 fAspectMask;
		/// <summary>Gets or sets the Vulkan image aspect mask.</summary>
		/// <value>A bitwise combination of Vulkan <c>VkImageAspectFlags</c> values.</value>
		/// <remarks />
		public UInt32 AspectMask {
			readonly get => fAspectMask;
			set => fAspectMask = value;
		}

		/// <param name="obj">The Vulkan texture info to compare with the current Vulkan texture info.</param>
		/// <summary>Determines whether the specified Vulkan texture info is equal to the current Vulkan texture info.</summary>
		/// <returns>
		///           <see langword="true" /> if the two values are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKGraphiteVkTextureInfo obj) =>
#pragma warning disable CS8909
			fSampleCount == obj.fSampleCount && fMipmapped == obj.fMipmapped && fFlags == obj.fFlags && fFormat == obj.fFormat && fImageTiling == obj.fImageTiling && fImageUsageFlags == obj.fImageUsageFlags && fSharingMode == obj.fSharingMode && fAspectMask == obj.fAspectMask;
#pragma warning restore CS8909

		/// <param name="obj">The object to compare with the current Vulkan texture info.</param>
		/// <summary>Determines whether the specified object is equal to the current Vulkan texture info.</summary>
		/// <returns>
		///           <see langword="true" /> if the specified object is equal to the current value; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKGraphiteVkTextureInfo f && Equals (f);

		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <summary>Indicates whether two Vulkan texture info values are equal.</summary>
		/// <returns>
		///           <see langword="true" /> if the two values are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKGraphiteVkTextureInfo left, SKGraphiteVkTextureInfo right) =>
			left.Equals (right);

		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <summary>Indicates whether two Vulkan texture info values are not equal.</summary>
		/// <returns>
		///           <see langword="true" /> if the two values are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKGraphiteVkTextureInfo left, SKGraphiteVkTextureInfo right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this Vulkan texture info.</summary>
		/// <returns>A hash code for the current value.</returns>
		/// <remarks />
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
