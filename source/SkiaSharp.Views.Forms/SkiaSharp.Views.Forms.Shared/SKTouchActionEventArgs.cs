using System;
using System.Linq;

namespace SkiaSharp.Views.Forms
{
	public class SKTouchActionEventArgs : EventArgs
	{
		public SKTouchActionEventArgs(long id, SKTouchActionType type, SKPoint[] locations)
		{
			Id = id;
			Type = type;
			Locations = locations ?? new SKPoint[0];
			Handled = true;
		}

		public SKTouchActionEventArgs(long id, SKTouchActionType type, SKPoint location)
		{
			Id = id;
			Type = type;
			Locations = new[] { location };
			Handled = true;
		}

		// this may be removed, but for now keep it
		internal bool Handled { get; set; }

		public long Id { get; private set; }

		public SKTouchActionType Type { get; private set; }

		public SKPoint[] Locations { get; private set; }

		public SKPoint Location => Locations.FirstOrDefault();

		public bool InContact => Type == SKTouchActionType.Pressed || Type == SKTouchActionType.Moved;
	}

	public enum SKTouchActionType
	{
		Pressed,
		Moved,
		Released,
		Cancelled
	}
}
