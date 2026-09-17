using System;

using Microsoft.Maui;

namespace SkiaSharp.Views.Maui
{
	/// <summary>Provides data for the <see cref="E:SkiaSharp.Views.Maui.Controls.SKCanvasView.Touch" /> event.</summary>
	/// <remarks>Set <see cref="P:SkiaSharp.Views.Maui.SKTouchEventArgs.Handled" /> to <see langword="true" /> to indicate the event has been handled and prevent further processing.</remarks>
	public class SKTouchEventArgs : EventArgs
	{
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.SKTouchEventArgs" /> class.</summary>
		/// <param name="id">The unique identifier for this touch point.</param>
		/// <param name="type">The type of touch action.</param>
		/// <param name="location">The location of the touch point in pixels.</param>
		/// <param name="inContact">Whether the pointer is in contact with the surface.</param>
		/// <remarks />
		public SKTouchEventArgs(long id, SKTouchAction type, SKPoint location, bool inContact)
			: this(id, type, SKMouseButton.Left, SKTouchDeviceType.Touch, location, inContact, 0, 1)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.SKTouchEventArgs" /> class with device type information.</summary>
		/// <param name="id">The unique identifier for this touch point.</param>
		/// <param name="type">The type of touch action.</param>
		/// <param name="mouseButton">The mouse button involved in the event.</param>
		/// <param name="deviceType">The type of input device.</param>
		/// <param name="location">The location of the touch point in pixels.</param>
		/// <param name="inContact">Whether the pointer is in contact with the surface.</param>
		/// <remarks />
		public SKTouchEventArgs(long id, SKTouchAction type, SKMouseButton mouseButton, SKTouchDeviceType deviceType, SKPoint location, bool inContact)
			: this(id, type, mouseButton, deviceType, location, inContact, 0, 1)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.SKTouchEventArgs" /> class with wheel delta.</summary>
		/// <param name="id">The unique identifier for this touch point.</param>
		/// <param name="type">The type of touch action.</param>
		/// <param name="mouseButton">The mouse button involved in the event.</param>
		/// <param name="deviceType">The type of input device.</param>
		/// <param name="location">The location of the touch point in pixels.</param>
		/// <param name="inContact">Whether the pointer is in contact with the surface.</param>
		/// <param name="wheelDelta">The mouse wheel delta value.</param>
		/// <remarks />
		public SKTouchEventArgs(long id, SKTouchAction type, SKMouseButton mouseButton, SKTouchDeviceType deviceType, SKPoint location, bool inContact, int wheelDelta)
			: this(id, type, mouseButton, deviceType, location, inContact, wheelDelta, 1)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.SKTouchEventArgs" /> class with pressure support.</summary>
		/// <param name="id">The unique identifier for this touch point.</param>
		/// <param name="type">The type of touch action.</param>
		/// <param name="mouseButton">The mouse button involved in the event.</param>
		/// <param name="deviceType">The type of input device.</param>
		/// <param name="location">The location of the touch point in pixels.</param>
		/// <param name="inContact">Whether the pointer is in contact with the surface.</param>
		/// <param name="wheelDelta">The mouse wheel delta value.</param>
		/// <param name="pressure">The pressure value from a pen or touch device (0.0 to 1.0).</param>
		/// <remarks />
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

		/// <summary>Gets or sets a value indicating whether the event has been handled.</summary>
		/// <value><see langword="true" /> if the event has been handled; otherwise, <see langword="false" />.</value>
		/// <remarks>Set this property to <see langword="true" /> to indicate the event has been handled and prevent further processing by the platform.</remarks>
		public bool Handled { get; set; }

		/// <summary>Gets the unique identifier for this touch point.</summary>
		/// <value>A unique identifier for tracking this touch point across multiple events.</value>
		/// <remarks>This ID remains consistent for a given touch point from Pressed through Moved to Released.</remarks>
		public long Id { get; private set; }

