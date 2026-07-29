using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SkiaSharp.Tests
{
	/// <summary>
	/// Stable backend ids. Also the visual-matrix renderer names and the golden
	/// directory names, so one word identifies a backend everywhere.
	/// </summary>
	public static class GpuBackends
	{
		public const string Raster = "raster";
		public const string GaneshGl = "ganesh-gl";
		public const string GaneshVulkan = "ganesh-vulkan";
		public const string GaneshVulkanSharpVk = "ganesh-vulkan-sharpvk";
		public const string GaneshMetal = "ganesh-metal";
		public const string GaneshDirect3D = "ganesh-direct3d";
		public const string GraphiteVulkan = "graphite-vulkan";
		public const string GraphiteMetal = "graphite-metal";
		public const string GraphiteDawn = "graphite-dawn";
	}

	/// <summary>
	/// Decides whether a GPU backend must work on this host, and is the only place
	/// in the suite allowed to skip a GPU test. Anything not skipped must work: no
	/// device, no driver, no ICD, a null context or a broken binding is a failure.
	///
	/// <para>
	/// <see cref="requiredOn"/> describes <b>platforms</b>;
	/// <see cref="EnvironmentVariable"/> describes <b>agents</b>. See
	/// documentation/dev/gpu-test-policy.md.
	/// </para>
	/// </summary>
	public static class GpuPolicy
	{
		/// <summary>Backend ids to skip on this agent, or <c>all</c>.</summary>
		public const string EnvironmentVariable = "SKIASHARP_TEST_SKIP_GPU";

		/// <summary>
		/// The same list for hosts that cannot read the agent environment (Android,
		/// iOS, Mac Catalyst, WASM), from the SkiaSharpTestSkipGpu MSBuild property.
		/// </summary>
		public const string AppContextKey = "SkiaSharp.Tests.SkipGpu";

		// Where each backend must work. Mirrors the gn args in native/*/build.cake
		// (skia_use_metal, skia_use_vulkan, skia_use_dawn, skia_use_direct3d).
		private static readonly Dictionary<string, TestPlatforms> requiredOn = new()
		{
			[GpuBackends.Raster] = TestPlatforms.All,
			[GpuBackends.GaneshGl] = TestPlatforms.Desktop | TestPlatforms.NanoServer,
			[GpuBackends.GaneshVulkan] = TestPlatforms.Windows | TestPlatforms.Linux | TestPlatforms.Android,
			[GpuBackends.GraphiteVulkan] = TestPlatforms.Windows | TestPlatforms.Linux | TestPlatforms.Android,
			[GpuBackends.GaneshVulkanSharpVk] = TestPlatforms.Windows,
			[GpuBackends.GaneshMetal] = TestPlatforms.Apple,
			[GpuBackends.GraphiteMetal] = TestPlatforms.Apple,
			[GpuBackends.GaneshDirect3D] = TestPlatforms.Windows,
			[GpuBackends.GraphiteDawn] = TestPlatforms.Browser,
		};

		// Backends this agent was told it cannot run. Declared after requiredOn,
		// which ParseOptOut reads: static initializers run in declaration order.
		private static readonly HashSet<string> disabled = ParseOptOut();

		/// <summary>Every known backend id.</summary>
		public static IEnumerable<string> All => requiredOn.Keys;

		/// <summary>Throws if the opt-out list names an unknown backend.</summary>
		public static void Validate() => _ = disabled.Count;

		/// <summary>
		/// Skips when <paramref name="backend"/> need not work here, and otherwise
		/// returns so the caller can bring it up <b>without</b> a catch.
		/// </summary>
		public static void RequireOrSkip(string backend)
		{
			if (SkipReason(backend) is { } reason)
				Assert.Skip(reason);
		}

		private static string SkipReason(string backend)
		{
			if (!requiredOn.TryGetValue(backend, out var platforms))
				throw new ArgumentException($"Unknown GPU backend '{backend}'.", nameof(backend));

			if (disabled.Contains(backend))
				return $"'{backend}' is disabled for this host via {EnvironmentVariable}.";

			if ((platforms & TestConfig.Current.Platform) == 0)
				return $"'{backend}' is not required on {TestConfig.Current.PlatformName}.";

			return null;
		}

		private static HashSet<string> ParseOptOut()
		{
			var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			var raw = AppContext.GetData(AppContextKey) as string;
			if (string.IsNullOrWhiteSpace(raw))
				raw = Environment.GetEnvironmentVariable(EnvironmentVariable);
			if (string.IsNullOrWhiteSpace(raw))
				return result;

			// raster is CPU: "all" never disables it, and naming it is an error.
			var gpu = requiredOn.Keys.Where(k => k != GpuBackends.Raster).ToList();
			var unknown = new List<string>();

			foreach (var token in raw.Split(new[] { ',', ';', ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
			{
				if (string.Equals(token, "all", StringComparison.OrdinalIgnoreCase))
					result.UnionWith(gpu);
				else if (gpu.FirstOrDefault(k => string.Equals(k, token, StringComparison.OrdinalIgnoreCase)) is { } id)
					result.Add(id);
				else
					unknown.Add(token);
			}

			// A typo must never quietly leave a backend required.
			if (unknown.Count > 0)
			{
				throw new InvalidOperationException(
					$"{EnvironmentVariable} names unknown backends: '{string.Join("', '", unknown)}'. " +
					$"Valid ids are: {string.Join(", ", gpu)}, or 'all'.");
			}

			return result;
		}
	}
}
