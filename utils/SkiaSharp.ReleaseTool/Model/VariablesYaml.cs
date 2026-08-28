using System.Text.RegularExpressions;
using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Model
{
	/// <summary>
	/// Read-only extraction of the two
	/// <c>scripts/azure-templates-variables.yml</c> lines the release
	/// tooling cares about. Ported from the regexes in Python's
	/// <c>release_prepare.py</c> (<c>_SKIASHARP_VERSION_RE</c>,
	/// <c>_PREVIEW_LABEL_RE</c>). Never a general-purpose YAML parser:
	/// only these two specific, known line shapes are recognised.
	/// </summary>
	public static partial class VariablesYaml
	{
		[GeneratedRegex(@"^\s*SKIASHARP_VERSION:\s*['""]?([^'""\s]+)", RegexOptions.Multiline)]
		private static partial Regex SkiaSharpVersionLine();

		[GeneratedRegex(@"^\s*PREVIEW_LABEL:\s*['""]?([^'""\r\n]+)", RegexOptions.Multiline)]
		private static partial Regex PreviewLabelLine();

		public static string ParseSkiaSharpVersion(string variablesText)
		{
			if (!TryParseSkiaSharpVersion(variablesText, out var value))
				throw new PlanException("could not parse SKIASHARP_VERSION");
			return value;
		}

		public static string ParsePreviewLabel(string variablesText)
		{
			if (!TryParsePreviewLabel(variablesText, out var value))
				throw new PlanException("could not parse PREVIEW_LABEL");
			return value;
		}

		internal static bool TryParseSkiaSharpVersion(string variablesText, out string value)
		{
			var match = SkiaSharpVersionLine().Match(variablesText);
			value = match.Success ? match.Groups[1].Value : "";
			return match.Success;
		}

		internal static bool TryParsePreviewLabel(string variablesText, out string value)
		{
			var match = PreviewLabelLine().Match(variablesText);
			value = match.Success ? match.Groups[1].Value.Trim() : "";
			return match.Success;
		}
	}
}
