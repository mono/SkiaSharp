FROM mcr.microsoft.com/dotnet/sdk:10.0.401-azurelinux3.0

RUN tdnf install -y llvm gcc glibc-devel binutils \
    && tdnf clean all \
    && pwsh -NoLogo -NoProfile -Command '$PSVersionTable.PSVersion.ToString()'

WORKDIR /work
