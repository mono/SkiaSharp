using System;
using System.Collections.Generic;
using System.Text;

namespace SkiaSharp
{
	/// <summary>Represents Direct3D texture resource information for GPU interoperability.</summary>
	/// <remarks />
	public class GRD3DTextureResourceInfo : IDisposable
	{
		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.GRD3DTextureResourceInfo" />.</summary>
		/// <remarks />
		public GRD3DTextureResourceInfo()
		{
		}

		/// <summary>Gets or sets the pointer to the underlying Direct3D resource.</summary>
		/// <value>The native pointer to the ID3D12Resource.</value>
		/// <remarks />
		public nint Resource { get; set; }

		/// <summary>Gets or sets the current resource state of the texture.</summary>
		/// <value>The D3D12_RESOURCE_STATES value.</value>
		/// <remarks />
		public uint ResourceState { get; set; }

		/// <summary>Gets or sets the DXGI format of the texture.</summary>
		/// <value>The DXGI format value.</value>
		/// <remarks />
		public uint Format { get; set; }

		/// <summary>Gets or sets the number of samples per pixel for multisampling.</summary>
		/// <value>The sample count.</value>
		/// <remarks />
		public uint SampleCount { get; set; }

		/// <summary>Gets or sets the number of mipmap levels in the texture.</summary>
		/// <value>The number of mipmap levels.</value>
		/// <remarks />
		public uint LevelCount { get; set; }

		/// <summary>Gets or sets the quality pattern for multisampling.</summary>
		/// <value>The sample quality pattern value.</value>
		/// <remarks />
		public uint SampleQualityPattern { get; set; }

		/// <summary>Gets or sets a value indicating whether the texture is protected.</summary>
		/// <value><see langword="true" /> if the texture is protected; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool Protected { get; set; }

		internal GRD3DTextureResourceInfoNative ToNative ()
		{
			return new GRD3DTextureResourceInfoNative {
				fResource = Resource,
				fResourceState = ResourceState,
				fFormat = Format,
				fSampleCount = SampleCount,
				fLevelCount = LevelCount,
				fSampleQualityPattern = SampleQualityPattern,
				fProtected = Protected ? (byte)1 : (byte)0
			};
		}

		/// <summary>Releases the unmanaged resources used by the object and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks />
		protected virtual void Dispose (bool disposing)
		{
		}

		/// <summary>Releases all resources used by this object.</summary>
		/// <remarks />
		public void Dispose ()
		{
			Dispose (disposing: true);
			GC.SuppressFinalize (this);
		}
	}
}
