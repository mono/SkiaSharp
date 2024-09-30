using System;

#if __MAUI__
using Microsoft.Maui;
#else
using Xamarin.Forms;
#endif

#if __MAUI__
namespace SkiaSharp.Views.Maui
#else
namespace SkiaSharp.Views.Forms
#endif
{
	public class SKTouchEventArgs : EventArgs
	{
		public SKTouchEventArgs(long id, SKTouchAction type, SKPoint location, bool inContact)
			: this(id, type, SKMouseButton.Left, SKTouchDeviceType.Touch, location, inContact, 0, 1)
		{
		}

		public SKTouchEventArgs(long id, SKTouchAction type, SKPoint location, bool inContact, float pressure)
			: this(id, type, SKMouseButton.Left, SKTouchDeviceType.Touch, location, inContact, 0, pressure)
		{
		}

		public SKTouchEventArgs(long id, SKTouchAction type, SKMouseButton mouseButton, SKTouchDeviceType deviceType, SKPoint location, bool inContact)
			: this(id, type, mouseButton, deviceType, location, inContact, 0, 1)
		{
		}

		public SKTouchEventArgs(long id, SKTouchAction type, SKMouseButton mouseButton, SKTouchDeviceType deviceType, SKPoint location, bool inContact, int wheelDelta)
			: this(id, type, mouseButton, deviceType, location, inContact, wheelDelta, 1)
		{
		}

		public SKTouchEventArgs(long id, SKTouchAction type, SKMouseButton mouseButton, SKTouchDeviceType deviceType, SKPoint location, bool inContact, int wheelDelta, float pressure)
		{
			Id = id;
			ActionType = type;
			DeviceType = deviceType;
			MouseButton = mouseButton;
			Location = location;
			InContact = inContact;
			WheelDelta = wheelDelta;
			Pressure = pressure;
		}

		public bool Handled { get; set; }

		public long Id { get; private set; }

		public SKTouchAction ActionType { get; private set; }

		public SKTouchDeviceType DeviceType { get; private set; }

		public SKMouseButton MouseButton { get; private set; }

		public SKPoint Location { get; private set; }

		public bool InContact { get; private set; }

		public int WheelDelta { get; private set; }

		public float Pressure { get; private set; }

		public override string ToString()
		{
			return $"{{ActionType={ActionType}, DeviceType={DeviceType}, Handled={Handled}, Id={Id}, InContact={InContact}, Location={Location}, MouseButton={MouseButton}, WheelDelta={WheelDelta}, Pressure={Pressure}}}";
		}
	}

	public enum SKTouchAction
	{
		Entered,
		Pressed,
		Moved,
		Released,
		Cancelled,
		Exited,
		WheelChanged,
	}

	public enum SKTouchDeviceType
	{
		Touch,
		Mouse,
		Pen,
		Eraser
	}

	public enum SKMouseButton
	{
		Unknown,

		Left,
		Middle,
		Right
	}
}
