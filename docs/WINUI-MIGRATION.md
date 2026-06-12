# WinUI Migration

This repo now has a native Windows shell at `Platforms/Gambler.Bot.WinUI`.

The UI-neutral WinUI migration layer is testable through `Platforms/Gambler.Bot.WinUI.Core`, and service tests live in `Platforms/Gambler.Bot.WinUI.Tests`.

## Direction

- Keep the existing Avalonia app available while the new Windows app is built.
- Move domain logic behind UI-neutral services before wiring it into WinUI.
- Prefer WinUI 3 controls, Windows App SDK packaging, and native Windows theme behavior.
- Replace Avalonia-only helpers with bindings, converters, or small WinUI controls.

## First Migration Targets

1. Extract settings and update checks from the Avalonia `App` and `MainViewModel` into UI-neutral services. Done for the WinUI client via `IAppSettingsService` and `IUpdateService`.
2. Port the site selection and login state into the new `Sites` workspace. Site catalog discovery is now wired through `ISiteCatalogService`; login state is next.
3. Port session metrics into the dashboard cards and activity feed. Native site, strategy, and runtime session state are now shared with the dashboard.
4. Replace `AvaloniaEdit` programmer mode with a native editor strategy. A first WinUI editor now supports app-data script documents, templates, save, and validation; Monaco/WebView2 syntax highlighting remains planned.
5. Rebuild the bet history as a native WinUI table with filtering and export actions. The native history page now reads persisted SQLite bet tables when `GamblerBot.db` is available.

## New WinUI Services

- `JsonAppSettingsService` stores native UI settings in `%APPDATA%\Gambler.Bot\WinUISettings.json`.
- `SettingsValidationService` normalizes defaults, live safety limits, stop-loss/take-profit values, automation ranges, and diagnostics retention values before settings are persisted.
- `VelopackUpdateService` reads current version and update availability without referencing Avalonia.
- `VelopackUpdateService` checks `https://github.com/sushiomsky/Gambler.Bot` for WinUI releases.
- `ReflectionSiteCatalogService` discovers enabled Core site classes and exposes native `SiteSummary` models.
- `ReflectionStrategyCatalogService` discovers strategy classes from `Gambler.Bot.Strategies`.
- `StrategyScriptService` creates, loads, saves, and validates Programmer Mode script documents under `%APPDATA%\Gambler.Bot\scripts`.
- `SiteSessionService`, `StrategySessionService`, and `AutomationStateService` provide shared native session state.
- `AutomationRuntimeService` validates active site and strategy, instantiates the matching Core/Strategies runtime classes, and runs cancellable simulation or guarded live loops with iteration telemetry and cumulative profit stops.
- Live automation loops are disabled by default and require explicit settings, an exact confirmation phrase, the live-bets-per-run limit, and stop-loss/take-profit limits.
- `LoginPreparationService` reads Core login metadata and powers the native Login page with native text/password controls.
- `LiveLoginService` performs normal Core site login, applies default currency settings, retains the logged-in Core site instance, and clears secret/MFA field values after each attempt.
- `BetExecutionService` prepares the next `PlaceBet` from active Core site and strategy, and can place one guarded live bet after live login when safety settings pass.
- `BetHistoryService` reads persisted SQLite bet tables without depending on Avalonia storage, including optional server seed, client seed, and nonce columns when present.
- `BetHistoryFilterService` filters loaded history records by text, outcome, verifier seed data, exact currency, profit range, and verifier-ready status.
- `BetHistorySummaryService` summarizes visible history records for native dashboard cards.
- `BetChartService` creates native cumulative profit chart snapshots for filtered history records.
- `BetHistoryExportService` exports loaded or filtered history records to CSV, including verifier seed columns.
- `ConsoleLogService` stores the latest diagnostic console entries for the native Console page and persists retained entries to `%APPDATA%\Gambler.Bot\ConsoleLog.jsonl`.
- `RollVerifierService` verifies provably-fair rolls by delegating to Core site `GetLucky` and `GetHash` implementations, with Bet History able to prefill verifier inputs when historical seed data is available.
- `InsightService` combines settings, catalog, session, and runtime state into diagnostics.
- `NavigationContext` passes UI-neutral services into pages while the shell is still lightweight.

## Native Pages

- `HomePage`: command center with settings, active site, active strategy, runtime state, simulation loop iterations, and diagnostics summary.
- `SitesPage`: supported site catalog with select and simulation actions.
- `LoginPage`: native login preparation using Core `LoginParameter` metadata, including hidden password/MFA input fields.
- `StrategiesPage`: strategy catalog with active strategy selection and native Programmer Mode script editing.
- `BetHistoryPage`: native history surface with persisted SQLite loading, advanced filtering, summary cards, profit sparkline chart, verifier prefill, and CSV export.
- `ConsolePage`: native diagnostic console with operator commands for status, site, strategy, and runtime state.
- `RollVerifierPage`: native provably-fair verifier for site/game/seed/nonce inputs, including navigation prefill from Bet History.
- `IntelligencePage`: diagnostics derived from the new services.

## Verification

- `dotnet build .\Platforms\Gambler.Bot.WinUI\Gambler.Bot.WinUI.csproj -c Debug` succeeds with 0 warnings and 0 errors.
- `dotnet build .\Gambler.Bot.sln -c Debug` succeeds with 0 warnings and 0 errors.
- `dotnet test .\Platforms\Gambler.Bot.WinUI.Tests\Gambler.Bot.WinUI.Tests.csproj -c Release` succeeds with 67 tests covering runtime safety, simulation loop execution, guarded live loop execution, live loop stop-loss/take-profit exits, guarded single live bet execution, live bet gating, guarded DuckDice live smoke test behavior, Programmer Mode script documents, roll verification, settings validation, persisted console logging, chart snapshots, session state, settings persistence, update URL configuration, insight diagnostics, SQLite bet history reading with verifier fields, advanced history filtering/summaries, and CSV export.
- `dotnet publish .\Platforms\Gambler.Bot.WinUI\Gambler.Bot.WinUI.csproj -c Release -r win-x64 --self-contained true -p:WindowsAppSDKSelfContained=true -p:WindowsPackageType=None -p:PublishTrimmed=false -p:PublishSingleFile=false` succeeds locally.
- `dotnet test .\Gambler.Bot.sln -c Debug --no-build` runs strategy tests successfully, but existing core site integration tests fail because local login parameter JSON is missing and some live seed reset expectations are not satisfied.

## Documentation And Releases

- User-facing documentation starts at `docs/user-guide/README.md`.
- GitHub release packaging is defined in `.github/workflows/winui-release.yml`.
- The release workflow builds, tests, publishes, zips, creates Velopack packages, uploads artifacts, and attaches both `Gambler.Bot.WinUI-win-x64.zip` and Velopack update assets to `v*` GitHub releases.
- Screenshots are planned in `docs/user-guide/images/`, but have not been captured from the running native app yet.
