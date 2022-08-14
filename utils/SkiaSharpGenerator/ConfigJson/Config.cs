using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SkiaSharpGenerator
{
	public class Config
	{
		[JsonPropertyName("mappings")]
		public Mappings Mappings { get; set; } = new Mappings();

		[JsonPropertyName("headers")]
		public Dictionary<string, string[]> Headers { get; set; } = new Dictionary<string, string[]>();

		[JsonPropertyName("source")]
		public Dictionary<string, string[]> Source { get; set; } = new Dictionary<string, string[]>();

		[JsonPropertyName("namespaces")]
		public Dictionary<string, NamespaceMapping> Namespaces { get; set; } = new Dictionary<string, NamespaceMapping>();

		[JsonPropertyName("exclude")]
		public Exclude Exclude { get; set; } = new Exclude();

		[JsonPropertyName("includeDirs")]
		public List<string> IncludeDirs { get; set; } = new List<string>();

		[JsonPropertyName("dllName")]
		public string DllName { get; set; } = "DllName";

		[JsonPropertyName("namespace")]
		public string Namespace { get; set; } = "Namespace";

		[JsonPropertyName("className")]
		public string ClassName { get; set; } = "ClassName";
	}
}
