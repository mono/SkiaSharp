using System;

namespace SkiaSharp.Views
{
	internal static class EnvironmentExtensions
	{
		private static readonly Lazy<bool> isValidEnvironment = new Lazy<bool>(() =>
		{
			try
			{
				// test an operation that requires the native library
				SKPMColor.PreMultiply(SKColors.Black);
				return true;
			}
			catch (DllNotFoundException)
			{
				// If we can't load the native library,
				// we may be in some designer.
				// We can make this assumption since any other member will fail
				// at some point in the draw operation.
				return false;
			}
		});

		internal static bool IsValidEnvironment => isValidEnvironment.Value;
	}
}
