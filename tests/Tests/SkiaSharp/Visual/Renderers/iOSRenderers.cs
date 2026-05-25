using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// IRenderer proxies for the on-simulator RenderHost.iOS app. Same
	/// shape as <see cref="AndroidRendererBase"/> and <see cref="WasmRendererBase"/>:
	/// IsAvailable does a cheap on-disk probe (bundle present? macOS host?);
	/// the heavy simctl install + launch + TCP connect happens lazily on
	/// first RenderAsync via <see cref="iOSHostSession"/>.
	/// </summary>
	public abstract class iOSRendererBase : IRenderer
	{
		public abstract string Name { get; }

		private static volatile iOSHostSession? _session;
		private static readonly SemaphoreSlim _initLock = new (1, 1);

		public bool IsAvailable =>
			RuntimeInformation.IsOSPlatform (OSPlatform.OSX) && HostBundleExists ();

		public string? UnavailableReason
		{
			get {
				if (!RuntimeInformation.IsOSPlatform (OSPlatform.OSX))
					return "iOS Simulator requires macOS";
				if (!HostBundleExists ())
					return "RenderHost.iOS .app bundle missing — run `dotnet build tests/Hosts/RenderHost.iOS -c Release` on macOS. " +
					       "Also requires `xcrun simctl` and a booted iOS Simulator.";
				return null;
			}
		}

		public async Task<byte[]> RenderAsync (ISkiaScene scene, SKImageInfo info, CancellationToken ct)
		{
			var session = await EnsureSession (ct);
			return await session.RenderAsync (Name, scene.Name, info.Width, info.Height, ct);
		}

		private static async Task<iOSHostSession> EnsureSession (CancellationToken ct)
		{
			if (_session != null) {
				if (!_session.IsAvailable)
					throw new RendererUnavailableException (_session.FailureReason ?? "iOS host unavailable");
				return _session;
			}
			await _initLock.WaitAsync (ct);
			try {
				if (_session == null)
					_session = await iOSHostSession.GetAsync (ct);
				if (!_session.IsAvailable)
					throw new RendererUnavailableException (_session.FailureReason ?? "iOS host unavailable");
				return _session;
			} finally {
				_initLock.Release ();
			}
		}

		private static bool HostBundleExists ()
		{
			var dir = AppContext.BaseDirectory.TrimEnd (Path.DirectorySeparatorChar);
			for (int i = 0; i < 12 && dir != null; i++) {
				var bin = Path.Combine (dir, "tests", "Hosts", "RenderHost.iOS", "bin", "Release");
				if (Directory.Exists (bin)) {
					foreach (var tfm in Directory.GetDirectories (bin)) {
						foreach (var rid in Directory.GetDirectories (tfm)) {
							if (Directory.EnumerateDirectories (rid, "*.app").GetEnumerator ().MoveNext ())
								return true;
						}
					}
				}
				dir = Path.GetDirectoryName (dir);
			}
			return false;
		}

		public void Dispose () { /* session is process-singleton */ }
	}

	public sealed class iOSRasterRenderer : iOSRendererBase
	{
		public override string Name => "ios-raster";
	}

	public sealed class iOSGaneshMetalRenderer : iOSRendererBase
	{
		public override string Name => "ios-ganesh-metal";
	}

	public sealed class iOSGraphiteMetalRenderer : iOSRendererBase
	{
		public override string Name => "ios-graphite-metal";
	}
}
