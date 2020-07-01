#!/usr/bin/env bash

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

msbuild /r /bl $DIR
(cd $DIR/bin/publish && python3 server.py)
