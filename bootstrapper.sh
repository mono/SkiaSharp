#!/usr/bin/env bash
set -e

dotnet tool restore
dotnet cake $@
