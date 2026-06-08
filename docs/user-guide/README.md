# Gambler.Bot WinUI User Guide

This guide describes the new native Windows client built with WinUI 3.

> Note: The WinUI client does not fully replace the old Avalonia UI yet. This guide documents the current state and explicitly marks unfinished areas.

## Contents

- [Installation](#installation)
- [First Launch](#first-launch)
- [Navigation](#navigation)
- [Dashboard](#dashboard)
- [Selecting Sites](#selecting-sites)
- [Login And Simulation](#login-and-simulation)
- [Strategies](#strategies)
- [Bet Preview](#bet-preview)
- [Bet History](#bet-history)
- [Intelligence](#intelligence)
- [Settings](#settings)
- [Live DuckDice Smoke Test](#live-duckdice-smoke-test)
- [Safety](#safety)
- [Troubleshooting](#troubleshooting)
- [Known Limitations](#known-limitations)
- [Screenshots](#screenshots)

## Installation

### Download From GitHub

1. Open the GitHub Releases page for the project.
2. Download `Gambler.Bot.WinUI-win-x64.zip` from the latest release.
3. Extract the ZIP file into a folder of your choice.
4. Start `Gambler.Bot.WinUI.exe`.

If the release ZIP is not available yet, the WinUI release workflow has not been run on GitHub.

### Local Build

```powershell
dotnet publish .\Platforms\Gambler.Bot.WinUI\Gambler.Bot.WinUI.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:WindowsAppSDKSelfContained=true `
  -p:WindowsPackageType=None `
  -p:PublishTrimmed=false `
  -p:PublishSingleFile=false `
  -o .\artifacts\Gambler.Bot.WinUI-win-x64
```

The app will be published to `artifacts\Gambler.Bot.WinUI-win-x64`.

## First Launch

After startup, the app opens on the Dashboard.

The Dashboard summarizes:

- app and update status,
- active site,
- active strategy,
- runtime status,
- diagnostic insights.

On first launch, no site and no strategy are active.

## Navigation

The left navigation contains the main workspaces:

- `Dashboard`: Command center for status and runtime actions.
- `Sites`: List of gambling sites discovered from the existing Core site classes.
- `Login`: Login preparation for the active site.
- `Strategies`: List of discovered strategy classes.
- `Bet History`: Native view over persisted SQLite bet tables.
- `Intelligence`: Diagnostics and contextual guidance.
- `Settings`: Native UI settings.
- `About`: Version and project information.

## Dashboard

The Dashboard is the central workspace.

Main actions:

- `Start`: starts the prepared runtime only when site and strategy state is valid.
- `Pause`: pauses the runtime state.
- `Stop`: resets the runtime state.
- `Preview Bet`: prepares the next bet from the active site and strategy without placing it.

The runtime now starts a native simulation loop. The Dashboard shows the current mode, loop iteration count, and latest prepared bet.

Safety rule: live automation is intentionally locked behind settings and is not allowed to place real bets in this build.

## Selecting Sites

The Sites page loads site metadata from the existing Core site classes.

Available actions:

- `Select`: selects a site but does not connect it.
- `Use simulation`: activates the site in simulation mode.
- `Refresh`: reloads the site catalog.

Selecting a site is not enough to start the runtime. You must either use simulation mode or complete live login.

## Login And Simulation

The Login page is generated from the active Core site's login metadata.

Field behavior:

- normal fields are shown as text boxes,
- secret fields and MFA codes are shown as password boxes,
- secret values are cleared from the login model after every login attempt.

Available actions:

- `Validate`: checks whether required fields have values.
- `Use simulation`: activates the active site without real login.
- `Live login`: attempts a real Core site login.

## Strategies

The Strategies page loads strategies from `Gambler.Bot.Strategies`.

Currently available:

- strategy catalog,
- active strategy selection,
- native Programmer Mode editor,
- script templates for C#, JavaScript, Lua, and Python,
- script save and basic entry-point validation,
- Dashboard and Intelligence diagnostics.

Programmer Mode scripts are stored under:

```text
%APPDATA%\Gambler.Bot\scripts
```

Preset strategies are read-only in the native editor. Select a Programmer Mode strategy, choose `Open editor`, edit the script, then use `Validate` and `Save`.

Still pending:

- Monaco/WebView2 syntax highlighting,
- preset management,
- advanced script diagnostics.

## Bet Preview

`Preview Bet` prepares the next bet.

Important behavior:

- It does not place a real bet.
- It uses the active site and active strategy.
- It is a safety and integration step before real execution.

## Bet History

Bet History reads existing SQLite tables from `GamblerBot.db` when the database is available.

Currently scanned tables:

- `DiceBets`
- `LimboBets`
- `TwistBets`
- `CrashBets`
- `PlinkoBets`
- `RouletteBets`

Available actions:

- `Refresh`: reloads persisted records.
- `Search history`: filters the visible records by site, game, currency, or outcome.
- `Outcome`: filters visible records by all results, wins, or losses.
- `Export`: writes the currently visible records to a CSV file in your Documents folder.

The summary cards above the table always reflect the currently visible records:

- visible record count,
- wins and losses,
- win rate,
- total amount wagered,
- net profit.

Still pending:

- advanced filtering,
- detailed bet view,
- native charts.

## Intelligence

The Intelligence page combines diagnostic information:

- active site,
- active strategy,
- runtime state,
- settings state,
- history availability,
- migration notes.

It is intended to become the central guidance layer for warnings, recommendations, and contextual help.

## Settings

Native UI settings are stored at:

```text
%APPDATA%\Gambler.Bot\WinUISettings.json
```

The old Avalonia settings area has not been fully replaced yet.

Available WinUI settings:

- native Windows theme preference,
- update prompt preference,
- risk guard,
- session insights,
- automation loop enablement,
- automation loop delay in milliseconds,
- maximum simulation iterations,
- live bet execution gate,
- live bet confirmation phrase,
- default storage provider.

## Live DuckDice Smoke Test

The repository contains a guarded live smoke test for DuckDice. It is intentionally narrow:

- site: `DuckDice`
- currency: `DECOY`
- bet amount: `0.01`
- game: `Dice`
- chance: `49.5`
- bet direction: high

The test does not place a live bet during normal CI or normal local test runs. It only runs when both a local API key and an explicit confirmation variable are present.

Store the API key outside the repository:

```powershell
.\scripts\Set-DuckDiceApiKey.ps1
```

Alternatively, use an environment variable for the current shell:

```powershell
$env:GAMBLER_BOT_DUCKDICE_API_KEY = '<your api key>'
```

To explicitly allow exactly one `0.01 DECOY` live smoke bet, set:

```powershell
.\scripts\Run-DuckDiceLiveSmoke.ps1
```

The local `.secrets/` directory and `*.local.json` files are ignored by Git. Do not paste API keys into tracked settings, documentation, screenshots, logs, or issue reports.

## Safety

Current WinUI safety mechanisms:

- the runtime cannot start without an active site,
- the runtime cannot start without simulation mode or successful live login,
- the runtime cannot start without an active strategy,
- live bet execution remains blocked unless the user explicitly enables the gate and enters the exact confirmation phrase,
- this build still refuses live bet placement even after the gate is armed, because the verified live execution adapter is not complete yet,
- the simulation loop can be limited with a maximum iteration count,
- `Preview Bet` never places a real bet,
- password and MFA fields are hidden,
- secrets are cleared from the login model after login attempts.

Recommendations:

- Test new strategies in simulation mode first.
- Use very small amounts when live execution is eventually enabled.
- Verify site, strategy, and currency before any live run.

## Troubleshooting

### The App Does Not Start

- Make sure the full ZIP was extracted.
- Do not run the app directly from inside the ZIP file.
- Use Windows 10 version 2004 or newer.

### No Sites Are Visible

- Make sure the Core projects were built.
- Check that the Core site assemblies are present in the output folder.

### Runtime Does Not Start

Check:

- Is a site active?
- Is simulation mode active or did live login complete?
- Is a strategy active?

### Bet History Is Empty

- The app searches for `GamblerBot.db`.
- If no old database exists, or if the bet tables are empty, the history view will be empty.

## Known Limitations

Not fully replaced yet:

- old Avalonia UI,
- full bot loop,
- live bet execution with confirmation gate,
- advanced strategy editor diagnostics,
- Monaco/WebView2 programmer mode syntax highlighting,
- charts,
- console,
- roll verifier,
- full settings surface,
- export functions,
- production-ready screenshot documentation.

## Screenshots

Screenshots must be captured from the running WinUI app. This section is prepared but not final yet.

Planned screenshots:

- `images/dashboard.png`
- `images/sites.png`
- `images/login.png`
- `images/strategies.png`
- `images/bet-history.png`
- `images/intelligence.png`
- `images/settings.png`
