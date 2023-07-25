using System;
using System.IO;
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

		public bool IsRuntimeMono => Type.GetType("Mono.Runtime") != null;

		public string[] UnicodeFontFamilies { get; protected set; }
		public string DefaultFontFamily { get; protected set; }

		public string PathRoot { get; protected set; }
		public string PathToFonts => Path.Combine(PathRoot, "Content", "fonts");
		public string PathToImages => Path.Combine(PathRoot, "Content", "images");
	}
}
