using Xunit;

namespace SkiaSharp.Tests
{
	public class SKDrawableTest : SKTest
	{
		[SkippableFact]
		public void CanInstantiateDrawable()
		{
			using (var drawable = new TestDrawable())
			{
			}
		}

		[SkippableFact]
		public void CanAccessBounds()
		{
			using (var drawable = new TestDrawable())
			{
				Assert.Equal(SKRect.Create(100, 100), drawable.Bounds);
				Assert.Equal(1, drawable.BoundsFireCount);
			}
		}

		[SkippableFact]
		public void CanAccessApproxBytes()
		{
			using (var drawable = new TestDrawable())
			{
				Assert.Equal(123, drawable.ApproximateBytesUsed);
				Assert.Equal(1, drawable.ApproxBytesCount);
			}
		}

		[Trait(Traits.FailingOn.Key, Traits.FailingOn.Values.macOS)] // Something with sk_sp<SkPicture> SkDrawable::onMakePictureSnapshot() is causing issues
		[Trait(Traits.FailingOn.Key, Traits.FailingOn.Values.iOS)] // Something with sk_sp<SkPicture> SkDrawable::onMakePictureSnapshot() is causing issues
		[Trait(Traits.FailingOn.Key, Traits.FailingOn.Values.MacCatalyst)] // Something with sk_sp<SkPicture> SkDrawable::onMakePictureSnapshot() is causing issues
		[SkippableFact]
		public void CanCreateSnapshot()
		{
			using (var drawable = new TestDrawable())
			{
				var picture = drawable.Snapshot();
				Assert.NotNull(picture);
				Assert.Equal(1, drawable.SnapshotFireCount);
			}
		}

		[Trait(Traits.FailingOn.Key, Traits.FailingOn.Values.macOS)] // Something with sk_sp<SkPicture> SkDrawable::onMakePictureSnapshot() is causing issues
		[Trait(Traits.FailingOn.Key, Traits.FailingOn.Values.iOS)] // Something with sk_sp<SkPicture> SkDrawable::onMakePictureSnapshot() is causing issues
		[Trait(Traits.FailingOn.Key, Traits.FailingOn.Values.MacCatalyst)] // Something with sk_sp<SkPicture> SkDrawable::onMakePictureSnapshot() is causing issues
		[SkippableFact]
		public void CanUseAllMembers()
		{
			using (var drawable = new TestDrawable())
			{
				Assert.Equal(SKRect.Create(100, 100), drawable.Bounds);
				Assert.Equal(1, drawable.BoundsFireCount);

				using (var bmp = new SKBitmap(100, 100))
				using (var canvas = new SKCanvas(bmp))
				{
					drawable.Draw(canvas, 0, 0);
					Assert.Equal(1, drawable.DrawFireCount);

					canvas.DrawDrawable(drawable, 0, 0);
					Assert.Equal(2, drawable.DrawFireCount);
				}

				var picture = drawable.Snapshot();
				Assert.NotNull(picture);
				Assert.Equal(1, drawable.SnapshotFireCount);
			}
		}

		[SkippableFact]
		public void DrawableDrawDraws()
		{
			using (var drawable = new TestDrawable())
			using (var bmp = new SKBitmap(100, 100))
			using (var canvas = new SKCanvas(bmp))
			{
				drawable.Draw(canvas, 0, 0);

				Assert.Equal(SKColors.Blue, bmp.GetPixel(50, 50));
			}
		}

		[SkippableFact]
		public void CanvasDrawsDrawable()
		{
			using (var drawable = new TestDrawable())
			using (var bmp = new SKBitmap(100, 100))
			using (var canvas = new SKCanvas(bmp))
			{
				canvas.DrawDrawable(drawable, 0, 0);

				Assert.Equal(SKColors.Blue, bmp.GetPixel(50, 50));
			}
		}
	}

	class TestDrawable : SKDrawable
	{
		public int DrawFireCount;
		public int BoundsFireCount;
		public int SnapshotFireCount;
		public int ApproxBytesCount;

		protected override void OnDraw(SKCanvas canvas)
		{
			DrawFireCount++;

			canvas.DrawColor(SKColors.Blue);
		}

		protected override SKRect OnGetBounds()
		{
			BoundsFireCount++;

			return SKRect.Create(100, 100);
		}

		protected override SKPicture OnSnapshot()
		{
			SnapshotFireCount++;

			return base.OnSnapshot();
		}

		protected override int OnGetApproximateBytesUsed ()
		{
			ApproxBytesCount++;

			return 123;
		}
	}
}
