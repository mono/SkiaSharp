FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

RUN mkdir packages
COPY . .

ARG TARGETARCH
RUN if [ "$TARGETARCH" = "arm64" ]; then RID=linux-arm64; else RID=linux-x64; fi && \
    dotnet publish SkiaSharpSample/ -c Release -r $RID -o /app/out

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app

COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "SkiaSharpSample.dll"]
