#!/usr/bin/env bash

msbuild /r /bl
(cd bin/publish && python3 server.py)
