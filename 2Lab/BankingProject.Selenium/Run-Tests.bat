@echo off
chcp 65001 >nul
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "Run-Tests.ps1"
echo.
timeout /t 60
