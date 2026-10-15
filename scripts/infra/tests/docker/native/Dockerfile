FROM mcr.microsoft.com/dotnet/sdk:10.0.401-azurelinux3.0

RUN tdnf install -y bash gcc glibc-devel binutils coreutils \
    && tdnf clean all

WORKDIR /work
