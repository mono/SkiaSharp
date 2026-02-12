# Docker Cross-Platform Testing

Use Docker to test on platforms different from your host machine.

## Quick Reference

```bash
# Check Docker availability
docker --version

# Adapt these examples to match the platform from the issue:
# Linux ARM64
docker run --platform linux/arm64 -it <dotnet-sdk-image> bash

# Linux x64  
docker run --platform linux/amd64 -it <dotnet-sdk-image> bash

# Other platforms - adjust --platform flag as needed
```

## Inside Container Setup

```bash
# Create test project
mkdir /app && cd /app
dotnet new console

# Add SkiaSharp - USE THE VERSION FROM THE ISSUE
dotnet add package SkiaSharp --version <version-from-issue>
dotnet add package SkiaSharp.NativeAssets.Linux --version <version-from-issue>

# Edit Program.cs with reproduction code from the issue
cat > Program.cs << 'EOF'
using SkiaSharp;

Console.WriteLine($"Arch: {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}");
Console.WriteLine($"OS: {System.Runtime.InteropServices.RuntimeInformation.OSDescription}");

// Add reproduction code from the issue here
using var bitmap = new SKBitmap(100, 100);
using var canvas = new SKCanvas(bitmap);
canvas.Clear(SKColors.White);
Console.WriteLine("Success!");
EOF

# Run test
dotnet run
```

## Inspecting Native Library

```bash
# Adapt paths based on your setup and the platform being tested:

# Check dependencies (DT_NEEDED entries)
readelf -d <path-to-libSkiaSharp.so> | grep NEEDED

# Check for undefined symbols
nm -u <path-to-libSkiaSharp.so> | head -20

# Check GLIBC version requirements
readelf -V <path-to-libSkiaSharp.so> | grep GLIBC
```

## When Docker is Unavailable

1. Document the limitation in PR description
2. Test on available platforms
3. Ask user to verify fix on their target platform
4. Add note: "Verified on [available platform], user verification needed for [target platform]"
