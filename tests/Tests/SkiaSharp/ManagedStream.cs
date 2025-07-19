using System;

namespace SkiaSharp.Tests
{
	public class ManagedStream : SKAbstractManagedStream
	{
		protected internal override IntPtr OnRead (IntPtr buffer, IntPtr size) => (IntPtr)0;

		protected internal override IntPtr OnPeek (IntPtr buffer, IntPtr size) => (IntPtr)0;

		protected internal override bool OnIsAtEnd () => false;

		protected internal override bool OnHasPosition () => false;

		protected internal override bool OnHasLength () => false;

		protected internal override bool OnRewind () => false;

		protected internal override IntPtr OnGetPosition () => (IntPtr)0;

		protected internal override IntPtr OnGetLength () => (IntPtr)0;

		protected internal override bool OnSeek (IntPtr position) => false;

		protected internal override bool OnMove (int offset) => false;

		protected internal override IntPtr OnCreateNew () => IntPtr.Zero;
	}
}
