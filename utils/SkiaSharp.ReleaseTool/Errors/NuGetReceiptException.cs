using SkiaSharp.ReleaseTool.Contracts;

namespace SkiaSharp.ReleaseTool.Errors
{
	public sealed class NuGetReceiptException : ReleaseToolException
	{
		public NuGetReceiptException(string message)
			: base(message)
		{
		}

		public NuGetReceiptException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}

	internal sealed class NuGetTransientException : ReleaseToolException
	{
		public NuGetTransientException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}

	public sealed class PackagesPendingException : ReleaseToolException
	{
		public PackagesPendingException(
			string message,
			IReadOnlyList<PendingPackage> missingPackages,
			TimeSpan elapsed,
			TimeSpan deadline)
			: base(message)
		{
			MissingPackages = missingPackages;
			Elapsed = elapsed;
			Deadline = deadline;
		}

		public IReadOnlyList<PendingPackage> MissingPackages { get; }

		public TimeSpan Elapsed { get; }

		public TimeSpan Deadline { get; }
	}
}
