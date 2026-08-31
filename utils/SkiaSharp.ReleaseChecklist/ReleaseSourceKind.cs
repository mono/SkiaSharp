namespace SkiaSharp.ReleaseChecklist;

/// <summary>Specifies the supported source branch shape for release preparation.</summary>
public enum ReleaseSourceKind
{
	/// <summary>The repository default branch.</summary>
	Main,
	/// <summary>A release-line branch named <c>release/X.Y.x</c>.</summary>
	Maintenance,
	/// <summary>An exact branch named <c>release/{identity}</c>.</summary>
	ExactRelease,
}
