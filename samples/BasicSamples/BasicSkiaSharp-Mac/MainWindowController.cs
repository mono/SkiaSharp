using System;
using AppKit;
using Foundation;

namespace BasicSkiaSharp
{
	public partial class MainWindowController : NSWindowController
	{
		public MainWindowController(IntPtr handle)
			: base(handle)
		{
		}

		[Export("initWithCoder:")]
		public MainWindowController(NSCoder coder)
			: base(coder)
		{
		}

		public MainWindowController()
			: base("MainWindow")
		{
		}

		public new MainWindow Window => (MainWindow)base.Window;
	}
}
