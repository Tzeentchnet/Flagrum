# Code Review and Dependency/Tooling Analysis

_Generated 17 May 2026. Updated after the .NET 10 migration and release-hardening passes._

## Executive Summary

This report began as a code review and dependency/tooling upgrade analysis. The implementation passes have now moved the repository from the .NET 8 application stack to the .NET 10 LTS stack, with SDK pinning, CI SDK selection, target framework updates, Microsoft package alignment, selected third-party package upgrades, source compatibility fixes, and an initial release-hardening pass.

The upgraded solution now builds, publishes, packages locally with Velopack, starts from the smoke-test publish directory, and passes its currently narrow .NET test project on .NET 10. NuGet vulnerability checks report no vulnerable packages. The former material round-trip failure was a stale test expectation: the material writer normalizes output, so the test now validates read/write/read equivalence and stable canonical rewrite output. A focused `FlagrumDbContext.DoesTableExist` regression test has also been added. The Debug warning count has been reduced from the observed 509-warning baseline to 4 documented legacy EBEX warnings. The npm toolchain audit debt remains untouched and should be handled separately.

## Current Migration Status

| Area | Status |
|---|---|
| SDKs | SDK `8.0.421` and SDK `10.0.300` are installed. The repository is pinned to SDK `10.0.300` via `global.json` with `rollForward: latestFeature`. |
| CI SDK | `.github/workflows/main.yml` and `.github/workflows/validation.yml` now install `10.0.x` with `actions/setup-dotnet@v4`. |
| CI validation | `.github/workflows/validation.yml` runs restore, build, tests, and an enforced NuGet vulnerability gate on pushes to `main`, pull requests, and manual dispatch. npm audit is surfaced as a non-blocking job while the frontend toolchain debt remains open. |
| Target frameworks | Application, library, premium stub, test, and research projects now target `net10.0` or `net10.0-windows`. The source generator remains `netstandard2.0`. |
| Debug build | `dotnet build Flagrum.slnx -c Debug --no-restore --no-incremental` succeeds with warnings. Latest observed count: 4 warnings, all `CS0162` unreachable-code warnings in legacy EBEX scaffolding. |
| Release publish/package smoke | `dotnet publish src/Flagrum/Flagrum.csproj -c Release -r win-x64 --self-contained true` succeeds when published outside the repo for smoke testing. Latest observed count: 456 warnings, with the local `Flagrum.Premium.dll` stub included. Unsigned Velopack packaging also succeeds locally with `DOTNET_ROLL_FORWARD=Major` for the .NET 9-targeted `vpk` tool. |
| Tests | `dotnet test tests/Flagrum.Test/Flagrum.Test.csproj -c Debug --no-build` discovers 2 tests and passes after adding the SQL regression test and retaining the normalized material writer test. |
| NuGet vulnerabilities | `dotnet list Flagrum.slnx package --vulnerable --include-transitive` reports no vulnerable packages for all projects. |
| NuGet outdated | Main app top-level outdated check now reports only deferred HelixToolkit packages. Project-by-project transitive checks still report several transitive packages, mostly through generators/build tooling and SQLite-related dependencies. |
| npm audit | `src/Flagrum.Application` still reports 63 dev dependency vulnerabilities: 1 critical, 10 high, 50 moderate, 2 low. |

## Implemented Changes

### SDK, CI, and Frameworks

* Added `global.json` to pin SDK `10.0.300`.
* Updated the release workflow from .NET `8.0.x` to `10.0.x`.
* Retargeted the release workflow's previous-release download, release publishing owner, and Velopack package author metadata from upstream `Kizari/Flagrum` to the `Tzeentchnet/Flagrum` fork.
* Added a separate `Validation` workflow for restore, build, tests, enforced NuGet vulnerability checks, and non-blocking npm audit visibility.
* Retargeted projects:
  * `src/Flagrum/Flagrum.csproj` to `net10.0-windows`.
  * `src/Flagrum.Application/Flagrum.Application.csproj` to `net10.0`.
  * `src/Flagrum.Components/Flagrum.Components.csproj` to `net10.0`.
  * `src/Flagrum.Core/Flagrum.Core.csproj` to `net10.0`.
  * `src/Flagrum.Abstractions/Flagrum.Abstractions.csproj` to `net10.0`.
  * `tests/Flagrum.Test/Flagrum.Test.csproj` to `net10.0`.
  * `tools/Flagrum.Research/Flagrum.Research.csproj` to `net10.0`.
