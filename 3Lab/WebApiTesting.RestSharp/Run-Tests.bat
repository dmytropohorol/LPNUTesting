@echo off
rem Zapusk testiv Web API. Vikno ne zakryvayetsya pislya progonu.
rem   Run-Tests.bat                                usi testy
rem   Run-Tests.bat -Filter "TestCategory=smoke"   lyshe osnovni scenariyi
rem   Run-Tests.bat -Filter "TestCategory=weather" lyshe zavdannya 2
rem
rem Tekst u comu fayli navmysno latynkoyu: kyrylytsya vseredyni .bat
rem lamaye rozbir komand u cmd.exe.
chcp 65001 >nul
cd /d "%~dp0"

powershell -NoProfile -ExecutionPolicy Bypass -File "Run-Tests.ps1" %*

echo.
echo ------------------------------------------------------
echo  Done. Window closes in 60 s, or press any key to exit
echo ------------------------------------------------------
timeout /t 60
