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

Safety rule: live automation cannot start unless simulation mode is active or live login completed successfully.

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
- Dashboard and Intelligence diagnostics.

Still pending:

- full native strategy editor,
- preset management,
- programmer mode with editor support.

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

Still pending:

- export,
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

## Safety

Current WinUI safety mechanisms:

- the runtime cannot start without an active site,
- the runtime cannot start without simulation mode or successful live login,
- the runtime cannot start without an active strategy,
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
- strategy editor,
- programmer mode,
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
