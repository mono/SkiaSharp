using SkiaSharp;
using Xunit;

namespace SkiaSharp.Direct3D.Tests;

// Every other test in this assembly needs a live Direct3D context and skips on agents that lack one
// (and on non-Windows). This test exercises a Direct3D-specific SkiaSharp type
// (GRD3DTextureResourceInfo) that is pure managed and needs no Direct3D runtime, so the suite always
// has at least one executed test (Microsoft.Testing.Platform fails a run that executes zero tests
// with exit code 8).
public class SmokeTest
{
[Fact]
public void GRD3DTextureResourceInfoRoundTripsManagedState()
{
using var info = new GRD3DTextureResourceInfo {
Resource = 0x1234,
Format = 28,        // DXGI_FORMAT_R8G8B8A8_UNORM
ResourceState = 4,
LevelCount = 1,
SampleCount = 1,
Protected = true,
};

Assert.Equal((nint)0x1234, info.Resource);
Assert.Equal(28u, info.Format);
Assert.Equal(1u, info.LevelCount);
Assert.True(info.Protected);
}
}
