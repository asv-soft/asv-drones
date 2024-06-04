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
	cd src\%project%\bin\Release\
rem	dotnet nuget push %project%.%ApiVersion%.nupkg --skip-duplicate --source https://api.nuget.org/v3/index.json
	dotnet nuget push %project%.%ApiVersion%.nupkg --skip-duplicate --source https://nuget.pkg.github.com/asv-soft/index.json


) else (
    echo ApiVersion not found
)

endlocal
pause



