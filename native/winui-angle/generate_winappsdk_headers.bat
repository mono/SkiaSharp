@echo off
REM Generate Windows App SDK headers from WINMD/IDL files.
REM Usage: generate_winappsdk_headers.bat <win_sdk_bin> <include_dir> <lib_dir>
REM
REM This script must be run under vcvarsall.bat so midlrt can find cl.exe.

setlocal enabledelayedexpansion

set "WIN_SDK_BIN=%~1"
set "INCLUDE_DIR=%~2"
set "LIB_DIR=%~3"

if "%WIN_SDK_BIN%"=="" (
    echo Usage: %0 ^<win_sdk_bin^> ^<include_dir^> ^<lib_dir^>
    exit /b 1
)
if "%INCLUDE_DIR%"=="" (
    echo Usage: %0 ^<win_sdk_bin^> ^<include_dir^> ^<lib_dir^>
    exit /b 1
)
if "%LIB_DIR%"=="" (
    echo Usage: %0 ^<win_sdk_bin^> ^<include_dir^> ^<lib_dir^>
    exit /b 1
)

set "WINMDIDL=%WIN_SDK_BIN%\winmdidl.exe"
set "MIDLRT=%WIN_SDK_BIN%\midlrt.exe"
set "UAP_VERSION=10.0.18362"

cd /d "%INCLUDE_DIR%"
if errorlevel 1 exit /b 1

REM Process WINMD files with winmdidl.exe
call :run_winmdidl "uap%UAP_VERSION%\Microsoft.Foundation.winmd" "Microsoft.Foundation.idl"
if errorlevel 1 exit /b 1
call :run_winmdidl "uap%UAP_VERSION%\Microsoft.Graphics.winmd" "Microsoft.Graphics.DirectX.idl"
if errorlevel 1 exit /b 1
call :run_winmdidl "uap%UAP_VERSION%\Microsoft.UI.winmd" "Microsoft.UI.idl"
if errorlevel 1 exit /b 1
call :run_winmdidl "uap10.0\Microsoft.UI.Text.winmd" "Microsoft.UI.Text.idl"
if errorlevel 1 exit /b 1
call :run_winmdidl "uap10.0\Microsoft.UI.Xaml.winmd" "Microsoft.UI.Xaml.idl"
if errorlevel 1 exit /b 1
call :run_winmdidl "uap10.0\Microsoft.Web.WebView2.Core.winmd" "Microsoft.Web.WebView2.Core.idl"
if errorlevel 1 exit /b 1

REM Process IDL files with midlrt.exe
call :run_midlrt "Microsoft.Foundation.idl"
if errorlevel 1 exit /b 1
call :run_midlrt "Microsoft.Graphics.DirectX.idl"
if errorlevel 1 exit /b 1
call :run_midlrt "Microsoft.UI.Composition.idl"
if errorlevel 1 exit /b 1
call :run_midlrt "Microsoft.UI.Composition.SystemBackdrops.idl"
if errorlevel 1 exit /b 1
call :run_midlrt "Microsoft.UI.Dispatching.idl"
if errorlevel 1 exit /b 1
call :run_midlrt "Microsoft.UI.idl"
if errorlevel 1 exit /b 1
call :run_midlrt "Microsoft.UI.Input.idl"
if errorlevel 1 exit /b 1
call :run_midlrt "Microsoft.UI.Text.idl"
if errorlevel 1 exit /b 1
call :run_midlrt "Microsoft.UI.Windowing.idl"
if errorlevel 1 exit /b 1
call :run_midlrt "Microsoft.UI.Xaml.Automation.idl"
if errorlevel 1 exit /b 1
call :run_midlrt "Microsoft.UI.Xaml.Automation.Peers.idl"
if errorlevel 1 exit /b 1
call :run_midlrt "Microsoft.UI.Xaml.Automation.Provider.idl"
if errorlevel 1 exit /b 1
call :run_midlrt "Microsoft.UI.Xaml.Automation.Text.idl"
if errorlevel 1 exit /b 1
call :run_midlrt "Microsoft.UI.Xaml.Controls.idl"
if errorlevel 1 exit /b 1
call :run_midlrt "Microsoft.UI.Xaml.Controls.Primitives.idl"
if errorlevel 1 exit /b 1
call :run_midlrt "Microsoft.UI.Xaml.Data.idl"
if errorlevel 1 exit /b 1
call :run_midlrt "Microsoft.UI.Xaml.Documents.idl"
if errorlevel 1 exit /b 1
call :run_midlrt "Microsoft.UI.Xaml.idl"
if errorlevel 1 exit /b 1
call :run_midlrt "Microsoft.UI.Xaml.Input.idl"
if errorlevel 1 exit /b 1
call :run_midlrt "Microsoft.UI.Xaml.Interop.idl"
if errorlevel 1 exit /b 1
call :run_midlrt "Microsoft.UI.Xaml.Media.Animation.idl"
if errorlevel 1 exit /b 1
call :run_midlrt "Microsoft.UI.Xaml.Media.idl"
if errorlevel 1 exit /b 1
call :run_midlrt "Microsoft.UI.Xaml.Media.Imaging.idl"
if errorlevel 1 exit /b 1
call :run_midlrt "Microsoft.UI.Xaml.Media.Media3D.idl"
if errorlevel 1 exit /b 1
call :run_midlrt "Microsoft.UI.Xaml.Navigation.idl"
if errorlevel 1 exit /b 1
call :run_midlrt "Microsoft.Web.WebView2.Core.idl"
if errorlevel 1 exit /b 1

echo Windows App SDK header generation complete.
exit /b 0

:run_winmdidl
REM %~1 = winmd relative path under lib, %~2 = expected output stamp file
if exist "%INCLUDE_DIR%\%~2" goto :eof
echo Processing WINMD %~1...
"%WINMDIDL%" "%LIB_DIR%\%~1" /metadata_dir:C:\Windows\System32\WinMetadata /metadata_dir:"%LIB_DIR%\uap%UAP_VERSION%" /metadata_dir:"%LIB_DIR%\uap10.0" /outdir:"%INCLUDE_DIR%" /nologo
exit /b %errorlevel%

:run_midlrt
REM %~1 = IDL filename in include dir
set "_noext=%~n1"
if exist "%INCLUDE_DIR%\%_noext%.h" goto :eof
echo Processing IDL %~1...
"%MIDLRT%" "%INCLUDE_DIR%\%~1" /metadata_dir C:\Windows\System32\WinMetadata /ns_prefix /nomidl /nologo
exit /b %errorlevel%
