
using Xunit;

namespace HarfBuzzSharp.Tests
{
	public class HBScriptTest : HBTest
	{
		[SkippableFact]
		public void ShouldCreateScriptFromString()
		{
			var script = Script.Parse("Latn");

			Assert.Equal(Script.Latin, script);
		}

		[SkippableFact]
		public void EqualScriptsAreEqual()
		{
			Assert.Equal(Script.Latin, Script.Latin);
		}

		[SkippableFact]
		public void DifferentScriptsAreUnequal()
		{
			Assert.NotEqual(Script.Latin, Script.Arabic);
		}

		[SkippableFact]
		public void EnsureScriptSurvivesToStringRoundTrip()
		{
			var script = Script.Parse("Latn");
			var str = script.ToString();
			var newScript = Script.Parse(str);

			Assert.Equal("Latn", str);
			Assert.Equal(script, newScript);
		}

		[InlineData("")]
		[InlineData(null)]
		[SkippableTheory]
		public void InvalidStringIsInvalidScript(string script)
		{
			Assert.Equal(Script.Invalid, Script.Parse(script));
		}

		[InlineData("x")]
		[InlineData("x123")]
		[SkippableTheory]
		public void UnknownStringIsUnknownScript(string script)
		{
			Assert.Equal(Script.Unknown, Script.Parse(script));
		}

		[InlineData("wWyZ")]
		[SkippableTheory]
		public void WeirdStringIsNotUnknownScript(string script)
		{
			Assert.NotEqual(Script.Unknown, Script.Parse(script));
		}

		[InlineData("arab")]
		[InlineData("Arab")]
		[InlineData("ARAB")]
		[InlineData("Arabic")]
		[SkippableTheory]
		public void ScriptParsingIsCorrect(string script)
		{
			Assert.Equal(Script.Arabic, Script.Parse(script));
		}

		[InlineData(0x61426344, "aBcDeF")]
		[InlineData(0x61426344, "aBcDe")]
		[InlineData(0x61426344, "aBcD")]
		[InlineData(0x61426320, "aBc")]
		[InlineData(0x61422020, "aB")]
		[InlineData(0x61202020, "a")]
		[SkippableTheory]
		public void TagParsingIsCorrect(uint expectedTag, string tag)
		{
			Assert.Equal((Tag)expectedTag, Tag.Parse(tag));
		}

		[InlineData("")]
		[InlineData(null)]
		[SkippableTheory]
		public void TagParsingOfInvalidTagsIsNone(string tag)
		{
			Assert.Equal(Tag.None, Tag.Parse(tag));
		}
	}
}
