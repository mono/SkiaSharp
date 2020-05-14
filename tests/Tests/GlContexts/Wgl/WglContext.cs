using System;

namespace SkiaSharp.Tests
{
	internal class WglContext : GlContext
	{
		private static readonly object fLock = new object();

		private static readonly Win32Window window = new Win32Window("WglContext");

		private IntPtr pbufferHandle;
		private IntPtr pbufferDeviceContextHandle;
		private IntPtr pbufferGlContextHandle;

		public WglContext()
		{
			if (!Wgl.HasExtension(window.DeviceContextHandle, "WGL_ARB_pixel_format") ||
				!Wgl.HasExtension(window.DeviceContextHandle, "WGL_ARB_pbuffer"))
			{
				throw new Exception("DC does not have extensions.");
			}

			var iAttrs = new int[]
			{
				Wgl.WGL_ACCELERATION_ARB, Wgl.WGL_FULL_ACCELERATION_ARB,
				Wgl.WGL_DRAW_TO_WINDOW_ARB, Wgl.TRUE,
				//Wgl.WGL_DOUBLE_BUFFER_ARB, (doubleBuffered ? TRUE : FALSE),
				Wgl.WGL_SUPPORT_OPENGL_ARB, Wgl.TRUE,
				Wgl.WGL_RED_BITS_ARB, 8,
				Wgl.WGL_GREEN_BITS_ARB, 8,
				Wgl.WGL_BLUE_BITS_ARB, 8,
				Wgl.WGL_ALPHA_BITS_ARB, 8,
				Wgl.WGL_STENCIL_BITS_ARB, 8,
				Wgl.NONE, Wgl.NONE
			};
			var piFormats = new int[1];
			uint nFormats;
			lock (fLock)
			{
				// HACK: This call seems to cause deadlocks on some systems.
				Wgl.wglChoosePixelFormatARB(window.DeviceContextHandle, iAttrs, null, (uint)piFormats.Length, piFormats, out nFormats);
			}
			if (nFormats == 0)
			{
				Destroy();
				throw new Exception("Could not get pixel formats.");
			}

			pbufferHandle = Wgl.wglCreatePbufferARB(window.DeviceContextHandle, piFormats[0], 1, 1, null);
			if (pbufferHandle == IntPtr.Zero)
			{
				Destroy();
				throw new Exception("Could not create Pbuffer.");
			}

			pbufferDeviceContextHandle = Wgl.wglGetPbufferDCARB(pbufferHandle);
			if (pbufferDeviceContextHandle == IntPtr.Zero)
			{
				Destroy();
				throw new Exception("Could not get Pbuffer DC.");
			}

			var prevDC = Wgl.wglGetCurrentDC();
			var prevGLRC = Wgl.wglGetCurrentContext();

			pbufferGlContextHandle = Wgl.wglCreateContext(pbufferDeviceContextHandle);

			Wgl.wglMakeCurrent(prevDC, prevGLRC);

			if (pbufferGlContextHandle == IntPtr.Zero)
			{
				Destroy();
				throw new Exception("Could not creeate Pbuffer GL context.");
			}
		}

		public override void MakeCurrent()
		{
			if (!Wgl.wglMakeCurrent(pbufferDeviceContextHandle, pbufferGlContextHandle))
			{
				Destroy();
				throw new Exception("Could not set the context.");
			}
		}

		public override void SwapBuffers()
		{
			if (!Gdi32.SwapBuffers(pbufferDeviceContextHandle))
			{
				Destroy();
				throw new Exception("Could not complete SwapBuffers.");
			}
		}

		public override void Destroy()
		{
			if (pbufferGlContextHandle != IntPtr.Zero)
			{
				Wgl.wglDeleteContext(pbufferGlContextHandle);
				pbufferGlContextHandle = IntPtr.Zero;
			}

			if (pbufferHandle != IntPtr.Zero)
			{
				if (pbufferDeviceContextHandle != IntPtr.Zero)
				{
					if (!Wgl.HasExtension(pbufferDeviceContextHandle, "WGL_ARB_pbuffer"))
					{
						// ASSERT
					}

					Wgl.wglReleasePbufferDCARB?.Invoke(pbufferHandle, pbufferDeviceContextHandle);
					pbufferDeviceContextHandle = IntPtr.Zero;
				}

				Wgl.wglDestroyPbufferARB?.Invoke(pbufferHandle);
				pbufferHandle = IntPtr.Zero;
			}
		}

		public override GRGlTextureInfo CreateTexture(SKSizeI textureSize)
		{
			var textures = new uint[1];
			Wgl.glGenTextures(textures.Length, textures);
			var textureId = textures[0];

			Wgl.glBindTexture(Wgl.GL_TEXTURE_2D, textureId);
			Wgl.glTexImage2D(Wgl.GL_TEXTURE_2D, 0, Wgl.GL_RGBA, textureSize.Width, textureSize.Height, 0, Wgl.GL_RGBA, Wgl.GL_UNSIGNED_BYTE, IntPtr.Zero);
			Wgl.glBindTexture(Wgl.GL_TEXTURE_2D, 0);

			return new GRGlTextureInfo
			{
				Id = textureId,
				Target = Wgl.GL_TEXTURE_2D,
				Format = Wgl.GL_RGBA8
			};
		}

		public override void DestroyTexture(uint texture)
		{
			Wgl.glDeleteTextures(1, new[] { texture });
		}
	}
}
