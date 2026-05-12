using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// IRenderer proxies that forward to the on-device RenderHost.Android
	/// app via TCP (through <see cref="AndroidHostSession"/>). One concrete
	/// proxy per Android-side renderer name.
	/// </summary>
	public abstract class AndroidRendererBase : IRenderer
	{
		public abstract string Name { get; }
		public abstract RendererCapabilities Caps { get; }

		// Session is process-singleton and lazily initialised on the first
		// RenderAsync — bringing it up takes ~5-10 s (adb install + activity
		// launch + port forward) so we don't pay it at test discovery time.
		private static volatile AndroidHostSession _session;
		private static readonly SemaphoreSlim _initLock = new (1, 1);

		public bool IsAvailable => HostApkExists ();

		public string UnavailableReason =>
			HostApkExists () ? null
				: "RenderHost.Android APK missing — run `dotnet build tests/Hosts/RenderHost.Android -c Release`. " +
				  "Also requires `adb` on PATH and an Android device/emulator attached.";

		public async Task<byte[]> RenderAsync (ISkiaScene scene, SKImageInfo info, CancellationToken ct)
		{
			var session = await EnsureSession (ct);
			return await session.RenderAsync (Name, scene.Name, info.Width, info.Height, ct);
		}

		private static async Task<AndroidHostSession> EnsureSession (CancellationToken ct)
		{
			if (_session != null) {
				if (!_session.IsAvailable)
					throw new RendererUnavailableException (_session.FailureReason);
				return _session;
			}
			await _initLock.WaitAsync (ct);
			try {
				if (_session == null)
					_session = await AndroidHostSession.GetAsync (ct);
				if (!_session.IsAvailable)
					throw new RendererUnavailableException (_session.FailureReason);
				return _session;
			} finally {
				_initLock.Release ();
			}
		}

		private static bool HostApkExists ()
		{
			// Cheap probe — only checks the file exists. Doesn't run adb.
			var dir = AppContext.BaseDirectory.TrimEnd (Path.DirectorySeparatorChar);
			for (int i = 0; i < 12 && dir != null; i++) {
				var bin = Path.Combine (dir, "tests", "Hosts", "RenderHost.Android", "bin", "Release");
				if (Directory.Exists (bin)) {
					foreach (var tfm in Directory.GetDirectories (bin)) {
						if (Directory.EnumerateFiles (tfm, "*.apk").GetEnumerator ().MoveNext ())
							return true;
					}
				}
				dir = Path.GetDirectoryName (dir);
			}
			return false;
		}

		public void Dispose () { /* session is process-singleton */ }
	}

	public sealed class AndroidRasterRenderer : AndroidRendererBase
	{
		public override string Name => "android-raster";
		public override RendererCapabilities Caps => RendererCapabilities.Cpu;
	}

	public sealed class AndroidGaneshVulkanRenderer : AndroidRendererBase
	{
		public override string Name => "android-ganesh-vulkan";
		public override RendererCapabilities Caps => RendererCapabilities.Gpu;
	}

	// Follow-ups (not in this commit):
	//   - AndroidGaneshGlesRenderer (needs an Android-specific EGL loader
	//     inside the host app; current EglLoader hardcodes libEGL.so.1 which
	//     Android doesn't ship — Android has libEGL.so).
	//   - AndroidGraphiteVulkanRenderer (waits for Skia upstream to stabilise
	//     Graphite-on-Android-Vulkan).
}