* Kept `tools/Flagrum.Generators/Flagrum.Generators.csproj` on `netstandard2.0` for analyzer compatibility.

### Package Upgrades

* Aligned Microsoft application packages to the .NET 10 line, including ASP.NET/Blazor packages, EF Core, localization/logging, DI abstractions, and `System.Drawing.Common`.
* Updated `Microsoft.AspNetCore.Components.WebView.Wpf` from the old preview line to `10.0.60`.
* Updated test tooling to `Microsoft.NET.Test.Sdk` `18.5.1`, `xunit` `2.9.3`, `xunit.runner.visualstudio` `3.1.5`, and `coverlet.collector` `10.0.0`.
* Updated Roslyn generator dependencies to `Microsoft.CodeAnalysis.*` `5.3.0`.
* Updated selected runtime packages, including CommunityToolkit, CsvHelper, DirectXTexNet, Injectio, MemoryPack, MessagePack, Newtonsoft.Json, PropertyChanged.SourceGenerator, Serilog packages, K4os LZ4, SQLitePCLRaw SQLCipher bundle, LinqKit, and Velopack.
* Removed explicit `System.Net.Http.Json` because .NET 10 restore reported it as unnecessary.
* Replaced the private `Flagrum.Premium` package/feed dependency with a local no-op stub project that registers free-mode services.

### Source Compatibility Fixes

* Fixed C# 14 `field` keyword conflicts by qualifying the existing EBEX member as `this.field` in `EbexDataItem` and `EbexGraphPinDataItem`.
* Fixed `byte[].Reverse().ToArray()` binding ambiguity in `ImageBinary` by using `Array.Reverse` on the byte array.
* Migrated Velopack startup hooks from `WithFirstRun`/`WithBeforeUninstallFastCallback` to `OnFirstRun`/`OnBeforeUninstallFastCallback`.
* Updated `UpdateHelper` to call `ApplyUpdatesAndRestart(newVersion.TargetFullRelease, Array.Empty<string>())` for Velopack `0.0.1298`.

### Release Hardening and Code Health

* Parameterized `FlagrumDbContext.DoesTableExist` and added a regression test proving table names are treated as values.
* Added a `Flagrum.Application` reference to the test project so application persistence behavior can be tested directly.
* Converted the workshop mod build context, preview/thumbnail processing, and editor save/build paths from non-event `async void` to awaitable `Task` flows.
* Updated workshop editor buttons to use async handlers for save/model selection and made the shared button component restore its disabled state in a `finally` block.
* Converted menu bootstrap cleanup to an awaited `Task` and replaced silent thumbnail-cleanup failure with structured logging.
* Reworked the version timer to dispatch guarded async work instead of using `async void` directly.
* Retargeted runtime update/version checks from `Kizari/Flagrum` to `Tzeentchnet/Flagrum`.
* Cleared the remaining CS4014 unawaited-task warnings from mod manager component flows.
* Replaced the last application call to `ThreadHelper.RunOnNewThread` with an explicit background `Task.Run`.
* Removed remaining `async void` methods from shared components by switching file-picker clicks to async button handlers and adding awaitable modal open/close methods behind existing wrappers.
* Made the WPF dispatcher shutdown call an explicit fire-and-forget operation.
* Removed `TaskExtensions.AwaitSynchronous` and replaced its call sites with synchronous APIs or awaitable call chains.
* Converted packed asset build instructions from a fake async `Task<FmodFragment>` contract to a synchronous `FmodFragment` build contract.
* Reworked mod-pack result selection so modal completion is awaited instead of blocking on `GetAwaiter().GetResult()`.
* Replaced WPF startup's manual thread `.Wait()` bridge with background async startup and fatal startup logging.
* Made generated source-generator infrastructure internal to eliminate duplicate public generated API conflicts across projects.
* Removed the unused Roslyn Workspaces package from the source generator project.
* Cleaned first-party nullable/uninitialized component state, modal refs, callbacks, WPF nullability, platform-service nullability, stale timers, stale preview cache fields, obsolete APIs, and platform analyzer warnings in desktop-only call paths.
* Added scoped vendored-context-menu nullable handling: a local vendor `.editorconfig` for vendored C# files plus minimal nullable/default fixes in vendored Razor components.
* Fixed stream read exactness in Scarlet texture loading, updated AES construction, corrected a 64-bit bit-test range check, and removed simple unused/dead warning sources.

