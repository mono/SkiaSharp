using System;

using Microsoft.Maui;

namespace SkiaSharp.Views.Maui
{
	/// <summary>
	/// Provides data for the <see cref="SKCanvasView.Touch" /> or <see cref="SKGLView.Touch" /> event.
	/// </summary>
	public class SKTouchEventArgs : EventArgs
	{
		/// <summary>
		/// Creates a new instance of the <see cref="SKTouchEventArgs" /> event arguments.
		/// </summary>
		/// <param name="id">The ID used to track the touch event.</param>
		/// <param name="type">The type of touch action that initiated this event.</param>
		/// <param name="location">The location of the touch.</param>
		/// <param name="inContact">Whether or not the touch device is in contact with the screen.</param>
		/// <remarks>
		/// This constructor sets the <see cref="SKTouchEventArgs.DeviceType" /> to <see cref="SKTouchDeviceType.Touch" /> and the <see cref="SKTouchEventArgs.MouseButton" /> to <see cref="SKMouseButton.Left" />.
		/// </remarks>
		public SKTouchEventArgs(long id, SKTouchAction type, SKPoint location, bool inContact)
			: this(id, type, SKMouseButton.Left, SKTouchDeviceType.Touch, location, inContact, 0, 1)
		{
		}

		/// <summary>
		/// Creates a new instance of the <see cref="SKTouchEventArgs" /> event arguments.
		/// </summary>
		/// <param name="id">The ID used to track the touch event.</param>
		/// <param name="type">The type of touch action that initiated this event.</param>
		/// <param name="mouseButton">The mouse button used to raise the touch event.</param>
		/// <param name="deviceType">The touch device used to raise the touch event.</param>
		/// <param name="location">The location of the touch.</param>
		/// <param name="inContact">Whether or not the touch device is in contact with the screen, or the mouse button pressed.</param>
		public SKTouchEventArgs(long id, SKTouchAction type, SKMouseButton mouseButton, SKTouchDeviceType deviceType, SKPoint location, bool inContact)
			: this(id, type, mouseButton, deviceType, location, inContact, 0, 1)
		{
		}

		/// <summary>
		/// Creates a new instance of the <see cref="SKTouchEventArgs" /> event arguments.
		/// </summary>
		/// <param name="id">The ID used to track the touch event.</param>
		/// <param name="type">The type of touch action that initiated this event.</param>
		/// <param name="mouseButton">The mouse button used to raise the touch event.</param>
		/// <param name="deviceType">The touch device used to raise the touch event.</param>
		/// <param name="location">The location of the touch.</param>
		/// <param name="inContact">Whether or not the touch device is in contact with the screen, or the mouse button pressed.</param>
		/// <param name="wheelDelta">The amount the wheel was scrolled.</param>
		public SKTouchEventArgs(long id, SKTouchAction type, SKMouseButton mouseButton, SKTouchDeviceType deviceType, SKPoint location, bool inContact, int wheelDelta)
			: this(id, type, mouseButton, deviceType, location, inContact, wheelDelta, 1)
		{
		}

		/// <summary>
		/// Creates a new instance of the <see cref="SKTouchEventArgs" /> event arguments.
		/// </summary>
		/// <param name="id">The ID used to track the touch event.</param>
		/// <param name="type">The type of touch action that initiated this event.</param>
		/// <param name="mouseButton">The mouse button used to raise the touch event.</param>
		/// <param name="deviceType">The touch device used to raise the touch event.</param>
		/// <param name="location">The location of the touch.</param>
		/// <param name="inContact">Whether or not the touch device is in contact with the screen, or the mouse button pressed.</param>
		/// <param name="wheelDelta">The amount the wheel was scrolled.</param>
		/// <param name="pressure">The pressure of the touch event.</param>
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

		/// <summary>
		/// Gets or sets a value indicating whether the event has been handled and should not propagate further.
		/// </summary>
		public bool Handled { get; set; }

