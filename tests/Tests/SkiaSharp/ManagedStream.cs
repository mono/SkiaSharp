using System;

namespace SkiaSharp.Tests
{
	public class ManagedStream : SKAbstractManagedStream
	{
		protected override IntPtr OnRead (IntPtr buffer, IntPtr size) => (IntPtr)0;

		protected override IntPtr OnPeek (IntPtr buffer, IntPtr size) => (IntPtr)0;

		protected override bool OnIsAtEnd () => false;

		protected override bool OnHasPosition () => false;

		protected override bool OnHasLength () => false;

		protected override bool OnRewind () => false;

		protected override IntPtr OnGetPosition () => (IntPtr)0;

		protected override IntPtr OnGetLength () => (IntPtr)0;

		protected override bool OnSeek (IntPtr position) => false;

		protected override bool OnMove (int offset) => false;

		protected override IntPtr OnCreateNew () => IntPtr.Zero;
	}
}
