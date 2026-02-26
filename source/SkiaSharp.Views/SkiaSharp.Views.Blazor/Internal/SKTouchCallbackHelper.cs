using System;
using System.ComponentModel;
using Microsoft.JSInterop;

namespace SkiaSharp.Views.Blazor.Internal
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class SKTouchCallbackHelper
	{
		private readonly Action<SKTouchCallbackHelper.PointerEventData> callback;

		public SKTouchCallbackHelper(Action<PointerEventData> callback)
		{
			this.callback = callback;
		}

		[JSInvokable]
		public void OnPointerEvent(PointerEventData data) => callback(data);

		public class PointerEventData
		{
			public long Id { get; set; }
			public int Action { get; set; }
			public int DeviceType { get; set; }
			public int MouseButton { get; set; }
			public float X { get; set; }
			public float Y { get; set; }
			public float Pressure { get; set; }
			public bool InContact { get; set; }
			public int WheelDelta { get; set; }
		}
	}
}
