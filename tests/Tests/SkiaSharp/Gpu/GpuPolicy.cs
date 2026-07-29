using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SkiaSharp.Tests
{
	public enum GpuBackend
	{
		Cpu,
		GaneshGl,
		GaneshVulkan,
		GaneshVulkanSharpVk,
		GaneshMetal,
		GaneshDirect3D,
		GraphiteVulkan,
		GraphiteMetal,
		GraphiteDawn,
	}

	[Flags]
	public enum TestPlatforms
	{
		None = 0,

		Windows = 1 << 0,
		MacOS = 1 << 1,
		Linux = 1 << 2,
		Android = 1 << 3,
		IOS = 1 << 4,
		MacCatalyst = 1 << 5,
		TvOS = 1 << 6,
		Browser = 1 << 7,

		// Kept apart from Windows because native/nanoserver/build.cake builds without
		// Vulkan or Direct3D: the OS is Windows but the library is not.
		NanoServer = 1 << 8,

		Apple = MacOS | IOS | MacCatalyst | TvOS,
		Desktop = Windows | MacOS | Linux,
		AnyWindows = Windows | NanoServer,
		All = Windows | MacOS | Linux | Android | IOS | MacCatalyst | TvOS | Browser | NanoServer,
	}

	/// <summary>
	/// Decides whether a GPU backend must work on this host, and is the only place in
	/// the suite allowed to skip a GPU test.
	///
	/// <para>
	/// A backend is <c>required</c> unless it is <c>unsupported</c> (the API does not
	/// exist here), <c>not-built</c> (we do not ship it here yet) or <c>disabled</c>
	/// (opted out for this agent). Everything else — no device, no driver, no ICD, a
	/// null context, a broken binding — is a test failure.
	/// </para>
	///
	/// <para>
	/// The table below describes <b>platforms</b>; <see cref="EnvironmentVariable"/>
	/// describes <b>agents</b>. See documentation/dev/gpu-test-policy.md.
	/// </para>
	/// </summary>
	public static class GpuPolicy
	{
		/// <summary>Backend ids to skip, or <c>all</c>. Comma/semicolon/space separated.</summary>
		public const string EnvironmentVariable = "SKIASHARP_TEST_SKIP_GPU";

		/// <summary>
		/// The same value for hosts that cannot read the agent environment (Android,
		/// iOS, Mac Catalyst, WASM), set from the SkiaSharpTestSkipGpu MSBuild property.
		/// </summary>
		public const string AppContextKey = "SkiaSharp.Tests.SkipGpu";

		// ExistsOn: where the API can ever exist. BuiltOn: where we actually ship it,
		// mirroring the gn args in native/*/build.cake (skia_use_metal, skia_use_vulkan,
		// skia_use_dawn, skia_use_direct3d) — keep the two in sync.
		private static readonly (GpuBackend Backend, string Id, TestPlatforms ExistsOn, TestPlatforms BuiltOn)[] backends =
		{
			(GpuBackend.Cpu, "raster", TestPlatforms.All, TestPlatforms.All),
			(GpuBackend.GaneshGl, "ganesh-gl", TestPlatforms.All, TestPlatforms.Desktop | TestPlatforms.NanoServer),
			(GpuBackend.GaneshVulkan, "ganesh-vulkan", TestPlatforms.All & ~TestPlatforms.Browser, TestPlatforms.Windows | TestPlatforms.Linux | TestPlatforms.Android),
			(GpuBackend.GraphiteVulkan, "graphite-vulkan", TestPlatforms.All & ~TestPlatforms.Browser, TestPlatforms.Windows | TestPlatforms.Linux | TestPlatforms.Android),
			(GpuBackend.GaneshVulkanSharpVk, "ganesh-vulkan-sharpvk", TestPlatforms.All & ~TestPlatforms.Browser, TestPlatforms.Windows),
			(GpuBackend.GaneshMetal, "ganesh-metal", TestPlatforms.Apple, TestPlatforms.Apple),
			(GpuBackend.GraphiteMetal, "graphite-metal", TestPlatforms.Apple, TestPlatforms.Apple),
			(GpuBackend.GaneshDirect3D, "ganesh-direct3d", TestPlatforms.AnyWindows, TestPlatforms.Windows),
			(GpuBackend.GraphiteDawn, "graphite-dawn", TestPlatforms.All, TestPlatforms.Browser),
		};

		// Lazy caches the parse exception, so a malformed opt-out list fails every GPU
		// test identically instead of only the first one to look.
		private static readonly Lazy<HashSet<GpuBackend>> disabled = new(ParseOptOut);
		private static readonly Lazy<TestPlatforms> platform = new(DetectPlatform);

		public static TestPlatforms Platform => platform.Value;

		public static string PlatformName =>
			Platform == TestPlatforms.None ? "unknown" : Platform.ToString().ToLowerInvariant();

		public static string Id(GpuBackend backend) => Find(backend).Id;

		/// <summary>Forces the opt-out list to parse, throwing if it names an unknown id.</summary>
		public static void Validate() => _ = disabled.Value;

		/// <summary>
		/// The state of <paramref name="backend"/> on this host, and why it need not
		/// work. <c>Reason</c> is <see langword="null"/> exactly when it must.
		/// </summary>
		public static (string State, string Reason) Resolve(GpuBackend backend)
		{
			var b = Find(backend);

			if (backend == GpuBackend.Cpu)
				return ("required", null);
			if (disabled.Value.Contains(backend))
				return ("disabled", $"'{b.Id}' is disabled on this host via {OptOutSource()}.");
			if ((b.ExistsOn & Platform) == 0)
				return ("unsupported", $"'{b.Id}' does not exist on {PlatformName}.");
			if ((b.BuiltOn & Platform) == 0)
				return ("not-built", $"'{b.Id}' is not built for {PlatformName}.");

			return ("required", null);
		}

		/// <summary>
		/// Skips when <paramref name="backend"/> need not work here, and otherwise
		/// returns so the caller can bring it up <b>without</b> a catch.
		/// </summary>
		public static void RequireOrSkip(GpuBackend backend)
		{
			var (state, reason) = Resolve(backend);
			if (reason is not null)
				Assert.Skip($"[{state}] {reason}");
		}

		/// <summary>
		/// Append to a failure from a required backend so a red test names the exact
		/// opt-out that would legitimise a skip.
		/// </summary>
		public static string OptOutHint(GpuBackend backend) =>
			$"If this host genuinely cannot run '{Id(backend)}', declare it: set " +
			$"{EnvironmentVariable}={Id(backend)} (or build with -p:SkiaSharpTestSkipGpu={Id(backend)}).";

		/// <summary>One line per backend for the ##SKIA-GPU-POLICY## marker.</summary>
		public static IEnumerable<string> Describe() =>
			backends.Select(b =>
			{
				var (state, reason) = Resolve(b.Backend);
				return $"backend={b.Id} state={state}" + (reason is null ? "" : $" reason={reason}");
			});

		private static (GpuBackend Backend, string Id, TestPlatforms ExistsOn, TestPlatforms BuiltOn) Find(GpuBackend backend)
		{
			foreach (var b in backends)
			{
				if (b.Backend == backend)
					return b;
			}
			throw new ArgumentOutOfRangeException(nameof(backend), backend, "Unknown GPU backend.");
		}

		private static string OptOutSource() =>
			AppContextValue() is not null ? AppContextKey : EnvironmentVariable;

		private static string AppContextValue() =>
			AppContext.GetData(AppContextKey) is string v && !string.IsNullOrWhiteSpace(v) ? v : null;

		private static HashSet<GpuBackend> ParseOptOut()
		{
			var result = new HashSet<GpuBackend>();

			var raw = AppContextValue() ?? Environment.GetEnvironmentVariable(EnvironmentVariable);
			if (string.IsNullOrWhiteSpace(raw))
				return result;

			var unknown = new List<string>();

			foreach (var token in raw.Split(new[] { ',', ';', ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
			{
				if (string.Equals(token, "all", StringComparison.OrdinalIgnoreCase))
				{
					foreach (var b in backends.Where(b => b.Backend != GpuBackend.Cpu))
						result.Add(b.Backend);
				}
				else if (backends.FirstOrDefault(b => b.Backend != GpuBackend.Cpu && string.Equals(b.Id, token, StringComparison.OrdinalIgnoreCase)) is { Id: not null } match)
				{
					result.Add(match.Backend);
				}
				else
				{
					unknown.Add(token);
				}
			}

			// A typo must never quietly leave a backend required — that is the same
			// silent hole this policy exists to close, moved into the pipeline.
			if (unknown.Count > 0)
			{
				var known = string.Join(", ", backends.Where(b => b.Backend != GpuBackend.Cpu).Select(b => b.Id));
				throw new InvalidOperationException(
					$"{EnvironmentVariable} names unknown backends: '{string.Join("', '", unknown)}'. " +
					$"Valid ids are: {known}, or 'all'.");
			}

			return result;
		}

		// Most specific first: Mac Catalyst also reports IsIOS, iOS/tvOS can report
		// IsMacOS, and Nano Server is Windows. Mirrors VisualPlatform.DetermineTag.
		private static TestPlatforms DetectPlatform()
		{
#if NET5_0_OR_GREATER
			if (OperatingSystem.IsBrowser())
				return TestPlatforms.Browser;
			if (OperatingSystem.IsAndroid())
				return TestPlatforms.Android;
			if (OperatingSystem.IsMacCatalyst())
				return TestPlatforms.MacCatalyst;
			if (OperatingSystem.IsIOS())
				return TestPlatforms.IOS;
			if (OperatingSystem.IsTvOS())
				return TestPlatforms.TvOS;
			if (OperatingSystem.IsMacOS())
				return TestPlatforms.MacOS;
			if (OperatingSystem.IsWindows())
				return WindowsFlavor();
			if (OperatingSystem.IsLinux())
				return TestPlatforms.Linux;
#else
			if (TestConfig.Current.IsMac)
				return TestPlatforms.MacOS;
			if (TestConfig.Current.IsWindows)
				return WindowsFlavor();
			if (TestConfig.Current.IsLinux)
				return TestPlatforms.Linux;
#endif
			return TestPlatforms.None;
		}

		private static TestPlatforms WindowsFlavor() =>
			TestConfig.Current.IsNanoServer ? TestPlatforms.NanoServer : TestPlatforms.Windows;
	}
}
