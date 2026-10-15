FROM mcr.microsoft.com/dotnet/sdk:10.0.401-azurelinux3.0

# Separate from the minimal managed-runtime images: these probes compile C.
RUN tdnf install -y python3-pip gcc glibc-devel binutils \
    && python3 -m pip install --no-cache-dir pytest==8.4.2 \
    && tdnf clean all

WORKDIR /work
