@echo off
setlocal enabledelayedexpansion
rem ====== projects ======

set project=Asv.Drones.Gui.Api


set "file=.\src\Directory.Build.props"


:: Ищем строку с ApiVersion и извлекаем значение
for /f "tokens=2 delims=> " %%a in ('findstr /i /c:"<ApiVersion>" "%file%"') do (
    set "line=%%a"
    for /f "tokens=1 delims=<" %%b in ("!line!") do (
        set "ApiVersion=%%b"
    )
)

:: Проверяем и выводим результат
if defined ApiVersion (
    echo ApiVersion: %ApiVersion%
    	dotnet restore ./src/%project%/%project%.csproj
	dotnet build /p:SolutionDir=../;ProductVersion=%ApiVersion% ./src/%project%/%project%.csproj -c Release
	dotnet pack ./src/%project%/%project%.csproj -c Release


) else (
    echo ApiVersion not found
)

endlocal
pause