using System;
using System.IO;
using System.Runtime.CompilerServices;
using SkiaSharp.Internals;

namespace SkiaSharp.Tests
{
	public abstract class TestConfig
	{
		private static readonly Lazy<DefaultTestConfig> _defaultConfig = new();

		private static TestConfig _current;

		public static TestConfig Current
		{
			get => _current ?? _defaultConfig.Value;
			set => _current = value;
		}

		public bool IsLinux => PlatformConfiguration.IsLinux;
		public bool IsMac => PlatformConfiguration.IsMac;
		public bool IsUnix => PlatformConfiguration.IsUnix;
		public bool IsWindows => PlatformConfiguration.IsWindows;
		public bool IsGlibc => PlatformConfiguration.IsGlibc;
		public bool IsMusl => PlatformConfiguration.IsLinux && !PlatformConfiguration.IsGlibc;
		public bool IsNanoServer => _isNanoServer.Value;

		/// <summary>The current host as a single flag.</summary>
		public TestPlatforms Platform => _platform;

		/// <summary>
		/// Lowercase name of the current host — <c>"macos"</c>, <c>"nanoserver"</c>,
		/// <c>"unknown"</c>. Also the golden directory tag (see VisualPlatform).
		/// </summary>
		public string PlatformName =>
			Platform == TestPlatforms.None ? "unknown" : Platform.ToString().ToLowerInvariant();

		private static readonly Lazy<bool> _isNanoServer = new(DetectNanoServer);

		// Windows Nano Server identifies itself in the registry as InstallationType
		// "Nano Server" (full Windows reports "Client"/"Server", Server Core reports
		// "Server Core"). Guard on Windows first, and keep the registry read in a
		// separate non-inlined method so the Microsoft.Win32 types are only resolved
		// (JIT-compiled) on Windows and never on the mobile/WASM test hosts.
		private static bool DetectNanoServer() =>
			PlatformConfiguration.IsWindows && ReadIsNanoServer();

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static bool ReadIsNanoServer()
		{
			try
			{
				using var key = Microsoft.Win32.Registry.LocalMachine
					.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
				return string.Equals(
					key?.GetValue("InstallationType") as string,
					"Nano Server",
					StringComparison.OrdinalIgnoreCase);
			}
			catch
			{
				// A constrained host can deny the registry read (SecurityException,
				// UnauthorizedAccessException, etc.). Treat any failure as "not Nano Server"
				// so a capability probe never fails the whole test run during config evaluation.
				return false;
			}
		}

		public bool IsAndroid =>
#if NET5_0_OR_GREATER
			OperatingSystem.IsAndroid();
#else
			false;
#endif

		public bool IsApple =>
#if NET5_0_OR_GREATER
			OperatingSystem.IsMacOS()
			|| OperatingSystem.IsIOS()
			|| OperatingSystem.IsMacCatalyst()
			|| OperatingSystem.IsTvOS();
#else
			// net48 (Windows-only TFM) predates the OperatingSystem.Is* probes; the
			// only Apple host it could ever be is desktop macOS, which the
			// RuntimeInformation-based IsMac flag detects.
			IsMac;
#endif

		public bool IsBrowser =>
#if NET5_0_OR_GREATER
			OperatingSystem.IsBrowser();
#else
			false;
#endif

		// Declared after _isNanoServer, which DetectPlatform reads: static field
		// initializers run in declaration order. Deliberately does not go through
		// Current, which would recurse into DefaultTestConfig's constructor.
		private static readonly TestPlatforms _platform = DetectPlatform();

		// Most specific first: Mac Catalyst also reports IsIOS, iOS/tvOS can report
		// IsMacOS, and Nano Server is Windows.
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
#endif
			if (PlatformConfiguration.IsWindows)
				return _isNanoServer.Value ? TestPlatforms.NanoServer : TestPlatforms.Windows;
			if (PlatformConfiguration.IsMac)
				return TestPlatforms.MacOS;
			if (PlatformConfiguration.IsLinux)
				return TestPlatforms.Linux;

			return TestPlatforms.None;
		}

		public string[] UnicodeFontFamilies { get; protected set; }
		public string DefaultFontFamily { get; protected set; }

		public string PathRoot { get; protected set; }
		public string PathToFonts => Path.Combine(PathRoot, "Content", "fonts");
		public string PathToImages => Path.Combine(PathRoot, "Content", "images");

		public virtual GlContext CreateGlContext() =>
			throw new PlatformNotSupportedException();
	}
}
