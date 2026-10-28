using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKManagedWStreamTest : SKTest
	{
		[Fact]
		public void DotNetStreamIsCollected()
		{
			var dotnet = new MemoryStream();
			var stream = new SKManagedWStream(dotnet, true);

			Assert.Equal(0, dotnet.Position);

			stream.Dispose();

			Assert.Throws<ObjectDisposedException>(() => dotnet.Position);
		}

		[Fact]
		public void DotNetStreamIsNotCollected()
		{
			var dotnet = new MemoryStream();
			var stream = new SKManagedWStream(dotnet, false);

			Assert.Equal(0, dotnet.Position);

			stream.Dispose();

			Assert.Equal(0, dotnet.Position);
		}

		[Fact]
		public unsafe void StreamIsNotDisposedWhenReferencedIsDisposed()
		{
			var stream = new SKManagedWStream(new MemoryStream(), true);
			var handle = stream.Handle;

			var document = SKDocument.CreatePdf(stream);
			document.Dispose();

			Assert.True(SKObject.GetInstance<SKManagedWStream>(handle, out _));
		}

		[Fact]
		public void StreamIsCollectedEvenWhenNotProperlyDisposed()
		{
			SkipOnMono();

			DoWork(out var handle, out var weakStream);

			CollectGarbage();

			SKHandleDictionaryTestHelpers.AssertCollected(handle, weakStream);

			static void DoWork(out IntPtr streamHandle, out WeakReference streamRef)
			{
				var dotnet = new MemoryStream();
				var stream = new SKManagedWStream(dotnet, true);
				streamHandle = stream.Handle;
				streamRef = new WeakReference(stream);
			}
		}

		[Fact]
		public void ManagedStreamWritesByteCorrectly()
		{
			var dotnet = new MemoryStream();
			var stream = new SKManagedWStream(dotnet);

			Assert.Equal(0, dotnet.Position);
			Assert.Equal(0, stream.BytesWritten);

			stream.Write8(123);

			Assert.Equal(1, dotnet.Position);
			Assert.Equal(1, stream.BytesWritten);
			Assert.Equal(new byte[] { 123 }, dotnet.ToArray());

			stream.Write8(246);

			Assert.Equal(2, dotnet.Position);
			Assert.Equal(2, stream.BytesWritten);
			Assert.Equal(new byte[] { 123, 246 }, dotnet.ToArray());
		}

		[Fact]
		public void ManagedStreamWritesChunkCorrectly()
		{
			var data = new byte[1024];
			for (var i = 0; i < data.Length; i++)
			{
				data[i] = (byte)(i % byte.MaxValue);
			}

			var dotnet = new MemoryStream();
			var stream = new SKManagedWStream(dotnet);

			Assert.Equal(0, dotnet.Position);
			Assert.Equal(0, stream.BytesWritten);

			stream.Write(data, data.Length);

			Assert.Equal(data.Length, dotnet.Position);
			Assert.Equal(data.Length, stream.BytesWritten);
			Assert.Equal(data, dotnet.ToArray());
		}

		[Fact]
		public unsafe void StreamIsReferencedAndNotDisposedPrematurely()
		{
			SkipOnMono();

			DoWork(out var docH, out var weakDoc, out var streamH, out var weakStream);

			CollectGarbage();

			SKHandleDictionaryTestHelpers.AssertCollected(streamH, weakStream);
			SKHandleDictionaryTestHelpers.AssertCollected(docH, weakDoc);

			static void DoWork(out IntPtr documentHandle, out WeakReference documentRef, out IntPtr streamHandle, out WeakReference streamRef)
			{
				var document = CreateDocument(out streamHandle, out streamRef);
				documentHandle = document.Handle;
				documentRef = new WeakReference(document);

				CollectGarbage();

				Assert.NotNull(document.BeginPage(100, 100));
				document.EndPage();
				document.Close();

				Assert.True(SKObject.GetInstance<SKManagedWStream>(streamHandle, out var stream));
				Assert.True(stream.OwnsHandle);
				Assert.False(stream.IgnorePublicDispose);

				GC.KeepAlive(document);
			}

			static SKDocument CreateDocument(out IntPtr streamHandle, out WeakReference streamRef)
			{
				var stream = new SKManagedWStream(new MemoryStream(), true);
				streamHandle = stream.Handle;
				streamRef = new WeakReference(stream);

				Assert.True(stream.OwnsHandle);
				Assert.False(stream.IgnorePublicDispose);
				Assert.True(SKObject.GetInstance<SKManagedWStream>(streamHandle, out _));

				return SKDocument.CreatePdf(stream);
			}
		}
	}
}
