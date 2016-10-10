using System;

namespace SkiaSharpSample
{
	// iOS has a very strict linker
	internal sealed class PreserveAttribute : Attribute
	{
		public bool AllMembers { get; set; }

		public bool Conditional { get; set; }
	}
}
