using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace SkiaSharp.Tests
{
	// Proxy-level unit tests for SKGraphiteShaderErrorHandlerDelegate. Invokes
	// DelegateProxies.SKGraphiteShaderErrorHandlerProxy directly with synthetic
	// ANSI strings + a real GCHandle userData — the same call shape Skia's
	// FfiShaderErrorHandler bridge produces. Verifies argument forwarding and
	// null-pointer normalization without needing a driver-level shader failure
	// (those are hard to synthesize deterministically on Lavapipe).
	//
	// End-to-end callback delivery under a real driver rejection is covered by
	// the iOS-simulator integration run described in the #4555 PR body.
	public unsafe class SKGraphiteShaderErrorHandlerTest : SKTest
	{
		[Fact]
		public void ProxyForwardsShaderAndErrorsToManagedDelegate()
		{
			string capturedShader = null;
			string capturedErrors = null;
			bool? capturedCached = null;

			SKGraphiteShaderErrorHandlerDelegate handler = (shader, errors, cached) =>
			{
				capturedShader = shader;
				capturedErrors = errors;
				capturedCached = cached;
			};

			DelegateProxies.Create(handler, out var gch, out var userData);
			try
			{
				InvokeProxy(userData, "void main() { }", "Compile error: line 1", shaderWasCached: true);

				Assert.Equal("void main() { }", capturedShader);
				Assert.Equal("Compile error: line 1", capturedErrors);
				Assert.True(capturedCached);
			}
			finally
			{
				gch.Free();
			}
		}

		[Fact]
		public void ProxyNormalizesNullPointersToEmptyStrings()
		{
			string capturedShader = "unchanged";
			string capturedErrors = "unchanged";

			SKGraphiteShaderErrorHandlerDelegate handler = (shader, errors, _) =>
			{
				capturedShader = shader;
				capturedErrors = errors;
			};

			DelegateProxies.Create(handler, out var gch, out var userData);
			try
			{
				var proxy = DelegateProxies.SKGraphiteShaderErrorHandlerProxy;
				proxy((void*)userData, null, null, 0);

				// Copilot review on #4586: guarantee user handlers never receive null,
				// so their code can treat both args as always-string.
				Assert.Equal(string.Empty, capturedShader);
				Assert.Equal(string.Empty, capturedErrors);
			}
			finally
			{
				gch.Free();
			}
		}

		[Fact]
		public void ProxySwallowsManagedExceptionsAcrossFfiBoundary()
		{
			SKGraphiteShaderErrorHandlerDelegate handler = (shader, errors, _) =>
				throw new InvalidOperationException("intentional test throw");

			DelegateProxies.Create(handler, out var gch, out var userData);
			try
			{
				// If the proxy leaked the exception through the (native) FFI boundary the
				// process would tear down. Reaching the assertion below is proof it did not.
				var ex = Record.Exception(() => InvokeProxy(userData, "s", "e", false));
				Assert.Null(ex);
			}
			finally
			{
				gch.Free();
			}
		}

		public enum Backend { Dawn, Metal, Vulkan }

		// A Create* call whose backend context is incomplete must throw without leaving the
		// shader-error handler rooted by a GCHandle (or its native bridge allocated).
		[Theory]
		[InlineData(Backend.Dawn)]
		[InlineData(Backend.Metal)]
		[InlineData(Backend.Vulkan)]
		public void FailedCreateDoesNotRootShaderErrorHandler(Backend backend)
		{
			var weak = CreateWithIncompleteBackend(backend);

			for (var i = 0; i < 3 && weak.IsAlive; i++)
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}

			Assert.False(weak.IsAlive, "The shader-error handler was still rooted after a failed Create call.");
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static WeakReference CreateWithIncompleteBackend(Backend backend)
		{
			var captured = new object();
			SKGraphiteShaderErrorHandlerDelegate handler = (_, _, _) => GC.KeepAlive(captured);
			var options = new SKGraphiteContextOptions { ShaderErrorHandler = handler };

			Assert.Throws<InvalidOperationException>(() =>
			{
				switch (backend)
				{
					case Backend.Dawn:
						using (var ctx = new SKGraphiteDawnBackendContext())
							SKGraphiteContext.CreateDawn(ctx, options);
						break;
					case Backend.Metal:
						using (var ctx = new SKGraphiteMtlBackendContext())
							SKGraphiteContext.CreateMetal(ctx, options);
						break;
					case Backend.Vulkan:
						using (var ctx = new SKGraphiteVkBackendContext())
							SKGraphiteContext.CreateVulkan(ctx, options);
						break;
				}
			});

			return new WeakReference(handler);
		}

		private static void InvokeProxy(IntPtr userData, string shader, string errors, bool shaderWasCached)
		{
			var proxy = DelegateProxies.SKGraphiteShaderErrorHandlerProxy;
			var shaderBytes = Encoding.ASCII.GetBytes(shader + "\0");
			var errorsBytes = Encoding.ASCII.GetBytes(errors + "\0");
			fixed (byte* shaderPtr = shaderBytes)
			fixed (byte* errorsPtr = errorsBytes)
			{
				proxy((void*)userData, shaderPtr, errorsPtr, shaderWasCached ? (byte)1 : (byte)0);
			}
		}
	}
}
