using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Json
{
	/// <summary>
	/// Deterministic JSON serialization matching Python's
	/// <c>json.dumps(value, sort_keys=True, separators=(",", ":"),
	/// ensure_ascii=True)</c> (the canonical/digest form, used by
	/// <c>release_common.canonical_json</c>) and
	/// <c>json.dumps(value, indent=2, sort_keys=True)</c> (the pretty
	/// on-disk form, used by <c>release_common.write_plan</c>/
	/// <c>write_json_file</c>) byte-for-byte, including object keys
	/// sorted ordinally at every nesting level and non-ASCII/surrogate
	/// escaping. Operates on parsed <see cref="JsonElement"/> trees
	/// rather than mutating <see cref="System.Text.Json.Nodes.JsonNode"/>,
	/// so the exact same code path digests both a freshly serialized plan
	/// and a plan re-read from disk.
	/// </summary>
	public static class CanonicalJson
	{
		/// <summary>
		/// The compact, sorted-keys, ASCII-escaped form used only for
		/// hashing -- never written to disk. When
		/// <paramref name="excludeTopLevelProperty"/> is given, that
		/// property is dropped from the root object only (matching
		/// Python's <c>compute_digest</c>, which strips
		/// <c>DIGEST_FIELD</c> from a shallow copy of the top-level dict).
		/// </summary>
		public static string ToCanonicalString(JsonElement root, string? excludeTopLevelProperty = null)
		{
			var builder = new StringBuilder();
			WriteCompact(builder, root, isRoot: true, excludeTopLevelProperty);
			return builder.ToString();
		}

		public static byte[] ToCanonicalUtf8Bytes(JsonElement root, string? excludeTopLevelProperty = null) =>
			Encoding.UTF8.GetBytes(ToCanonicalString(root, excludeTopLevelProperty));

		/// <summary>The lowercase hex SHA-256 digest of the canonical form.</summary>
		public static string ComputeSha256Hex(JsonElement root, string? excludeTopLevelProperty = null)
		{
			var hash = SHA256.HashData(ToCanonicalUtf8Bytes(root, excludeTopLevelProperty));
			return Convert.ToHexStringLower(hash);
		}

		/// <summary>
		/// The 2-space-indented, sorted-keys, ASCII-escaped form written
		/// to plan/report files on disk (never itself hashed). Matches
		/// Python's <c>json.dumps(value, indent=2, sort_keys=True)</c>
		/// exactly, including collapsing an empty object/array to
		/// <c>{}</c>/<c>[]</c> with no inner newline.
		/// </summary>
		public static string ToPrettyString(JsonElement root, int indentSize = 2)
		{
			var builder = new StringBuilder();
			WritePretty(builder, root, depth: 0, indentSize);
			return builder.ToString();
		}

		private static void WriteCompact(StringBuilder sb, JsonElement element, bool isRoot, string? excludeKey)
		{
			switch (element.ValueKind)
			{
				case JsonValueKind.Object:
					sb.Append('{');
					var first = true;
					foreach (var property in SortedProperties(element, isRoot ? excludeKey : null))
					{
						if (!first)
							sb.Append(',');
						first = false;
						WriteString(sb, property.Name);
						sb.Append(':');
						WriteCompact(sb, property.Value, isRoot: false, excludeKey: null);
					}
					sb.Append('}');
					break;
				case JsonValueKind.Array:
					sb.Append('[');
					var firstItem = true;
					foreach (var item in element.EnumerateArray())
					{
						if (!firstItem)
							sb.Append(',');
						firstItem = false;
						WriteCompact(sb, item, isRoot: false, excludeKey: null);
					}
					sb.Append(']');
					break;
				default:
					WriteScalar(sb, element);
					break;
			}
		}

		private static void WritePretty(StringBuilder sb, JsonElement element, int depth, int indentSize)
		{
			switch (element.ValueKind)
			{
				case JsonValueKind.Object:
				{
					var properties = SortedProperties(element, excludeKey: null);
					WriteContainer(sb, properties, depth, indentSize, '{', '}', (b, item, d) =>
					{
						WriteString(b, item.Name);
						b.Append(": ");
						WritePretty(b, item.Value, d, indentSize);
					});
					break;
				}
				case JsonValueKind.Array:
				{
					var items = element.EnumerateArray().ToArray();
					WriteContainer(sb, items, depth, indentSize, '[', ']', (b, item, d) => WritePretty(b, item, d, indentSize));
					break;
				}
				default:
					WriteScalar(sb, element);
					break;
			}
		}

		private static void WriteContainer<T>(
			StringBuilder sb, IReadOnlyList<T> items, int depth, int indentSize,
			char open, char close, Action<StringBuilder, T, int> writeItem)
		{
			if (items.Count == 0)
			{
				sb.Append(open).Append(close);
				return;
			}
			sb.Append(open).Append('\n');
			for (var i = 0; i < items.Count; i++)
			{
				Indent(sb, depth + 1, indentSize);
				writeItem(sb, items[i], depth + 1);
				if (i < items.Count - 1)
					sb.Append(',');
				sb.Append('\n');
			}
			Indent(sb, depth, indentSize);
			sb.Append(close);
		}

		private static void Indent(StringBuilder sb, int depth, int indentSize) => sb.Append(' ', depth * indentSize);

		private static List<JsonProperty> SortedProperties(JsonElement obj, string? excludeKey)
		{
			var properties = new List<JsonProperty>();
			foreach (var property in obj.EnumerateObject())
			{
				if (excludeKey is not null && property.Name == excludeKey)
					continue;
				properties.Add(property);
			}
			properties.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
			return properties;
		}

		private static void WriteScalar(StringBuilder sb, JsonElement element)
		{
			switch (element.ValueKind)
			{
				case JsonValueKind.String:
					WriteString(sb, element.GetString()!);
					break;
				case JsonValueKind.Number:
					WriteNumber(sb, element);
					break;
				case JsonValueKind.True:
					sb.Append("true");
					break;
				case JsonValueKind.False:
					sb.Append("false");
					break;
				case JsonValueKind.Null:
					sb.Append("null");
					break;
				default:
					throw new ValidationException($"canonical JSON does not support value kind {element.ValueKind}");
			}
		}

		private static void WriteNumber(StringBuilder sb, JsonElement element)
		{
			// Every field in a digested plan (schemaVersion, counts, etc.)
			// is a whole integer; Python's plans never contain a float.
			// Rejecting non-integers here rather than reproducing
			// Python's float `repr` keeps canonicalization exact instead
			// of merely "close enough".
			if (!element.TryGetInt64(out var value))
				throw new ValidationException(
					$"canonical JSON does not support floating-point numbers (found {element.GetRawText()})");
			sb.Append(value.ToString(CultureInfo.InvariantCulture));
		}

		private static void WriteString(StringBuilder sb, string value)
		{
			sb.Append('"');
			foreach (var c in value)
			{
				switch (c)
				{
					case '"':
						sb.Append("\\\"");
						break;
					case '\\':
						sb.Append("\\\\");
						break;
					case '\b':
						sb.Append("\\b");
						break;
					case '\f':
						sb.Append("\\f");
						break;
					case '\n':
						sb.Append("\\n");
						break;
					case '\r':
						sb.Append("\\r");
						break;
					case '\t':
						sb.Append("\\t");
						break;
					default:
						// Matches CPython's `py_encode_basestring_ascii`: every
						// control character and everything outside printable
						// ASCII (0x20-0x7e) is \u-escaped one UTF-16 code unit
						// at a time, which for a surrogate pair naturally
						// produces the same two \uXXXX escapes Python emits.
						if (c < 0x20 || c > 0x7e)
						{
							sb.Append("\\u");
							sb.Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
						}
						else
						{
							sb.Append(c);
						}
						break;
				}
			}
			sb.Append('"');
		}
	}
}
