Building on Linux is simple, and just has a few requirements:

 1. Install .NET Core  
    _https://www.microsoft.com/net/core_  
 1. Install Mono (`mono-complete` and `referenceassemblies-pcl`)  
    _http://www.mono-project.com/download/#download-lin_
 1. Install MSBuild (`msbuild`)  
 1. Install Native Dependencies (`libfontconfig1-dev` and `libglu1-mesa-dev`)

Because SkiaSharp currently only builds with g++ 4.8, you may have to install that version:

 1. Install `g++-4.8-multilib`

Once all the dependencies are installed, start the SkiaSharp download and build:

 1. Clone SkiaSharp
 1. Initialize submodules
 1. Build

An example script that can be run on Ubuntu 16.10:

```bash
# Install .NET Core
curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.gpg
sudo mv microsoft.gpg /etc/apt/trusted.gpg.d/microsoft.gpg
sudo sh -c 'echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-ubuntu-xenial-prod xenial main" > /etc/apt/sources.list.d/dotnetdev.list'
sudo apt-get update
sudo apt-get install dotnet-sdk-2.1.4

# Install Mono
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
echo "deb http://download.mono-project.com/repo/debian wheezy main" | sudo tee /etc/apt/sources.list.d/mono-xamarin.list
sudo apt-get update
sudo apt-get install mono-complete referenceassemblies-pcl msbuild

# Install Native Dependencies
sudo apt-get install libfontconfig1-dev libglu1-mesa-dev

# Install g++ 4.8
sudo apt install g++-4.8-multilib

# Install Emoji Fonts
sudo apt install ttf-ancient-fonts

# Clone SkiaSharp
mkdir -p ~/Projects
cd ~/Projects
git clone https://github.com/mono/SkiaSharp.git
cd SkiaSharp

# Initialize submodules
git submodule update --init --recursive

# Build
CC=gcc-4.8 CXX=g++-4.8 AR=gcc-ar-4.8 NM=gcc-nm-4.8 ./bootstrapper.sh -t everything
```