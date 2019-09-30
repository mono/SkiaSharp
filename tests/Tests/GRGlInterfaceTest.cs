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
		public void CreateDefaultInterfaceIsValid()
		{
			using (var ctx = CreateGlContext()) {
				ctx.MakeCurrent();

				var glInterface = GRGlInterface.CreateNativeGlInterface();

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
					var lib = MacDynamicLibraries.dlopen("/System/Library/Frameworks/OpenGL.framework/Versions/A/Libraries/libGL.dylib", 1);

					var glInterface = GRGlInterface.AssembleGlInterface((context, name) => {
						return MacDynamicLibraries.dlsym(lib, name);
					});

					Assert.NotNull(glInterface);
					Assert.True(glInterface.Validate());

					MacDynamicLibraries.dlclose(lib);
				} else if (IsWindows) {
					var lib = WindowsDynamicLibraries.LoadLibrary("opengl32.dll");

					var glInterface = GRGlInterface.AssembleGlInterface((context, name) => {
						var ptr = WindowsDynamicLibraries.GetProcAddress(lib, name);
						if (ptr == IntPtr.Zero) {
							ptr = wglGetProcAddress(name);
						}
						return ptr;
					});

					Assert.NotNull(glInterface);
					Assert.True(glInterface.Validate());

					WindowsDynamicLibraries.FreeLibrary(lib);
				} else if (IsLinux) {
					var glInterface = GRGlInterface.AssembleGlInterface((context, name) => {
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
