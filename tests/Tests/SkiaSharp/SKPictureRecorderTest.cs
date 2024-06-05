using Xunit;

namespace SkiaSharp.Tests
{
	public class SKPictureRecorderTest : SKTest
	{
		[SkippableFact]
		public void CanCreateRecorder()
		{
			var recorder = new SKPictureRecorder();

			recorder.Dispose();
		}

		[SkippableFact]
		public void CanBeginRecording()
		{
			using var recorder = new SKPictureRecorder();

			recorder.BeginRecording(SKRect.Create(100, 100));
		}

		[SkippableFact]
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
		[SkippableTheory]
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
		[SkippableTheory]
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
		[SkippableTheory]
		public void UsingRTreeClipsOperations(bool useRTree, int x, int y, int w, int h)
		{
			using var recorder = new SKPictureRecorder();
			var canvas = recorder.BeginRecording(SKRect.Create(100, 100), useRTree);

			canvas.DrawRect(60, 60, 20, 20, new());
			canvas.DrawRect(20, 20, 20, 20, new());
			
			using var picture = recorder.EndRecording();

			Assert.Equal(SKRect.Create(x, y, w, h), picture.CullRect);
		}
	}
}
