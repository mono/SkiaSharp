using System;
using System.IO;

namespace SkiaSharpSample;

public static class SamplesManager
{
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
}
