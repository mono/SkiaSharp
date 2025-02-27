#if !NET7_0_OR_GREATER
using System;
using System.ComponentModel;
using Microsoft.JSInterop;

namespace SkiaSharp.Views.Blazor.Internal
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class FloatFloatActionHelper
	{
		private readonly Action<float, float> action;

		public FloatFloatActionHelper(Action<float, float> action)
		{
			this.action = action;
		}

		[JSInvokable]
		public void Invoke(float width, float height) => action?.Invoke(width, height);
	}
}
#endif
