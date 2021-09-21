using System;
using System.ComponentModel;
using Microsoft.JSInterop;

namespace SkiaSharp.Views.Blazor.Internal
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class ActionHelper
	{
		private readonly Action action;

		public ActionHelper(Action action)
		{
			this.action = action;
		}

		[JSInvokable]
		public void Invoke() => action?.Invoke();
	}
}
