//
// Bindings for GRContext
//
// Author:
//   Matthew Leibowitz
//
// Copyright 2016 Xamarin Inc
//

using System;

namespace SkiaSharp
{
	public class GRContext : SKObject
	{
		[Preserve]
		internal GRContext (IntPtr h, bool owns)
			: base (h, owns)
		{
		}
		
		public static GRContext Create (GRBackend backend)
		{
			return Create (backend, IntPtr.Zero);
		}

		public static GRContext Create (GRBackend backend, IntPtr backendContext)
		{
			return GetObject<GRContext> (SkiaApi.gr_context_create_with_defaults (backend, backendContext));
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.gr_context_unref (Handle);
			}

			base.Dispose (disposing);
		}
	}
}

