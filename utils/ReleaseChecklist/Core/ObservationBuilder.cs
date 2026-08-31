using System.Globalization;

namespace ReleaseChecklist.Core;

/// <summary>Builds immutable, human-readable observations.</summary>
public sealed class ObservationBuilder
{
	private readonly Dictionary<string, string?> fields = new(StringComparer.Ordinal);

	/// <summary>Adds a field whose value is <see langword="null" />.</summary>
	/// <param name="name">The field name.</param>
	/// <returns>The current builder.</returns>
	public ObservationBuilder AddNull(string name) => AddValue(name, null);

	/// <summary>Adds a text field.</summary>
	/// <param name="name">The field name.</param>
	/// <param name="value">The field value.</param>
	/// <returns>The current builder.</returns>
	public ObservationBuilder Add(string name, string value) =>
		AddValue(name, value ?? throw new ArgumentNullException(nameof(value)));

	/// <summary>Adds an integer field.</summary>
	/// <param name="name">The field name.</param>
	/// <param name="value">The field value.</param>
	/// <returns>The current builder.</returns>
	public ObservationBuilder Add(string name, long value) =>
		AddValue(name, value.ToString(CultureInfo.InvariantCulture));

	/// <summary>Adds an integer field.</summary>
	/// <param name="name">The field name.</param>
	/// <param name="value">The field value.</param>
	/// <returns>The current builder.</returns>
	public ObservationBuilder Add(string name, int value) => Add(name, (long)value);

	/// <summary>Adds a Boolean field.</summary>
	/// <param name="name">The field name.</param>
	/// <param name="value">The field value.</param>
	/// <returns>The current builder.</returns>
	public ObservationBuilder Add(string name, bool value) =>
		AddValue(name, value ? "true" : "false");

	/// <summary>Creates an immutable observation from the current fields.</summary>
	/// <returns>The new observation.</returns>
	public Observation Build() => new(fields);

	private ObservationBuilder AddValue(string name, string? value)
	{
		if (!fields.TryAdd(name, value))
			throw new ArgumentException($"Duplicate observation field '{name}'.", nameof(name));
		return this;
	}
}
