using System.Text.Json.Serialization;

namespace SkiaSharpGenerator
{
	public class NamespaceMapping
	{
		[JsonPropertyName("cs")]
		public string? CsName { get; set; }

		[JsonPropertyName("prefix")]
		public string? Prefix { get; set; }

		[JsonPropertyName("exclude")]
		public bool? Exclude { get; set; }
	}
}
