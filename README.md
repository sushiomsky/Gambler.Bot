# Gambler.Bot

Gambler.Bot is a cross-platform betting client for supported gambling sites. It focuses on strategy automation, session tracking, and a structured UI for managing games, bets, and account settings.

## What You Get

- Strategy support for martingale, labouchere, d'Alembert, fibonacci, preset lists, and programmer-mode scripts.
- Game coverage for dice, limbo, and twist, with room for additional game-specific workflows.
- Session tools for bet history, profit charts, reset handling, triggers, and simulator-style testing.
- Database-backed storage with support for SQLite, SQL Server, MySQL, and PostgreSQL.
- A desktop-oriented Avalonia UI with shared code for the additional platform hosts in `Platforms/`.
- A new native Windows WinUI 3 client is being built in `Platforms/Gambler.Bot.WinUI`.

## Repository Layout

- `Gambler.Bot/` contains the main application shell, view models, assets, and shared UI.
- `Platforms/` contains the platform entry points for desktop, Android, browser, and iOS.
- `Platforms/Gambler.Bot.WinUI/` contains the new native Windows client.
- `Platforms/Gambler.Bot.WinUI.Core/` exposes the UI-neutral WinUI migration services for tests.
- `Platforms/Gambler.Bot.WinUI.Tests/` contains fast unit tests for the new native client services.
- `docs/user-guide/` contains the English user guide for the WinUI client.
- `ProgrammerMode.md` documents the script-driven betting runtime.
- `scripts/` contains release and hook helpers.
- `.githooks/` contains the repository-managed git hooks.

## Native Windows Client

The WinUI client is the planned replacement for the Avalonia UI on Windows. It currently provides the native shell, site selection, login preparation, strategy selection, bet history loading, intelligence diagnostics, runtime state handling, and release packaging.

Build and test it with:

```powershell
dotnet build .\Platforms\Gambler.Bot.WinUI\Gambler.Bot.WinUI.csproj -c Release
dotnet test .\Platforms\Gambler.Bot.WinUI.Tests\Gambler.Bot.WinUI.Tests.csproj -c Release
```

User documentation starts at `docs/user-guide/README.md`.

## Versioning And Releases

Release automation is handled through repository hooks and the central `Directory.Build.props` file.

1. Install the repo hooks with:

```powershell
pwsh -File scripts/install-hooks.ps1
```

2. Create a release commit with one of these messages:

```text
release
release patch
release minor
release major
release 1.2.3
```

3. The hook updates `Directory.Build.props`, stages the version change, tags the commit, pushes the branch and tags, and creates a GitHub release when a token is available.

If no GitHub token is present, the tag still gets pushed and the hook reports that the release creation step was skipped.

WinUI release binaries are produced by `.github/workflows/winui-release.yml`. Tags matching `v*` attach `Gambler.Bot.WinUI-win-x64.zip` to the GitHub release.

## Contact

- Email: `schnickfitzel1@gmail.com`
- Telegram: `@yzymowep`

## Notes

- The application includes update and platform-specific code paths that still reference the upstream project for runtime behavior.
- This README intentionally avoids external documentation links so the repo stays self-contained.
