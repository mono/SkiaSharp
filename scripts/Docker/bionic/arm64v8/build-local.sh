#!/usr/bin/env bash
set -e

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

(cd $DIR && docker build  --tag skiasharp-bionic-arm64 .)
(cd $DIR/../../../../ && \
    docker run --rm --name skiasharp-bionic-arm64 --volume $(pwd):/work skiasharp-bionic-arm64 /bin/bash -c "\
        dotnet tool restore ; \
        dotnet cake --target=externals-linux --configuration=Release --buildarch=arm64 --variant=bionic")
