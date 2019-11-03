using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SkiaSharpGenerator
{
	public class Utils
	{
		public static string CleanName(string type, bool isField = false)
		{
			var isSkiaType = !isField && (type.StartsWith("sk_") || type.StartsWith("gr_"));
			if (isSkiaType || isField)
			{
				string[] parts;

				if (type.Any(c => char.IsLower(c)) && type.Any(c => char.IsUpper(c)))
				{
					// there is a mix of cases, so split on the uppercase
					parts = Regex.Split(type, @"(?<!^)(?=[A-Z])");

					// remove the initial "f" prefix
					if (parts[0] == "f" && char.IsUpper(parts[1][0]))
						parts = parts[1..];
				}
				else
				{
					// this is either an fully uppercase enum or lowercase type
					parts = type.ToLowerInvariant().Split('_');
					if (parts[^1] == "t")
						parts = parts[0..^1];
				}

				var start = 0;
				var end = parts.Length;
				if (isField)
				{
					var sk = Array.IndexOf(parts, "sk");
					var gr = Array.IndexOf(parts, "gr");
					if (sk != -1)
						end = sk;
					if (gr != -1)
						end = gr;
				}
				else
				{
					parts[0] = parts[0].ToUpperInvariant();
					start = 1;
				}

				for (int i = start; i < end; i++)
				{
					var part = parts[i];
					parts[i] = part[0].ToString().ToUpperInvariant() + part[1..];
				}

				return string.Concat(parts[..end]);
			}

			return type;
		}
	}
}
