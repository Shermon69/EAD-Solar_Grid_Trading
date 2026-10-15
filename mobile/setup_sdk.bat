@echo off
set "JAVA_HOME=C:\Program Files\Java\jdk-17"
echo Using JAVA_HOME=%JAVA_HOME%
echo.
echo Accepting licenses...
echo y | C:\Android\cmdline-tools\bin\sdkmanager.bat --sdk_root=C:\Android --licenses
echo.
echo Installing SDK components (this may take several minutes)...
C:\Android\cmdline-tools\bin\sdkmanager.bat --sdk_root=C:\Android "platform-tools" "platforms;android-34" "build-tools;34.0.0"
echo.
echo Done! SDK components installed.
pause
