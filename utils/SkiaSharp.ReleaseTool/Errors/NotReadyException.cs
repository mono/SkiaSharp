namespace SkiaSharp.ReleaseTool.Errors
{
	/// <summary>
	/// One (package id, version) pair that has not yet converged (e.g. is
	/// not yet visible/listed on NuGet.org). Mirrors the small
	/// <c>{"id": ..., "version": ...}</c> dict Python's
	/// <c>release_nuget.poll_catalog_entries</c> attaches to
	/// <c>NotReadyError.missing</c>.
	/// </summary>
	public sealed record MissingPackageRef(string Id, string Version);

	/// <summary>
	/// An external system has not converged yet (for example NuGet
	/// indexing). Mirrors Python's <c>release_common.NotReadyError</c>:
	/// distinguished from <see cref="ConflictException"/> because the
	/// caller should report a bounded, rerunnable "pending" result rather
	/// than a hard failure.
	///
	/// Carries structured context -- beyond the human-readable message --
	/// so a caller (in particular ``finish plan``'s pending report) can
	/// surface exactly which packages are still missing/unlisted and how
	/// much of the polling budget was spent, without having to re-parse
	/// the message text.
	/// </summary>
	public sealed class NotReadyException : ReleaseToolException
	{
		public NotReadyException(
			string message,
			IReadOnlyList<MissingPackageRef>? missing = null,
			double? elapsedSeconds = null,
			double? deadlineSeconds = null)
			: base(message)
		{
			Missing = missing ?? Array.Empty<MissingPackageRef>();
			ElapsedSeconds = elapsedSeconds;
			DeadlineSeconds = deadlineSeconds;
		}

		public IReadOnlyList<MissingPackageRef> Missing { get; }

		public double? ElapsedSeconds { get; }

		public double? DeadlineSeconds { get; }
	}
}
