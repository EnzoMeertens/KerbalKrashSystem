@echo off

echo.

if [%1] == [] (
  echo.
  echo No project (.csproj^) file supplied. Scanning local directory...
  echo.

  for %%f in (.\*) do (
    if "%%~xf" EQU ".csproj" (
      echo Found %%f.
      set filename=%%f
    )
  )

  echo.

  if filename == "" (
    echo No project (.csproj^) file found. Exiting...
    pause
    exit
  )
) else (
  set filename=%1
)

"%~dp0/Compile/MSBuild.exe" /t:ReBuild /p:Configuration=Release /p:ResGenToolPath=%~dp0/Compile %filename% > compile.log

if %errorlevel% EQU 0 (
  color A
  echo.
  echo Recompiled successfully!
  echo.
  echo Copying "/bin/Release/KerbalKrashSystem.dll" to "/GameData/KerbalKrashSystem/Plugins/KerbalKrashSystem.dll"...
  if not exist "%~dp0/GameData/KerbalKrashSystem/Plugins/" mkdir "%~dp0/GameData/KerbalKrashSystem/Plugins/"
  xcopy "bin/Release/KerbalKrashSystem.dll" "GameData/KerbalKrashSystem/Plugins/" /Y /I /F
)

if %errorlevel% NEQ 0 (
  color C
  echo.
  echo Recompile failed. Please see compile.log for more information.
)

echo.
echo.

pause