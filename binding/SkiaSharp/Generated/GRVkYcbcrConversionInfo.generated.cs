using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// gr_vk_ycbcrconversioninfo_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct GRVkYcbcrConversionInfo : IEquatable<GRVkYcbcrConversionInfo> {
		// public uint32_t fFormat
		private UInt32 fFormat;
		public UInt32 Format {
			readonly get => fFormat;
			set => fFormat = value;
		}

		// public uint64_t fExternalFormat
		private UInt64 fExternalFormat;
		public UInt64 ExternalFormat {
			readonly get => fExternalFormat;
			set => fExternalFormat = value;
		}

		// public uint32_t fYcbcrModel
		private UInt32 fYcbcrModel;
		public UInt32 YcbcrModel {
			readonly get => fYcbcrModel;
			set => fYcbcrModel = value;
		}

		// public uint32_t fYcbcrRange
		private UInt32 fYcbcrRange;
		public UInt32 YcbcrRange {
			readonly get => fYcbcrRange;
			set => fYcbcrRange = value;
		}

		// public uint32_t fXChromaOffset
		private UInt32 fXChromaOffset;
		public UInt32 XChromaOffset {
			readonly get => fXChromaOffset;
			set => fXChromaOffset = value;
		}

		// public uint32_t fYChromaOffset
		private UInt32 fYChromaOffset;
		public UInt32 YChromaOffset {
			readonly get => fYChromaOffset;
			set => fYChromaOffset = value;
		}

		// public uint32_t fChromaFilter
		private UInt32 fChromaFilter;
		public UInt32 ChromaFilter {
			readonly get => fChromaFilter;
			set => fChromaFilter = value;
		}

		// public uint32_t fForceExplicitReconstruction
		private UInt32 fForceExplicitReconstruction;
		public UInt32 ForceExplicitReconstruction {
			readonly get => fForceExplicitReconstruction;
			set => fForceExplicitReconstruction = value;
		}

		// public gr_vk_ycbcr_components_t fComponents
		private GRVkYcbcrComponents fComponents;
		public GRVkYcbcrComponents Components {
			readonly get => fComponents;
			set => fComponents = value;
		}

		// public bool fSamplerFilterMustMatchChromaFilter
		private Byte fSamplerFilterMustMatchChromaFilter;
		public bool SamplerFilterMustMatchChromaFilter {
			readonly get => fSamplerFilterMustMatchChromaFilter > 0;
			set => fSamplerFilterMustMatchChromaFilter = value ? (byte)1 : (byte)0;
		}

		// public bool fSupportsLinearFilter
		private Byte fSupportsLinearFilter;
		public bool SupportsLinearFilter {
			readonly get => fSupportsLinearFilter > 0;
			set => fSupportsLinearFilter = value ? (byte)1 : (byte)0;
		}

		public readonly bool Equals (GRVkYcbcrConversionInfo obj) =>
#pragma warning disable CS8909
			fFormat == obj.fFormat && fExternalFormat == obj.fExternalFormat && fYcbcrModel == obj.fYcbcrModel && fYcbcrRange == obj.fYcbcrRange && fXChromaOffset == obj.fXChromaOffset && fYChromaOffset == obj.fYChromaOffset && fChromaFilter == obj.fChromaFilter && fForceExplicitReconstruction == obj.fForceExplicitReconstruction && fComponents == obj.fComponents && fSamplerFilterMustMatchChromaFilter == obj.fSamplerFilterMustMatchChromaFilter && fSupportsLinearFilter == obj.fSupportsLinearFilter;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is GRVkYcbcrConversionInfo f && Equals (f);

		public static bool operator == (GRVkYcbcrConversionInfo left, GRVkYcbcrConversionInfo right) =>
			left.Equals (right);

		public static bool operator != (GRVkYcbcrConversionInfo left, GRVkYcbcrConversionInfo right) =>
			!left.Equals (right);

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
