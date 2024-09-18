using Xunit;
using System;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests;

public class PlaceholderTest
{
	private const string SKIA = "libSkiaSharp";
	private const string HARFBUZZ = "libHarfBuzzSharp";

	[Fact]
	public void CheckVersion()
	{
		var str = Marshal.PtrToStringAnsi(sk_version_get_string());
		var milestone = sk_version_get_milestone();
		var increment = sk_version_get_increment();

		Assert.True(milestone > 0);
		Assert.True(increment >= 0);
		Assert.Equal($"{milestone}.{increment}", str);
	}

	[Fact]
	public void CheckHarfBuzz()
	{
		const int LATIN = 1281455214;
		const int LTR = 4;

		var dir = hb_script_get_horizontal_direction(LATIN);

		Assert.Equal(LTR, dir);
	}

	[Fact]
	public void CanSerializeAndDeserializePicture()
	{
		using var recorder = new SKPictureRecorder();
		using var canvas = recorder.BeginRecording(SKRect.Create(0, 0, 40, 40));
		using var picture = recorder.EndRecording();

		using var data = picture.Serialize();

		using var deserialized = SKPicture.Deserialize(data);

		Assert.NotNull(deserialized);
	}

	[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
	static extern IntPtr sk_version_get_string();

	[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
	static extern int sk_version_get_milestone();

	[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
	static extern int sk_version_get_increment();

	[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
	static extern int hb_script_get_horizontal_direction(int script);
}
