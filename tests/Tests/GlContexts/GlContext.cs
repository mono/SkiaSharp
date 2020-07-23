using System;

namespace SkiaSharp.Tests
{
	public abstract class GlContext : IDisposable
	{
		public abstract void MakeCurrent();
		public abstract void SwapBuffers();
		public abstract void Destroy();
		public abstract GRGlTextureInfo CreateTexture(SKSizeI textureSize);
		public abstract void DestroyTexture(uint texture);

		void IDisposable.Dispose() => Destroy();
	}
}
