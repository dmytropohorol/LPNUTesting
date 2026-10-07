<#
.SYNOPSIS
    Складає рішення та запускає Web тести XYZ Bank (Selenium + SpecFlow).

.DESCRIPTION
    Лабораторна робота №2, варіант 4: вхід Bank Manager Login, операція Open Account.
    Скрипт сам знаходить MSBuild.exe та vstest.console.exe через vswhere.

.PARAMETER Headless
    Запустити Chrome без вікна. Зручно, коли не треба дивитися на браузер.

.PARAMETER Filter
    Фільтр тестів, напр. -Filter "TestCategory=smoke" або -Filter "Name~Negative".

.EXAMPLE
    .\Run-Tests.ps1
    .\Run-Tests.ps1 -Headless
    .\Run-Tests.ps1 -Filter "TestCategory=smoke"
#>
[CmdletBinding()]
param(
    [switch] $Headless,
    [string] $Filter
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$solution = Join-Path $root 'BankingProject.Selenium.sln'
$testDll = Join-Path $root 'BankingTests\bin\Debug\net472\BankingTests.dll'
$results = Join-Path $root 'TestResults'

function Find-VsTool {
    param([string] $RelativePath)

    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (-not (Test-Path $vswhere)) { throw 'Не знайдено vswhere.exe. Потрібна Visual Studio 2019 або новіша.' }

    foreach ($install in (& $vswhere -all -products * -format value -property installationPath)) {
        $candidate = Join-Path $install $RelativePath
        if (Test-Path $candidate) { return $candidate }
    }

    throw "Не знайдено: $RelativePath"
}

$msbuild = Find-VsTool 'MSBuild\Current\Bin\MSBuild.exe'
$vstest = Find-VsTool 'Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe'

Write-Host '== Відновлення пакетів NuGet ==' -ForegroundColor Cyan
& $msbuild $solution -t:Restore -v:quiet -nologo
if ($LASTEXITCODE -ne 0) { throw "Помилка відновлення пакетів (код $LASTEXITCODE)." }

Write-Host '== Збирання рішення ==' -ForegroundColor Cyan
& $msbuild $solution -p:Configuration=Debug -v:minimal -nologo
if ($LASTEXITCODE -ne 0) { throw "Помилка збирання (код $LASTEXITCODE)." }

if ($Headless) {
    $env:HEADLESS = '1'
    Write-Host 'Режим: Chrome без вікна' -ForegroundColor Gray
}
else {
    Remove-Item Env:\HEADLESS -ErrorAction SilentlyContinue
    Write-Host 'Режим: Chrome з вікном' -ForegroundColor Gray
}

if (Test-Path $results) { Remove-Item $results -Recurse -Force }

$arguments = @($testDll, '/Logger:console;verbosity=normal', "/ResultsDirectory:$results")
if ($Filter) { $arguments += "/TestCaseFilter:$Filter" }

Write-Host ''
Write-Host '== Запуск Web тестів ==' -ForegroundColor Cyan
& $vstest @arguments
$exitCode = $LASTEXITCODE

$shots = Join-Path $root 'BankingTests\bin\Debug\net472\Screenshots'
if (Test-Path $shots) {
    $files = @(Get-ChildItem $shots -Filter '*.png' -ErrorAction SilentlyContinue)
    if ($files.Count -gt 0) {
        Write-Host ''
        Write-Host "Знімки екрана для сценаріїв, що впали: $shots" -ForegroundColor Yellow
    }
}

exit $exitCode
