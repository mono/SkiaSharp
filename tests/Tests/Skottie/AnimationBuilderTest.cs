using System;
using System.IO;
using SkiaSharp.Resources;
using SkiaSharp.SceneGraph;
using SkiaSharp.Skottie;
using Xunit;

namespace SkiaSharp.Tests
{
	public class AnimationBuilderTest : SKTest
	{
		public static TheoryData<string> DefaultLottieFiles =>
			AnimationTest.DefaultLottieFiles;

		public static TheoryData<string> Base64Files =>
			new TheoryData<string>
			{
				"lottie-base64_dotnet-bot.json",
				"lottie-base64_women-thinking.json",
			};

		[SkippableTheory]
		[MemberData(nameof(DefaultLottieFiles))]
		public void DefaultBuilderIsTheSameAsDefaultCreate(string filename)
		{
			var path = Path.Combine(PathToImages, filename);
			using var data = SKData.Create(path);
			var directAnimation = Animation.Create(data);

			var builderAnimation = Animation.CreateBuilder().Build(data);
			Assert.NotNull(builderAnimation);
			Assert.NotEqual(IntPtr.Zero, builderAnimation.Handle);
		
			Assert.Equal(directAnimation.Duration, builderAnimation.Duration);
			Assert.Equal(directAnimation.Fps, builderAnimation.Fps);
			Assert.Equal(directAnimation.InPoint, builderAnimation.InPoint);
			Assert.Equal(directAnimation.OutPoint, builderAnimation.OutPoint);
			Assert.Equal(directAnimation.Version, builderAnimation.Version);
			Assert.Equal(directAnimation.Size, builderAnimation.Size);
		}

		[SkippableTheory]
		[MemberData(nameof(DefaultLottieFiles))]
		public void DefaultBuilderHasStats(string filename)
		{
			var path = Path.Combine(PathToImages, filename);
			using var data = SKData.Create(path);

			var builder = Animation.CreateBuilder();
			var animation = builder.Build(data);
			Assert.NotNull(animation);
		
			var stats = builder.Stats;
			Assert.True(stats.SceneParseTime >= TimeSpan.Zero);
			Assert.True(stats.JsonParseTime >= TimeSpan.Zero);
			Assert.True(stats.TotalLoadTime >= TimeSpan.Zero);
			Assert.True(stats.JsonSize > 0);
			Assert.True(stats.AnimatorCount > 0);
		}

		[Trait(Traits.FailingOn.Key, Traits.FailingOn.Values.iOS)]
		[Trait(Traits.FailingOn.Key, Traits.FailingOn.Values.MacCatalyst)]
		[Trait(Traits.FailingOn.Key, Traits.FailingOn.Values.macOS)]
		[SkippableTheory]
		[MemberData(nameof(Base64Files))]
		public void CanLoadBase64ImagesFromData(string filename)
		{
			var path = Path.Combine(PathToImages, filename);
			using var data = SKData.Create(path);

			var animation = Animation
				.CreateBuilder()
				.SetResourceProvider(new DataUriResourceProvider())
				.Build(data);

			Assert.NotNull(animation);
			Assert.True(animation.Duration > TimeSpan.Zero);
		}

		[Trait(Traits.FailingOn.Key, Traits.FailingOn.Values.iOS)]
		[Trait(Traits.FailingOn.Key, Traits.FailingOn.Values.MacCatalyst)]
		[Trait(Traits.FailingOn.Key, Traits.FailingOn.Values.macOS)]
		[SkippableTheory]
		[MemberData(nameof(Base64Files))]
		public void CanLoadBase64ImagesFromFilename(string filename)
		{
			var path = Path.Combine(PathToImages, filename);

			var animation = Animation
				.CreateBuilder()
				.SetResourceProvider(new DataUriResourceProvider())
				.Build(path);

			Assert.NotNull(animation);
			Assert.True(animation.Duration > TimeSpan.Zero);
		}

		[Trait(Traits.FailingOn.Key, Traits.FailingOn.Values.iOS)]
		[Trait(Traits.FailingOn.Key, Traits.FailingOn.Values.MacCatalyst)]
		[Trait(Traits.FailingOn.Key, Traits.FailingOn.Values.macOS)]
		[SkippableTheory]
		[MemberData(nameof(Base64Files))]
		public void CanRenderWithBase64(string filename)
		{
			var animation = Animation
				.CreateBuilder()
				.SetResourceProvider(new DataUriResourceProvider())
				.Build(Path.Combine(PathToImages, filename));

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
		public void WrappedResourceManagersAreNotCollectedPrematurely()
		{
			var (builder, weak) = CreateBuilder();

			CollectGarbage();

			Assert.True(weak.IsAlive);

			using var animation = builder.Build(Path.Combine(PathToImages, "lottie-base64_dotnet-bot.json"));

			builder.Dispose();
			CollectGarbage();

			Assert.False(weak.IsAlive);

			static (AnimationBuilder, WeakReference) CreateBuilder()
			{
				var provider = new DataUriResourceProvider();

				var builder = Animation
					.CreateBuilder()
					.SetResourceProvider(provider);

				return (builder, new WeakReference(provider));
			}
		}
	}
}
