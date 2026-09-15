#if !NET7_0_OR_GREATER
using System;
using System.ComponentModel;
using Microsoft.JSInterop;

namespace SkiaSharp.Views.Blazor.Internal
{
	/// <summary>Provides a JavaScript-invokable wrapper for an action with two floating-point arguments.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class FloatFloatActionHelper
	{
		private readonly Action<float, float> action;

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Blazor.Internal.FloatFloatActionHelper" /> class.</summary>
		/// <param name="action">The action to invoke from JavaScript.</param>
		public FloatFloatActionHelper(Action<float, float> action)
		{
			this.action = action;
		}

		/// <summary>Invokes the wrapped action.</summary>
		/// <param name="width">The width argument.</param>
		/// <param name="height">The height argument.</param>
		[JSInvokable]
		public void Invoke(float width, float height) => action?.Invoke(width, height);
	}
}
#endif
