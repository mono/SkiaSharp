using System.Xml.Linq;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Trait("Category", "Infrastructure")]
[RunGroup(SampleRunGroup.Build)]
public class SampleSceneLifecycleTests
{
    [Theory]
    [InlineData("Basic/iOS/SkiaSharpSample", false)]
    [InlineData("Basic/tvOS/SkiaSharpSample", false)]
    [InlineData("Basic/MacCatalyst/SkiaSharpSample", false)]
    [InlineData("Basic/Maui/SkiaSharpSample/Platforms/iOS", true)]
    [InlineData("Basic/Maui/SkiaSharpSample/Platforms/MacCatalyst", true)]
    [InlineData("Gallery/Maui/Platforms/iOS", true)]
    [InlineData("Gallery/Maui/Platforms/MacCatalyst", true)]
    public void UIKitHeadsDeclareASingleSceneWithTheirRegisteredDelegate(string relativeDirectory, bool isMaui)
    {
        var directory = Path.Combine(Repo.RootDir, "samples", relativeDirectory);
        var plist = XDocument.Load(Path.Combine(directory, "Info.plist"));
        var root = Assert.Single(plist.Elements("plist").Elements("dict"));
        var manifest = Value(root, "UIApplicationSceneManifest");
        Assert.Equal("false", Value(manifest, "UIApplicationSupportsMultipleScenes").Name.LocalName);
        var configurations = Value(manifest, "UISceneConfigurations");
        Assert.Equal("UIWindowSceneSessionRoleApplication", Assert.Single(configurations.Elements("key")).Value);
        var applicationRole = Value(configurations, "UIWindowSceneSessionRoleApplication");
        Assert.Equal("array", applicationRole.Name.LocalName);
        var configuration = Assert.Single(applicationRole.Elements());
        Assert.Equal(isMaui ? "__MAUI_DEFAULT_SCENE_CONFIGURATION__" : "Default Configuration",
            Value(configuration, "UISceneConfigurationName").Value);
        var delegateName = Value(configuration, "UISceneDelegateClassName").Value;
        Assert.Equal("SceneDelegate", delegateName);
        var source = File.ReadAllText(Path.Combine(directory, "SceneDelegate.cs"));
        Assert.Contains($"[Register(\"{delegateName}\")]", source);
        Assert.Contains(isMaui
            ? "class SceneDelegate : MauiUISceneDelegate"
            : "class SceneDelegate : UIResponder, IUIWindowSceneDelegate", source);

        if (isMaui)
        {
            Assert.DoesNotContain(plist.Descendants("key"), key => key.Value == "UISceneStoryboardFile");
            Assert.DoesNotContain(root.Elements("key"), key => key.Value == "UIMainStoryboardFile");
        }
        else
        {
            Assert.Equal("Main", Value(configuration, "UISceneStoryboardFile").Value);
            Assert.True(File.Exists(Path.Combine(directory, "Main.storyboard")));
            Assert.Contains("[Export(\"window\")]", source);
            Assert.Contains("[Export(\"scene:willConnectToSession:options:\")]", source);
            Assert.Contains("Window?.RootViewController", source);
            Assert.Contains("AppDelegate.DefaultPage", source);
            Assert.DoesNotContain("Window", File.ReadAllText(Path.Combine(directory, "AppDelegate.cs")));
        }
    }

    [Theory]
    [InlineData("iOS")]
    [InlineData("tvOS")]
    public void MigratedNativeHeadsDoNotUseTheApplicationStoryboard(string platform)
    {
        var plist = XDocument.Load(Path.Combine(Repo.RootDir, "samples", "Basic", platform, "SkiaSharpSample", "Info.plist"));
        var root = Assert.Single(plist.Elements("plist").Elements("dict"));
        Assert.DoesNotContain(root.Elements("key"), key => key.Value == "UIMainStoryboardFile");
    }

    [Fact]
    public void NativeIOSPreservesItsLaunchStoryboard()
    {
        var directory = Path.Combine(Repo.RootDir, "samples", "Basic", "iOS", "SkiaSharpSample");
        var plist = XDocument.Load(Path.Combine(directory, "Info.plist"));
        var root = Assert.Single(plist.Elements("plist").Elements("dict"));
        Assert.Equal("LaunchScreen", Value(root, "UILaunchStoryboardName").Value);
        Assert.True(File.Exists(Path.Combine(directory, "LaunchScreen.storyboard")));
    }

    private static XElement Value(XElement dictionary, string name)
    {
        Assert.Equal("dict", dictionary.Name.LocalName);
        var key = Assert.Single(dictionary.Elements("key"), key => key.Value == name);
        return Assert.IsType<XElement>(key.NextNode);
    }
}
