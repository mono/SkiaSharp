using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SkiaSharpGenerator
{
	public class Utils
	{
		public static string CleanName(string type, bool isEnumMember = false)
		{
			var prefix = "";
			var suffix = "";
			if (type.StartsWith("sk_") || type.StartsWith("gr_"))
			{
				prefix = type[0..2].ToUpperInvariant();
				type = type[3..];
			}
			if (type.EndsWith("_t"))
				type = type[0..^2];
			if (type.EndsWith("_proc"))
			{
				type = type[0..^5];
				suffix = "ProxyDelegate";
			}

			// special case for managed<type>
			if (type.StartsWith("managed"))
				type = type.Insert(7, "_");

			string[] parts;

			if (type.Any(c => char.IsLower(c)) && type.Any(c => char.IsUpper(c)))
			{
				// there is a mix of cases, so split on the uppercase, then underscores
				parts = Regex.Split(type, @"(?<!^)(?=[A-Z])");
				parts = parts.SelectMany(p => p.Split('_')).ToArray();

				// remove the initial "f" prefix
				if (parts[0] == "f" && char.IsUpper(parts[1][0]))
					parts = parts[1..];
			}
			else
			{
				// this is either an fully uppercase enum or lowercase type
				parts = type.ToLowerInvariant().Split('_');
			}

			var end = parts.Length;
			if (isEnumMember)
			{
				// enum members have the enum name in them, so drop it
				var sk = Array.IndexOf(parts, "sk");
				var gr = Array.IndexOf(parts, "gr");
				if (sk != -1)
					end = sk;
				if (gr != -1)
					end = gr;
			}

			for (var i = 0; i < end; i++)
			{
				var part = parts[i];
				parts[i] = part[0].ToString().ToUpperInvariant() + part[1..];
			}

			return prefix + string.Concat(parts[..end]) + suffix;
		}
	}
}
