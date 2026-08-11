using System;

namespace SkiaSharp.Views.Maui
{
	public sealed class SKGraphiteRenderFailedEventArgs : EventArgs
	{
		public SKGraphiteRenderFailedEventArgs (Exception exception)
		{
			Exception = exception ?? throw new ArgumentNullException (nameof (exception));
		}

		public Exception Exception { get; }
	}
}
