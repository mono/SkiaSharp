using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SkiaSharpGenerator
{
	public class TypeMapping
	{
		[JsonPropertyName("cs")]
		public string? CsType { get; set; }

		[JsonPropertyName("internal")]
		public bool IsInternal { get; set; } = false;

		[JsonPropertyName("flags")]
		public bool IsFlags { get; set; } = false;

		[JsonPropertyName("obsolete")]
		public bool IsObsolete { get; set; } = false;

		[JsonPropertyName("properties")]
		public bool GenerateProperties { get; set; } = true;

		[JsonPropertyName("readonly")]
		public bool IsReadOnly { get; set; } = false;

		[JsonPropertyName("equality")]
		public bool GenerateEquality { get; set; } = true;

		[JsonPropertyName("members")]
		public Dictionary<string, string> Members { get; set; } = new Dictionary<string, string>();
	}
}
