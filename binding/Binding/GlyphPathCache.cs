using System;
using System.Collections.Generic;

namespace SkiaSharp
{
	internal sealed class GlyphPathCache : Dictionary<ushort, SKPath>, IDisposable
	{
		public SKFont Font { get; }

		public GlyphPathCache (SKFont font)
		{
			Font = font;
		}

		public SKPath GetPath (ushort glyphId)
		{
			if (!TryGetValue (glyphId, out var glyphPath)) {
				glyphPath = Font.GetPath (glyphId);
				this[glyphId] = glyphPath;
			}

			return glyphPath;
		}

		public void Dispose ()
		{
			foreach (var path in Values) {
				path.Dispose ();
			}
		}
	}
}
