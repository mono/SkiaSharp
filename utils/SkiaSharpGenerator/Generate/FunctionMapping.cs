using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SkiaSharpGenerator
{
	public class FunctionMapping
	{
		[JsonPropertyName("cs")]
		public string? CsType { get; set; }

		[JsonPropertyName("parameters")]
		public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
	}
}
