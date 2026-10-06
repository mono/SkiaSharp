using Xunit;

namespace SkiaSharp.Tests.Samples.PlatformTests;

[Trait("Category", "Infrastructure")]
public class ManualPlatformPolicyTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("false", false)]
    [InlineData("0", false)]
    [InlineData("true", true)]
    [InlineData("1", true)]
    public void CIFlagNeedsAnEnabledValue(string? value, bool expected) =>
        Assert.Equal(expected, ManualPlatformPolicy.IsEnabled(value));

    [Fact]
    public void NumericRuntimeOptionsSelectTheExplicitPortAndApiLevel()
    {
        var port = AppContext.GetData("SampleTest.AppiumPort");
        var api = AppContext.GetData("AndroidApiLevel");
        try
        {
            AppContext.SetData("SampleTest.AppiumPort", 4823);
            AppContext.SetData("AndroidApiLevel", 36);
            Assert.Equal(4823, AppiumFixture.Port);
            Assert.Equal("36", MauiAndroidTests.ExpectedApiLevel);
        }
        finally
        {
            AppContext.SetData("SampleTest.AppiumPort", port);
            AppContext.SetData("AndroidApiLevel", api);
        }
    }

    [Fact]
    public void EveryGuiRunnerIsManualPlatform()
    {
        foreach (var type in new[]
        {
            typeof(BlazorTests), typeof(MauiAndroidTests), typeof(MauiiOSTests),
            typeof(MauiMacCatalystTests), typeof(MauiWindowsTests)
        })
            Assert.Contains(type.CustomAttributes, attribute =>
                attribute.AttributeType == typeof(TraitAttribute) &&
                attribute.ConstructorArguments[0].Value as string == "Category" &&
                attribute.ConstructorArguments[1].Value as string == "ManualPlatform");
    }
}
