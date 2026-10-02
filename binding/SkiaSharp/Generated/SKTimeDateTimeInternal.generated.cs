using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_document_pdf_datetime_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKTimeDateTimeInternal : IEquatable<SKTimeDateTimeInternal> {
		// public int16_t fTimeZoneMinutes
		public Int16 fTimeZoneMinutes;

		// public uint16_t fYear
		public UInt16 fYear;

		// public uint8_t fMonth
		public Byte fMonth;

		// public uint8_t fDayOfWeek
		public Byte fDayOfWeek;

		// public uint8_t fDay
		public Byte fDay;

		// public uint8_t fHour
		public Byte fHour;

		// public uint8_t fMinute
		public Byte fMinute;

		// public uint8_t fSecond
		public Byte fSecond;

		public readonly bool Equals (SKTimeDateTimeInternal obj) =>
#pragma warning disable CS8909
			fTimeZoneMinutes == obj.fTimeZoneMinutes && fYear == obj.fYear && fMonth == obj.fMonth && fDayOfWeek == obj.fDayOfWeek && fDay == obj.fDay && fHour == obj.fHour && fMinute == obj.fMinute && fSecond == obj.fSecond;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKTimeDateTimeInternal f && Equals (f);

		public static bool operator == (SKTimeDateTimeInternal left, SKTimeDateTimeInternal right) =>
			left.Equals (right);

		public static bool operator != (SKTimeDateTimeInternal left, SKTimeDateTimeInternal right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fTimeZoneMinutes);
			hash.Add (fYear);
			hash.Add (fMonth);
			hash.Add (fDayOfWeek);
			hash.Add (fDay);
			hash.Add (fHour);
			hash.Add (fMinute);
			hash.Add (fSecond);
			return hash.ToHashCode ();
		}

	}
}
