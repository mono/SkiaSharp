namespace ReleaseChecklist.GitHub;

/// <summary>Identifies a GitHub repository by owner and name.</summary>
public readonly record struct GitHubRepositoryIdentity
{
	/// <summary>Initializes a new instance of the <see cref="GitHubRepositoryIdentity" /> struct.</summary>
	/// <param name="owner">The repository owner.</param>
	/// <param name="name">The repository name.</param>
	public GitHubRepositoryIdentity(string owner, string name)
	{
		Owner = Require(owner, nameof(owner));
		Name = Require(name, nameof(name));
	}

	/// <summary>Gets the repository owner.</summary>
	/// <value>The owner name.</value>
	public string Owner { get; }

	/// <summary>Gets the repository name.</summary>
	/// <value>The repository name.</value>
	public string Name { get; }

	/// <summary>Returns the <c>owner/name</c> form.</summary>
	/// <returns>The canonical repository identity.</returns>
	public override string ToString() => $"{Owner}/{Name}";

	private static string Require(string value, string parameter) =>
		!string.IsNullOrWhiteSpace(value) &&
		value.All(static c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.')
			? value
			: throw new ArgumentException("Invalid GitHub repository component.", parameter);
}
