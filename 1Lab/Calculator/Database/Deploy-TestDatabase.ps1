# Розгортає базу CalcTestDb з тестовими даними для методу Format().
#   .\Deploy-TestDatabase.ps1
#   .\Deploy-TestDatabase.ps1 -ServerInstance '.\SQLEXPRESS'
param(
    [string] $ServerInstance = '(localdb)\MSSQLLocalDB'
)

$ErrorActionPreference = 'Stop'
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$connectionString = "Data Source=$ServerInstance;Initial Catalog=master;Integrated Security=True;Connect Timeout=60"

function Invoke-SqlFile([string] $Path) {
    $text = [System.IO.File]::ReadAllText($Path, [System.Text.Encoding]::UTF8)

    # SqlClient не розуміє GO, тому скрипт виконується пакетами між GO.
    $batches = [regex]::Split($text, '(?im)^\s*GO\s*$') | Where-Object { $_.Trim() }

    $conn = New-Object System.Data.SqlClient.SqlConnection $connectionString
    $conn.Open()
    try {
        foreach ($batch in $batches) {
            $cmd = $conn.CreateCommand()
            $cmd.CommandText = $batch
            [void] $cmd.ExecuteNonQuery()
        }
    }
    finally {
        $conn.Close()
    }
}

Write-Host "Сервер: $ServerInstance" -ForegroundColor Cyan

foreach ($name in '01_CreateSchema.sql', '02_SeedReferenceData.sql', '03_SeedFormatTestCases.sql') {
    Write-Host "  -> $name" -ForegroundColor Gray
    Invoke-SqlFile (Join-Path $scriptDir $name)
}

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
}
