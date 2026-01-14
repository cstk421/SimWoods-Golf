@echo off
echo Building SimLock (Self-Contained - No .NET required on target)...
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
if not exist "publish-standalone" mkdir publish-standalone
if not exist "publish-standalone\Assets" mkdir publish-standalone\Assets
if not exist "publish-standalone\Videos" mkdir publish-standalone\Videos

echo.
echo Publishing SimLock.exe (self-contained)...
dotnet publish src\SimLock.Locker\SimLock.Locker.csproj -c Release -o publish-standalone -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

echo.
echo Publishing SimLock.Admin.exe (self-contained)...
dotnet publish src\SimLock.Admin\SimLock.Admin.csproj -c Release -o publish-standalone -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

echo.
echo Publishing SimLock.Monitor.exe (self-contained)...
dotnet publish src\SimLock.Monitor\SimLock.Monitor.csproj -c Release -o publish-standalone -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

echo.
echo Copying config and assets...
copy config.json publish-standalone\
copy Assets\*.* publish-standalone\Assets\ 2>nul
copy Videos\*.* publish-standalone\Videos\ 2>nul

echo.
echo ========================================
echo Build complete! (Self-Contained)
echo Output: %~dp0publish-standalone
echo ========================================
echo.
pause
