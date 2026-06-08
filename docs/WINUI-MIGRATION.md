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
4. Replace `AvaloniaEdit` programmer mode with a native editor strategy, likely Monaco in WebView2.
5. Rebuild the bet history as a native WinUI table with filtering and export actions. The native history page now reads persisted SQLite bet tables when `GamblerBot.db` is available.

## New WinUI Services

- `JsonAppSettingsService` stores native UI settings in `%APPDATA%\Gambler.Bot\WinUISettings.json`.
- `VelopackUpdateService` reads current version and update availability without referencing Avalonia.
- `ReflectionSiteCatalogService` discovers enabled Core site classes and exposes native `SiteSummary` models.
- `ReflectionStrategyCatalogService` discovers strategy classes from `Gambler.Bot.Strategies`.
- `SiteSessionService`, `StrategySessionService`, and `AutomationStateService` provide shared native session state.
- `AutomationRuntimeService` validates active site and strategy and instantiates the matching Core/Strategies runtime classes before changing runtime state; it is the handoff point for the real betting loop.
- Live automation remains intentionally blocked until a real login/account state service is extracted; simulation mode can prepare the runtime.
- `LoginPreparationService` reads Core login metadata and powers the native Login page with native text/password controls.
- `LiveLoginService` performs normal Core site login and clears secret/MFA field values after each attempt.
- `BetExecutionService` prepares the next `PlaceBet` from active Core site and strategy without placing it.
- `BetHistoryService` reads persisted SQLite bet tables without depending on Avalonia storage.
- `BetHistoryFilterService` filters loaded history records by text and outcome.
- `BetHistorySummaryService` summarizes visible history records for native dashboard cards.
- `BetHistoryExportService` exports loaded or filtered history records to CSV.
- `InsightService` combines settings, catalog, session, and runtime state into diagnostics.
- `NavigationContext` passes UI-neutral services into pages while the shell is still lightweight.

## Native Pages

- `HomePage`: command center with settings, active site, active strategy, runtime state, and diagnostics summary.
- `SitesPage`: supported site catalog with select and simulation actions.
- `LoginPage`: native login preparation using Core `LoginParameter` metadata, including hidden password/MFA input fields.
- `StrategiesPage`: strategy catalog with active strategy selection.
- `BetHistoryPage`: native history surface with persisted SQLite loading, search/outcome filtering, summary cards, and CSV export.
- `IntelligencePage`: diagnostics derived from the new services.

## Verification

- `dotnet build .\Platforms\Gambler.Bot.WinUI\Gambler.Bot.WinUI.csproj -c Debug` succeeds with 0 warnings and 0 errors.
- `dotnet build .\Gambler.Bot.sln -c Debug` succeeds with 0 warnings and 0 errors.
- `dotnet test .\Platforms\Gambler.Bot.WinUI.Tests\Gambler.Bot.WinUI.Tests.csproj -c Release` succeeds with 28 tests covering runtime safety, session state, settings persistence, insight diagnostics, SQLite bet history reading, history filtering/summaries, and CSV export.
- `dotnet publish .\Platforms\Gambler.Bot.WinUI\Gambler.Bot.WinUI.csproj -c Release -r win-x64 --self-contained true -p:WindowsAppSDKSelfContained=true -p:WindowsPackageType=None -p:PublishTrimmed=false -p:PublishSingleFile=false` succeeds locally.
- `dotnet test .\Gambler.Bot.sln -c Debug --no-build` runs strategy tests successfully, but existing core site integration tests fail because local login parameter JSON is missing and some live seed reset expectations are not satisfied.

## Documentation And Releases

- User-facing documentation starts at `docs/user-guide/README.md`.
- GitHub release packaging is defined in `.github/workflows/winui-release.yml`.
- The release workflow builds, tests, publishes, zips, uploads, and attaches `Gambler.Bot.WinUI-win-x64.zip` to `v*` GitHub releases.
- Screenshots are planned in `docs/user-guide/images/`, but have not been captured from the running native app yet.
