using System;
using System.IO;
using SkiaSharp.SceneGraph;
using SkiaSharp.Skottie;
using Xunit;

namespace SkiaSharp.Tests
{
	public class AnimationTest : SKTest
	{
		public static TheoryData<string> DefaultLottieFiles =>
			new TheoryData<string>
			{
				"LottieLogo1.json",
				"LottieLogo1_bom.json",
			};

		[SkippableTheory]
		[InlineData("LottieLogo1.json", 0)]
		[InlineData("LottieLogo1_bom.json", 3)]
		public void EnsureLottieHasCorrectPreamble(string filename, int preamble)
		{
			var path = Path.Combine(PathToImages, filename);
			var data = SKData.Create(path);

			Assert.Equal(preamble, Utils.GetPreambleSize(data));
		}

		[Trait(Traits.FailingOn.Key, Traits.FailingOn.Values.iOS)]
		[Trait(Traits.FailingOn.Key, Traits.FailingOn.Values.MacCatalyst)]
		[SkippableTheory]
		[MemberData(nameof(DefaultLottieFiles))]
		public void When_Default_TryParse(string filename)
		{
			var path = Path.Combine(PathToImages, filename);
			var result = Animation.TryParse(File.ReadAllText(path), out var animation);

			Assert.True(result);
			Assert.NotNull(animation);
			Assert.NotEqual(IntPtr.Zero, animation.Handle);
		}

		[Trait(Traits.FailingOn.Key, Traits.FailingOn.Values.iOS)]
		[Trait(Traits.FailingOn.Key, Traits.FailingOn.Values.MacCatalyst)]
		[SkippableTheory]
		[MemberData(nameof(DefaultLottieFiles))]
		public void When_Default_Parse(string filename)
		{
			var path = Path.Combine(PathToImages, filename);
			var animation = Animation.Parse(File.ReadAllText(path));

			Assert.NotNull(animation);
			Assert.NotEqual(IntPtr.Zero, animation.Handle);
		}

		[SkippableTheory]
		[MemberData(nameof(DefaultLottieFiles))]
		public void When_Default_TryCreate_From_SKData(string filename)
		{
			var path = Path.Combine(PathToImages, filename);
			using var data = SKData.Create(path);
			var result = Animation.TryCreate(data, out var animation);

			Assert.True(result);
			Assert.NotNull(animation);
			Assert.NotEqual(IntPtr.Zero, animation.Handle);
		}

		[SkippableTheory]
		[MemberData(nameof(DefaultLottieFiles))]
		public void When_Default_Create_From_SKData(string filename)
		{
			var path = Path.Combine(PathToImages, filename);
			using var data = SKData.Create(path);
			var animation = Animation.Create(data);

			Assert.NotNull(animation);
			Assert.NotEqual(IntPtr.Zero, animation.Handle);
		}

		[SkippableTheory]
		[MemberData(nameof(DefaultLottieFiles))]
		public void When_Default_TryCreate_From_SKStream(string filename)
		{
			var path = Path.Combine(PathToImages, filename);
			using var fileStream = File.OpenRead(path);
			using var managedStream = new SKManagedStream(fileStream);
			var result = Animation.TryCreate(managedStream, out var animation);

			Assert.True(result);
			Assert.NotNull(animation);
			Assert.NotEqual(IntPtr.Zero, animation.Handle);
		}

		[SkippableTheory]
		[MemberData(nameof(DefaultLottieFiles))]
		public void When_Default_Create_From_SKStream(string filename)
		{
			var path = Path.Combine(PathToImages, filename);
			using var fileStream = File.OpenRead(path);
			using var managedStream = new SKManagedStream(fileStream);
			var animation = Animation.Create(managedStream);

			Assert.NotNull(animation);
			Assert.NotEqual(IntPtr.Zero, animation.Handle);
		}

