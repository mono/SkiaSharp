using System;

namespace SkiaSharp.Views.Forms
{
	public class GetPropertyValueEventArgs<T> : EventArgs
	{
		public T Value { get; set; }
	}
}
