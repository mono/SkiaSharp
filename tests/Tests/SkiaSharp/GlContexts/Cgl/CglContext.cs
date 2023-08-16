using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests
{
	internal class CglContext : GlContext
	{
		private IntPtr fContext;

		public CglContext()
		{
			var attributes = new [] {
				CGLPixelFormatAttribute.kCGLPFAOpenGLProfile, (CGLPixelFormatAttribute)CGLOpenGLProfile.kCGLOGLPVersion_3_2_Core,
				CGLPixelFormatAttribute.kCGLPFADoubleBuffer, 
				CGLPixelFormatAttribute.kCGLPFANone
			};

			IntPtr pixFormat;
			int npix;

			Cgl.CGLChoosePixelFormat(attributes, out pixFormat, out npix);

			if (pixFormat == IntPtr.Zero) {
				throw new Exception("CGLChoosePixelFormat failed.");
			}

			Cgl.CGLCreateContext(pixFormat, IntPtr.Zero, out fContext);
			Cgl.CGLReleasePixelFormat(pixFormat);

			if (fContext == IntPtr.Zero) {
				throw new Exception("CGLCreateContext failed.");
			}
		}

		public override void MakeCurrent()
		{
			Cgl.CGLSetCurrentContext(fContext);
		}

		public override void SwapBuffers()
		{
			Cgl.CGLFlushDrawable(fContext);
		}

		public override void Destroy()
		{
			if (fContext != IntPtr.Zero) {
				Cgl.CGLReleaseContext(fContext);
				fContext = IntPtr.Zero;
			}
		}

		public override GRGlTextureInfo CreateTexture(SKSizeI textureSize)
		{
			var textures = new uint[1];
			Cgl.glGenTextures(textures.Length, textures);
			var textureId = textures[0];

			Cgl.glBindTexture(Cgl.GL_TEXTURE_2D, textureId);
			Cgl.glTexImage2D(Cgl.GL_TEXTURE_2D, 0, Cgl.GL_RGBA, textureSize.Width, textureSize.Height, 0, Cgl.GL_RGBA, Cgl.GL_UNSIGNED_BYTE, IntPtr.Zero);
			Cgl.glBindTexture(Cgl.GL_TEXTURE_2D, 0);

			return new GRGlTextureInfo {
				Id = textureId,
				Target = Cgl.GL_TEXTURE_2D,
				Format = Cgl.GL_RGBA8
			};
		}

		public override void DestroyTexture(uint texture)
		{
			Cgl.glDeleteTextures(1, new[] { texture });
		}
	}
}
