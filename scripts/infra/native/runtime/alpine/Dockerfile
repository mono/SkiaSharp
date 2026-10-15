FROM mcr.microsoft.com/dotnet/sdk:10.0.401-alpine3.23

RUN apk add --no-cache python3 py3-pip gcc musl-dev binutils \
    && python3 -m venv /opt/native-tests \
    && /opt/native-tests/bin/python -m pip install --no-cache-dir pytest==8.4.2

ENV PATH="/opt/native-tests/bin:${PATH}"
WORKDIR /work
