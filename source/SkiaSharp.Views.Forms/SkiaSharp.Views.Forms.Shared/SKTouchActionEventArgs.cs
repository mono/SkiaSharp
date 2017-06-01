using System;
using System.Linq;

namespace SkiaSharp.Views.Forms
{
	public class SKTouchActionEventArgs : EventArgs
	{
		public SKTouchActionEventArgs(long id, SKTouchActionType type, SKPoint location, bool inContact)
			: this(id, type, SKMouseButton.Left, SKTouchDeviceType.Touch, location, inContact)
		{
		}

		public SKTouchActionEventArgs(long id, SKTouchActionType type, SKMouseButton mouseButton, SKTouchDeviceType deviceType, SKPoint location, bool inContact)
		{
			Id = id;
			ActionType = type;
			DeviceType = deviceType;
			MouseButton = mouseButton;
			Location = location;
			InContact = inContact;
			Handled = true;
		}

		// this may be removed, but for now keep it
		internal bool Handled { get; set; }

		public long Id { get; private set; }

		public SKTouchActionType ActionType { get; private set; }

		public SKTouchDeviceType DeviceType { get; private set; }

		public SKMouseButton MouseButton { get; private set; }

		public SKPoint Location { get; private set; }

		public bool InContact { get; private set; }

		public override string ToString()
		{
			return $"{{ActionType={ActionType}, DeviceType={DeviceType}, Id={Id}, InContact={InContact}, Location={Location}, MouseButton={MouseButton}}}";
		}
	}

	public enum SKTouchActionType
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
