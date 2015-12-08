@echo off
cd %~dp0

SETLOCAL
SET CACHED_NUGET=%LocalAppData%\NuGet\NuGet.exe

IF EXIST %CACHED_NUGET% goto copynuget
echo Downloading latest version of NuGet.exe...
IF NOT EXIST %LocalAppData%\NuGet md %LocalAppData%\NuGet
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest 'https://www.nuget.org/nuget.exe' -OutFile '%CACHED_NUGET%'"

:copynuget
IF EXIST .nuget\nuget.exe goto restore
md .nuget
copy %CACHED_NUGET% .nuget\nuget.exe > nul
:restore
.nuget\NuGet.exe "Install" "FAKE" "-OutputDirectory" "packages" "-ExcludeVersion" "-Version" "3.27.5"
.nuget\NuGet.exe "Install" "SrvUpload" "-OutputDirectory" "packages" "-ExcludeVersion" -Source "http://dale-srvbld/guestAuth/app/nuget/v1/FeedService.svc"

set arg1=%1

"packages\FAKE\tools\Fake.exe" build.fsx target=%arg1%