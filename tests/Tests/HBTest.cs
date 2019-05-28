using System.IO;
using HarfBuzzSharp;

namespace SkiaSharp.Tests
{
	public class HBTest : SKTest
	{
		protected static readonly Blob Blob;
		protected static readonly Font Font;

		static HBTest()
		{
			Blob = Blob.FromFile(Path.Combine(PathToFonts, "content-font.ttf"));
			Blob.MakeImmutable();
			Font = new Font(new Face(Blob, 0));
		}
	}
}
