#if WINDOWS || NETFRAMEWORK
using System.Text;
using Xunit;

namespace SkiaSharp.Tests
{
	public class Win32WindowTest
	{
		[Fact]
		public void DistinctClassNamesDoNotCollide()
		{
			using var first = new Win32Window("Win32WindowTest_First_\u03a9");
			using var second = new Win32Window("Win32WindowTest_Second");

			Assert.NotEqual(first.WindowHandle, second.WindowHandle);
			Assert.True(User32.IsWindow(first.WindowHandle));
			Assert.True(User32.IsWindow(second.WindowHandle));

			var title = new StringBuilder(128);
			User32.GetWindowText(first.WindowHandle, title, title.Capacity);
			Assert.Equal($"The Invisible Man ({first.WindowClassName})", title.ToString());
		}
	}
}
#endif
