using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SkiaSharpGenerator
{
	public class Exclude
	{
		[JsonPropertyName("files")]
		public List<string> Files { get; set; } = new List<string>();

		[JsonPropertyName("symbols")]
		public List<string> Symbols { get; set; } = new List<string>();
	}
}
