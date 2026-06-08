using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Tests
{
	// Reverse-P/Invoke callback proxies (managed methods invoked BY native Skia) must never
	// let a managed exception unwind through native stack frames. On .NET Framework (notably
	// x86) that corrupts the native heap/stack and later surfaces as an AccessViolationException
	// in an unrelated, concurrently-running native call; on .NET it triggers a fatal FailFast.
	//
	// Before the proxies were hardened, every one of these tests crashed the test host process
	// (the exception propagated out of the [MonoPInvokeCallback]/[UnmanagedCallersOnly] boundary).
	// After hardening, the exception is contained and the native operation fails cleanly.
	public class SKDelegateProxiesExceptionIsolationTest : SKTest
	{
		private sealed class ThrowingReadStream : Stream
		{
			private readonly bool canSeek;

			public ThrowingReadStream (bool canSeek) => this.canSeek = canSeek;

			public override bool CanRead => true;
			public override bool CanSeek => canSeek;
			public override bool CanWrite => false;
			public override long Length => 1024;
			public override long Position { get; set; }
			public override void Flush () { }
			public override int Read (byte[] buffer, int offset, int count) =>
				throw new InvalidOperationException ("boom from managed stream Read");
			public override long Seek (long offset, SeekOrigin origin) => 0;
			public override void SetLength (long value) { }
			public override void Write (byte[] buffer, int offset, int count) { }
		}

		private sealed class ThrowingWriteStream : Stream
		{
			public override bool CanRead => false;
			public override bool CanSeek => false;
			public override bool CanWrite => true;
			public override long Length => throw new NotSupportedException ();
			public override long Position { get => 0; set { } }
			public override void Flush () { }
			public override int Read (byte[] buffer, int offset, int count) => 0;
			public override long Seek (long offset, SeekOrigin origin) => 0;
			public override void SetLength (long value) { }
			public override void Write (byte[] buffer, int offset, int count) =>
				throw new InvalidOperationException ("boom from managed stream Write");
		}

		[SkippableFact]
		public void ThrowingSeekableManagedStreamReadDoesNotCrashProcess ()
		{
			using var managed = new SKManagedStream (new ThrowingReadStream (canSeek: true), true);

			// SKCodec.Create drives native reads that call back into the managed stream.
			// The callback throws; it must be contained and Create must fail gracefully.
			using var codec = SKCodec.Create (managed);

			// Reaching this line proves the process was not torn down by an exception
			// unwinding through native frames.
			Assert.Null (codec);
		}

		[SkippableFact]
		public void ThrowingNonSeekableManagedStreamReadDoesNotCrashProcess ()
		{
			using var managed = new SKManagedStream (new ThrowingReadStream (canSeek: false), true);

			using var codec = SKCodec.Create (managed);

			Assert.Null (codec);
		}

		[SkippableFact]
		public void ThrowingManagedWStreamWriteDoesNotCrashProcess ()
		{
			// The failed encode below makes libpng emit a native "sk_write_fn cannot write to
			// stream" diagnostic to the console. The device/browser test harnesses (WASM and the
			// Apple/Android mlaunch runners) translate any native "error :" console output into a
			// fatal build error even though this test passes, so it is skipped on those legs. The
			// isolation mechanism is identical to the read-stream tests above, which run on every
			// platform, and the never-crash property is fully exercised by the desktop/.NET Core
			// legs where an unisolated callback throw would FailFast the host.
			SkipOnPlatform (IsBrowser || IsIOS || IsMacCatalyst || IsAndroid, "Device/browser test harnesses treat the expected native libpng write-failure log as a fatal console error.");

			using var bitmap = new SKBitmap (16, 16);
			using var managed = new SKManagedWStream (new ThrowingWriteStream ());

			// Native encode drives writes that call back into the managed write stream.
			// The callback throws; it must be contained and Encode must report failure.
			var encoded = bitmap.Encode (managed, SKEncodedImageFormat.Png, 100);

			Assert.False (encoded);
		}
	}
}
