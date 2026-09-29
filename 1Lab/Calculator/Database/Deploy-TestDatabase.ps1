<#
.SYNOPSIS
    Розгортає базу даних CalcTestDb з набором тестових даних для модульних
    тестів методу AnalaizerClass.Format() (лабораторна робота №1, варіант 4).

.DESCRIPTION
    Послідовно виконує скрипти 01_CreateSchema.sql, 02_SeedReferenceData.sql
    та 03_SeedFormatTestCases.sql. Пакети, розділені директивою GO,
    виконуються окремо (SqlClient не розуміє GO).

.PARAMETER ServerInstance
    Екземпляр MS SQL Server. За замовчуванням — (localdb)\MSSQLLocalDB.

.EXAMPLE
    .\Deploy-TestDatabase.ps1
    .\Deploy-TestDatabase.ps1 -ServerInstance '.\SQLEXPRESS'
#>
[CmdletBinding()]
param(
    [string] $ServerInstance = '(localdb)\MSSQLLocalDB'
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$scripts = @(
    '01_CreateSchema.sql',
    '02_SeedReferenceData.sql',
    '03_SeedFormatTestCases.sql'
)

function Invoke-SqlFile {
    param([string] $Path, [string] $ConnectionString)

    $text = [System.IO.File]::ReadAllText($Path, [System.Text.Encoding]::UTF8)

    # Розбиваємо на пакети по рядках, що містять лише GO
    $batches = [System.Text.RegularExpressions.Regex]::Split(
        $text, '(?im)^\s*GO\s*$') |
        Where-Object { $_.Trim().Length -gt 0 }

    $conn = New-Object System.Data.SqlClient.SqlConnection $ConnectionString
    $conn.Open()
    try {
        foreach ($batch in $batches) {
            $cmd = $conn.CreateCommand()
            $cmd.CommandText = $batch
            $cmd.CommandTimeout = 120
            [void] $cmd.ExecuteNonQuery()
            $cmd.Dispose()
        }
    }
    finally {
        $conn.Close()
        $conn.Dispose()
    }
}

function Start-LocalDbInstance {
    param([string] $DataSource)

    # LocalDB зупиняє екземпляр після простою. Автозапуск під час підключення
    # інколи не встигає, і перша спроба падає з помилкою 4060 — тому запускаємо явно.
    if ($DataSource -notmatch '^\(localdb\)\\(.+)$') { return }

    $instance = $Matches[1]
    $exe = Get-ChildItem 'C:\Program Files\Microsoft SQL Server' -Filter 'SqlLocalDB.exe' `
                         -Recurse -ErrorAction SilentlyContinue |
           Sort-Object FullName | Select-Object -Last 1

    if (-not $exe) { return }

    & $exe.FullName start $instance 2>&1 | Out-Null
}

Start-LocalDbInstance -DataSource $ServerInstance

$connectionString =
    "Data Source=$ServerInstance;Initial Catalog=master;Integrated Security=True;" +
    "Connect Timeout=60;MultipleActiveResultSets=True"

Write-Host "Сервер: $ServerInstance" -ForegroundColor Cyan

foreach ($name in $scripts) {
    $path = Join-Path $scriptDir $name
    if (-not (Test-Path $path)) { throw "Не знайдено скрипт: $path" }
    Write-Host "  -> $name" -ForegroundColor Gray
    Invoke-SqlFile -Path $path -ConnectionString $connectionString
}

# Підсумок
$summary = @"
USE [CalcTestDb];
SELECT cat.TestCategoryId, cat.CategoryName, COUNT(*) AS Cases
FROM   dbo.FormatTestCase tc
       JOIN dbo.TestCategory cat ON cat.TestCategoryId = tc.TestCategoryId
GROUP BY cat.TestCategoryId, cat.CategoryName
ORDER BY cat.TestCategoryId;
"@

$conn = New-Object System.Data.SqlClient.SqlConnection $connectionString
$conn.Open()
try {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $summary
    $reader = $cmd.ExecuteReader()
    $total = 0
    Write-Host ''
    Write-Host ('{0,3}  {1,-34} {2,5}' -f 'Id', 'Категорія', 'Тестів') -ForegroundColor Cyan
    while ($reader.Read()) {
        $total += $reader.GetInt32(2)
        Write-Host ('{0,3}  {1,-34} {2,5}' -f $reader.GetInt32(0), $reader.GetString(1), $reader.GetInt32(2))
    }
    $reader.Close()
    Write-Host ('{0,3}  {1,-34} {2,5}' -f '', 'РАЗОМ', $total) -ForegroundColor Green
}
finally {
    $conn.Close()
    $conn.Dispose()
}
