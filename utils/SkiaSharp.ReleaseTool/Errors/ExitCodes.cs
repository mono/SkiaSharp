namespace SkiaSharp.ReleaseTool.Errors
{
	/// <summary>
	/// Process exit codes for <c>skiasharp-release</c>, matching the
	/// Python CLI's <c>release.py main()</c> exactly so a workflow step
	/// written against the Python tool needs no changes when it is
	/// switched to this one.
	/// </summary>
	public static class ExitCodes
	{
		/// <summary>The command completed successfully.</summary>
		public const int Success = 0;

		/// <summary>
		/// Every <see cref="ReleaseToolException"/> not given a more
		/// specific exit code maps here. Matches Python's <c>main()</c>,
		/// which catches <c>ReleaseToolError</c> and returns 1.
		/// </summary>
		public const int GenericError = 1;

		/// <summary>
		/// <c>finish plan</c>'s exit code when NuGet.org indexing has not
		/// converged yet. Distinct from <see cref="GenericError"/> so a
		/// workflow can tell "rerun me later, nothing is wrong" (this
		/// code, with a pending report always written to <c>--output</c>)
		/// apart from a genuine failure. Matches Python's
		/// <c>FINISH_PLAN_PENDING_EXIT_CODE</c>.
		/// </summary>
		public const int FinishPlanPending = 2;
	}
}
