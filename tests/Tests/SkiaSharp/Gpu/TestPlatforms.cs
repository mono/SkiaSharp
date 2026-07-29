using System;

namespace SkiaSharp.Tests
{
	/// <summary>
	/// The host platforms the test suite runs on, as flags so a GPU backend can
	/// declare the whole set of platforms it exists on — or is built for — in a
	/// single value (see <see cref="GpuPolicy"/>).
	///
	/// <para>
	/// This is deliberately a test-only concept: it names the hosts we build and
	/// run <c>libSkiaSharp</c> on, not every OS .NET can target.
	/// </para>
	/// </summary>
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

		/// <summary>
		/// Windows Nano Server. Tracked separately from <see cref="Windows"/>
		/// because it runs a <i>different native build</i>:
		/// <c>native/nanoserver/build.cake</c> passes <c>supportVulkan=false</c> and
		/// <c>supportDirect3D=false</c>, so those backends are absent from
		/// <c>libSkiaSharp.dll</c> there even though the OS is Windows.
		/// </summary>
		NanoServer = 1 << 8,

		/// <summary>The Apple platforms — the only ones that have Metal.</summary>
		Apple = MacOS | IOS | MacCatalyst | TvOS,

		/// <summary>The three desktop hosts the Console test app runs on.</summary>
		Desktop = Windows | MacOS | Linux,

		/// <summary>Every host running a Windows kernel, full or Nano.</summary>
		AnyWindows = Windows | NanoServer,

		All = Windows | MacOS | Linux | Android | IOS | MacCatalyst | TvOS | Browser | NanoServer,
	}

	/// <summary>
	/// Identifies the platform the tests are currently running on as a single
	/// <see cref="TestPlatforms"/> flag.
	/// </summary>
	public static class TestPlatform
	{
		private static readonly Lazy<TestPlatforms> current = new Lazy<TestPlatforms>(Detect);

		/// <summary>
		/// The current host, or <see cref="TestPlatforms.None"/> if it could not
		/// be identified. <c>None</c> is never expected — <c>GpuPolicyTests</c>
		/// fails on it rather than letting an unknown host quietly classify every
		/// GPU backend as unavailable.
		/// </summary>
		public static TestPlatforms Current => current.Value;

		/// <summary>Lowercase name of the current host, for log output.</summary>
		public static string Name =>
			Current == TestPlatforms.None ? "unknown" : Current.ToString().ToLowerInvariant();

		// Order matters: Mac Catalyst also reports IsIOS, and iOS/tvOS also report
		// IsMacOS on some runtimes, so the most specific probe has to win. This
		// mirrors VisualPlatform.DetermineTag, which builds the golden directory
		// tags from the same distinctions. Nano Server is likewise probed before
		// Windows, because it IS Windows but ships a different native build.
		private static TestPlatforms Detect()
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
				return TestConfig.Current.IsNanoServer ? TestPlatforms.NanoServer : TestPlatforms.Windows;
			if (OperatingSystem.IsLinux())
				return TestPlatforms.Linux;
			return TestPlatforms.None;
#else
			// net48 is a Windows-only TFM in practice, but keep the same shape as
			// the rest of the suite and derive from the RuntimeInformation probes.
			if (TestConfig.Current.IsMac)
				return TestPlatforms.MacOS;
			if (TestConfig.Current.IsWindows)
				return TestConfig.Current.IsNanoServer ? TestPlatforms.NanoServer : TestPlatforms.Windows;
			if (TestConfig.Current.IsLinux)
				return TestPlatforms.Linux;
			return TestPlatforms.None;
#endif
		}
	}
}
