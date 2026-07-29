using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SkiaSharp.Tests
{
	/// <summary>
	/// Decides whether a GPU backend is <b>required</b> on this host or may be
	/// skipped, and why. This is the single skip seam for GPU work in the whole
	/// test suite: nothing else may convert a failed backend bring-up into a
	/// skip.
	///
	/// <para>
	/// The guiding rule is that <b>the matrix describes platforms and the
	/// environment variable describes agents</b>. Nothing about a platform's
	/// inherent capabilities is ever expressed as configuration — Metal on
	/// Windows and Vulkan on macOS need no setup to skip. Configuration exists
	/// only for "this backend should work here, but this particular machine
	/// can't run it".
	/// </para>
	///
	/// <para>
	/// Anything not skipped is <see cref="GpuAvailability.Required"/>: no device,
	/// no driver, no ICD, no display, a null context or a broken binding are all
	/// <b>test failures</b>. That is the entire point — a silent skip is how a
	/// regression hides.
	/// </para>
	/// </summary>
	public static class GpuPolicy
	{
		/// <summary>
		/// Comma/semicolon/whitespace separated list of backend ids to skip on
		/// this host, or <c>all</c>. Case-insensitive. An unrecognised id is a
		/// hard error, so a typo can never silently leave a backend required.
		/// </summary>
		public const string EnvironmentVariable = "SKIASHARP_TEST_SKIP_GPU";

		/// <summary>
		/// The same value, for hosts that cannot read the agent's environment
		/// (Android, iOS, Mac Catalyst, WASM). Populated from the
		/// <c>SkiaSharpTestSkipGpu</c> MSBuild property via
		/// <c>RuntimeHostConfigurationOption</c>; see <c>tests/Directory.Build.targets</c>.
		/// </summary>
		public const string AppContextKey = "SkiaSharp.Tests.SkipGpu";

		/// <summary>Opts every GPU backend out in one word.</summary>
		public const string SkipAllToken = "all";

		private static readonly char[] separators = { ',', ';', ' ', '\t', '\r', '\n' };

		// One row per backend.
		//
		// ExistsOn — platforms where the API can ever exist. Outside it the
		//            backend is Unsupported: a permanent fact, never configured.
		// BuiltOn  — the subset we actually build and wire up today, taken from
		//            the gn args in native/*/build.cake (skia_use_metal,
		//            skia_use_vulkan, skia_use_dawn). Inside ExistsOn but outside
		//            BuiltOn the backend is NotBuilt.
		//
		// KEEP IN SYNC with native/*/build.cake. Enabling a backend on a new
		// platform is a one-token change to its BuiltOn set, after which that
		// platform's cells become required and must pass.
		private static readonly Entry[] entries =
		{
			new Entry(
				GpuBackend.Cpu, "raster",
				existsOn: TestPlatforms.All,
				builtOn: TestPlatforms.All,
				unsupportedReason: null,
				notBuiltReason: null),

			new Entry(
				GpuBackend.GaneshGl, "ganesh-gl",
				existsOn: TestPlatforms.All,
				builtOn: TestPlatforms.Desktop,
				unsupportedReason: null,
				notBuiltReason:
					"OpenGL is not wired up for this host: there is no GlContext implementation " +
					"outside the desktop hosts (WGL on Windows, CGL on macOS, GLX on Linux — see " +
					"tests/Tests/SkiaSharp/GlContexts). Add one and extend GpuPolicy to require it."),

			new Entry(
				GpuBackend.GaneshVulkan, "ganesh-vulkan",
				existsOn: TestPlatforms.All & ~TestPlatforms.Browser,
				builtOn: TestPlatforms.Windows | TestPlatforms.Linux | TestPlatforms.Android,
				unsupportedReason: "WebAssembly has no Vulkan.",
				notBuiltReason: VulkanNotBuilt),

			new Entry(
				GpuBackend.GraphiteVulkan, "graphite-vulkan",
				existsOn: TestPlatforms.All & ~TestPlatforms.Browser,
				builtOn: TestPlatforms.Windows | TestPlatforms.Linux | TestPlatforms.Android,
				unsupportedReason: "WebAssembly has no Vulkan.",
				notBuiltReason: VulkanNotBuilt),

			new Entry(
				GpuBackend.GaneshVulkanSharpVk, "ganesh-vulkan-sharpvk",
				existsOn: TestPlatforms.All & ~TestPlatforms.Browser,
				builtOn: TestPlatforms.Windows,
				unsupportedReason: "WebAssembly has no Vulkan.",
				notBuiltReason:
					"The legacy SharpVk vehicle can only create a Vulkan context on Windows; " +
					"ganesh-vulkan (Silk.NET) covers the other hosts."),

			new Entry(
				GpuBackend.GaneshMetal, "ganesh-metal",
				existsOn: TestPlatforms.Apple,
				builtOn: TestPlatforms.Apple,
				unsupportedReason: MetalUnsupported,
				notBuiltReason: null),

			new Entry(
				GpuBackend.GraphiteMetal, "graphite-metal",
				existsOn: TestPlatforms.Apple,
				builtOn: TestPlatforms.Apple,
				unsupportedReason: MetalUnsupported,
				notBuiltReason: null),

			new Entry(
				GpuBackend.GaneshDirect3D, "ganesh-direct3d",
				existsOn: TestPlatforms.Windows,
				builtOn: TestPlatforms.Windows,
				unsupportedReason: "Direct3D is a Windows-only API.",
				notBuiltReason: null),

			new Entry(
				GpuBackend.GraphiteDawn, "graphite-dawn",
				existsOn: TestPlatforms.All,
				builtOn: TestPlatforms.Browser,
				unsupportedReason: null,
				notBuiltReason:
					"Dawn/WebGPU is only built for WebAssembly — skia_use_dawn is set in " +
					"native/wasm/build.cake and nowhere else. Add the platform to BuiltOn when a " +
					"desktop or device build enables it."),
		};

		private const string MetalUnsupported =
			"Metal is an Apple-only API (macOS, iOS, Mac Catalyst, tvOS).";

		private const string VulkanNotBuilt =
			"Vulkan is not built for the Apple platforms today: native/macos/build.cake and " +
			"native/ios/build.cake do not set skia_use_vulkan (no MoltenVK in the build). Add the " +
			"platform to BuiltOn when they do.";

		// Lazy so the parse happens once and any error is raised identically on
		// every access: Lazy caches the exception, so a malformed opt-out list
		// fails every GPU test with the same message instead of only the first.
		private static readonly Lazy<HashSet<GpuBackend>> disabled =
			new Lazy<HashSet<GpuBackend>>(ParseOptOut);

		/// <summary>Every backend, in report order (<c>raster</c> first).</summary>
		public static IEnumerable<GpuBackend> All => entries.Select(e => e.Backend);

		/// <summary>The stable string id used in goldens, opt-outs and messages.</summary>
		public static string Id(GpuBackend backend) => GetEntry(backend).Id;

		/// <summary>
		/// Forces the opt-out list to be parsed, throwing if it names an id that
		/// does not exist. Called by <c>GpuPolicyTests</c> so a typo fails one
		/// obvious test rather than being discovered indirectly.
		/// </summary>
		public static void Validate() => _ = disabled.Value;

		/// <summary>
		/// Whether <paramref name="backend"/> must work on this host, and — when
		/// it need not — why.
		/// </summary>
		public static (GpuAvailability State, string Reason) Resolve(GpuBackend backend)
		{
			var entry = GetEntry(backend);

			// Raster is CPU-only: there is nothing to be unavailable.
			if (backend == GpuBackend.Cpu)
				return (GpuAvailability.Required, null);

			if (disabled.Value.Contains(backend))
				return (GpuAvailability.Disabled,
					$"'{entry.Id}' is explicitly disabled on this host via {OptOutSource()}.");

			var current = TestPlatform.Current;
			if (current == TestPlatforms.None)
				return (GpuAvailability.Unsupported,
					"The host platform could not be identified, so no GPU backend can be required here.");

			if ((entry.ExistsOn & current) == 0)
				return (GpuAvailability.Unsupported,
					entry.UnsupportedReason ?? $"'{entry.Id}' does not exist on {TestPlatform.Name}.");

			if ((entry.BuiltOn & current) == 0)
				return (GpuAvailability.NotBuilt,
					entry.NotBuiltReason ?? $"'{entry.Id}' is not built for {TestPlatform.Name}.");

			return (GpuAvailability.Required, null);
		}

		/// <summary>
		/// Skips the calling test when <paramref name="backend"/> is not required
		/// on this host, and otherwise returns so the caller can bring the
		/// backend up <b>without</b> a catch — any failure from here on is a real
		/// test failure. This is the only place in the suite that may skip a GPU
		/// test.
		/// </summary>
		public static void RequireOrSkip(GpuBackend backend)
		{
			var resolved = Resolve(backend);
			if (resolved.State != GpuAvailability.Required)
				Assert.Skip($"[{StateToken(resolved.State)}] {resolved.Reason}");
		}

		/// <summary>
		/// The sentence to append to a failure message from a required backend,
		/// naming the exact opt-out that would legitimise the skip. Every hard
		/// failure should carry it so the reader never has to guess the syntax.
		/// </summary>
		public static string OptOutHint(GpuBackend backend)
		{
			var id = Id(backend);
			return
				$"If this host genuinely cannot run '{id}', declare it: set {EnvironmentVariable}={id} " +
				$"(or build the device/browser host with -p:SkiaSharpTestSkipGpu={id}).";
		}

		/// <summary>
		/// One line per backend describing how it resolved on this host, for the
		/// <c>##SKIA-GPU-POLICY##</c> marker. The TRX is the only output channel
		/// present on every host, so this is how a CI leg reports which backends
		/// it actually required.
		/// </summary>
		public static IReadOnlyList<string> Describe()
		{
			var lines = new List<string>(entries.Length);
			foreach (var entry in entries)
			{
				var resolved = Resolve(entry.Backend);
				var line = $"backend={entry.Id} state={StateToken(resolved.State)}";
				if (!string.IsNullOrEmpty(resolved.Reason))
					line += $" reason={resolved.Reason}";
				lines.Add(line);
			}
			return lines;
		}

		/// <summary>Lowercase, hyphenated form of a state, for log output.</summary>
		public static string StateToken(GpuAvailability state) =>
			state switch
			{
				GpuAvailability.Required => "required",
				GpuAvailability.Disabled => "disabled",
				GpuAvailability.Unsupported => "unsupported",
				GpuAvailability.NotBuilt => "not-built",
				_ => state.ToString().ToLowerInvariant(),
			};

		private static Entry GetEntry(GpuBackend backend)
		{
			foreach (var entry in entries)
			{
				if (entry.Backend == backend)
					return entry;
			}
			throw new ArgumentOutOfRangeException(nameof(backend), backend, "Unknown GPU backend.");
		}

		private static string OptOutSource() =>
			AppContextValue() is not null
				? $"the {AppContextKey} runtime setting (-p:SkiaSharpTestSkipGpu)"
				: $"the {EnvironmentVariable} environment variable";

		private static string AppContextValue() =>
			AppContext.GetData(AppContextKey) is string value && !string.IsNullOrWhiteSpace(value)
				? value
				: null;

		private static HashSet<GpuBackend> ParseOptOut()
		{
			var result = new HashSet<GpuBackend>();

			var raw = AppContextValue() ?? Environment.GetEnvironmentVariable(EnvironmentVariable);
			if (string.IsNullOrWhiteSpace(raw))
				return result;

			var unknown = new List<string>();

			foreach (var token in raw.Split(separators, StringSplitOptions.RemoveEmptyEntries))
			{
				if (string.Equals(token, SkipAllToken, StringComparison.OrdinalIgnoreCase))
				{
					foreach (var entry in entries)
					{
						if (entry.Backend != GpuBackend.Cpu)
							result.Add(entry.Backend);
					}
					continue;
				}

				var match = entries.FirstOrDefault(e =>
					e.Backend != GpuBackend.Cpu &&
					string.Equals(e.Id, token, StringComparison.OrdinalIgnoreCase));

				if (match is null)
					unknown.Add(token);
				else
					result.Add(match.Backend);
			}

			// A typo must never quietly leave a backend required — that would be
			// the same silent hole this policy exists to close, just moved into
			// the pipeline definition.
			if (unknown.Count > 0)
			{
				var known = string.Join(", ", entries.Where(e => e.Backend != GpuBackend.Cpu).Select(e => e.Id));
				throw new InvalidOperationException(
					$"{EnvironmentVariable} names {(unknown.Count == 1 ? "an unknown backend" : "unknown backends")}: " +
					$"'{string.Join("', '", unknown)}'. Valid ids are: {known}, or '{SkipAllToken}'.");
			}

			return result;
		}

		private sealed class Entry
		{
			public Entry(
				GpuBackend backend,
				string id,
				TestPlatforms existsOn,
				TestPlatforms builtOn,
				string unsupportedReason,
				string notBuiltReason)
			{
				Backend = backend;
				Id = id;
				ExistsOn = existsOn;
				BuiltOn = builtOn;
				UnsupportedReason = unsupportedReason;
				NotBuiltReason = notBuiltReason;
			}

			public GpuBackend Backend { get; }

			public string Id { get; }

			/// <summary>Platforms where the API can ever exist.</summary>
			public TestPlatforms ExistsOn { get; }

			/// <summary>Platforms where we build and wire it up today.</summary>
			public TestPlatforms BuiltOn { get; }

			public string UnsupportedReason { get; }

			public string NotBuiltReason { get; }
		}
	}
}
