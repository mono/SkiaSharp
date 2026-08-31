namespace SkiaSharp.ReleaseChecklist;

/// <summary>Describes the post-stable-release version bump, when one is required.</summary>
/// <param name="Required"><see langword="true" /> for a stable non-hotfix release.</param>
/// <param name="Branch">The bump branch name, or <see langword="null" /> when not required.</param>
/// <param name="NextSkia">The next SkiaSharp version, or <see langword="null" /> when not required.</param>
/// <param name="NextHarfBuzz">The next HarfBuzzSharp version, or <see langword="null" /> when not required.</param>
public sealed record StableBump(
	bool Required,
	string? Branch,
	string? NextSkia,
	string? NextHarfBuzz)
{
	/// <summary>Gets a value representing a release that requires no stable bump.</summary>
	/// <value>The shared non-applicable value.</value>
	public static StableBump None { get; } = new(false, null, null, null);
}
