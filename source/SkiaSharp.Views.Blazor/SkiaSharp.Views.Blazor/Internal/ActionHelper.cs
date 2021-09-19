﻿using Microsoft.JSInterop;
using System;
using System.ComponentModel;

namespace SkiaSharp.Views.Blazor.Internal
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class ActionHelper
	{
		private Action action;

		public ActionHelper(Action action)
		{
			this.action = action;
		}

		[JSInvokable]
		public void Invoke() => action?.Invoke();
	}
}