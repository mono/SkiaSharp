#!/usr/bin/env bash
set -e

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

(cd $DIR && docker build --tag skiasharp-alpine .)
(cd $DIR/../../../../ && \
    docker run --rm --name skiasharp-alpine --volume $(pwd):/work skiasharp-alpine /bin/bash -c "\
        dotnet tool restore ; \
        dotnet cake --target=externals-linux --configuration=Release --buildarch=x64 --variant=alpine")
