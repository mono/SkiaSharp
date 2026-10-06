using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Xunit;

namespace SkiaSharp.Tests
{
	[Collection(Visual.GpuRenderingCollection.Name)]
	public class GRGlInterfaceTest : SKTest
	{
		[Fact]
		public void InterfaceConstructionWithoutContextDoesNotCrash()
		{
			SkipOnPlatform(IsIOS || IsMacCatalyst, "GRGlInterface construction without context crashes on iOS/MacCatalyst");
			SkipOnPlatform(IsBrowser, "GRGlInterface native call aborts the WASM runtime without a WebGL canvas");

			var glInterface = GRGlInterface.Create();

			Assert.Null(glInterface);
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void CreateDefaultInterfaceIsValid()
		{
			using (var ctx = CreateGlContext()) {
				ctx.MakeCurrent();

				var glInterface = GRGlInterface.Create();

				Assert.NotNull(glInterface);
				Assert.True(glInterface.Validate());
			}
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
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
					// glXGetProcAddress cannot be used on its own here: under GLVND
					// (the vendor-neutral libGL every modern distro ships) it hands
					// back a dispatch stub for *any* name, including one that does
					// not exist, so a loader that trusts it gives Skia pointers that
					// are not functions and the first extension query walks off into
					// nothing. dlsym answers honestly, exactly like the macOS branch.
					var lib = LibraryLoader.LoadLibrary("libGL.so.1");

					var glInterface = GRGlInterface.Create(name => {
						return LibraryLoader.GetSymbol(lib, name);
					});

					Assert.NotNull(glInterface);
					Assert.True(glInterface.Validate());

					LibraryLoader.FreeLibrary(lib);
				} else {
					// more platforms !!!
					throw new Exception("Some strange platform that is not Windows, macOS nor Linux...");
				}
			}
		}

		[DllImport("opengl32.dll", CallingConvention = CallingConvention.Winapi)]
		public static extern IntPtr wglGetProcAddress([MarshalAs(UnmanagedType.LPStr)] string lpszProc);
	}
}
