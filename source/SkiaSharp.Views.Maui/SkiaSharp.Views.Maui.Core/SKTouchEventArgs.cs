using System;

using Microsoft.Maui;

namespace SkiaSharp.Views.Maui
{
	public class SKTouchEventArgs : EventArgs
	{
		/// <param name="id"></param>
		/// <param name="type"></param>
		/// <param name="location"></param>
		/// <param name="inContact"></param>
		public SKTouchEventArgs(long id, SKTouchAction type, SKPoint location, bool inContact)
			: this(id, type, SKMouseButton.Left, SKTouchDeviceType.Touch, location, inContact, 0, 1)
		{
		}

		/// <param name="id"></param>
		/// <param name="type"></param>
		/// <param name="mouseButton"></param>
		/// <param name="deviceType"></param>
		/// <param name="location"></param>
		/// <param name="inContact"></param>
		public SKTouchEventArgs(long id, SKTouchAction type, SKMouseButton mouseButton, SKTouchDeviceType deviceType, SKPoint location, bool inContact)
			: this(id, type, mouseButton, deviceType, location, inContact, 0, 1)
		{
		}

		/// <param name="id"></param>
		/// <param name="type"></param>
		/// <param name="mouseButton"></param>
		/// <param name="deviceType"></param>
		/// <param name="location"></param>
		/// <param name="inContact"></param>
		/// <param name="wheelDelta"></param>
		public SKTouchEventArgs(long id, SKTouchAction type, SKMouseButton mouseButton, SKTouchDeviceType deviceType, SKPoint location, bool inContact, int wheelDelta)
			: this(id, type, mouseButton, deviceType, location, inContact, wheelDelta, 1)
		{
		}

		/// <param name="id"></param>
		/// <param name="type"></param>
		/// <param name="mouseButton"></param>
		/// <param name="deviceType"></param>
		/// <param name="location"></param>
		/// <param name="inContact"></param>
		/// <param name="wheelDelta"></param>
		/// <param name="pressure"></param>
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
		Pen
	}

	public enum SKMouseButton
	{
		Unknown,

		Left,
		Middle,
		Right
	}
}
