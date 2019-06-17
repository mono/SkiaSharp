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
	}
}
