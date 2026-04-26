using Xunit;

namespace SkiaSharp.Tests
{
	public class SKVersionTest : SKTest
	{
		[SkippableFact]
		[Trait(Traits.Category.Key, Traits.Category.Values.Smoke)]
		public void NativeLibraryIsCompatible()
		{
			Assert.True(SkiaSharpVersion.CheckNativeLibraryCompatible());
		}

		[SkippableFact]
		[Trait(Traits.Category.Key, Traits.Category.Values.Smoke)]
		public void NativeVersionIsNotZero()
		{
			var native = SkiaSharpVersion.Native;
			Assert.NotNull(native);
			Assert.True(native > new System.Version(0, 0));
		}

		[SkippableFact]
		[Trait(Traits.Category.Key, Traits.Category.Values.Smoke)]
		public void NativeMinimumMatchesNativeVersion()
		{
			var native = SkiaSharpVersion.Native;
			var minimum = SkiaSharpVersion.NativeMinimum;

			// The native library major version should match what C# expects
			Assert.Equal(minimum.Major, native.Major);
		}
	}
}
