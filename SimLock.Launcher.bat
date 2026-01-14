@echo off
setlocal

:: SimLock Launcher - Checks for .NET Runtime and launches app
:: This launcher ensures end users have .NET 8 Desktop Runtime installed

set "DOTNET_MIN_VERSION=8.0"
set "DOTNET_DOWNLOAD_URL=https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe"

:: Check if dotnet is available
where dotnet >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    goto :PROMPT_INSTALL
)

:: Check if .NET Desktop Runtime 8.0+ is installed
dotnet --list-runtimes 2>nul | findstr /C:"Microsoft.WindowsDesktop.App 8." >nul
if %ERRORLEVEL% NEQ 0 (
    goto :PROMPT_INSTALL
)

:: .NET is installed, launch SimLock
goto :LAUNCH_APP

:PROMPT_INSTALL
echo.
echo ============================================
echo  SimLock requires .NET 8 Desktop Runtime
echo ============================================
echo.
echo .NET 8 Desktop Runtime is not installed on this computer.
echo.

:: Use PowerShell to show a message box
powershell -Command "Add-Type -AssemblyName PresentationFramework; $result = [System.Windows.MessageBox]::Show('.NET 8 Desktop Runtime is required to run SimLock.`n`nWould you like to download and install it now?', 'SimLock - Runtime Required', 'YesNo', 'Information'); if ($result -eq 'Yes') { Start-Process '%DOTNET_DOWNLOAD_URL%'; exit 0 } else { exit 1 }"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Please run this launcher again after installing .NET 8 Desktop Runtime.
    echo.
    pause
    exit /b 0
)

echo.
echo SimLock cannot run without .NET 8 Desktop Runtime.
echo You can download it manually from: https://dotnet.microsoft.com/download/dotnet/8.0
echo.
pause
exit /b 1

:LAUNCH_APP
cd /d "%~dp0"
if exist "SimLock.exe" (
    start "" "SimLock.exe"
) else (
    echo Error: SimLock.exe not found in %~dp0
    pause
)
exit /b 0
