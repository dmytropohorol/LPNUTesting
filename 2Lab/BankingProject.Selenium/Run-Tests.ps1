# Збирає рішення і запускає Web тести XYZ Bank.
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
dotnet test (Join-Path $root 'BankingProject.Selenium.sln') --nologo --logger 'console;verbosity=normal'
exit $LASTEXITCODE