## Validation Commands

| Command | Result |
|---|---|
| `dotnet --list-sdks` | `8.0.421` and `10.0.300` visible. |
| `dotnet restore Flagrum.slnx` | Succeeds after package cleanup. |
| `dotnet build Flagrum.slnx -c Debug --no-restore --no-incremental` | Succeeds with 4 warnings after warning cleanup. Remaining warnings are all documented `CS0162` unreachable-code warnings in legacy EBEX scaffolding. |
| `dotnet test tests/Flagrum.Test/Flagrum.Test.csproj -c Debug --no-build` | Succeeds: 2 tests discovered, 2 passed. |
| `dotnet list Flagrum.slnx package --vulnerable --include-transitive` | No vulnerable packages reported. |
| `dotnet list src/Flagrum/Flagrum.csproj package --outdated` | Top-level outdated packages are `HelixToolkit.SharpDX.Assimp` `2.22.0 -> 3.1.2` and `HelixToolkit.SharpDX.Core.Wpf` `2.22.0 -> 2.27.3`. |
| `dotnet list src/Flagrum/Flagrum.csproj package --outdated --include-transitive` | Restore/build succeeds, then the .NET CLI reports `Sequence contains no matching element`. The top-level command works. |
| `dotnet publish src/Flagrum/Flagrum.csproj -c Release -r win-x64 --self-contained true` | Succeeds in a temp output folder and includes the local `Flagrum.Premium.dll` stub. Latest observed count: 456 warnings. |
| `vpk pack` against the temp publish directory | Succeeds for an unsigned local package when `DOTNET_ROLL_FORWARD=Major` is set. The full package includes `Flagrum.Premium.dll`, `Drautos.dll`, and `steam_api64.dll`. Signing and install/update behavior still need release-pipeline validation. |
| Published `Flagrum.exe` startup probe | Starts from the temp publish directory and remains running for 15 seconds, then is killed by the smoke test. This confirms basic startup reaches the WPF event loop, but not deeper profile, WebView, or editor behavior. |
| `npm audit --json` in `src/Flagrum.Application` | 63 dev dependency vulnerabilities remain. |

## Project Baseline After Upgrade

| Project | Current framework | Role | Notes |
|---|---:|---|---|
| `src/Flagrum/Flagrum.csproj` | `net10.0-windows` | WPF desktop host | Uses Razor SDK, WPF, WebView, Velopack, HelixToolkit, and the local `Flagrum.Premium` stub. |
| `src/Flagrum.Application/Flagrum.Application.csproj` | `net10.0` | Razor/Blazor application library | Contains EF Core, Blazor WebAssembly references, SQLCipher, Steamworks binary reference, and generator analyzer reference. |
| `src/Flagrum.Components/Flagrum.Components.csproj` | `net10.0` | Shared Razor components | Blazor WebAssembly and localization packages aligned to .NET 10. |
| `src/Flagrum.Core/Flagrum.Core.csproj` | `net10.0` | Core serialization, graphics, archive, compression, logging | Allows unsafe blocks and uses binary/graphics-heavy dependencies. |
| `src/Flagrum.Premium/Flagrum.Premium.csproj` | `net10.0` | Open-source premium stub | Produces `Flagrum.Premium.dll` with the expected service extension and registers free-mode services. |
| `src/Flagrum.Abstractions/Flagrum.Abstractions.csproj` | `net10.0` | Shared abstractions | DI abstractions dependency aligned to .NET 10. |
| `tools/Flagrum.Generators/Flagrum.Generators.csproj` | `netstandard2.0` | Roslyn source generators | Roslyn packages updated to `5.3.0`. |
| `tools/Flagrum.Research/Flagrum.Research.csproj` | `net10.0` | Research utility | References Application, Core, and the generator as an analyzer. |
| `tests/Flagrum.Test/Flagrum.Test.csproj` | `net10.0` | xUnit tests | Test SDK, runner, xUnit, and Coverlet updated. The material round-trip and SQL table-name regression tests pass. |

