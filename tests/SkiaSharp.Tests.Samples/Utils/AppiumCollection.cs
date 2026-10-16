using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

[CollectionDefinition(nameof(AppiumCollection))]
public sealed class AppiumCollection : ICollectionFixture<AppiumFixture>
{
}