		/// <summary>
		/// Gets the ID that can be used to track this particular event.
		/// </summary>
		/// <remarks>
		/// This ID represents the finger/pointer ID or mouse button number that initiated the event. This ID will remain the same for the duration of the touch operation.
		/// </remarks>
		public long Id { get; private set; }

		/// <summary>
		/// Gets a value indicating which type of touch action resulted in this event being raised.
		/// </summary>
		public SKTouchAction ActionType { get; private set; }

		/// <summary>
		/// Gets a value indicating which type of touch device was used to raise this event.
		/// </summary>
		public SKTouchDeviceType DeviceType { get; private set; }

		/// <summary>
		/// Gets a value indicating which mouse button resulted in this event being raised.
		/// </summary>
		/// <remarks>
		/// If the mouse was used, then this property will indicate which button was pressed. If a finger (touch) or pen was used, then this property will indicate left button.
		/// </remarks>
		public SKMouseButton MouseButton { get; private set; }

		/// <summary>
		/// Gets the location of the touch on the view (in pixel coordinates).
		/// </summary>
		public SKPoint Location { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the device is touching the screen, or being pressed, at the current time.
		/// </summary>
		public bool InContact { get; private set; }

		/// <summary>
		/// Gets the amount that the mouse wheel was scrolled.
		/// </summary>
		public int WheelDelta { get; private set; }

		/// <summary>
		/// Gets the pressure of the touch event.
		/// </summary>
		/// <remarks>
		/// The pressure generally ranges from 0 (no pressure at all) to 1 (normal pressure), however values higher than 1 may be generated depending on the calibration of the input device.
		/// </remarks>
		public float Pressure { get; private set; }

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>A string that represents the current object.</returns>
		public override string ToString()
		{
			return $"{{ActionType={ActionType}, DeviceType={DeviceType}, Handled={Handled}, Id={Id}, InContact={InContact}, Location={Location}, MouseButton={MouseButton}, WheelDelta={WheelDelta}, Pressure={Pressure}}}";
		}
	}

	/// <summary>
	/// Specifies constants that define which touch/mouse action took place.
	/// </summary>
	public enum SKTouchAction
	{
		/// <summary>
		/// The touch/mouse entered the view.
		/// </summary>
		Entered,
		/// <summary>
		/// A finger or pen was touched on the screen, or a mouse button was pressed.
		/// </summary>
		Pressed,
		/// <summary>
		/// The touch (while down) or mouse (pressed or released) moved in the view.
		/// </summary>
		Moved,
		/// <summary>
		/// A finger or pen was lifted off the screen, or a mouse button was released.
		/// </summary>
		Released,
		/// <summary>
		/// The touch/mouse operation was cancelled.
		/// </summary>
		Cancelled,
		/// <summary>
		/// The touch/mouse exited the view.
		/// </summary>
		Exited,
		/// <summary>
		/// The mouse wheel was scrolled.
		/// </summary>
		WheelChanged,
	}

	/// <summary>
	/// Specifies constants that define which touch device was used.
	/// </summary>
	public enum SKTouchDeviceType
	{
		/// <summary>
		/// A finger on the screen was being used when the event was raised.
		/// </summary>
		Touch,
		/// <summary>
		/// A mouse was being used when the event was raised.
		/// </summary>
		Mouse,
		/// <summary>
		/// A pen on the screen was being used when the event was raised.
		/// </summary>
		Pen
	}

	/// <summary>
	/// Specifies constants that define which mouse button was pressed.
	/// </summary>
	public enum SKMouseButton
	{
		/// <summary>
		/// An unknown mouse button was pressed.
		/// </summary>
		Unknown,

		/// <summary>
		/// The left mouse button was pressed, or, a finger/pen was touched on the screen.
		/// </summary>
		Left,
		/// <summary>
		/// The middle mouse button was pressed.
		/// </summary>
		Middle,
		/// <summary>
		/// The right mouse button was pressed.
		/// </summary>
		Right
	}
}
