using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using NUnit.Framework;

using SkiaSharp;
using SkiaSharp.HarfBuzz;

namespace HarfBuzzSharp.Tests
{
	public class SKShaperTest : TestBase
	{
		[Test]
		public void DrawShapedTextExtensionMethodDraws()
		{
			using (var surface = SKSurface.Create(new SKImageInfo(512, 512)))
			using (var tf = SKTypeface.FromFamilyName("Tahoma"))
			using (var shaper = new SKShaper(tf))
			using (var paint = new SKPaint { IsAntialias = true, TextSize = 64, Typeface = tf })
			{
				surface.Canvas.Clear(SKColors.White);

				surface.Canvas.DrawShapedText(shaper, "متن", 100, 200, paint);

				surface.Canvas.Flush();

				using (var img = surface.Snapshot())
				using (var data = img.Encode(SKEncodedImageFormat.Png, 100))
				using (var stream = File.OpenWrite(Path.Combine(PathToImages, "test.png")))
				{
					data.AsStream().CopyTo(stream);
				}
			}
		}

		[Test]
		public void CorrectlyShapesArabicScriptAtAnOffset()
		{
			var clusters = new uint[] { 4, 2, 0 };
			var codepoints = new uint[] { 998, 920, 995 };
			var points = new SKPoint[] { new SKPoint(100, 200), new SKPoint(148.25f, 200), new SKPoint(170.75f, 200) };

			using (var tf = SKTypeface.FromFamilyName("Tahoma"))
			using (var shaper = new SKShaper(tf))
			using (var paint = new SKPaint { IsAntialias = true, TextSize = 64, Typeface = tf })
			{
				var result = shaper.Shape("متن", 100, 200, paint);

				CollectionAssert.AreEqual(clusters, result.Clusters);
				CollectionAssert.AreEqual(codepoints, result.Codepoints);
				CollectionAssert.AreEqual(points, result.Points);
			}
		}

		[Test]
		public void CorrectlyShapesArabicScript()
		{
			var clusters = new uint[] { 4, 2, 0 };
			var codepoints = new uint[] { 998, 920, 995 };
			var points = new SKPoint[] { new SKPoint(0, 0), new SKPoint(48.25f, 0), new SKPoint(70.75f, 0) };

			using (var tf = SKTypeface.FromFamilyName("Tahoma"))
			using (var shaper = new SKShaper(tf))
			using (var paint = new SKPaint { IsAntialias = true, TextSize = 64, Typeface = tf })
			{
				var result = shaper.Shape("متن", paint);

				CollectionAssert.AreEqual(clusters, result.Clusters);
				CollectionAssert.AreEqual(codepoints, result.Codepoints);
				CollectionAssert.AreEqual(points, result.Points);
			}
		}
	}

	public abstract class TestBase
	{
		protected static readonly string PathToAssembly = Path.GetDirectoryName(typeof(TestBase).GetTypeInfo().Assembly.Location);
		protected static readonly string PathToFonts = Path.Combine(PathToAssembly, "fonts");
		protected static readonly string PathToImages = Path.Combine(PathToAssembly, "images");

#if NET_STANDARD
		protected static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
		protected static bool IsMac => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
		protected static bool IsUnix => IsLinux || IsMac;
		protected static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#else
		private static class MacPlatformDetector
		{
			internal static readonly Lazy<bool> IsMac = new Lazy<bool>(IsRunningOnMac);

			[DllImport("libc")]
			static extern int uname(IntPtr buf);

			static bool IsRunningOnMac()
			{
				IntPtr buf = IntPtr.Zero;
				try
				{
					buf = Marshal.AllocHGlobal(8192);
					// This is a hacktastic way of getting sysname from uname ()
					if (uname(buf) == 0)
					{
						string os = Marshal.PtrToStringAnsi(buf);
						if (os == "Darwin")
							return true;
					}
				}
				catch
				{
				}
				finally
				{
					if (buf != IntPtr.Zero)
						Marshal.FreeHGlobal(buf);
				}
				return false;
			}
		}

		protected static bool IsMac => MacPlatformDetector.IsMac.Value;
		protected static bool IsUnix => Environment.OSVersion.Platform == PlatformID.Unix || IsMac;
		protected static bool IsLinux => IsUnix && !IsMac;
		protected static bool IsWindows => !IsUnix;
#endif
	}
}