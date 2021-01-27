#!/usr/bin/env bash
set -e

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

ARCH="arm"
if [ "$1" ]; then
    ARCH="$1"
fi

(cd $DIR && docker build --tag skiasharp-$ARCH .)
(cd $DIR/../../../../ && docker run --rm --name skiasharp-$ARCH --volume $(pwd):/work skiasharp-$ARCH /bin/bash ./bootstrapper.sh -t externals-linux-clang-cross -c Release --buildarch=$ARCH)
