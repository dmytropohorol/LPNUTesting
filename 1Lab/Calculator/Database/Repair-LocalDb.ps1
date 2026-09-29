<#
.SYNOPSIS
    Відновлює працездатність LocalDB і бази CalcTestDb.

.DESCRIPTION
    Запускайте, якщо тести падають з помилкою підключення до бази:
      "A network-related or instance-specific error ... error: 50"
      "Error occurred during LocalDB instance startup: SQL Server process failed to start"
      "Cannot open database CalcTestDb requested by the login"

    Скрипт діє від м'якого до жорсткого:
      1. перевіряє підключення — якщо все гаразд, нічого не робить;
      2. прибирає осиротілі процеси sqlservr.exe, які тримають файли
         екземпляра й не дають йому стартувати (найчастіша причина);
      3. якщо не допомогло — повністю перестворює екземпляр LocalDB
         і розгортає базу наново зі SQL-скриптів.

    База даних повністю відтворювана зі скриптів у цій теці, тому
    жорстке перестворення нічого не втрачає.

.EXAMPLE
    .\Repair-LocalDb.ps1
#>
[CmdletBinding()]
param(
    [string] $InstanceName = 'MSSQLLocalDB'
)

$ErrorActionPreference = 'Stop'
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$dataSource = "(localdb)\$InstanceName"

function Get-SqlLocalDbExe {
    $exe = Get-ChildItem 'C:\Program Files\Microsoft SQL Server' -Filter 'SqlLocalDB.exe' `
                         -Recurse -ErrorAction SilentlyContinue |
           Sort-Object FullName | Select-Object -Last 1

    if (-not $exe) {
        throw 'Не знайдено SqlLocalDB.exe. Схоже, SQL Server Express LocalDB не встановлено.'
    }

    return $exe.FullName
}

function Test-Connection2 {
    param([string] $Catalog = 'CalcTestDb')

    try {
        $connection = New-Object System.Data.SqlClient.SqlConnection `
            "Data Source=$dataSource;Initial Catalog=$Catalog;Integrated Security=True;Connect Timeout=15"
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = 'SELECT COUNT(*) FROM dbo.FormatTestCase'
        $count = $command.ExecuteScalar()
        $connection.Close()
        return [int] $count
    }
    catch {
        return -1
    }
}

$localdb = Get-SqlLocalDbExe

# --- Крок 1: може, все вже працює ---------------------------------------------
Write-Host 'Крок 1: перевіряю підключення...' -ForegroundColor Cyan
$rows = Test-Connection2
if ($rows -gt 0) {
    Write-Host "  База доступна, тест-кейсів: $rows. Ремонт не потрібен." -ForegroundColor Green
    exit 0
}

# --- Крок 2: осиротілі процеси sqlservr ---------------------------------------
Write-Host 'Крок 2: прибираю осиротілі процеси sqlservr...' -ForegroundColor Cyan
$stray = @(Get-Process sqlservr -ErrorAction SilentlyContinue)
foreach ($process in $stray) {
    Write-Host "  зупиняю sqlservr PID $($process.Id)" -ForegroundColor Gray
    try { $process | Stop-Process -Force } catch { }
}
if ($stray.Count -gt 0) { Start-Sleep -Seconds 2 }

& $localdb start $InstanceName 2>&1 | Out-Null
Start-Sleep -Seconds 2

$rows = Test-Connection2
if ($rows -gt 0) {
    Write-Host "  Готово. База доступна, тест-кейсів: $rows." -ForegroundColor Green
    exit 0
}

# --- Крок 3: повне перестворення екземпляра -----------------------------------
Write-Host 'Крок 3: перестворюю екземпляр LocalDB...' -ForegroundColor Yellow

Get-Process sqlservr -ErrorAction SilentlyContinue | ForEach-Object {
    try { $_ | Stop-Process -Force } catch { }
}

& $localdb stop $InstanceName -k 2>&1 | Out-Null
& $localdb delete $InstanceName 2>&1 | Out-Null

$instanceDir = Join-Path $env:LOCALAPPDATA "Microsoft\Microsoft SQL Server Local DB\Instances\$InstanceName"
if (Test-Path $instanceDir) {
    Remove-Item $instanceDir -Recurse -Force -ErrorAction SilentlyContinue
}

# Файли бази лишаються «осиротілими» і заважають CREATE DATABASE — відкладаємо їх
$backupDir = Join-Path $env:TEMP ("CalcTestDb_old_" + (Get-Date -Format 'yyyyMMdd_HHmmss'))
$orphans = @(Get-ChildItem $env:USERPROFILE -Filter 'CalcTestDb*' -ErrorAction SilentlyContinue)
if ($orphans.Count -gt 0) {
    New-Item -ItemType Directory -Force -Path $backupDir | Out-Null
    foreach ($file in $orphans) {
        Move-Item $file.FullName -Destination $backupDir -Force -ErrorAction SilentlyContinue
    }
    Write-Host "  старі файли бази відкладено в $backupDir" -ForegroundColor Gray
}

& $localdb create $InstanceName -s 2>&1 | Out-Null
Start-Sleep -Seconds 2

Write-Host 'Крок 4: розгортаю базу даних наново...' -ForegroundColor Cyan
& (Join-Path $scriptDir 'Deploy-TestDatabase.ps1') -ServerInstance $dataSource | Out-Null

$rows = Test-Connection2
if ($rows -gt 0) {
    Write-Host ''
    Write-Host "Готово. База відновлена, тест-кейсів: $rows." -ForegroundColor Green
    exit 0
}

Write-Host ''
Write-Host 'Не вдалося відновити автоматично.' -ForegroundColor Red
Write-Host 'Подивіться журнал:' -ForegroundColor Red
Write-Host "  $env:LOCALAPPDATA\Microsoft\Microsoft SQL Server Local DB\Instances\$InstanceName\error.log"
exit 1
