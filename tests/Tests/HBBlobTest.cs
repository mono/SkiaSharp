using System;
using System.IO;
using System.Reflection;

using Xunit;

namespace HarfBuzzSharp.Tests
{
	public class HBBlobTest : HBTest
	{
		[SkippableFact]
		public void ShouldCreateFromFileName()
		{
			using (var blob = Blob.FromFile(Path.Combine(PathToFonts, "Distortable.ttf")))
			{
				Assert.Equal(16384, blob.Length);
			}
		}

		[SkippableFact]
		public void ShouldCreateFromStream()
		{
			using (var blob = Blob.FromStream(File.Open(Path.Combine(PathToFonts, "Funkster.ttf"), FileMode.Open, FileAccess.Read)))
			{
				Assert.Equal(236808, blob.Length);
			}
		}

		[SkippableFact]
		public void EmptyBlobsAreExactlyTheSameInstance()
		{
			var emptyBlob1 = Blob.Empty;
			var emptyBlob2 = Blob.Empty;

			Assert.Equal(emptyBlob1, emptyBlob2);
			Assert.Equal(emptyBlob1.Handle, emptyBlob2.Handle);
			Assert.Same(emptyBlob1, emptyBlob2);
		}

		[SkippableFact]
		public void EmptyBlobsAreNotDisposed()
		{
			var emptyBlob = Blob.Empty;
			emptyBlob.Dispose();

			Assert.False(emptyBlob.IsDisposed());
			Assert.NotEqual(IntPtr.Zero, emptyBlob.Handle);
		}
	}
}
