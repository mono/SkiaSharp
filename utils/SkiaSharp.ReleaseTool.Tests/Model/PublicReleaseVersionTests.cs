using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Model;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Model
{
	public sealed class PublicReleaseVersionTests
	{
		[Theory]
		[InlineData("4.151.1", "4.151.1", "stable", null, false)]
		[InlineData("4.151.1.2", "4.151.1.2", "stable", null, true)]
		[InlineData("4.152.0-preview.2.123", "4.152.0-preview.2", "preview.2", "123", false)]
		[InlineData("4.152.0-rc.1.26426.14", "4.152.0-rc.1", "rc.1", "26426.14", false)]
		[InlineData("4.152.0.1-rc.3.20260828.7", "4.152.0.1-rc.3", "rc.3", "20260828.7", true)]
		public void Parses_exact_public_versions(
			string value,
			string identity,
			string label,
			string? revision,
			bool hotfix)
		{
			var parsed = PublicReleaseVersion.Parse(value);

			Assert.Equal(value, parsed.Text);
			Assert.Equal(identity, parsed.Identity.Raw);
			Assert.Equal(label, parsed.Identity.Label);
			Assert.Equal(revision, parsed.BuildRevision);
			Assert.Equal(hotfix, parsed.Identity.IsHotfix);
		}

		[Theory]
		[InlineData("4.152")]
		[InlineData("4.152.0-preview.1")]
		[InlineData("4.152.0-beta.1.123")]
		[InlineData("4.152.0-rc.0.123")]
		[InlineData("4.152.0-rc.1.1234.1")]
		[InlineData("4.152.0.0")]
		[InlineData("04.152.0")]
		[InlineData("4.152.0+metadata")]
		public void Rejects_versions_outside_public_policy(string value) =>
			Assert.Throws<PlanException>(() => PublicReleaseVersion.Parse(value));
	}
}
