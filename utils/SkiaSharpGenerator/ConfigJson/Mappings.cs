using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SkiaSharpGenerator
{
	public class Mappings
	{
		[JsonPropertyName("types")]
		public Dictionary<string, TypeMapping> Types { get; set; } = new Dictionary<string, TypeMapping>();

		[JsonPropertyName("functions")]
		public Dictionary<string, FunctionMapping> Functions { get; set; } = new Dictionary<string, FunctionMapping>();
	}
}
