@echo off

REM Check if all three arguments are provided
if "%~2"=="" (
    echo Usage: %0 ^<visual studio root^> ^<vcvars args^> ^<script^>
    exit /b 1
)

set __VSCMD_ARG_NO_LOGO=1
set VSCMD_START_DIR=%CD%

REM Run vcvarsall.bat script
call "%~1\VC\Auxiliary\Build\vcvarsall.bat" %~2
shift
shift

REM drop the path to the VS install
set "args="
:getRemainingArgs
if "%~1" neq "" (
  set ^"args=%args% %1"
  shift /1
  goto :getRemainingArgs
)

REM Check if vcvarsall.bat ran successfully
if %errorlevel% neq 0 (
    echo Error: Failed to run vcvarsall.bat
    exit /b 1
)

REM Run the provided command
%args%
