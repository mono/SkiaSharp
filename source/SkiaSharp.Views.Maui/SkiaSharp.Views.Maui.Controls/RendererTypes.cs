using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace SkiaSharp.Views.Maui.Controls
{
	/// <summary>Provides event data for retrieving a property value from a renderer.</summary>
	/// <typeparam name="T">The type of the property value to retrieve.</typeparam>
	/// <remarks>This class is used by controller interfaces to request property values from platform renderers. The event handler sets the <see cref="P:SkiaSharp.Views.Maui.Controls.GetPropertyValueEventArgs`1.Value" /> property with the requested value.</remarks>
	public class GetPropertyValueEventArgs<T> : EventArgs
	{
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.Controls.GetPropertyValueEventArgs`1" /> class.</summary>
		/// <remarks />
		public GetPropertyValueEventArgs()
		{
		}

		/// <summary>Gets or sets the property value.</summary>
		/// <value>The property value of type <typeparamref name="T" />.</value>
		/// <remarks>Event handlers should set this property with the requested value. The event raiser reads this property after the event handler returns.</remarks>
		public T Value { get; set; }
	}
}
