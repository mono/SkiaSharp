#!/usr/bin/env bash

msbuild /r /bl utils/NativeLibraryMiniTest/wasm
(cd utils/NativeLibraryMiniTest/wasm/bin/publish && python3 server.py)
