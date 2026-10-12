<#
.SYNOPSIS
    Складає рішення та запускає тести Web API (RestSharp + SpecFlow).

.DESCRIPTION
    Лабораторна робота №3. Завдання 1: операції CRUD сервісу restful-booker.
    Завдання 2: категорія Weather зі списку відкритих API, сервіс Open-Meteo.

.PARAMETER Filter
    Фільтр тестів, напр. -Filter "TestCategory=smoke" або -Filter "TestCategory=weather".

.EXAMPLE
    .\Run-Tests.ps1
    .\Run-Tests.ps1 -Filter "TestCategory=smoke"
    .\Run-Tests.ps1 -Filter "TestCategory=weather"
#>
[CmdletBinding()]
param(
    [string] $Filter
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$solution = Join-Path $root 'WebApiTesting.RestSharp.sln'
$results = Join-Path $root 'TestResults'

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) { throw 'Не знайдено dotnet. Потрібен .NET SDK.' }

Write-Host '== Перевірка доступності сервісів ==' -ForegroundColor Cyan
foreach ($probe in @(
    @{ Name = 'restful-booker'; Url = 'https://restful-booker.herokuapp.com/ping' },
    @{ Name = 'Open-Meteo'; Url = 'https://api.open-meteo.com/v1/forecast?latitude=49.84&longitude=24.03&current=temperature_2m' }
)) {
    try {
        $code = (Invoke-WebRequest -Uri $probe.Url -Method Get -TimeoutSec 30 -UseBasicParsing).StatusCode
        Write-Host ("  {0,-16} {1}" -f $probe.Name, $code) -ForegroundColor Gray
    }
    catch {
        Write-Host ("  {0,-16} недоступний: {1}" -f $probe.Name, $_.Exception.Message) -ForegroundColor Yellow
        Write-Host '  Тести потребують мережі: обидві служби зовнішні.' -ForegroundColor Yellow
    }
}

Write-Host ''
Write-Host '== Збирання рішення ==' -ForegroundColor Cyan
& dotnet build $solution -c Debug -v quiet --nologo
if ($LASTEXITCODE -ne 0) { throw "Помилка збирання (код $LASTEXITCODE)." }

if (Test-Path $results) { Remove-Item $results -Recurse -Force }

$arguments = @('test', $solution, '-c', 'Debug', '--no-build', '--nologo',
               '--logger', 'console;verbosity=normal',
               '--results-directory', $results)
if ($Filter) { $arguments += @('--filter', $Filter) }

Write-Host ''
Write-Host '== Запуск тестів Web API ==' -ForegroundColor Cyan
& dotnet @arguments
$exitCode = $LASTEXITCODE

Write-Host ''
if ($exitCode -eq 0) {
    Write-Host 'Усі тести пройшли.' -ForegroundColor Green
}
else {
    Write-Host 'Є тести, що впали. Журнал обміну з сервісом наведений вище.' -ForegroundColor Red
}

exit $exitCode
