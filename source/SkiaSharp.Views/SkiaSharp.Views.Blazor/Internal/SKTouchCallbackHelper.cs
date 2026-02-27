using System;
using System.ComponentModel;
using Microsoft.JSInterop;

namespace SkiaSharp.Views.Blazor.Internal
{
	[EditorBrowsable(EditorBrowsableState.Never)]
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

#if !NET7_0_OR_GREATER
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class SKTouchCallbackHelper
	{
		private readonly Func<PointerEventData, bool> callback;

		public SKTouchCallbackHelper(Func<PointerEventData, bool> callback)
		{
			this.callback = callback;
		}

		[JSInvokable]
		public bool OnPointerEvent(PointerEventData data) => callback(data);
	}
#endif
}
