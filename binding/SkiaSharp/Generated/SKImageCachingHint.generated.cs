using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_image_caching_hint_t
	/// <summary>Hints to image calls where the system might cache computed intermediates.</summary>
	/// <remarks />
	public enum SKImageCachingHint {
		// ALLOW_SK_IMAGE_CACHING_HINT = 0
		/// <summary>Use the system's default behaviour.</summary>
		Allow = 0,
		// DISALLOW_SK_IMAGE_CACHING_HINT = 1
		/// <summary>Caching should be avoided.</summary>
		Disallow = 1,
	}
}
