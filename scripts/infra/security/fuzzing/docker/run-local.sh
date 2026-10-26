#!/usr/bin/env bash
set -ex

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

(cd "$DIR" && docker build --tag skiasharp-fuzzing .)

(cd "$DIR/../../../../.." && \
    docker run --rm --name skiasharp-fuzzing --volume "$(pwd)":/work skiasharp-fuzzing /bin/bash -c " \
        dotnet tool restore ; \
        dotnet cake --target=fuzzing $*")
