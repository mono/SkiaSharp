using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SkiaSharpGenerator
{
	public class Exclude
	{
		[JsonPropertyName("files")]
		public List<string> Files { get; set; } = new List<string>();

		[JsonPropertyName("types")]
		public List<string> Types { get; set; } = new List<string>();
	}
}
