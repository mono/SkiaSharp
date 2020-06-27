#!/usr/bin/env bash

msbuild /r /bl utils/NativeLibraryMiniTest
(cd utils/NativeLibraryMiniTest/bin/publish && python3 server.py)
