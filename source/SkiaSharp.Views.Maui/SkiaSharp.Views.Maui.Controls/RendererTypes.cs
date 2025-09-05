using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace SkiaSharp.Views.Maui.Controls
{
	/// <typeparam name="T"></typeparam>
	public class GetPropertyValueEventArgs<T> : EventArgs
	{
		public T Value { get; set; }
	}
}
