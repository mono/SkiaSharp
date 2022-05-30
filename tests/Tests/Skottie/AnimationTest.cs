using System;
using System.Collections.Generic;
using Xunit;
using System.IO;
using SkiaSharp.SceneGraph;
using SkiaSharp.Skottie;

namespace SkiaSharp.Tests
{
	public class AnimationTest : SKTest
	{
		[Trait(CategoryKey, ApiCategory)]
		[SkippableFact]
		public void When_Default_Make()
		{
			var path = Path.Combine(PathToImages, "LottieLogo1.json");

			var result = SkiaSharp.Skottie.Animation.TryParse(File.ReadAllText(path), out var animation);
			Assert.True(result);
			Assert.NotEqual(IntPtr.Zero, animation?.Handle);
		}

		[Trait(CategoryKey, ApiCategory)]
		[SkippableFact]
		public void When_Default_Make_From_SKStream()
		{
			var path = Path.Combine(PathToImages, "LottieLogo1.json");

			using var fileStream = File.OpenRead(path);
			using var managedStream = new SKManagedStream(fileStream);
			var result = SkiaSharp.Skottie.Animation.TryCreate(managedStream, out var animation);
			Assert.True(result);
			Assert.NotEqual(IntPtr.Zero, animation?.Handle);
		}

		[Trait(CategoryKey, ApiCategory)]
		[SkippableFact]
		public void When_Default_Make_From_Stream()
		{
			var path = Path.Combine(PathToImages, "LottieLogo1.json");

			using var fileStream = File.OpenRead(path);
			var result = SkiaSharp.Skottie.Animation.TryCreate(fileStream, out var animation);
			Assert.True(result);
			Assert.NotEqual(IntPtr.Zero, animation?.Handle);
		}

		[Trait(CategoryKey, ApiCategory)]
		[SkippableFact]
		public void When_Seek_Without_Controller()
		{
			var path = Path.Combine(PathToImages, "LottieLogo1.json");

			using var fileStream = File.OpenRead(path);
			var result = SkiaSharp.Skottie.Animation.TryCreate(fileStream, out var animation);
			Assert.True(result);
			Assert.NotEqual(IntPtr.Zero, animation?.Handle);

			animation.Seek(.1);
		}

		[Trait(CategoryKey, ApiCategory)]
		[SkippableFact]
		public void When_Seek_With_Controller()
		{
			var path = Path.Combine(PathToImages, "LottieLogo1.json");

			using var fileStream = File.OpenRead(path);
			var result = SkiaSharp.Skottie.Animation.TryCreate(fileStream, out var animation);
			Assert.True(result);
			Assert.NotEqual(IntPtr.Zero, animation?.Handle);

			var controller = new InvalidationController();

			animation.Seek(.1, controller);
		}

		private Animation BuildDefaultAnimation()
		{
			var path = Path.Combine(PathToImages, "LottieLogo1.json");

			using var fileStream = File.OpenRead(path);
			var result = SkiaSharp.Skottie.Animation.TryCreate(fileStream, out var animation);
			Assert.True(result);
			Assert.NotEqual(IntPtr.Zero, animation?.Handle);

			return animation;
		}

		[Trait(CategoryKey, ApiCategory)]
		[SkippableFact]
		public void When_SeekFrame_Without_Controller()
		{
			var animation = BuildDefaultAnimation();

			animation.SeekFrame(.1);
		}

		[Trait(CategoryKey, ApiCategory)]
		[SkippableFact]
		public void When_SeekFrame_With_Controller()
		{
			var animation = BuildDefaultAnimation();

			var controller = new InvalidationController();

			animation.SeekFrame(.1, controller);
		}

		[Trait(CategoryKey, ApiCategory)]
		[SkippableFact]
		public void When_SeekFrameTime_Without_Controller()
		{
			var animation = BuildDefaultAnimation();

			animation.SeekFrameTime(.1);
		}

		[Trait(CategoryKey, ApiCategory)]
		[SkippableFact]
		public void When_SeekFrameTime_With_Controller()
		{
			var animation = BuildDefaultAnimation();

			var controller = new InvalidationController();

			animation.SeekFrameTime(.1, controller);
		}

		[Trait(CategoryKey, ApiCategory)]
		[SkippableFact]
		public void When_Duration()
		{
			var animation = BuildDefaultAnimation();

			Assert.True(animation.Duration > 0);
		}

		[Trait(CategoryKey, ApiCategory)]
		[SkippableFact]
		public void When_Fps()
		{
			var animation = BuildDefaultAnimation();

			Assert.True(animation.Fps == 30);
		}

		[Trait(CategoryKey, ApiCategory)]
		[SkippableFact]
		public void When_InPoint()
		{
			var animation = BuildDefaultAnimation();

			Assert.True(animation.InPoint == 0);
		}

		[Trait(CategoryKey, ApiCategory)]
		[SkippableFact]
		public void When_OutPoint()
		{
			var animation = BuildDefaultAnimation();

			Assert.True(animation.OutPoint > 0);
		}

		[Trait(CategoryKey, ApiCategory)]
		[SkippableFact]
		public void When_Version()
		{
			var animation = BuildDefaultAnimation();

			Assert.True(animation.Version.Length > 0);
		}

		[Trait(CategoryKey, ApiCategory)]
		[SkippableFact]
		public void When_Size()
		{
			var animation = BuildDefaultAnimation();

			Assert.True(animation.Size.Height > 0);
		}
	}
}
