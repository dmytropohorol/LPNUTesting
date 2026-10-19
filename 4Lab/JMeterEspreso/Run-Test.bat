@echo off
rem Run-Test.bat [chytachi] [vizyty] [sayt], za zamovchuvannyam 5 3 espreso.tv
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

set "USERS=%~1"
set "LOOPS=%~2"
set "HOST=%~3"
if "%USERS%"=="" set "USERS=5"
if "%LOOPS%"=="" set "LOOPS=3"
if "%HOST%"=="" set "HOST=espreso.tv"

if exist results rmdir /s /q results
mkdir results

call "%JMETER_HOME%\bin\jmeter.bat" -n -t EspresoLoad.jmx -Jhost=%HOST% -Jusers=%USERS% -Jloops=%LOOPS% -Jrampup=30 -l results\results.jtl -j results\jmeter.log

:end
echo.
timeout /t 60