## Resolved Findings in Latest Pass

### P0: Raw SQL Interpolation in Table Existence Check

Status: Fixed. `FlagrumDbContext.DoesTableExist` now uses a command parameter for `tableName`, and `FlagrumDbContextTests.DoesTableExistTreatsTableNameAsValue` verifies that a malicious-looking table name does not match an existing table.

### P0: Fire-and-Forget Async Work in User-Facing Flows

Status: Fixed for the identified non-event `async void` methods and the shared component `async void` helpers found during the final sweep. The source tree no longer reports `async void` matches under `src/**/*.{cs,razor}`. Workshop mod build/image processing, editor save/build, menu cleanup, version timer, file-picker clicks, and modal open/close work now return or dispatch guarded `Task` flows.

### P1: Remaining Unawaited Component Tasks

Status: Fixed for the known build warnings. The mod manager modal, context-menu, conflict-resolution, editor save, and file-index refresh paths now either await async work or explicitly discard fire-and-forget UI refreshes. The latest build output contains no CS4014 warnings.

### P1: Sync-over-Async Helper Encourages Blocking

Status: Fixed for the known source matches. `TaskExtensions.AwaitSynchronous` and the unused `ThreadHelper` wrapper were removed. The remaining call sites were converted to synchronous APIs where the work is inherently synchronous, awaitable modal flows where the caller already has async context, and background async startup for the WPF host. The source tree no longer reports `AwaitSynchronous`, `.Wait()`, `GetAwaiter().GetResult()`, `ThreadHelper`, or `RunOnNewThread` matches under `src/**/*.cs`.

### P1: Compiler Warning Budget Was Too Noisy

Status: Mostly fixed. The observed Debug warning trend is now `509 -> 343 -> 238 -> 74 -> 4` after generator conflict cleanup, nullable/default-state cleanup, vendored context-menu handling, platform analyzer scoping, obsolete API cleanup, and generator package cleanup.

Remaining warnings are intentionally documented rather than fixed in this pass:

* `src/Flagrum.Core/Entities/Type/EbexModuleContainer.cs`: two `CS0162` warnings in legacy EBEX code where old disabled parsing/fallback scaffolding remains after unconditional returns.
* `src/Flagrum.Core/Entities/Type/EbexDataType.cs`: one `CS0162` warning where an older attribute-resolution implementation remains after the active return expression.
* `src/Flagrum.Core/Entities/EbexWriter.cs`: one `CS0162` warning where draft fixid-creation code remains after the current intentional `NotImplementedException`.

Those code paths are legacy/reference scaffolding and should be cleaned in a focused EBEX maintenance pass, ideally with binary fixture coverage. They were left intact to avoid changing behavior or deleting reference logic during warning cleanup.

## Major Findings Still Open

### P1: DbContext Lifetime and Persistence Model Need Review

`AddFlagrumApplicationManual` registers `FlagrumDbContext` with transient lifetime.

Location: `src/Flagrum.Application/Services/DependencyInjection.cs`

Risk: Transient DbContexts can be correct for short operations, but mixed persistence patterns and profile migrations raise upgrade risk.

Recommendation: Document expected unit-of-work boundaries, evaluate scoped/factory registration, and add migration tests against representative profile databases.

### P1: JavaScript Toolchain Has Audit Debt

`src/Flagrum.Application/package.json` still uses Gulp, PostCSS, precss, autoprefixer, and Tailwind 2. The lockfile is npm lockfile version 1.

Risk: `npm audit` still reports 63 dev dependency vulnerabilities, and `precss` is deprecated.

