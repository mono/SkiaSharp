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
		public bool IsNanoServer => _isNanoServer.Value;

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
			using var key = Microsoft.Win32.Registry.LocalMachine
				.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
			return string.Equals(
				key?.GetValue("InstallationType") as string,
				"Nano Server",
				StringComparison.OrdinalIgnoreCase);
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
