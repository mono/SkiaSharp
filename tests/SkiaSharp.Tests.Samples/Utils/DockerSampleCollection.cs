using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

[CollectionDefinition(nameof(DockerSampleCollection))]
public sealed class DockerSampleCollection : ICollectionFixture<DockerSampleFixture>
{
}