Recommendation: Treat frontend tooling as a separate stage. Replace `precss`, refresh the lockfile with a current npm version, and decide whether to keep Gulp 5 or move to a smaller PostCSS CLI/npm script pipeline. Verify the Gulp output path before changing the pipeline.

### P1: Release Pipeline Validation Is Not Yet Release-Gated

The release workflow publishes, signs, and packages releases without running tests or audits in the packaging job itself. A separate `Validation` workflow now runs restore, build, tests, enforced NuGet vulnerability checks, and a non-blocking npm audit job on pushes, pull requests, and manual dispatch.

Location: `.github/workflows/main.yml`, `.github/workflows/validation.yml`

Risk: Upgrade regressions can still reach release packaging if the validation workflow is ignored or not required before manual release dispatch.

Recommendation: Require the `Validation` workflow before release, either through branch protection/manual release discipline or by adding an explicit validation/preflight job to the release workflow. Keep npm audit non-blocking until the frontend toolchain debt is resolved.

### P2: Process and URI Launching Should Be Centralized

Several components call `Process.Start` directly with `UseShellExecute = true` or launch file paths/URIs from UI flows.

Risk: Repeated launch code makes validation inconsistent.

Recommendation: Add a centralized launch service for safe URI/file opening. Keep game-launch behavior separate with explicit executable path validation.

### P2: Error Handling Is Inconsistent

Some code paths swallow exceptions or only recover silently, especially around cleanup/profile-style operations. The workshop thumbnail cleanup path now logs failures, but other areas still need review.

Risk: Silent failure makes upgrade regressions hard to triage.

Recommendation: Replace swallowed exceptions with structured logging and explicit recovery behavior.

### P2: Test Coverage Is Too Narrow for Platform Migration

The test project now covers the material round-trip behavior and the SQL table-name regression, but it is still far too narrow for the platform migration.

Risk: The highest-risk areas for a .NET 10 upgrade are not covered: EF/migrations, profile databases, generated DI, app startup, WebView rendering, workshop builds, and release packaging.

Recommendation: Expand tests before release. Start with SQL helper behavior, serialization/binary round-trips, migration smoke tests, and generator output compilation.

## Dependency Notes After Upgrade

### Microsoft/.NET Stack

| Area | Current after implementation | Notes |
|---|---:|---|
| Target framework | `net10.0`, `net10.0-windows` | Main platform move is complete at project-file level. |
| SDK | `10.0.300` | Pinned in `global.json`; SDK 8 remains installed for comparison. |
| `Microsoft.AspNetCore.Components.WebAssembly` | `10.0.8` | Aligned to .NET 10. |
| `Microsoft.AspNetCore.Components.WebAssembly.DevServer` | `10.0.8` | PrivateAssets package aligned to .NET 10. |
| `Microsoft.AspNetCore.Components.WebView.Wpf` | `10.0.60` | Replaces old preview WebView package. |
| EF Core SQLite/Design | `10.0.8` | Needs profile database and migration smoke tests. |
| `Microsoft.Extensions.*` packages | `10.0.8` where direct | Some transitive dependencies still resolve to `10.0.0` through third-party packages. |
| `System.Drawing.Common` | `10.0.8` | Keep Windows-only usage validated. |
| `System.Net.Http.Json` | Removed as explicit reference | Restore reported the explicit reference was unnecessary under .NET 10. |

### Non-Microsoft NuGet Packages

