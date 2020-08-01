using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SkiaSharpSample
{
	public static class SamplesManager
	{
		private static readonly SampleBase[] sampleList;

		static SamplesManager()
		{
			var samplesBase = typeof(SampleBase).GetTypeInfo();
			var assembly = samplesBase.Assembly;

			sampleList = assembly.DefinedTypes
				.Where(t => samplesBase.IsAssignableFrom(t) && !t.IsAbstract)
				.Select(t => (SampleBase)Activator.CreateInstance(t.AsType()))
				.ToArray();

			SkiaSharpVersion = GetAssemblyVersion<SkiaSharp.SKSurface>();
#if !HAS_UNO
			HarfBuzzSharpVersion = GetAssemblyVersion<HarfBuzzSharp.Blob>();
#endif
		}

		public static string SkiaSharpVersion { get; }

		public static string HarfBuzzSharpVersion { get; }

		public static string TempDataPath { get; set; }

		public static string EnsureTempDataDirectory(string name)
		{
			var root = Path.Combine(TempDataPath, name);
			if (!Directory.Exists(root))
				Directory.CreateDirectory(root);
			return root;
		}

		public static string ContentFontPath
		{
			get { return SampleMedia.Fonts.ContentFontPath; }
			set { SampleMedia.Fonts.ContentFontPath = value; }
		}

		public static event Action<string> OpenFile;

		public static void OnOpenFile(string path)
		{
			OpenFile?.Invoke(path);
		}

		public static IEnumerable<string> GetSampleTitles(SamplePlatforms platform)
		{
			return sampleList.Where(s => s.SupportedPlatform.HasFlag(platform)).Select(s => s.Title);
		}

		public static IEnumerable<SampleBase> GetSamples(SamplePlatforms platform)
		{
			return sampleList.Where(s => s.SupportedPlatform.HasFlag(platform));
		}

		public static SampleBase GetSample(string title)
		{
			return sampleList.Where(s => s.Title == title).FirstOrDefault();
		}

		private static string GetAssemblyVersion<T>()
		{
			var apiAssembly = typeof(T).Assembly;
			var attributes = apiAssembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute));
			var attribute = (AssemblyInformationalVersionAttribute)attributes.FirstOrDefault();
			return attribute?.InformationalVersion ?? "<unavailable>";
		}
	}
}
