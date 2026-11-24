
using Xunit;

namespace HarfBuzzSharp.Tests
{
	public class HBScriptTest : HBTest
	{
		public static TheoryData<Script, string> UpdatedScripts => new TheoryData<Script, string> {
			{ Script.Elymaic, "Elym" },
			{ Script.Nandinagari, "Nand" },
			{ Script.NyiakengPuachueHmong, "Hmnp" },
			{ Script.Wancho, "Wcho" },
			{ Script.Chorasmian, "Chrs" },
			{ Script.DivesAkuru, "Diak" },
			{ Script.KhitanSmallScript, "Kits" },
			{ Script.Yezidi, "Yezi" },
			{ Script.CyproMinoan, "Cpmn" },
			{ Script.OldUyghur, "Ougr" },
			{ Script.Tangsa, "Tnsa" },
			{ Script.Toto, "Toto" },
			{ Script.Vithkuqi, "Vith" },
			{ Script.Math, "Zmth" },
			{ Script.Kawi, "Kawi" },
			{ Script.NagMundari, "Nagm" },
			{ Script.Garay, "Gara" },
			{ Script.GurungKhema, "Gukh" },
			{ Script.KiratRai, "Krai" },
			{ Script.OlOnal, "Onao" },
			{ Script.Sunuwar, "Sunu" },
			{ Script.Todhri, "Todr" },
			{ Script.TuluTigalari, "Tutg" },
			{ Script.BeriaErfe, "Berf" },
			{ Script.Sidetic, "Sidt" },
			{ Script.TaiYo, "Tayo" },
			{ Script.TolongSiki, "Tols" },
			{ Script.Jurchen, "Jurc" },
			{ Script.ProtoCuneiform, "Pcun" },
			{ Script.Seal, "Seal" },
		};

		[Theory]
		[MemberData(nameof(UpdatedScripts))]
		public void UpdatedScriptsMatchNativeTags(Script script, string tag)
		{
			Assert.Equal((uint)Tag.Parse(tag), (uint)script);
			Assert.Equal(tag, script.ToString());
			Assert.Equal(script, Script.Parse(tag));
			Assert.True(Script.TryParse(tag, out var parsed));
			Assert.Equal(script, parsed);
		}

		[Fact]
		public void ShouldCreateScriptFromString()
		{
			var script = Script.Parse("Latn");

			Assert.Equal(Script.Latin, script);
		}

		[Fact]
		public void EqualScriptsAreEqual()
		{
			Assert.Equal(Script.Latin, Script.Latin);
		}

		[Fact]
		public void DifferentScriptsAreUnequal()
		{
			Assert.NotEqual(Script.Latin, Script.Arabic);
		}

		[Fact]
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
		[Theory]
		public void InvalidStringIsInvalidScript(string script)
		{
			Assert.Equal(Script.Invalid, Script.Parse(script));
		}

		[InlineData("x")]
		[InlineData("x123")]
		[Theory]
		public void UnknownStringIsUnknownScript(string script)
		{
			Assert.Equal(Script.Unknown, Script.Parse(script));
		}

		[InlineData("wWyZ")]
		[Theory]
		public void WeirdStringIsNotUnknownScript(string script)
		{
			Assert.NotEqual(Script.Unknown, Script.Parse(script));
		}

		[InlineData("arab")]
		[InlineData("Arab")]
		[InlineData("ARAB")]
		[InlineData("Arabic")]
		[Theory]
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
		[Theory]
		public void TagParsingIsCorrect(uint expectedTag, string tag)
		{
			Assert.Equal((Tag)expectedTag, Tag.Parse(tag));
		}

		[InlineData("")]
		[InlineData(null)]
		[Theory]
		public void TagParsingOfInvalidTagsIsNone(string tag)
		{
			Assert.Equal(Tag.None, Tag.Parse(tag));
		}
	}
}
