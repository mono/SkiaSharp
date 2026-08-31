using System.Collections.Frozen;

namespace ReleaseChecklist.Core;

/// <summary>Captures named authoritative state values for reporting and in-run drift detection.</summary>
/// <remarks>
/// Observations are not persisted plans or approvals. The runner compares the observation from a
/// step's initial check with a fresh observation immediately before applying that step's action.
/// </remarks>
public sealed class Observation : IEquatable<Observation>
{
	/// <summary>Gets an observation with no fields.</summary>
	/// <value>The shared empty observation.</value>
	public static Observation Empty { get; } = new(
		new Dictionary<string, string?>(StringComparer.Ordinal));

	/// <summary>Initializes a new instance of the <see cref="Observation" /> class.</summary>
	/// <param name="fields">The named state values.</param>
	/// <exception cref="ArgumentException">A field name is empty, contains a control character, or is duplicated.</exception>
	public Observation(IEnumerable<KeyValuePair<string, string?>> fields)
	{
		var values = new Dictionary<string, string?>(StringComparer.Ordinal);
		foreach (var pair in fields)
		{
			if (string.IsNullOrEmpty(pair.Key) || pair.Key.Any(char.IsControl))
				throw new ArgumentException("Observation field names must be nonempty.", nameof(fields));
			if (!values.TryAdd(pair.Key, pair.Value))
				throw new ArgumentException($"Duplicate observation field '{pair.Key}'.", nameof(fields));
		}
		Fields = values.ToFrozenDictionary(StringComparer.Ordinal);
	}

	/// <summary>Gets the observed state values.</summary>
	/// <value>The fields indexed by ordinal field name.</value>
	public IReadOnlyDictionary<string, string?> Fields { get; }

	/// <summary>Determines whether another observation contains the same fields and values.</summary>
	/// <param name="other">The observation to compare.</param>
	/// <returns><see langword="true" /> if every field and value is equal; otherwise, <see langword="false" />.</returns>
	public bool Equals(Observation? other) =>
		other is not null &&
		Fields.Count == other.Fields.Count &&
		Fields.All(pair =>
			other.Fields.TryGetValue(pair.Key, out var value) &&
			string.Equals(pair.Value, value, StringComparison.Ordinal));

	/// <inheritdoc />
	public override bool Equals(object? obj) => Equals(obj as Observation);

	/// <inheritdoc />
	public override int GetHashCode()
	{
		var hash = new HashCode();
		foreach (var pair in Fields.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
		{
			hash.Add(pair.Key, StringComparer.Ordinal);
			hash.Add(pair.Value, StringComparer.Ordinal);
		}
		return hash.ToHashCode();
	}

	/// <summary>Returns the fields as an ordered, human-readable string.</summary>
	/// <returns>The ordered field list.</returns>
	public override string ToString() =>
		string.Join(
			", ",
			Fields.OrderBy(static pair => pair.Key, StringComparer.Ordinal)
				.Select(static pair => $"{pair.Key}={pair.Value ?? "null"}"));
}
