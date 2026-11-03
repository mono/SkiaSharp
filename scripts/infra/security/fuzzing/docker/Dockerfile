# Stage 1: Build libfuzzer-dotnet from source so the final fuzzing container stays free of the C/C++ compiler toolchain.
FROM mcr.microsoft.com/dotnet/sdk:10.0.401-azurelinux3.0 AS builder

RUN tdnf install -y clang gcc compiler-rt glibc-devel kernel-headers binutils \
    && tdnf clean all

ARG LIBFUZZER_DOTNET_URL=https://raw.githubusercontent.com/Metalnem/libfuzzer-dotnet/v2025.05.02.0904/libfuzzer-dotnet.cc
ARG LIBFUZZER_DOTNET_SHA512=401e5f1bade73c5d0ea046b75eb6f42c943da388a2eeb9267f3af9e7c3b35a60ab44350bff561ed466d14034b879cafb017bc4e3245b8450795513c3f7019ae1

RUN curl -fsSL -o /tmp/libfuzzer-dotnet.cc "$LIBFUZZER_DOTNET_URL" \
    && echo "$LIBFUZZER_DOTNET_SHA512  /tmp/libfuzzer-dotnet.cc" | sha512sum -c - \
    && clang -fsanitize=fuzzer -ftrap-function=abort /tmp/libfuzzer-dotnet.cc -o /usr/local/bin/libfuzzer-dotnet \
    && rm /tmp/libfuzzer-dotnet.cc

# Stage 2: Runtime environment for running `dotnet cake --target=fuzzing`.
FROM mcr.microsoft.com/dotnet/sdk:10.0.401-azurelinux3.0

ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    FUZZER_EXE=/usr/local/bin/libfuzzer-dotnet

RUN tdnf install -y fontconfig \
    && tdnf clean all

COPY --from=builder /usr/local/bin/libfuzzer-dotnet /usr/local/bin/libfuzzer-dotnet

WORKDIR /work
