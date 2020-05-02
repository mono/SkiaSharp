using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKManagedWStreamTest : SKTest
	{
		[SkippableFact]
		public void DotNetStreamIsCollected()
		{
			var dotnet = new MemoryStream();
			var stream = new SKManagedWStream(dotnet, true);

			Assert.Equal(0, dotnet.Position);

			stream.Dispose();

			Assert.Throws<ObjectDisposedException>(() => dotnet.Position);
		}

		[SkippableFact]
		public void DotNetStreamIsNotCollected()
		{
			var dotnet = new MemoryStream();
			var stream = new SKManagedWStream(dotnet, false);

			Assert.Equal(0, dotnet.Position);

			stream.Dispose();

			Assert.Equal(0, dotnet.Position);
		}

		[SkippableFact]
		public unsafe void StreamIsNotDisposedWhenReferencedIsDisposed()
		{
			var stream = new SKManagedWStream(new MemoryStream(), true);
			var handle = stream.Handle;

			var document = SKDocument.CreatePdf(stream);
			document.Dispose();

			Assert.True(SKObject.GetInstance<SKManagedWStream>(handle, out _));
		}

		[SkippableFact]
		public void StreamIsCollectedEvenWhenNotProperlyDisposed()
		{
			VerifyImmediateFinalizers();

			var handle = DoWork();

			CollectGarbage();

			var exists = SKObject.GetInstance<SKManagedWStream>(handle, out _);
			Assert.False(exists);

			static IntPtr DoWork()
			{
				var dotnet = new MemoryStream();
				var stream = new SKManagedWStream(dotnet, true);
				return stream.Handle;
			}
		}

		[SkippableFact]
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

		[SkippableFact]
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

		[SkippableFact]
		public unsafe void StreamIsReferencedAndNotDisposedPrematurely()
		{
			VerifyImmediateFinalizers();

			DoWork(out var docH, out var streamH);

			CollectGarbage();

			Assert.False(SKObject.GetInstance<SKManagedWStream>(streamH, out _));
			Assert.False(SKObject.GetInstance<SKDocument>(docH, out _));

			static void DoWork(out IntPtr documentHandle, out IntPtr streamHandle)
			{
				var document = CreateDocument(out streamHandle);
				documentHandle = document.Handle;

				CollectGarbage();

				Assert.NotNull(document.BeginPage(100, 100));
				document.EndPage();
				document.Close();

				Assert.True(SKObject.GetInstance<SKManagedWStream>(streamHandle, out var stream));
				Assert.True(stream.OwnsHandle);
				Assert.False(stream.IgnorePublicDispose);
			}

			static SKDocument CreateDocument(out IntPtr streamHandle)
			{
				var stream = new SKManagedWStream(new MemoryStream(), true);
				streamHandle = stream.Handle;

				Assert.True(stream.OwnsHandle);
				Assert.False(stream.IgnorePublicDispose);
				Assert.True(SKObject.GetInstance<SKManagedWStream>(streamHandle, out _));

				return SKDocument.CreatePdf(stream);
			}
		}
	}
}
