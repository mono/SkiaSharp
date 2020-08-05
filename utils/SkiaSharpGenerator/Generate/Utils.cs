using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SkiaSharpGenerator
{
	public class Utils
	{
		private static readonly string[] keywords =
		{
			"out", "in", "var", "ref"
		};

		public static string CleanName(string type)
		{
			var prefix = "";
			var suffix = "";

			type = type.TrimStart('_');

			if (type.EndsWith("_t"))
				type = type[0..^2];

			if (type.StartsWith("hb_ot_"))
				type = "open_type_" + type[6..];
			else if (type.StartsWith("hb_"))
				type = type[3..];
			else if (type.StartsWith("sk_") || type.StartsWith("gr_"))
			{
				prefix = type[0..2].ToUpperInvariant();
				type = type[3..];
			}
			if (type.EndsWith("_proc") || type.EndsWith("_func"))
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

			for (var i = 0; i < parts.Length; i++)
			{
				var part = parts[i];
				parts[i] = part[0].ToString().ToUpperInvariant() + part[1..];
			}

			return prefix + string.Concat(parts) + suffix;
		}

		public static string CleanEnumFieldName(string fieldName, string cppEnumName)
		{
			if (cppEnumName.EndsWith("_t"))
				cppEnumName = cppEnumName[..^2];

			fieldName = RemovePrefixSuffix(fieldName, cppEnumName);

			// special case for "flags" name and "flag" member
			if (cppEnumName.EndsWith("_flags"))
				fieldName = RemovePrefixSuffix(fieldName, cppEnumName[..^1]);

			// special case for bad skia enum fields
			var lower = fieldName.ToLowerInvariant();
			var indexOfSplitter = lower.IndexOf("_sk_");
			if (indexOfSplitter == -1)
				indexOfSplitter = lower.IndexOf("_gr_");
			if (indexOfSplitter != -1)
				fieldName = fieldName[0..indexOfSplitter];

			return CleanName(fieldName);

			static string RemovePrefixSuffix(string member, string type)
			{
				if (member.ToLowerInvariant().EndsWith("_" + type.ToLowerInvariant()))
					member = member[..^(type.Length + 1)];

				if (member.ToLowerInvariant().StartsWith(type.ToLowerInvariant() + "_"))
					member = member[(type.Length + 1)..];

				return member;
			}
		}

		public static string SafeName(string name)
		{
			if (keywords.Contains(name))
				return "@" + name;
			return name;
		}
	}
}
