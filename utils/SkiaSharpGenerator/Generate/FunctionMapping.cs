using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SkiaSharpGenerator
{
	public class FunctionMapping
	{
		[JsonPropertyName("parameters")]
		public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
	}
}
