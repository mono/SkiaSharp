using SharpVk;

using PhysicalDeviceFeaturesNative = SharpVk.Interop.PhysicalDeviceFeatures;

namespace SkiaSharp
{
	internal static class Utils
	{
		public static PhysicalDeviceFeaturesNative ToNative(this PhysicalDeviceFeatures features) =>
			new PhysicalDeviceFeaturesNative
			{
				RobustBufferAccess = features.RobustBufferAccess,
				FullDrawIndexUint32 = features.FullDrawIndexUint32,
				ImageCubeArray = features.ImageCubeArray,
				IndependentBlend = features.IndependentBlend,
				GeometryShader = features.GeometryShader,
				TessellationShader = features.TessellationShader,
				SampleRateShading = features.SampleRateShading,
				DualSourceBlend = features.DualSourceBlend,
				LogicOp = features.LogicOp,
				MultiDrawIndirect = features.MultiDrawIndirect,
				DrawIndirectFirstInstance = features.DrawIndirectFirstInstance,
				DepthClamp = features.DepthClamp,
				DepthBiasClamp = features.DepthBiasClamp,
				FillModeNonSolid = features.FillModeNonSolid,
				DepthBounds = features.DepthBounds,
				WideLines = features.WideLines,
				LargePoints = features.LargePoints,
				AlphaToOne = features.AlphaToOne,
				MultiViewport = features.MultiViewport,
				SamplerAnisotropy = features.SamplerAnisotropy,
				TextureCompressionETC2 = features.TextureCompressionETC2,
				TexturecompressionastcLdr = features.TexturecompressionastcLdr,
				TextureCompressionBC = features.TextureCompressionBC,
				OcclusionQueryPrecise = features.OcclusionQueryPrecise,
				PipelineStatisticsQuery = features.PipelineStatisticsQuery,
				VertexPipelineStoresAndAtomics = features.VertexPipelineStoresAndAtomics,
				FragmentStoresAndAtomics = features.FragmentStoresAndAtomics,
				ShaderTessellationAndGeometryPointSize = features.ShaderTessellationAndGeometryPointSize,
				ShaderImageGatherExtended = features.ShaderImageGatherExtended,
				ShaderStorageImageExtendedFormats = features.ShaderStorageImageExtendedFormats,
				ShaderStorageImageMultisample = features.ShaderStorageImageMultisample,
				ShaderStorageImageReadWithoutFormat = features.ShaderStorageImageReadWithoutFormat,
				ShaderStorageImageWriteWithoutFormat = features.ShaderStorageImageWriteWithoutFormat,
				ShaderUniformBufferArrayDynamicIndexing = features.ShaderUniformBufferArrayDynamicIndexing,
				ShaderSampledImageArrayDynamicIndexing = features.ShaderSampledImageArrayDynamicIndexing,
				ShaderStorageBufferArrayDynamicIndexing = features.ShaderStorageBufferArrayDynamicIndexing,
				ShaderStorageImageArrayDynamicIndexing = features.ShaderStorageImageArrayDynamicIndexing,
				ShaderClipDistance = features.ShaderClipDistance,
				ShaderCullDistance = features.ShaderCullDistance,
				ShaderFloat64 = features.ShaderFloat64,
				ShaderInt64 = features.ShaderInt64,
				ShaderInt16 = features.ShaderInt16,
				ShaderResourceResidency = features.ShaderResourceResidency,
				ShaderResourceMinLod = features.ShaderResourceMinLod,
				SparseBinding = features.SparseBinding,
				SparseResidencyBuffer = features.SparseResidencyBuffer,
				SparseResidencyImage2D = features.SparseResidencyImage2D,
				SparseResidencyImage3D = features.SparseResidencyImage3D,
				SparseResidency2Samples = features.SparseResidency2Samples,
				SparseResidency4Samples = features.SparseResidency4Samples,
				SparseResidency8Samples = features.SparseResidency8Samples,
				SparseResidency16Samples = features.SparseResidency16Samples,
				SparseResidencyAliased = features.SparseResidencyAliased,
				VariableMultisampleRate = features.VariableMultisampleRate,
				InheritedQueries = features.InheritedQueries,
			};
	}
}
