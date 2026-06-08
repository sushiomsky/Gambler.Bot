$ErrorActionPreference = 'Stop'

$env:GAMBLER_BOT_ENABLE_DUCKDICE_LIVE_BET = 'DECOY_0.01_OK'

dotnet test .\Platforms\Gambler.Bot.WinUI.Tests\Gambler.Bot.WinUI.Tests.csproj `
    -c Release `
    --filter DuckDiceDecoyMinimumBetCanBePlacedWhenExplicitlyEnabled
