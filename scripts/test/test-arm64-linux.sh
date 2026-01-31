#!/bin/bash
# Test script to verify ARM64 Linux fix for issue #3369
# This should be run on an ARM64 Linux machine (Raspberry Pi 5, AWS Graviton, etc.)

set -e

echo "======================================"
echo "SkiaSharp ARM64 Linux Test"
echo "======================================"
echo ""

# Check architecture
ARCH=$(uname -m)
echo "Architecture: $ARCH"

if [ "$ARCH" != "aarch64" ] && [ "$ARCH" != "arm64" ]; then
    echo "WARNING: This test should be run on ARM64 (aarch64) architecture"
    echo "Current architecture is: $ARCH"
fi

# Check OS
OS=$(uname -s)
echo "Operating System: $OS"

if [ "$OS" != "Linux" ]; then
    echo "ERROR: This test should be run on Linux"
    exit 1
fi

# Check if libuuid is available
echo ""
echo "Checking for libuuid..."
if ldconfig -p | grep -q libuuid; then
    echo "✓ libuuid found"
else
    echo "✗ libuuid NOT found - installing..."
    sudo apt-get update && sudo apt-get install -y uuid-dev || \
    sudo yum install -y libuuid-devel || \
    sudo apk add util-linux-dev
fi

# Check .NET
echo ""
echo "Checking .NET SDK..."
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    echo "✓ .NET SDK version: $DOTNET_VERSION"
else
    echo "✗ .NET SDK not found"
    echo "Please install .NET 9 SDK: https://dotnet.microsoft.com/download"
    exit 1
fi

# Create test project
echo ""
echo "Creating test project..."
TEST_DIR="/tmp/skiasharp-arm64-test-$(date +%s)"
mkdir -p "$TEST_DIR"
cd "$TEST_DIR"

cat > Program.cs << 'ENDPROG'
using SkiaSharp;
using System;

Console.WriteLine("Starting SkiaSharp ARM64 Linux test...");
Console.WriteLine($"Runtime: {System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier}");
Console.WriteLine($"Architecture: {System.Runtime.InteropServices.RuntimeInformation.OSArchitecture}");
Console.WriteLine("");

bool allTestsPassed = true;

try
{
    Console.WriteLine("Test 1: Create bitmap");
    using var bitmap = new SKBitmap(100, 100);
    Console.WriteLine("  ✓ Bitmap created successfully");
    
    Console.WriteLine("Test 2: Create canvas");
    using var canvas = new SKCanvas(bitmap);
    Console.WriteLine("  ✓ Canvas created successfully");
    
    Console.WriteLine("Test 3: Clear canvas");
    canvas.Clear(SKColors.White);
    Console.WriteLine("  ✓ Canvas cleared successfully");
    
    Console.WriteLine("Test 4: Create paint");
    using var paint = new SKPaint
    {
        Color = SKColors.Red,
        IsAntialias = true,
        Style = SKPaintStyle.Fill
    };
    Console.WriteLine("  ✓ Paint created successfully");
    
    Console.WriteLine("Test 5: Draw circle");
    canvas.DrawCircle(50, 50, 40, paint);
    Console.WriteLine("  ✓ Circle drawn successfully");
    
    Console.WriteLine("Test 6: Create image from bitmap");
    using var image = SKImage.FromBitmap(bitmap);
    Console.WriteLine("  ✓ Image created successfully");
    
    Console.WriteLine("Test 7: Encode image (triggers PDF/UUID code path)");
    using var data = image.Encode(SKEncodedImageFormat.Png, 100);
    Console.WriteLine("  ✓ Image encoded successfully");
    
    if (data.Size > 0)
    {
        Console.WriteLine($"  ✓ Encoded data size: {data.Size} bytes");
    }
    else
    {
        Console.WriteLine("  ✗ ERROR: Encoded data is empty");
        allTestsPassed = false;
    }
}
catch (DllNotFoundException ex)
{
    Console.WriteLine($"✗ ERROR: Native library not found: {ex.Message}");
    Console.WriteLine("  This likely means SkiaSharp.NativeAssets.Linux is not installed");
    allTestsPassed = false;
}
catch (Exception ex)
{
    Console.WriteLine($"✗ ERROR: {ex.GetType().Name}: {ex.Message}");
    Console.WriteLine($"  Stack trace: {ex.StackTrace}");
    allTestsPassed = false;
}

Console.WriteLine("");
if (allTestsPassed)
{
    Console.WriteLine("========================================");
    Console.WriteLine("ALL TESTS PASSED! ✓");
    Console.WriteLine("========================================");
    Environment.Exit(0);
}
else
{
    Console.WriteLine("========================================");
    Console.WriteLine("TESTS FAILED! ✗");
    Console.WriteLine("========================================");
    Environment.Exit(1);
}
ENDPROG

cat > TestArm64.csproj << 'ENDPROJ'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="SkiaSharp" Version="3.119.0" />
    <PackageReference Include="SkiaSharp.NativeAssets.Linux" Version="3.119.0" />
  </ItemGroup>
</Project>
ENDPROJ

echo ""
echo "Restoring packages..."
dotnet restore

echo ""
echo "Building test..."
dotnet build

echo ""
echo "======================================"
echo "Running test..."
echo "======================================"
dotnet run --no-build

# Save exit code
TEST_RESULT=$?

echo ""
echo "Test directory: $TEST_DIR"

exit $TEST_RESULT
