@echo off
rem Zapusk Web testiv XYZ Bank. Vikno ne zakryvayetsya pislya progonu.
rem   Run-Tests.bat                              usi testy, z viknom brauzera
rem   Run-Tests.bat -Headless                    bez vikna brauzera
rem   Run-Tests.bat -Filter "TestCategory=smoke" lyshe odyn scenariy
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
