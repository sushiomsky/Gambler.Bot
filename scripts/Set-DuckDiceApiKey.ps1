param(
    [string]$ApiKey
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($ApiKey)) {
    $ApiKey = Read-Host 'DuckDice API key'
}

if ([string]::IsNullOrWhiteSpace($ApiKey)) {
    throw 'DuckDice API key was empty.'
}

$secretDirectory = Join-Path $env:APPDATA 'Gambler.Bot\secrets'
$secretPath = Join-Path $secretDirectory 'duckdice-api-key.txt'

New-Item -ItemType Directory -Force -Path $secretDirectory | Out-Null
Set-Content -LiteralPath $secretPath -Value $ApiKey.Trim() -NoNewline

Write-Host "DuckDice API key saved outside the repository: $secretPath"
