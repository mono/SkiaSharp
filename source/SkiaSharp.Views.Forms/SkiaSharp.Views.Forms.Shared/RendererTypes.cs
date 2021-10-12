using System;

#if __MAUI__
using Microsoft.Maui;
using Microsoft.Maui.Controls;
#else
using Xamarin.Forms;
#endif

#if __MAUI__
namespace SkiaSharp.Views.Maui.Controls
#else
namespace SkiaSharp.Views.Forms
#endif
{
	public class GetPropertyValueEventArgs<T> : EventArgs
	{
		public T Value { get; set; }
	}
}
