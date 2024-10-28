#!/usr/bin/env bash
set -e

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

(cd $DIR && docker build --tag skiasharp-alpine-x64 .)
(cd $DIR/../../../../ && \
    docker run --rm --name skiasharp-alpine-x64 --volume $(pwd):/work skiasharp-alpine-x64 /bin/bash -c "\
        dotnet tool restore ; \
        dotnet cake --target=externals-linux --configuration=Release --buildarch=x64 --variant=alpine")