		/// <summary>Gets the type of touch action that occurred.</summary>
		/// <value>A <see cref="T:SkiaSharp.Views.Maui.SKTouchAction" /> value indicating the type of action.</value>
		/// <remarks />
		public SKTouchAction ActionType { get; private set; }

		/// <summary>Gets the type of input device that generated the event.</summary>
		/// <value>A <see cref="T:SkiaSharp.Views.Maui.SKTouchDeviceType" /> value indicating the device type.</value>
		/// <remarks />
		public SKTouchDeviceType DeviceType { get; private set; }

		/// <summary>Gets the mouse button involved in the event.</summary>
		/// <value>A <see cref="T:SkiaSharp.Views.Maui.SKMouseButton" /> value indicating which mouse button was used.</value>
		/// <remarks />
		public SKMouseButton MouseButton { get; private set; }

		/// <summary>Gets the location of the touch point in pixels relative to the view.</summary>
		/// <value>The coordinates of the touch point in the canvas coordinate system.</value>
		/// <remarks />
		public SKPoint Location { get; private set; }

		/// <summary>Gets a value indicating whether the pointer is in contact with the surface.</summary>
		/// <value><see langword="true" /> if the finger is touching the screen or the mouse button is pressed; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool InContact { get; private set; }

		/// <summary>Gets the mouse wheel delta value.</summary>
		/// <value>The amount the mouse wheel was scrolled. Positive values indicate scrolling up or forward, negative values indicate scrolling down or backward.</value>
		/// <remarks>This value is only meaningful when <see cref="P:SkiaSharp.Views.Maui.SKTouchEventArgs.ActionType" /> is <see cref="F:SkiaSharp.Views.Maui.SKTouchAction.WheelChanged" />.</remarks>
		public int WheelDelta { get; private set; }

		/// <summary>Gets the pressure of the touch or pen input.</summary>
		/// <value>A value between 0.0 and 1.0 indicating the pressure, where 0.0 is no pressure and 1.0 is maximum pressure.</value>
		/// <remarks>Pressure is typically available only for pen or stylus input devices.</remarks>
		public float Pressure { get; private set; }

		/// <summary>Returns a string representation of the touch event.</summary>
		/// <returns>A string describing the touch event properties.</returns>
		/// <remarks />
		public override string ToString()
		{
			return $"{{ActionType={ActionType}, DeviceType={DeviceType}, Handled={Handled}, Id={Id}, InContact={InContact}, Location={Location}, MouseButton={MouseButton}, WheelDelta={WheelDelta}, Pressure={Pressure}}}";
		}
	}

	/// <summary>Specifies the type of touch action that occurred.</summary>
	/// <remarks />
	public enum SKTouchAction
	{
		/// <summary>A pointer entered the view bounds.</summary>
		Entered,
		/// <summary>A pointer made contact with the view (finger touched or mouse button pressed).</summary>
		Pressed,
		/// <summary>The pointer moved while in contact with the view.</summary>
		Moved,
		/// <summary>A pointer was released from contact with the view (finger lifted or mouse button released).</summary>
		Released,
		/// <summary>The touch interaction was cancelled.</summary>
		Cancelled,
		/// <summary>A pointer exited the view bounds.</summary>
		Exited,
		/// <summary>The mouse wheel was scrolled.</summary>
		WheelChanged,
	}

	/// <summary>Specifies the type of input device that generated a touch event.</summary>
	/// <remarks />
	public enum SKTouchDeviceType
	{
		/// <summary>The input came from a touch screen (finger).</summary>
		Touch,
		/// <summary>The input came from a mouse device.</summary>
		Mouse,
		/// <summary>The input came from a pen or stylus device.</summary>
		Pen
	}

	/// <summary>Specifies which mouse button was used in a touch event.</summary>
	/// <remarks />
	public enum SKMouseButton
	{
		/// <summary>The mouse button is unknown or not applicable (e.g., for touch input).</summary>
		Unknown,

		/// <summary>The left mouse button.</summary>
		Left,
		/// <summary>The middle mouse button.</summary>
		Middle,
		/// <summary>The right mouse button.</summary>
		Right
	}
}
