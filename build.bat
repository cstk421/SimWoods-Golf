@echo off
echo Building SimLock...
echo.

cd /d "%~dp0"

echo Building solution...
dotnet build SimLock.sln -c Release

if %ERRORLEVEL% NEQ 0 (
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo Creating output directory...
if not exist "publish" mkdir publish
if not exist "publish\Assets" mkdir publish\Assets
if not exist "publish\Videos" mkdir publish\Videos

echo.
echo Publishing SimLock.exe...
dotnet publish src\SimLock.Locker\SimLock.Locker.csproj -c Release -o publish --self-contained false

echo.
echo Publishing SimLock.Admin.exe...
dotnet publish src\SimLock.Admin\SimLock.Admin.csproj -c Release -o publish --self-contained false

echo.
echo Publishing SimLock.Monitor.exe...
dotnet publish src\SimLock.Monitor\SimLock.Monitor.csproj -c Release -o publish --self-contained false

echo.
echo Publishing SimLock.Launcher.exe...
dotnet publish src\SimLock.Launcher\SimLock.Launcher.csproj -c Release -o publish --self-contained false

echo.
echo Copying config and assets...
copy config.json publish\
copy Assets\*.* publish\Assets\ 2>nul
copy Videos\*.* publish\Videos\ 2>nul

echo.
echo ========================================
echo Build complete!
echo Output: %~dp0publish
echo ========================================
echo.
echo Files in publish folder:
dir /b publish\*.exe
echo.
echo DEPLOYMENT INSTRUCTIONS:
echo ========================
echo 1. Copy the entire 'publish' folder to target machine
echo 2. Add your logo.png to publish\Assets\
echo 3. Run SimLock.Launcher.exe (checks for .NET, then runs SimLock)
echo    - If .NET 8 is missing, it will prompt to install
echo 4. Use SimLock.Admin.exe to configure settings
echo 5. Run SimLock.Monitor.exe to auto-launch on gspro.exe
echo.
pause
