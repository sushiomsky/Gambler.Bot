# Release Workflow

This repository uses git hooks to keep versioning and release creation consistent.

## Setup

Run the hook installer once per clone:

```powershell
pwsh -File scripts/install-hooks.ps1
```

That command sets `core.hooksPath` to `.githooks`, so git will use the tracked hooks in this repo.

## Release Triggers

Commit messages beginning with `release` activate the versioning workflow.

Examples:

```text
release
release patch
release minor
release major
release 1.2.3
```

## What The Hooks Do

- `prepare-commit-msg` bumps `VersionPrefix` in `Directory.Build.props`.
- The updated version file is staged automatically.
- `post-commit` creates an annotated tag using the new version.
- The hook pushes the branch and tags to `origin`.
- If `GITHUB_TOKEN` or `GH_TOKEN` is available, the hook creates a GitHub release through the GitHub API.

## Version Source

The repository uses the root `Directory.Build.props` file as the single source of truth for the version number. The app reads that version at runtime, and the release hook updates the same value before the commit is finalized.

## Contact

- Email: `schnickfitzel1@gmail.com`
- Telegram: `@yzymowep`
