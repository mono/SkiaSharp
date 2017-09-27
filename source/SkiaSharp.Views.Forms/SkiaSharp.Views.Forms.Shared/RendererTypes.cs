using System;

namespace SkiaSharp.Views.Forms
{
	internal class GetPropertyValueEventArgs<T> : EventArgs
	{
		public T Value { get; set; }
	}
}
