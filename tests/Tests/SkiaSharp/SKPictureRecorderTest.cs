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
		public void CanCreateRecorderAndDrawOnCanvas()
		{
			var recorder = new SKPictureRecorder();

			var canvas = recorder.BeginRecording(SKRect.Create(100, 100));
			canvas.DrawColor(SKColors.Blue);
			canvas.Dispose();

			recorder.Dispose();
		}

		[SkippableFact]
		public void CanCreateFromRecorder()
		{
			var cullRect = SKRect.Create(100, 100);

			using (var recorder = new SKPictureRecorder())
			using (var canvas = recorder.BeginRecording(cullRect))
			{
				canvas.DrawColor(SKColors.Blue);

				using (var drawable = recorder.EndRecordingAsDrawable())
				{
					Assert.NotNull(drawable);
					Assert.Equal(cullRect, drawable.Bounds);
				}
			}
		}
	}
}