		[SkippableTheory]
		[MemberData(nameof(DefaultLottieFiles))]
		public void When_Default_TryCreate_From_Stream(string filename)
		{
			var path = Path.Combine(PathToImages, filename);
			using var fileStream = File.OpenRead(path);
			var result = Animation.TryCreate(fileStream, out var animation);

			Assert.True(result);
			Assert.NotEqual(IntPtr.Zero, animation?.Handle);
		}

		[SkippableTheory]
		[MemberData(nameof(DefaultLottieFiles))]
		public void When_Default_Create_From_Stream(string filename)
		{
			var path = Path.Combine(PathToImages, filename);
			using var fileStream = File.OpenRead(path);
			var animation = Animation.Create(fileStream);

			Assert.NotNull(animation);
			Assert.NotEqual(IntPtr.Zero, animation.Handle);
		}

		[SkippableTheory]
		[MemberData(nameof(DefaultLottieFiles))]
		public void When_Default_TryCreate_From_NonSeekableStream(string filename)
		{
			var path = Path.Combine(PathToImages, filename);
			using var fileStream = File.OpenRead(path);
			using var nonseekable = new NonSeekableReadOnlyStream(fileStream);
			var result = Animation.TryCreate(nonseekable, out var animation);

			Assert.True(result);
			Assert.NotEqual(IntPtr.Zero, animation?.Handle);
		}

		[SkippableTheory]
		[MemberData(nameof(DefaultLottieFiles))]
		public void When_Default_Create_From_NonSeekableStream(string filename)
		{
			var path = Path.Combine(PathToImages, filename);
			using var fileStream = File.OpenRead(path);
			using var nonseekable = new NonSeekableReadOnlyStream(fileStream);
			var animation = Animation.Create(nonseekable);

			Assert.NotNull(animation);
			Assert.NotEqual(IntPtr.Zero, animation.Handle);
		}

		[SkippableTheory]
		[MemberData(nameof(DefaultLottieFiles))]
		public void When_Default_TryCreate_From_File(string filename)
		{
			var path = Path.Combine(PathToImages, filename);
			var result = Animation.TryCreate(path, out var animation);

			Assert.True(result);
			Assert.NotEqual(IntPtr.Zero, animation?.Handle);
		}

		[SkippableTheory]
		[MemberData(nameof(DefaultLottieFiles))]
		public void When_Default_Create_From_File(string filename)
		{
			var path = Path.Combine(PathToImages, filename);
			var animation = Animation.Create(path);

			Assert.NotNull(animation);
			Assert.NotEqual(IntPtr.Zero, animation.Handle);
		}

		private Animation BuildDefaultAnimation() =>
			BuildAnimation("LottieLogo1.json");

		private Animation BuildAnimation(string filename)
		{
			var path = Path.Combine(PathToImages, filename);
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

			Assert.Equal(TimeSpan.FromSeconds(5.9666666), animation.Duration);
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

		[SkippableFact]
		public void Can_Render()
		{
			var animation = BuildDefaultAnimation();

			using var bmp = new SKBitmap((int)animation.Size.Width, (int)animation.Size.Height);
			bmp.Erase(SKColors.Red);
			var beforePixels = bmp.Pixels;

			using var canvas = new SKCanvas(bmp);
			animation.Seek(0.1);
			animation.Render(canvas, bmp.Info.Rect);
			var afterPixels = bmp.Pixels;

			Assert.NotEqual(beforePixels, afterPixels);
		}

		[SkippableFact]
		public void Can_Not_Render_With_Base64()
		{
			var animation = BuildAnimation("lottie-base64_dotnet-bot.json");

			using var bmp = new SKBitmap((int)animation.Size.Width, (int)animation.Size.Height);
			bmp.Erase(SKColors.Red);
			var beforePixels = bmp.Pixels;

			using var canvas = new SKCanvas(bmp);
			animation.Seek(0.1);
			animation.Render(canvas, bmp.Info.Rect);
			var afterPixels = bmp.Pixels;

			Assert.Equal(beforePixels, afterPixels);
		}
	}
}
