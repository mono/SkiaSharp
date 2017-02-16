using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests
{
	public abstract class GlContext : IDisposable
	{
		public abstract void MakeCurrent();
		public abstract void SwapBuffers();
		public abstract void Destroy();

		void IDisposable.Dispose() => Destroy();
	}
}
