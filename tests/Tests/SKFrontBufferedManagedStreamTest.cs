using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKFrontBufferedManagedStreamTest : SKTest
	{
		// All tests will buffer this string, and compare output to the original.
		// The string is long to ensure that all of our lengths being tested are
		// smaller than the string length.
		private const string gAbcsString = "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwx";
		private readonly byte[] gAbcs = StringUtilities.GetEncodedText (gAbcsString, SKTextEncoding.Utf8);

		// Test that hasLength() returns the correct value, based on the stream
		// being wrapped. A length can only be known if the wrapped stream has a
		// length and it has a position (so its initial position can be taken into
		// account when computing the length).
		private static void test_hasLength (SKStream bufferedStream, SKStream streamBeingBuffered)
		{
			if (streamBeingBuffered.HasLength && streamBeingBuffered.HasPosition)
			{
				Assert.True (bufferedStream.HasLength);
			}
			else
			{
				Assert.False (bufferedStream.HasLength);
			}
		}

		private static void test_rewind (SKStream bufferedStream, bool shouldSucceed)
		{
			var success = bufferedStream.Rewind ();
			Assert.Equal (shouldSucceed, success);
		}

		static void test_read (SKStream bufferedStream, byte[] expectations, int bytesToRead)
		{
			// output for reading bufferedStream.
			var storage = new byte[bytesToRead];

			var bytesRead = bufferedStream.Read (storage, bytesToRead);
			Assert.True (bytesRead == bytesToRead || bufferedStream.IsAtEnd);
			Assert.Equal (expectations.Take (bytesRead), storage.Take (bytesRead));
		}

		[SkippableTheory]
		[InlineData (6)]
		[InlineData (15)]
		[InlineData (64)]
		public void SkippingDoesNotPreventReading (int bufferSize)
		{
			var memStream = new SKMemoryStream (gAbcs);
			var bufferedStream = new SKFrontBufferedManagedStream (memStream, bufferSize);

			test_hasLength (bufferedStream, memStream);

			// Skip half the buffer.
			bufferedStream.Skip (bufferSize / 2);

			// Rewind, then read part of the buffer, which should have been read.
			test_rewind (bufferedStream, true);
			test_read (bufferedStream, gAbcs, bufferSize / 4);

			// Now skip beyond the buffered piece, but still within the total buffer.
			bufferedStream.Skip (bufferSize / 2);

			// Test that reading will still work.
			test_read (bufferedStream, gAbcs.Skip (memStream.Position).ToArray (), bufferSize / 4);

			test_rewind (bufferedStream, true);
			test_read (bufferedStream, gAbcs, bufferSize);
		}

		[SkippableTheory]
		[InlineData (6)]
		[InlineData (15)]
		[InlineData (64)]
		public void IncrementalBuffering (int bufferSize)
		{
			var memStream = new SKMemoryStream (gAbcs);
			var bufferedStream = new SKFrontBufferedManagedStream (memStream, bufferSize);

			test_hasLength (bufferedStream, memStream);

			// First, test reading less than the max buffer size.
			test_read (bufferedStream, gAbcs, bufferSize / 2);

			// Now test rewinding back to the beginning and reading less than what was
			// already buffered.
			test_rewind (bufferedStream, true);
			test_read (bufferedStream, gAbcs, bufferSize / 4);

			// Now test reading part of what was buffered, and buffering new data.
			test_read (bufferedStream, gAbcs.Skip (bufferSize / 4).ToArray (), bufferSize / 2);

			// Now test reading what was buffered, buffering new data, and
			// reading directly from the stream.
			test_rewind (bufferedStream, true);
			test_read (bufferedStream, gAbcs, bufferSize << 1);

			// We have reached the end of the buffer, so rewinding will fail.
			// This test assumes that the stream is larger than the buffer; otherwise the
			// result of rewind should be true.
			test_rewind (bufferedStream, false);
		}

		[SkippableTheory]
		[InlineData (6)]
		[InlineData (15)]
		[InlineData (64)]
		public void PerfectlySizedBuffer (int bufferSize)
		{
			var memStream = new SKMemoryStream (gAbcs);
			var bufferedStream = new SKFrontBufferedManagedStream (memStream, bufferSize);

			test_hasLength (bufferedStream, memStream);

			// Read exactly the amount that fits in the buffer.
			test_read (bufferedStream, gAbcs, bufferSize);

			// Rewinding should succeed.
			test_rewind (bufferedStream, true);

			// Once again reading buffered info should succeed
			test_read (bufferedStream, gAbcs, bufferSize);

			// Read past the size of the buffer. At this point, we cannot return.
			test_read (bufferedStream, gAbcs.Skip (memStream.Position).ToArray (), 1);
			test_rewind (bufferedStream, false);
		}

		[SkippableTheory]
		[InlineData (6)]
		[InlineData (15)]
		[InlineData (64)]
		public void InitialOffset (int bufferSize)
		{
			var memStream = new SKMemoryStream (gAbcs);

			// Skip a few characters into the memStream, so that bufferedStream represents an offset into
			// the stream it wraps.
			var arbitraryOffset = 17;
			memStream.Skip (arbitraryOffset);
			var bufferedStream = new SKFrontBufferedManagedStream (memStream, bufferSize);

			// Since SkMemoryStream has a length, bufferedStream must also.
			Assert.True (bufferedStream.HasLength);

			var amountToRead = 10;
			var bufferedLength = bufferedStream.Length;
			var currentPosition = 0;

			// Read the stream in chunks. After each read, the position must match currentPosition,
			// which sums the amount attempted to read, unless the end of the stream has been reached.
			// Importantly, the end should not have been reached until currentPosition == bufferedLength.
			while (currentPosition < bufferedLength)
			{
				Assert.False (bufferedStream.IsAtEnd);
				test_read (bufferedStream, gAbcs.Skip (arbitraryOffset + currentPosition).ToArray (), amountToRead);
				currentPosition = Math.Min (currentPosition + amountToRead, bufferedLength);
				Assert.Equal (currentPosition, memStream.Position - arbitraryOffset);
			}
			Assert.True (bufferedStream.IsAtEnd);
			Assert.Equal (bufferedLength, currentPosition);
		}

		// Dummy stream that optionally has a length and/or position. Tests that FrontBufferedStream's
		// length depends on the stream it's buffering having a length and position.
		private class LengthOptionalStream : ManagedStream
		{
			private readonly bool fHasLength;
			private readonly bool fHasPosition;

			public LengthOptionalStream (bool hasLength, bool hasPosition)
			{
				fHasLength = hasLength;
				fHasPosition = hasPosition;
			}

			protected override bool OnHasLength () => fHasLength;

			protected override bool OnHasPosition () => fHasPosition;
		}

		[SkippableTheory]
		[InlineData (6)]
		[InlineData (15)]
		[InlineData (64)]
		public void LengthCombinations (int bufferSize)
		{
			{
				var stream = new LengthOptionalStream (true, true);
				var buffered = new SKFrontBufferedManagedStream (stream, bufferSize);
				test_hasLength (buffered, stream);
			}
			{
				var stream = new LengthOptionalStream (true, false);
				var buffered = new SKFrontBufferedManagedStream (stream, bufferSize);
				test_hasLength (buffered, stream);
			}
			{
				var stream = new LengthOptionalStream (false, true);
				var buffered = new SKFrontBufferedManagedStream (stream, bufferSize);
				test_hasLength (buffered, stream);
			}
			{
				var stream = new LengthOptionalStream (false, false);
				var buffered = new SKFrontBufferedManagedStream (stream, bufferSize);
				test_hasLength (buffered, stream);
			}
		}

		//// Test that a FrontBufferedStream does not allow reading after the end of a stream.
		// This class is a dummy SkStream which reports that it is at the end on the first
		// read (simulating a failure). Then it tracks whether someone calls read() again.
		private class FailingStream : ManagedStream
		{
			private bool fAtEnd;

			protected override bool OnIsAtEnd () => fAtEnd;

			protected override IntPtr OnRead (IntPtr buffer, IntPtr size)
			{
				Assert.False (fAtEnd);
				fAtEnd = true;
				return (IntPtr)0;
			}
		}

		[SkippableFact]
		public void StreamPeek ()
		{
			var gAbcsString = "abcdefghijklmnopqrstuvwxyz";
			var gAbcs = StringUtilities.GetEncodedText (gAbcsString, SKTextEncoding.Utf8);

			var memStream = new SKMemoryStream (gAbcs);

			test_fully_peekable_stream (memStream, memStream.Length);
		}

		[SkippableFact]
		public void StreamPeek2 ()
		{
			var gAbcsString = "abcdefghijklmnopqrstuvwxyz";
			var gAbcs = StringUtilities.GetEncodedText (gAbcsString, SKTextEncoding.Utf8);

			var memStream = new SKMemoryStream (gAbcs);

			for (var bufferSize = 1; bufferSize < memStream.Length; bufferSize++)
			{
				var bufferedStream = new SKFrontBufferedManagedStream (memStream, bufferSize);

				var peeked = 0;
				for (var i = 1; !bufferedStream.IsAtEnd; i++)
				{
					var unpeekableBytes = compare_peek_to_read (bufferedStream, i);
					if (unpeekableBytes > 0)
					{
						// This could not have returned a number greater than i.
						Assert.True (unpeekableBytes <= i);

						// We have reached the end of the buffer. Verify that it was at least
						// bufferSize.
						Assert.True (peeked + i - unpeekableBytes >= bufferSize);
						// No more peeking is supported.
						break;
					}
					peeked += i;
				}
			}
		}

		[SkippableFact]
		public void StreamPeek3 ()
		{
			var gAbcsString = "abcdefghijklmnopqrstuvwxyz";
			var gAbcs = StringUtilities.GetEncodedText (gAbcsString, SKTextEncoding.Utf8);

			for (var bufferSize = 1; bufferSize < gAbcs.Length; bufferSize++)
			{
				var memStream = new SKMemoryStream (gAbcs);
				var bufferedStream = new SKFrontBufferedManagedStream (memStream, bufferSize);

				var bytesToPeek = bufferSize + 1;

				var peekStorage = SKData.Create (bytesToPeek);
				var readStorage = SKData.Create (bytesToPeek);

				for (var start = 0; start <= bufferSize; start++)
				{
					// Skip to the starting point
					Assert.Equal (start, bufferedStream.Skip (start));

					var bytesPeeked = bufferedStream.Peek (peekStorage.Data, bytesToPeek);
					if (0 == bytesPeeked)
					{
						// Peeking should only fail completely if we have read/skipped beyond the buffer.
						Assert.True (start >= bufferSize);
						break;
					}

					// Only read the amount that was successfully peeked.
					var bytesRead = bufferedStream.Read (readStorage.Data, bytesPeeked);
					Assert.Equal (bytesPeeked, bytesRead);
					Assert.Equal (peekStorage.ToArray ().Take (bytesPeeked), readStorage.ToArray ().Take (bytesPeeked));

					// This should be safe to rewind.
					Assert.True (bufferedStream.Rewind ());
				}
			}
		}

		private void test_fully_peekable_stream (SKStream stream, int limit)
		{
			for (var i = 1; !stream.IsAtEnd; i++)
			{
				Assert.Equal (0, compare_peek_to_read (stream, i));
			}
		}

		private int compare_peek_to_read (SKStream stream, int bytesToPeek)
		{
			Assert.True (bytesToPeek > 0);

			var peekStorage = SKData.Create (bytesToPeek);
			var readStorage = SKData.Create (bytesToPeek);

			var bytesPeeked = stream.Peek (peekStorage.Data, bytesToPeek);
			var bytesRead = stream.Read (readStorage.Data, bytesToPeek);

			// bytesRead should only be less than attempted if the stream is at the
			// end.
			Assert.True (bytesRead == bytesToPeek || stream.IsAtEnd);

			// peek and read should behave the same, except peek returned to the
			// original position, so they read the same data.
			Assert.Equal (peekStorage.ToArray ().Take (bytesPeeked), readStorage.ToArray ().Take (bytesPeeked));

			// A stream should never be able to peek more than it can read.
			Assert.True (bytesRead >= bytesPeeked);

			return bytesRead - bytesPeeked;
		}
	}
}
