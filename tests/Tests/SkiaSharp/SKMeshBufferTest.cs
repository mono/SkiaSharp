using System;
using System.Runtime.InteropServices;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKMeshBufferTest : SKTest
	{
		[Fact]
		public void CanCreateVertexBuffer()
		{
			var vertices = new float[] { 0, 0, 100, 0, 50, 100 };
			var data = MemoryMarshal.AsBytes(vertices.AsSpan());

			using var vb = SKMeshVertexBuffer.Create(data);

			Assert.NotNull(vb);
			Assert.Equal(vertices.Length * sizeof(float), vb.Size);
		}

		[Fact]
		public void CanCreateZeroInitializedVertexBuffer()
		{
			using var vb = SKMeshVertexBuffer.Create(24);

			Assert.NotNull(vb);
			Assert.Equal(24, vb.Size);
		}

		[Fact]
		public void CanCreateIndexBuffer()
		{
			var indices = new ushort[] { 0, 1, 2 };
			var data = MemoryMarshal.AsBytes(indices.AsSpan());

			using var ib = SKMeshIndexBuffer.Create(data);

			Assert.NotNull(ib);
			Assert.Equal(indices.Length * sizeof(ushort), ib.Size);
		}

		[Fact]
		public void BufferCreationValidatesArguments()
		{
			Assert.Throws<ArgumentException>(() => SKMeshVertexBuffer.Create(ReadOnlySpan<byte>.Empty));
			Assert.Throws<ArgumentNullException>(() => SKMeshVertexBuffer.Create(IntPtr.Zero, 8));
			Assert.Throws<ArgumentOutOfRangeException>(() => SKMeshVertexBuffer.Create((IntPtr)1, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => SKMeshVertexBuffer.Create((IntPtr)1, -4));
			Assert.Throws<ArgumentOutOfRangeException>(() => SKMeshVertexBuffer.Create(0));
			Assert.Throws<ArgumentOutOfRangeException>(() => SKMeshVertexBuffer.Create(-1));
			Assert.Throws<ArgumentNullException>(() => SKMeshVertexBuffer.Create(null, new byte[] { 1, 2, 3, 4 }));
			Assert.Throws<ArgumentNullException>(() => SKMeshVertexBuffer.Create(null, (IntPtr)1, 4));
			Assert.Throws<ArgumentNullException>(() => SKMeshVertexBuffer.Create(null, 4));

			Assert.Throws<ArgumentException>(() => SKMeshIndexBuffer.Create(ReadOnlySpan<byte>.Empty));
			Assert.Throws<ArgumentNullException>(() => SKMeshIndexBuffer.Create(IntPtr.Zero, 8));
			Assert.Throws<ArgumentOutOfRangeException>(() => SKMeshIndexBuffer.Create((IntPtr)1, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => SKMeshIndexBuffer.Create((IntPtr)1, -4));
			Assert.Throws<ArgumentOutOfRangeException>(() => SKMeshIndexBuffer.Create(0));
			Assert.Throws<ArgumentOutOfRangeException>(() => SKMeshIndexBuffer.Create(-1));
			Assert.Throws<ArgumentNullException>(() => SKMeshIndexBuffer.Create(null, new byte[] { 1, 2, 3, 4 }));
			Assert.Throws<ArgumentNullException>(() => SKMeshIndexBuffer.Create(null, (IntPtr)1, 4));
			Assert.Throws<ArgumentNullException>(() => SKMeshIndexBuffer.Create(null, 4));

			using var vb = SKMeshVertexBuffer.Create(4);
			Assert.Throws<ArgumentNullException>(() => vb.Copy(null));
			Assert.Throws<ArgumentNullException>(() => vb.Update(null, new byte[] { 1, 2, 3, 4 }, 0));
			Assert.Throws<ArgumentNullException>(() => vb.Update(null, (IntPtr)1, 0, 4));

			using var ib = SKMeshIndexBuffer.Create(4);
			Assert.Throws<ArgumentNullException>(() => ib.Copy(null));
			Assert.Throws<ArgumentNullException>(() => ib.Update(null, new byte[] { 1, 2, 3, 4 }, 0));
			Assert.Throws<ArgumentNullException>(() => ib.Update(null, (IntPtr)1, 0, 4));
		}

		[Fact]
		public void CanCopyAndUpdateCpuBuffers()
		{
			var vertices = new float[] { 0, 0, 100, 0, 50, 100 };
			using var vb = SKMeshVertexBuffer.Create(MemoryMarshal.AsBytes(vertices.AsSpan()));
			Assert.NotNull(vb);

			using var vbCopy = vb.Copy();
			Assert.NotNull(vbCopy);
			Assert.Equal(vb.Size, vbCopy.Size);

			var updatedVertex = new float[] { 25, 75 };
			Assert.True(vbCopy.Update(MemoryMarshal.AsBytes(updatedVertex.AsSpan()), 16));
			Assert.False(vbCopy.Update(MemoryMarshal.AsBytes(updatedVertex.AsSpan()), 2)); // unaligned offset

			var indices = new ushort[] { 0, 1, 2, 0 };
			using var ib = SKMeshIndexBuffer.Create(MemoryMarshal.AsBytes(indices.AsSpan()));
			Assert.NotNull(ib);

			using var ibCopy = ib.Copy();
			Assert.NotNull(ibCopy);
			Assert.Equal(ib.Size, ibCopy.Size);
			Assert.True(ibCopy.Update(MemoryMarshal.AsBytes(new ushort[] { 2, 3 }.AsSpan()), 4));
		}
	}
}
