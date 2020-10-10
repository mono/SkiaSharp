using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Xunit;

namespace SkiaSharp.Tests
{
	public class GRGlInterfaceTest : SKTest
	{
		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void InterfaceConstructionWithoutContextDoesNotCrash()
		{
			var glInterface = GRGlInterface.Create();

			Assert.Null(glInterface);
		}

		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void CreateDefaultInterfaceIsValid()
		{
			using (var ctx = CreateGlContext()) {
				ctx.MakeCurrent();

				var glInterface = GRGlInterface.Create();

				Assert.NotNull(glInterface);
				Assert.True(glInterface.Validate());
			}
		}

		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void AssembleInterfaceIsValid()
		{
			using (var ctx = CreateGlContext()) {
				ctx.MakeCurrent();

				if (IsMac) {
					var lib = LibraryLoader.LoadLibrary("/System/Library/Frameworks/OpenGL.framework/Versions/A/Libraries/libGL.dylib");

					var glInterface = GRGlInterface.Create(name => {
						return LibraryLoader.GetSymbol(lib, name);
					});

					Assert.NotNull(glInterface);
					Assert.True(glInterface.Validate());

					LibraryLoader.FreeLibrary(lib);
				} else if (IsWindows) {
					var lib = LibraryLoader.LoadLibrary("opengl32.dll");

					var glInterface = GRGlInterface.Create(name => {
						var ptr = LibraryLoader.GetSymbol(lib, name);
						if (ptr == IntPtr.Zero) {
							ptr = wglGetProcAddress(name);
						}
						return ptr;
					});

					Assert.NotNull(glInterface);
					Assert.True(glInterface.Validate());

					LibraryLoader.FreeLibrary(lib);
				} else if (IsLinux) {
					var glInterface = GRGlInterface.Create(name => {
						return glXGetProcAddress(name);
					});

					Assert.NotNull(glInterface);
					Assert.True(glInterface.Validate());
				} else {
					// more platforms !!!
					throw new Exception("Some strange platform that is not Windows, macOS nor Linux...");
				}
			}
		}

		[DllImport("opengl32.dll", CallingConvention = CallingConvention.Winapi)]
		public static extern IntPtr wglGetProcAddress([MarshalAs(UnmanagedType.LPStr)] string lpszProc);

		[DllImport("libGL.so.1")]
		public static extern IntPtr glXGetProcAddress([MarshalAs(UnmanagedType.LPStr)] string lpszProc);
	}
}
