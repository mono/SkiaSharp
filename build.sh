#!/usr/bin/env bash
set -e

dotnet tool restore
dotnet run --file build.cs -- $@
