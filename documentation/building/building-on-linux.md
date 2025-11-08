# Building SkiaSharp on Linux

This guide provides detailed instructions for building `libSkiaSharp.so` on Linux, including custom builds for specific distributions.

## Overview

Building SkiaSharp on Linux is straightforward and doesn't strictly require the mono/SkiaSharp repository. However, cloning the repository is recommended as it includes additional tools and scripts that simplify the build process.

## Prerequisites

### Required Tools

```bash
# Ubuntu/Debian
sudo apt-get update
sudo apt-get install -y \
    git \
    python3 \
    clang \
    ninja-build \
    libfontconfig1-dev \
    libglu1-mesa-dev

# Fedora/RHEL/CentOS
sudo dnf install -y \
    git \
    python3 \
    clang \
    ninja-build \
    fontconfig-devel \
    mesa-libGLU-devel

# Alpine
apk add --no-cache \
    git \
    python3 \
    clang \
    ninja \
    fontconfig-dev \
    mesa-dev
```

## Quick Start with SkiaSharp Repository

### 1. Clone the Repository

```bash
git clone https://github.com/mono/SkiaSharp.git
cd SkiaSharp
```

### 2. Build Native Libraries

```bash
# Install .NET tool
dotnet tool install -g cake.tool

# Build Linux native libraries
dotnet cake --target=externals-linux --buildarch=x64
```

The output will be in `output/native/linux/x64/libSkiaSharp.so`.

## Building from Skia Repository Directly

If you prefer to work directly with the Skia repository without SkiaSharp's build system:

### 1. Clone Skia and Depot Tools

```bash
# Clone mono's fork of Skia
git clone https://github.com/mono/skia.git -b <branch-or-tag>
cd skia

# Clone depot_tools (Google's build tools)
cd ..
git clone https://chromium.googlesource.com/chromium/tools/depot_tools.git
```

**Branch Notes:**
- For current SkiaSharp version, check which Skia branch is being used
- `xamarin-mobile-bindings` is typically the Xamarin "master" branch
- `master` is reserved for tracking Google's master

### 2. Sync Dependencies

```bash
cd skia
python3 tools/git-sync-deps
```

### 3. Generate Build Files

Use `gn` to generate build files with the necessary arguments:

```bash
./bin/gn gen 'out/linux/x64' --args='
    is_official_build=true 
    skia_enable_tools=false
    target_os="linux" 
    target_cpu="x64"
    skia_use_icu=false 
    skia_use_sfntly=false 
    skia_use_piex=true
    skia_use_system_expat=false 
    skia_use_system_freetype2=false 
    skia_use_system_libjpeg_turbo=false 
    skia_use_system_libpng=false 
    skia_use_system_libwebp=false 
    skia_use_system_zlib=false
    skia_enable_gpu=true
    extra_cflags=[ "-DSKIA_C_DLL" ]
    linux_soname_version="<version>"'
```

> **Note:** The latest build arguments can be found in [`native/linux/build.cake`](../../native/linux/build.cake) in the SkiaSharp repository.

### 4. Build the Library

```bash
../depot_tools/ninja 'SkiaSharp' -C 'out/linux/x64'
```

The output will be `out/linux/x64/libSkiaSharp.so.<version>`, which can be renamed to `libSkiaSharp.so`.

## Customization Options

You can customize the build by modifying the `--args` value passed to `gn`:

### Disable GPU Support

```bash
--args='... skia_enable_gpu=false ...'
```

### Build Static Library (v2.80+)

```bash
--args='... is_static_skiasharp=true ...'
```

### Customize Compilers

```bash
--args='... cc="gcc" cxx="g++" ar="ar" ...'
```

### Add Compiler Flags

**Both C & C++:**
```bash
--args='... extra_cflags=[ "-O3", "-fPIC" ] ...'
```

**C only:**
```bash
--args='... extra_cflags_c=[ "-std=c11" ] ...'
```

**C++ only:**
```bash
--args='... extra_cflags_cc=[ "-std=c++17" ] ...'
```

### Add Linker Flags

```bash
--args='... extra_ldflags=[ "-static-libstdc++" ] ...'
```

### Add Assembler Flags

```bash
--args='... extra_asmflags=[ "-msse4.2" ] ...'
```