| Package | Current after implementation | Notes |
|---|---:|---|
| `Blazored.TextEditor` | `1.0.8` | `dotnet list package --outdated` reports `Not found at the sources`. Keep until editor replacement/verification is planned. |
| `CommunityToolkit.Mvvm` | `8.4.2` | Updated. |
| `CommunityToolkit.HighPerformance` | `8.4.2` | Updated. |
| `CsvHelper` | `33.1.0` | Updated. |
| `DirectXTexNet` | `1.0.7` | Updated. |
| `FontAwesome5` | `2.1.11` | Updated. |
| `HelixToolkit.SharpDX.Assimp` | `2.22.0` | Deferred; latest top-level is `3.1.2`, likely requires viewer/model validation. |
| `HelixToolkit.SharpDX.Core.Wpf` | `2.22.0` | Deferred; latest top-level is `2.27.3`. Keep paired with Assimp compatibility decision. |
| `Injectio` | `6.1.0` | Updated and build validated. |
| `K4os.Compression.LZ4.Streams` | `1.3.8` | Updated. |
| `LinqKit` | `1.3.11` | Updated in research tool. |
| `MemoryPack` | `1.21.4` | Updated; persisted format compatibility still needs tests. |
| `MessagePack` | `3.1.4` | Updated; serialization compatibility still needs tests. |
| `Newtonsoft.Json` | `13.0.4` | Updated. |
| `PropertyChanged.SourceGenerator` | `1.1.2` | Updated. |
| `Serilog.Extensions.Logging` | `10.0.0` | Updated. |
| `Serilog.Sinks.File` | `7.0.0` | Updated; log output should be smoke tested. |
| `SQLitePCLRaw.bundle_e_sqlcipher` | `2.1.11` | Updated. Validate encrypted configuration/profile database opening. |
| `Velopack` | `0.0.1298` | Updated; compile-time API migrations completed. Pack/update workflow still needs release validation. |
| `xunit`, runner, test SDK, Coverlet | `2.9.3`, `3.1.5`, `18.5.1`, `10.0.0` | Updated; test discovery and coverage work. |

Packages already effectively pinned or unchanged include `boilerplatezero`, `Drautos`, `Ionic.Zlib.Core`, `MarkdownSharp`, and `Microsoft-WindowsAPICodePack-Shell`. `Flagrum.Premium` is now a local stub rather than a private NuGet package.

### Remaining Transitive Outdated Packages

Project-by-project `--include-transitive` checks still report several transitive packages. These are not currently vulnerable, but they are useful cleanup targets after functional validation:

* Generator/build chain packages such as `Humanizer.Core`, `Microsoft.Build.Framework`, `Microsoft.NET.StringTools`, `Microsoft.CodeAnalysis.*`, and `Microsoft.CodeAnalysis.Analyzers` through consuming packages.
* `System.Composition.*`, `System.CodeDom`, `System.Configuration.ConfigurationManager`, `System.Security.*`, and related transitive packages.
* `SQLitePCLRaw.*` e_sqlite3 transitive packages at `2.1.11` where the latest is `3.0.3`.
* `Serilog` transitive package at `4.2.0` where latest is `4.3.1`.
* `xunit.analyzers` transitive package at `1.18.0` where latest is `1.27.0`.

## Recommended Next Steps

1. Require or manually verify the `Validation` workflow before release dispatch, and decide whether the release workflow should include an explicit validation/preflight job.
2. Run manual interactive smoke tests for WebView rendering, profile database open/migration, configuration persistence, and the mod manager/editor flows.
3. Validate release packaging end to end beyond the local smoke: signed Velopack packaging, clean install, and update from the previous stable release.
4. Continue the remaining P1/P2 code-health work: DbContext lifetime, silent exception handling, and centralized process/URI launching.
5. Modernize npm tooling as a separate change: verify CSS output path, replace deprecated `precss`, refresh the lockfile, and then handle Tailwind/Gulp migration with visual checks.
6. Defer HelixToolkit major/minor upgrades until model/viewer behavior can be tested, because this is a graphics/UI runtime risk rather than a simple package hygiene update.

## Open Questions

* Are representative old profile databases available for migration testing?
* Should Flagrum adopt central package management (`Directory.Packages.props`) now that most direct versions have been aligned?
* Is the Gulp destination `../Flagrum.Desktop/wwwroot` still used by a local-only project name, or should it become `../Flagrum/wwwroot`?
* Which workshop build and game-launch scenarios should be release-blocking smoke tests?

## Final Recommendation

The .NET 10 code and package migration is far enough along to build, publish, pass the current .NET test project, and run local unsigned Velopack/startup smoke checks, but it should not be treated as release-ready yet. The next milestone should be interactive release validation: require or manually verify validation before release dispatch, smoke test profile/WebView/editor behavior, and validate signed Velopack install/update paths. After that, tackle the remaining code-health risks and npm audit debt in focused follow-up changes.