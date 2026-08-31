namespace SkiaSharp.ReleaseChecklist.Tests;

public class ReleaseIdentityTests
{
	[Theory]
	[InlineData("4.152.0-preview.1", ReleaseChannel.Preview, false)]
	[InlineData("4.152.0-rc.2", ReleaseChannel.ReleaseCandidate, false)]
	[InlineData("4.152.0", ReleaseChannel.Stable, false)]
	[InlineData("4.151.0.1-preview.1", ReleaseChannel.Preview, true)]
	[InlineData("4.151.0.1", ReleaseChannel.Stable, true)]
	public void ParsesSupportedIdentities(string value, ReleaseChannel channel, bool hotfix)
	{
		var identity = ReleaseIdentity.Parse(value);

		Assert.Equal(value, identity.Raw);
		Assert.Equal(channel, identity.Channel);
		Assert.Equal(hotfix, identity.IsHotfix);
		Assert.Equal($"release/{value}", identity.ReleaseBranch);
	}

	[Theory]
	[InlineData("4.152")]
	[InlineData("4.152.0-preview.0")]
	[InlineData("4.152.0-beta.1")]
	[InlineData("4.152.0+metadata")]
	public void RejectsUnsupportedIdentities(string value) =>
		Assert.Throws<ReleasePolicyException>(() => ReleaseIdentity.Parse(value));
}
