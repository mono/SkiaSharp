using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// gr_vk_ycbcrconversioninfo_t
	/// <summary>Describes the parameters for Vulkan YCbCr sampler conversion.</summary>
	/// <remarks><![CDATA[
	/// ## Remarks
	///
	/// `GRVkYcbcrConversionInfo` provides the Vulkan-specific parameters required to create a `VkSamplerYcbcrConversion` object. It is used when creating GPU-backed surfaces or images that use a YCbCr color model, such as Android hardware buffers with YUV formats.
	///
	/// The properties map directly to fields in `VkSamplerYcbcrConversionCreateInfo`.
	///
	/// ## Examples
	///
	/// Creating a basic YCbCr conversion info for a Vulkan YUV420 image:
	///
	/// ```csharp
	/// var conversionInfo = new GRVkYcbcrConversionInfo
	/// {
	///     Format = 1000156003, // VK_FORMAT_G8_B8R8_2PLANE_420_UNORM
	///     ExternalFormat = 0,
	///     YcbcrModel = 1,      // VK_SAMPLER_YCBCR_MODEL_CONVERSION_YCBCR_709
	///     YcbcrRange = 0,      // VK_SAMPLER_YCBCR_RANGE_ITU_FULL
	///     ChromaFilter = 1,    // VK_FILTER_LINEAR
	///     XChromaOffset = 0,
	///     YChromaOffset = 0,
	///     ForceExplicitReconstruction = 0,
	/// };
	/// ```
	/// ]]></remarks>
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct GRVkYcbcrConversionInfo : IEquatable<GRVkYcbcrConversionInfo> {
		// public uint32_t fFormat
		private UInt32 fFormat;
		/// <summary>Gets or sets the Vulkan image format for this YCbCr conversion.</summary>
		/// <value>A raw <c>VkFormat</c> value identifying the Vulkan image format used by this conversion.</value>
		/// <remarks></remarks>
		public UInt32 Format {
			readonly get => fFormat;
			set => fFormat = value;
		}

		// public uint64_t fExternalFormat
		private UInt64 fExternalFormat;
		/// <summary>Gets or sets the Vulkan external format for this YCbCr conversion.</summary>
		/// <value>The Vulkan external format identifier, or <c>0</c> if no external format is used.</value>
		/// <remarks></remarks>
		public UInt64 ExternalFormat {
			readonly get => fExternalFormat;
			set => fExternalFormat = value;
		}

		// public uint32_t fYcbcrModel
		private UInt32 fYcbcrModel;
		/// <summary>Gets or sets the YCbCr color model conversion.</summary>
		/// <value>A raw <c>VkSamplerYcbcrModelConversion</c> value identifying the YCbCr color model.</value>
		/// <remarks></remarks>
		public UInt32 YcbcrModel {
			readonly get => fYcbcrModel;
			set => fYcbcrModel = value;
		}

		// public uint32_t fYcbcrRange
		private UInt32 fYcbcrRange;
		/// <summary>Gets or sets the quantization range of the encoded YCbCr values.</summary>
		/// <value>A raw <c>VkSamplerYcbcrRange</c> value indicating whether the data uses full or narrow/ITU range.</value>
		/// <remarks></remarks>
		public UInt32 YcbcrRange {
			readonly get => fYcbcrRange;
			set => fYcbcrRange = value;
		}

		// public uint32_t fXChromaOffset
		private UInt32 fXChromaOffset;
		/// <summary>Gets or sets the horizontal chroma sample location offset.</summary>
		/// <value>A raw <c>VkChromaLocation</c> value for the horizontal chroma sample position.</value>
		/// <remarks></remarks>
		public UInt32 XChromaOffset {
			readonly get => fXChromaOffset;
			set => fXChromaOffset = value;
		}

		// public uint32_t fYChromaOffset
		private UInt32 fYChromaOffset;
		/// <summary>Gets or sets the vertical chroma sample location offset.</summary>
		/// <value>A raw <c>VkChromaLocation</c> value for the vertical chroma sample position.</value>
		/// <remarks></remarks>
		public UInt32 YChromaOffset {
			readonly get => fYChromaOffset;
			set => fYChromaOffset = value;
		}

		// public uint32_t fChromaFilter
		private UInt32 fChromaFilter;
		/// <summary>Gets or sets the chroma filter used when reconstructing YCbCr chroma values.</summary>
		/// <value>A raw <c>VkFilter</c> value specifying the filter applied when reconstructing chroma.</value>
		/// <remarks></remarks>
		public UInt32 ChromaFilter {
			readonly get => fChromaFilter;
			set => fChromaFilter = value;
		}

		// public uint32_t fForceExplicitReconstruction
		private UInt32 fForceExplicitReconstruction;
		/// <summary>Gets or sets whether explicit chroma reconstruction is required.</summary>
		/// <value><c>1</c> to require explicit chroma reconstruction; <c>0</c> to allow implicit reconstruction.</value>
		/// <remarks></remarks>
		public UInt32 ForceExplicitReconstruction {
			readonly get => fForceExplicitReconstruction;
			set => fForceExplicitReconstruction = value;
		}

		// public gr_vk_ycbcr_components_t fComponents
		private GRVkYcbcrComponents fComponents;
		/// <summary>Gets or sets the component mapping for this YCbCr conversion.</summary>
		/// <value>A <see cref="T:SkiaSharp.GRVkYcbcrComponents" /> describing the channel swizzle mapping for this conversion.</value>
		/// <remarks></remarks>
		public GRVkYcbcrComponents Components {
			readonly get => fComponents;
			set => fComponents = value;
		}

		// public bool fSamplerFilterMustMatchChromaFilter
		private Byte fSamplerFilterMustMatchChromaFilter;
		/// <summary>Gets or sets whether the sampler filter must match the chroma reconstruction filter.</summary>
		/// <value><see langword="true" /> if the sampler filter must match the chroma filter; otherwise, <see langword="false" />.</value>
		/// <remarks></remarks>
		public bool SamplerFilterMustMatchChromaFilter {
			readonly get => fSamplerFilterMustMatchChromaFilter > 0;
			set => fSamplerFilterMustMatchChromaFilter = value ? (byte)1 : (byte)0;
		}

		// public bool fSupportsLinearFilter
		private Byte fSupportsLinearFilter;
		/// <summary>Gets or sets whether linear filtering is supported for this YCbCr conversion.</summary>
		/// <value><see langword="true" /> if linear filtering is supported for this YCbCr conversion; otherwise, <see langword="false" />.</value>
		/// <remarks></remarks>
		public bool SupportsLinearFilter {
			readonly get => fSupportsLinearFilter > 0;
			set => fSupportsLinearFilter = value ? (byte)1 : (byte)0;
		}

		/// <summary>Indicates whether this conversion info is equal to another <see cref="T:SkiaSharp.GRVkYcbcrConversionInfo" />.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.GRVkYcbcrConversionInfo" /> to compare with this instance.</param>
		/// <returns><see langword="true" /> if both instances have identical conversion parameters; otherwise, <see langword="false" />.</returns>
		/// <remarks></remarks>
		public readonly bool Equals (GRVkYcbcrConversionInfo obj) =>
#pragma warning disable CS8909
			fFormat == obj.fFormat && fExternalFormat == obj.fExternalFormat && fYcbcrModel == obj.fYcbcrModel && fYcbcrRange == obj.fYcbcrRange && fXChromaOffset == obj.fXChromaOffset && fYChromaOffset == obj.fYChromaOffset && fChromaFilter == obj.fChromaFilter && fForceExplicitReconstruction == obj.fForceExplicitReconstruction && fComponents == obj.fComponents && fSamplerFilterMustMatchChromaFilter == obj.fSamplerFilterMustMatchChromaFilter && fSupportsLinearFilter == obj.fSupportsLinearFilter;
#pragma warning restore CS8909

		/// <summary>Indicates whether this conversion info is equal to the specified object.</summary>
		/// <param name="obj">The object to compare with this instance.</param>
		/// <returns><see langword="true" /> if <paramref name="obj" /> is a <see cref="T:SkiaSharp.GRVkYcbcrConversionInfo" /> with identical conversion parameters; otherwise, <see langword="false" />.</returns>
		/// <remarks></remarks>
		public readonly override bool Equals (object obj) =>
			obj is GRVkYcbcrConversionInfo f && Equals (f);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.GRVkYcbcrConversionInfo" /> values are equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.GRVkYcbcrConversionInfo" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.GRVkYcbcrConversionInfo" /> to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks></remarks>
		public static bool operator == (GRVkYcbcrConversionInfo left, GRVkYcbcrConversionInfo right) =>
			left.Equals (right);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.GRVkYcbcrConversionInfo" /> values are not equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.GRVkYcbcrConversionInfo" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.GRVkYcbcrConversionInfo" /> to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks></remarks>
		public static bool operator != (GRVkYcbcrConversionInfo left, GRVkYcbcrConversionInfo right) =>
			!left.Equals (right);

		/// <summary>Returns a hash code for this conversion info.</summary>
		/// <returns>A hash code for this <see cref="T:SkiaSharp.GRVkYcbcrConversionInfo" /> instance.</returns>
		/// <remarks></remarks>
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fFormat);
			hash.Add (fExternalFormat);
			hash.Add (fYcbcrModel);
			hash.Add (fYcbcrRange);
			hash.Add (fXChromaOffset);
			hash.Add (fYChromaOffset);
			hash.Add (fChromaFilter);
			hash.Add (fForceExplicitReconstruction);
			hash.Add (fComponents);
			hash.Add (fSamplerFilterMustMatchChromaFilter);
			hash.Add (fSupportsLinearFilter);
			return hash.ToHashCode ();
		}

	}
}
