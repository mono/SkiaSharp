using SkiaSharp;
using Xunit;

namespace SkiaSharp.Vulkan.Tests
{
// Every other test in this assembly needs a live Vulkan context and skips on agents that lack
// one. This test exercises a Vulkan-specific SkiaSharp type (GRVkImageInfo) that is pure managed
// and needs no Vulkan runtime, so the suite always has at least one executed test
// (Microsoft.Testing.Platform fails a run that executes zero tests with exit code 8).
public class SmokeTest
{
[Fact]
public void GRVkImageInfoRoundTripsManagedState()
{
var info = new GRVkImageInfo {
Image = 0xDEADBEEF,
Format = 37,      // VK_FORMAT_R8G8B8A8_UNORM
ImageTiling = 0,  // VK_IMAGE_TILING_OPTIMAL
ImageLayout = 2,  // VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL
ImageUsageFlags = 0x10,
LevelCount = 1,
SampleCount = 1,
};

Assert.Equal(0xDEADBEEFul, info.Image);
Assert.Equal(37u, info.Format);
Assert.Equal(2u, info.ImageLayout);
Assert.Equal(1u, info.LevelCount);
}
}
}
