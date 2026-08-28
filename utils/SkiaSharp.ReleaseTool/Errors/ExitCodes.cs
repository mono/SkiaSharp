namespace SkiaSharp.ReleaseTool.Errors
{
	/// <summary>Process exit codes for <c>skiasharp-release</c>.</summary>
	public static class ExitCodes
	{
		/// <summary>The command completed successfully.</summary>
		public const int Success = 0;

		/// <summary>Every unhandled <see cref="ReleaseToolException"/> maps here.</summary>
		public const int GenericError = 1;
	}
}
