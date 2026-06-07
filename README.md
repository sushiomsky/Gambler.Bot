# Gambler.Bot

Gambler.Bot is a cross-platform betting client for supported gambling sites. It focuses on strategy automation, session tracking, and a structured UI for managing games, bets, and account settings.

## What You Get

- Strategy support for martingale, labouchere, d'Alembert, fibonacci, preset lists, and programmer-mode scripts.
- Game coverage for dice, limbo, and twist, with room for additional game-specific workflows.
- Session tools for bet history, profit charts, reset handling, triggers, and simulator-style testing.
- Database-backed storage with support for SQLite, SQL Server, MySQL, and PostgreSQL.
- A desktop-oriented Avalonia UI with shared code for the additional platform hosts in `Platforms/`.

## Repository Layout

- `Gambler.Bot/` contains the main application shell, view models, assets, and shared UI.
- `Platforms/` contains the platform entry points for desktop, Android, browser, and iOS.
- `ProgrammerMode.md` documents the script-driven betting runtime.
- `scripts/` contains release and hook helpers.
- `.githooks/` contains the repository-managed git hooks.

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

## Contact

- Email: `schnickfitzel1@gmail.com`
- Telegram: `@yzymowep`

## Notes

- The application includes update and platform-specific code paths that still reference the upstream project for runtime behavior.
- This README intentionally avoids external documentation links so the repo stays self-contained.
