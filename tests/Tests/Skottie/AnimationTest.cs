using System;
using System.IO;
using SkiaSharp.SceneGraph;
using SkiaSharp.Skottie;
using Xunit;

namespace SkiaSharp.Tests
{
	public class AnimationTest : SKTest
	{
		[SkippableFact]
		public void When_Default_TryParse()
		{
			var path = Path.Combine(PathToImages, "LottieLogo1.json");
			var result = Animation.TryParse(File.ReadAllText(path), out var animation);

			Assert.True(result);
			Assert.NotNull(animation);
			Assert.NotEqual(IntPtr.Zero, animation.Handle);
		}

		[SkippableFact]
		public void When_Default_Parse()
		{
			var path = Path.Combine(PathToImages, "LottieLogo1.json");
			var animation = Animation.Parse(File.ReadAllText(path));

			Assert.NotNull(animation);
			Assert.NotEqual(IntPtr.Zero, animation.Handle);
		}

		[SkippableFact]
		public void When_Default_TryCreate_From_SKStream()
		{
			var path = Path.Combine(PathToImages, "LottieLogo1.json");
			using var fileStream = File.OpenRead(path);
			using var managedStream = new SKManagedStream(fileStream);
			var result = Animation.TryCreate(managedStream, out var animation);

			Assert.True(result);
			Assert.NotNull(animation);
			Assert.NotEqual(IntPtr.Zero, animation.Handle);
		}

		[SkippableFact]
		public void When_Default_Create_From_SKStream()
		{
			var path = Path.Combine(PathToImages, "LottieLogo1.json");
			using var fileStream = File.OpenRead(path);
			using var managedStream = new SKManagedStream(fileStream);
			var animation = Animation.Create(managedStream);

			Assert.NotNull(animation);
			Assert.NotEqual(IntPtr.Zero, animation.Handle);
		}

		[SkippableFact]
		public void When_Default_TryCreate_From_Stream()
		{
			var path = Path.Combine(PathToImages, "LottieLogo1.json");
			using var fileStream = File.OpenRead(path);
			var result = Animation.TryCreate(fileStream, out var animation);

			Assert.True(result);
			Assert.NotEqual(IntPtr.Zero, animation?.Handle);
		}

		[SkippableFact]
		public void When_Default_Create_From_Stream()
		{
			var path = Path.Combine(PathToImages, "LottieLogo1.json");
			using var fileStream = File.OpenRead(path);
			var animation = Animation.Create(fileStream);

			Assert.NotNull(animation);
			Assert.NotEqual(IntPtr.Zero, animation.Handle);
		}

		[SkippableFact]
		public void When_Default_TryCreate_From_File()
		{
			var path = Path.Combine(PathToImages, "LottieLogo1.json");
			var result = Animation.TryCreate(path, out var animation);

			Assert.True(result);
			Assert.NotEqual(IntPtr.Zero, animation?.Handle);
		}

		[SkippableFact]
		public void When_Default_Create_From_File()
		{
			var path = Path.Combine(PathToImages, "LottieLogo1.json");
			var animation = Animation.Create(path);

			Assert.NotNull(animation);
			Assert.NotEqual(IntPtr.Zero, animation.Handle);
		}

		private Animation BuildDefaultAnimation()
		{
			var path = Path.Combine(PathToImages, "LottieLogo1.json");
			var result = Animation.TryCreate(path, out var animation);

			Assert.True(result);
			Assert.NotEqual(IntPtr.Zero, animation?.Handle);

			return animation;
		}

		[SkippableFact]
		public void When_Seek_Without_Controller()
		{
			var animation = BuildDefaultAnimation();

			animation.Seek(.1);
		}

		[SkippableFact]
		public void When_Seek_With_Controller()
		{
			var animation = BuildDefaultAnimation();

			var controller = new InvalidationController();

			animation.Seek(.1, controller);
		}

		[SkippableFact]
		public void When_SeekFrame_Without_Controller()
		{
			var animation = BuildDefaultAnimation();

			animation.SeekFrame(.1);
		}

		[SkippableFact]
		public void When_SeekFrame_With_Controller()
		{
			var animation = BuildDefaultAnimation();

			var controller = new InvalidationController();

			animation.SeekFrame(.1, controller);
		}

		[SkippableFact]
		public void When_SeekFrameTime_Without_Controller()
		{
			var animation = BuildDefaultAnimation();

			animation.SeekFrameTime(.1);
		}

		[SkippableFact]
		public void When_SeekFrameTime_With_Controller()
		{
			var animation = BuildDefaultAnimation();

			var controller = new InvalidationController();

			animation.SeekFrameTime(.1, controller);
		}

		[SkippableFact]
		public void When_SeekFrameTimeSpan_Without_Controller()
		{
			var animation = BuildDefaultAnimation();

			animation.SeekFrameTime(TimeSpan.FromSeconds(.1));
		}

		[SkippableFact]
		public void When_SeekFrameTimeSpan_With_Controller()
		{
			var animation = BuildDefaultAnimation();

			var controller = new InvalidationController();

			animation.SeekFrameTime(TimeSpan.FromSeconds(.1), controller);
		}

		[SkippableFact]
		public void When_Duration()
		{
			var animation = BuildDefaultAnimation();

			Assert.Equal(5.9667, animation.Duration, PRECISION);
		}

		[SkippableFact]
		public void When_Fps()
		{
			var animation = BuildDefaultAnimation();

			Assert.Equal(30, animation.Fps);
		}

		[SkippableFact]
		public void When_InPoint()
		{
			var animation = BuildDefaultAnimation();

			Assert.Equal(0, animation.InPoint);
		}

		[SkippableFact]
		public void When_OutPoint()
		{
			var animation = BuildDefaultAnimation();

			Assert.Equal(179, animation.OutPoint);
		}

		[SkippableFact]
		public void When_Version()
		{
			var animation = BuildDefaultAnimation();

			Assert.Equal("4.4.26", animation.Version);
		}

		[SkippableFact]
		public void When_Size()
		{
			var animation = BuildDefaultAnimation();

			Assert.Equal(new SKSize(375, 667), animation.Size);
		}
	}
}