## Building for Different Architectures

### ARM (32-bit)

```bash
dotnet cake --target=externals-linux --buildarch=arm
```

Or with direct Skia build:

```bash
./bin/gn gen 'out/linux/arm' --args='... target_cpu="arm" ...'
../depot_tools/ninja 'SkiaSharp' -C 'out/linux/arm'
```

### ARM64

```bash
dotnet cake --target=externals-linux --buildarch=arm64
```

Or:

```bash
./bin/gn gen 'out/linux/arm64' --args='... target_cpu="arm64" ...'
../depot_tools/ninja 'SkiaSharp' -C 'out/linux/arm64'
```

## Docker-Based Building

Building for arbitrary Linux distributions is easier with Docker containers. This approach allows you to build for any distro without configuring your host system.

### Prerequisites

- Docker installed on your development machine
- Can use Linux or WSL 2 with Docker

### Building with Docker

1. **Create a Dockerfile** for your target distribution (or use existing ones from `scripts/Docker/`)

2. **Build the Docker image:**

```bash
cd scripts/Docker/<distro>/<arch>
docker build --tag skiasharp-<distro> .
```

3. **Run the build in the container:**

```bash
cd /path/to/SkiaSharp

docker run --rm \
    --name skiasharp-<distro> \
    --volume $(pwd):/work \
    skiasharp-<distro> \
    /bin/bash ./bootstrapper.sh \
        -t externals-linux \
        --variant=<distro-name> \
        --buildarch=<arch>
```

### Example: Alpine Linux

```bash
# Build the Docker image
cd scripts/Docker/alpine/amd64
docker build --tag skiasharp-alpine .

# Return to repository root
cd ../../../..

# Build SkiaSharp in the container
docker run --rm \
    --name skiasharp-alpine \
    --volume $(pwd):/work \
    skiasharp-alpine \
    /bin/bash ./bootstrapper.sh \
        -t externals-linux \
        --variant=alpine \
        --buildarch=x64 \
        --gn=gn \
        --ninja=ninja
```

**Parameters:**
- `--variant`: Sets the output folder name in `./output/native/`
- `--buildarch`: Target architecture (x64, arm, arm64)
- `--gn`: Path to gn tool (use system package if needed)
- `--ninja`: Path to ninja tool (use system package if needed)

### File Ownership Fix

After building in Docker, files may have incorrect ownership:

```bash
# Fix ownership to match current user
sudo chown -R $(id -u):$(id -g) .
```

Alternatively, [configure your container](https://vsupalov.com/docker-shared-permissions/) to use your user.

## Troubleshooting

### Depot Tools Not Working

Some platforms require using system-installed `gn` and `ninja` instead of depot_tools:

```bash
# Install system packages
sudo apt-get install gn ninja-build

# Use system tools
--gn=gn --ninja=ninja
```

### Missing Dependencies

If you get linker errors, ensure you have development packages installed:

```bash
sudo apt-get install -y \
    libfontconfig1-dev \
    libfreetype6-dev \
    libglu1-mesa-dev \
    libgl1-mesa-dev
```

### Build Hangs or Fails

- Ensure you have enough RAM (4GB minimum, 8GB recommended)
- Ensure sufficient disk space (10GB free minimum)
- Check that Python 3 is available
- Verify clang/gcc version is modern enough (clang 10+, gcc 9+)

## Using Custom Builds

Once built, you can use your custom `libSkiaSharp.so`:

1. **Copy to your application:**
   ```bash
   cp output/native/linux/x64/libSkiaSharp.so /path/to/your/app/
   ```

2. **Update your .csproj** to include the native library:
   ```xml
   <ItemGroup>
     <None Include="libSkiaSharp.so" CopyToOutputDirectory="PreserveNewest" />
   </ItemGroup>
   ```

3. **Or create a custom NuGet package** (see [Creating New Libraries](creating-new-libraries.md))

## Related Documentation

- [Building SkiaSharp](building-skiasharp.md) - General build guide
- [Linux Native Assets](../user-guide/linux-native-assets.md) - Using pre-built packages
- [Creating New Libraries](creating-new-libraries.md) - Packaging custom builds
- [Contributing Guide](../contributing/README.md)
