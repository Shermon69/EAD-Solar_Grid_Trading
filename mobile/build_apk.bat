@echo off
set "JAVA_HOME=C:\Progra~1\Java\jdk-17"
echo Using JAVA_HOME=%JAVA_HOME%
call gradlew.bat assembleDebug
if %ERRORLEVEL%==0 (
    echo.
    echo BUILD SUCCESS!
    echo APK is at: app\build\outputs\apk\debug\app-debug.apk
) else (
    echo.
    echo BUILD FAILED - see errors above
)
pause
