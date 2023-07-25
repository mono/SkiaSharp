using System;

using Xunit;

namespace HarfBuzzSharp.Tests
{
	public class HBUnicodeFuncsTest : HBTest
	{
		[SkippableFact]
		public void ShouldBeImmutable()
		{
			using (var unicodeFuncs = UnicodeFunctions.Default)
			{
				Assert.True(unicodeFuncs.IsImmutable);
			}
		}

		[SkippableFact]
		public void ShouldGetScript()
		{
			using (var unicodeFuncs = UnicodeFunctions.Default)
			{
				var script = unicodeFuncs.GetScript('A');

				Assert.Equal(Script.Latin, script);
			}
		}

		[SkippableFact]
		public void ShouldGetCombiningClass()
		{
			using (var unicodeFuncs = UnicodeFunctions.Default)
			{
				var combiningClass = unicodeFuncs.GetCombiningClass('A');

				Assert.Equal(UnicodeCombiningClass.NotReordered, combiningClass);
			}
		}

		[SkippableFact]
		public void ShouldGetGeneralCategory()
		{
			using (var unicodeFuncs = UnicodeFunctions.Default)
			{
				var generalCategory = unicodeFuncs.GetGeneralCategory('A');

				Assert.Equal(UnicodeGeneralCategory.UppercaseLetter, generalCategory);
			}
		}

		[SkippableFact]
		public void EmptyUnicodeFunctionsAreExactlyTheSameInstance()
		{
			var emptyUnicodeFunctions1 = UnicodeFunctions.Empty;
			var emptyUnicodeFunctions2 = UnicodeFunctions.Empty;

			Assert.Equal(emptyUnicodeFunctions1, emptyUnicodeFunctions2);
			Assert.Equal(emptyUnicodeFunctions1.Handle, emptyUnicodeFunctions2.Handle);
			Assert.Same(emptyUnicodeFunctions1, emptyUnicodeFunctions2);
		}

		[SkippableFact]
		public void EmptyUnicodeFunctionsAreNotDisposed()
		{
			var emptyUnicodeFunctions = UnicodeFunctions.Empty;
			emptyUnicodeFunctions.Dispose();

			Assert.False(emptyUnicodeFunctions.IsDisposed());
			Assert.NotEqual(IntPtr.Zero, emptyUnicodeFunctions.Handle);
		}

		[SkippableFact]
		public void ShouldCreateUnicodeFunctionsFromParent()
		{
			using (var unicodeFunctions = new UnicodeFunctions(UnicodeFunctions.Default))
			{
				Assert.NotNull(unicodeFunctions.Parent);
			}
		}

		[SkippableFact]
		public void ShouldSetCombiningClassDelegate()
		{
			using (var unicodeFunctions = new UnicodeFunctions(UnicodeFunctions.Default))
			{
				unicodeFunctions.SetCombiningClassDelegate((f, u) => UnicodeCombiningClass.Above);

				Assert.Equal(UnicodeCombiningClass.Above, unicodeFunctions.GetCombiningClass(0));
			}
		}

		[SkippableFact]
		public void ShouldSetGeneralCategoryDelegate()
		{
			using (var unicodeFunctions = new UnicodeFunctions(UnicodeFunctions.Default))
			{
				unicodeFunctions.SetGeneralCategoryDelegate((f, u) => UnicodeGeneralCategory.Control);

				Assert.Equal(UnicodeGeneralCategory.Control, unicodeFunctions.GetGeneralCategory(0));
			}
		}

		[SkippableFact]
		public void ShouldSetMirroringDelegate()
		{
			using (var unicodeFunctions = new UnicodeFunctions(UnicodeFunctions.Default))
			{
				unicodeFunctions.SetMirroringDelegate((f, u) => 1337);

				Assert.Equal(1337, unicodeFunctions.GetMirroring(0));
			}
		}

		[SkippableFact]
		public void ShouldSetComposeDelegate()
		{
			using (var unicodeFunctions = new UnicodeFunctions(UnicodeFunctions.Default))
			{
				unicodeFunctions.SetComposeDelegate((UnicodeFunctions f, uint a, uint b, out uint ab) =>
				{
					ab = 1337;
					return true;
				});

				Assert.True(unicodeFunctions.TryCompose(1, 2, out var composed));

				Assert.Equal(1337, composed);
			}
		}

		[SkippableFact]
		public void ShouldSetDecomposeDelegate()
		{
			using (var unicodeFunctions = new UnicodeFunctions(UnicodeFunctions.Default))
			{
				unicodeFunctions.SetDecomposeDelegate((UnicodeFunctions f, uint ab, out uint a, out uint b) =>
				{
					a = 1337;
					b = 7331;
					return true;
				});

				Assert.True(unicodeFunctions.TryDecompose(0, out var first, out var second));

				Assert.Equal(1337, first);
				Assert.Equal(7331, second);
			}
		}
	}
}
