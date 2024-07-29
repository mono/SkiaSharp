using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace SkiaSharpGenerator
{
	public class FunctionMapping
	{
		[JsonPropertyName("cs")]
		public string? CsType { get; set; }

		[JsonPropertyName("parameters")]
		public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();

		[JsonPropertyName("generateProxy")]
		public bool? GenerateProxy { get; set; }

		[JsonPropertyName("proxySuffixes")]
		public List<string>? ProxySuffixes { get; set; }
	}
}
