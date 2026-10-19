@echo off
chcp 65001 >nul
cd /d "%~dp0"
set "JMETER_HOME=C:\soft\apache-jmeter-5.6.3"
for /d %%D in ("C:\soft\jdk-21*") do set "JAVA_HOME=%%D"
if not defined JAVA_HOME (
    echo Ne znaydeno Java u C:\soft\jdk-21*
    goto :end
)
if not exist "%JMETER_HOME%\bin\jmeter.bat" (
    echo Ne znaydeno JMeter u %JMETER_HOME%
    goto :end
)
set "PATH=%JAVA_HOME%\bin;%PATH%"

set JM_START=start "Apache_JMeter"
set JM_LAUNCH=javaw.exe
call "%JMETER_HOME%\bin\jmeter.bat" -t "%~dp0EspresoLoad.jmx"
exit /b

:end
timeout /t 60
