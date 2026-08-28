using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Json
{
	/// <summary>
	/// Reads and writes the two digested, approval-bearing plan artifacts
	/// (<see cref="Artifacts.PreparePlan"/>, <see cref="Artifacts.FinishPlan"/>).
	/// Mirrors Python's <c>release_common.write_plan</c>/<c>read_plan</c>:
	/// the file on disk is the pretty, sorted-keys, UTF-8-without-BOM,
	/// trailing-newline form; <see cref="CanonicalJson"/>'s compact form
	/// is used only to compute/verify the <c>planDigest</c> field, never
	/// written anywhere.
	/// </summary>
	public static class PlanStore
	{
		private const string DigestPropertyName = "planDigest";

		/// <summary>
		/// Recomputes <paramref name="plan"/>'s canonical digest (ignoring
		/// any value it currently carries), stamps it, and writes the
		/// pretty-printed result to <paramref name="path"/>. Returns the
		/// same instance, mutated in place with its digest populated.
		/// </summary>
		public static T Write<T>(string path, T plan, JsonTypeInfo<T> typeInfo)
			where T : IDigestedPlan
		{
			using (var beforeDigest = JsonDocument.Parse(JsonSerializer.Serialize(plan, typeInfo)))
			{
				plan.PlanDigest = CanonicalJson.ComputeSha256Hex(beforeDigest.RootElement, DigestPropertyName);
			}

			using var stamped = JsonDocument.Parse(JsonSerializer.Serialize(plan, typeInfo));
			WriteFile(path, CanonicalJson.ToPrettyString(stamped.RootElement));
			return plan;
		}

		/// <summary>
		/// Loads, digest-verifies, and strictly deserializes a plan file.
		/// This is the only supported way a later "apply"/"create-draft"
		/// step should consume a plan file; it never interprets unknown
		/// fields as anything other than tampering (see
		/// <see cref="System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute"/>
		/// on <typeparamref name="T"/> and its nested DTOs).
		/// </summary>
		public static T Read<T>(string path, JsonTypeInfo<T> typeInfo)
			where T : IDigestedPlan
		{
			if (!File.Exists(path))
				throw new ValidationException($"plan file not found: {path}");
			var text = File.ReadAllText(path, Encoding.UTF8);

			JsonDocument document;
			try
			{
				document = JsonDocument.Parse(text);
			}
			catch (JsonException ex)
			{
				throw new ValidationException($"plan file is not valid JSON: {ex.Message}", ex);
			}
			using (document)
			{
				if (document.RootElement.ValueKind != JsonValueKind.Object)
					throw new ValidationException("plan file must contain a JSON object");

				VerifyDigest(document.RootElement);

				try
				{
					return JsonSerializer.Deserialize(text, typeInfo)
						?? throw new ValidationException("plan file must contain a JSON object");
				}
				catch (JsonException ex)
				{
					throw new ValidationException($"plan file failed shape validation: {ex.Message}", ex);
				}
			}
		}

		/// <summary>
		/// Raises <see cref="ValidationException"/> if <paramref name="root"/>'s
		/// stored <c>planDigest</c> does not match its recomputed
		/// canonical digest. Exposed separately from <see cref="Read{T}"/>
		/// so tests can assert digest tampering is caught independently
		/// of the strict-shape check.
		/// </summary>
		public static void VerifyDigest(JsonElement root)
		{
			if (!root.TryGetProperty(DigestPropertyName, out var digestElement) ||
				digestElement.ValueKind != JsonValueKind.String ||
				string.IsNullOrEmpty(digestElement.GetString()))
			{
				throw new ValidationException("plan is missing its canonical digest");
			}

			var stored = digestElement.GetString()!;
			var expected = CanonicalJson.ComputeSha256Hex(root, DigestPropertyName);
			if (!string.Equals(stored, expected, StringComparison.Ordinal))
			{
				throw new ValidationException(
					"plan digest mismatch: the plan file was modified after it was generated " +
					$"(expected {expected}, found {stored})");
			}
		}

		private static void WriteFile(string path, string prettyJson)
		{
			var directory = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(directory))
				Directory.CreateDirectory(directory);
			// UTF-8 without a byte-order mark, trailing "\n" appended --
			// matches Python's `Path.write_text(json.dumps(...) + "\n",
			// encoding="utf-8")`.
			File.WriteAllText(path, prettyJson + "\n", new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
		}
	}
}
