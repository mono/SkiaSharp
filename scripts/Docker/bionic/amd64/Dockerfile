# Arguments:
#   DOTNET_SDK_VERSION   - the version of dotnet for the Cake script        [ 8.0 | * ]
FROM amd64/debian:bookworm

# Install the required packages
RUN apt-get update && apt-get install -y \
    build-essential \
    git \
    wget \
    unzip \
    cmake \
    ninja-build \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Configure Android NDK r26b
RUN wget https://dl.google.com/android/repository/android-ndk-r26b-linux.zip && \
    unzip android-ndk-r26b-linux.zip -d /opt && \
    rm android-ndk-r26b-linux.zip
ENV ANDROID_NDK_HOME /opt/android-ndk-r26b
ENV PATH $PATH:$ANDROID_NDK_HOME/toolchains/llvm/prebuilt/linux-x86_64/bin

# Copy source
COPY . .

# Install the .NET SDK
ARG DOTNET_SDK_VERSION=8.0
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
RUN wget https://builds.dotnet.microsoft.com/dotnet/scripts/v1/dotnet-install.sh -O dotnet-install.sh \
    && bash dotnet-install.sh --channel ${DOTNET_SDK_VERSION} --install-dir /usr/share/dotnet --verbose \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm dotnet-install.sh \
    && dotnet help \
    && dotnet --info

WORKDIR /work

# Cmake
RUN cmake -DCMAKE_TOOLCHAIN_FILE=$ANDROID_NDK_HOME/build/cmake/android.toolchain.cmake \
          -DANDROID_ABI=x86_64 \             # target architecture
          -DANDROID_NATIVE_API_LEVEL=29 \    # Android 10.0 (API 29)
          -DANDROID_TOOLCHAIN=clang \        # use clang
          -DCMAKE_BUILD_TYPE=Release \
          -B build \
          .
RUN cmake --build build --parallel

# Clean build cache
RUN rm -rf build
