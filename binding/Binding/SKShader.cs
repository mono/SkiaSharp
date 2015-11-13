using System;

namespace SkiaSharp
{
	public class SKShader : IDisposable
	{
		internal IntPtr handle;

		public SKShader ()
		{
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero) {
				SkiaApi.sk_shader_unref (handle);
				// We set this in case the user tries to use the fetched Canvas (which depends on us) to perform some operations
				handle = IntPtr.Zero;
			}
		}

		~SKShader()
		{
			Dispose (false);
		}

	}
}

