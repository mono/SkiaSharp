using System;
using System.Linq;

namespace SkiaSharp.Views.Forms
{
	public class SKTouchEventArgs : EventArgs
	{
		public SKTouchEventArgs(long id, SKTouchAction type, SKPoint location, bool inContact)
			: this(id, type, SKMouseButton.Left, SKTouchDeviceType.Touch, location, inContact)
		{
		}

		public SKTouchEventArgs(long id, SKTouchAction type, SKMouseButton mouseButton, SKTouchDeviceType deviceType, SKPoint location, bool inContact)
		{
			Id = id;
			ActionType = type;
			DeviceType = deviceType;
			MouseButton = mouseButton;
			Location = location;
			InContact = inContact;
		}

		public bool Handled { get; set; }

		public long Id { get; private set; }

		public SKTouchAction ActionType { get; private set; }

		public SKTouchDeviceType DeviceType { get; private set; }

		public SKMouseButton MouseButton { get; private set; }

		public SKPoint Location { get; private set; }

		public bool InContact { get; private set; }

		public override string ToString()
		{
			return $"{{ActionType={ActionType}, DeviceType={DeviceType}, Handled={Handled}, Id={Id}, InContact={InContact}, Location={Location}, MouseButton={MouseButton}}}";
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
