using System.IO;
using SkiaSharp.Tests;

namespace HarfBuzzSharp.Tests
{
	public class HBTest : BaseTest
	{
		protected static readonly Blob Blob;
		protected static readonly Face Face;
		protected static readonly Font Font;

		static HBTest()
		{
			Blob = Blob.FromFile(Path.Combine(PathToFonts, "content-font.ttf"));
			Blob.MakeImmutable();
			Face = new Face(Blob, 0);
			Font = new Font(Face);
		}
	}
}
