#if !NET7_0_OR_GREATER
using System;
using System.ComponentModel;
using Microsoft.JSInterop;

namespace SkiaSharp.Views.Blazor.Internal
{
	/// <summary>Provides a JavaScript-invokable wrapper for an action.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class ActionHelper
	{
		private readonly Action action;

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Blazor.Internal.ActionHelper" /> class.</summary>
		/// <param name="action">The action to invoke from JavaScript.</param>
		public ActionHelper(Action action)
		{
			this.action = action;
		}

		/// <summary>Invokes the wrapped action.</summary>
		[JSInvokable]
		public void Invoke() => action?.Invoke();
	}
}
#endif
