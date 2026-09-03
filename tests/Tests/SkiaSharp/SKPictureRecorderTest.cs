using Xunit;

namespace SkiaSharp.Tests
{
	public class SKPictureRecorderTest : SKTest
	{
		[Fact]
		public void CanCreateRecorder()
		{
			var recorder = new SKPictureRecorder();

			recorder.Dispose();
		}

		[Fact]
		public void CanBeginRecording()
		{
			using var recorder = new SKPictureRecorder();

			recorder.BeginRecording(SKRect.Create(100, 100));
		}

		[Fact]
		public void DisposingCanvasBeforeRecorderDoesNotCrash()
		{
			var recorder = new SKPictureRecorder();

			var canvas = recorder.BeginRecording(SKRect.Create(100, 100));
			canvas.DrawColor(SKColors.Blue);
			canvas.Dispose();

			recorder.Dispose();
		}

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
		public void CanCreateRecorderAndDrawOnCanvas(bool useRTree)
		{
			var cullRect = SKRect.Create(100, 100);

			using var recorder = new SKPictureRecorder();

			var canvas = recorder.BeginRecording(cullRect, useRTree);
			canvas.DrawColor(SKColors.Blue);

			using var picture = recorder.EndRecording();
			Assert.NotNull(picture);
			Assert.Equal(cullRect, picture.CullRect);
		}

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
		public void CanCreateDrawableFromRecorder(bool useRTree)
		{
			var cullRect = SKRect.Create(100, 100);

			using var recorder = new SKPictureRecorder();
			var canvas = recorder.BeginRecording(cullRect, useRTree);

			canvas.DrawColor(SKColors.Blue);

			using var drawable = recorder.EndRecordingAsDrawable();
			Assert.NotNull(drawable);
			Assert.Equal(cullRect, drawable.Bounds);
		}

		[InlineData(false, 0, 0, 100, 100)]
		[InlineData(true, 20, 20, 60, 60)]
		[Theory]
		public void UsingRTreeClipsOperations(bool useRTree, int x, int y, int w, int h)
		{
			using var recorder = new SKPictureRecorder();
			var canvas = recorder.BeginRecording(SKRect.Create(100, 100), useRTree);

			canvas.DrawRect(60, 60, 20, 20, new());
			canvas.DrawRect(20, 20, 20, 20, new());
			
			using var picture = recorder.EndRecording();

			Assert.Equal(SKRect.Create(x, y, w, h), picture.CullRect);
		}

		[Fact]
		public void DrawableFromRecorderReproducesGeometryOnPlayback()
		{
			// Guards against regressions from the M154 SkBigPicture -> SkPicture refactor
			// (upstream b392fb672d). Ensures that a picture recorded through
			// EndRecordingAsDrawable reproduces its recorded geometry and colors when
			// played back through both SKDrawable.Draw and SKDrawable.Snapshot -> Playback.
			// Uses only stable, cross-platform, CPU-deterministic APIs.

			var cullRect = SKRect.Create(0, 0, 40, 40);

			using var recorder = new SKPictureRecorder();
			var recCanvas = recorder.BeginRecording(cullRect);
			DrawTestBitmap(recCanvas, 40, 40);

			using var drawable = recorder.EndRecordingAsDrawable();
			Assert.NotNull(drawable);
			Assert.Equal(cullRect, drawable.Bounds);

			// Path 1: Draw the drawable directly onto a fresh canvas and confirm the
			// four quadrant colors of the recorded geometry survive the drawable playback.
			using (var bmp = new SKBitmap(40, 40))
			using (var cnv = new SKCanvas(bmp))
			{
				drawable.Draw(cnv, 0, 0);
				ValidateTestBitmap(bmp);
			}

			// Path 2: Snapshot the drawable into an SKPicture and validate that both
			// Playback and DrawPicture reproduce the same recorded geometry.
			using var snapshot = drawable.Snapshot();
			Assert.NotNull(snapshot);
			Assert.Equal(cullRect, snapshot.CullRect);
			Assert.True(snapshot.ApproximateBytesUsed > 0);

			using (var bmp = new SKBitmap(40, 40))
			using (var cnv = new SKCanvas(bmp))
			{
				snapshot.Playback(cnv);
				ValidateTestBitmap(bmp);
			}

			using (var bmp = new SKBitmap(40, 40))
			using (var cnv = new SKCanvas(bmp))
			{
				cnv.DrawPicture(snapshot);
				ValidateTestBitmap(bmp);
			}
		}
	}
}
