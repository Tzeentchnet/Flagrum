# Distribution Workflow

Flagrum is distributed from the [Tzeentchnet/Flagrum](https://github.com/Tzeentchnet/Flagrum) GitHub repository using
an automated workflow with a manual trigger.

## Versioning

Flagrum uses a typical 3-part version code, in the format `A.B.C`.

| Part | Description                                                                  |
|------|------------------------------------------------------------------------------|
| A    | Incremented when a full rewrite of the software is performed.                |
| B    | Incremented when a major refactor is performed, or a major feature is added. |
| C    | Incremented when any non-major updates are performed.                        |

## Preparing a New Release

When all commits for a new update have been committed, a new final commit should be prepared as follows:

1. `<BaseVersion>` updated appropriately in `Flagrum.csproj`.
2. New changelog fragment added to `docs/changelog` for this version.

This should then be pushed to source control.

The `Validation` workflow should pass on the release commit before manually triggering the release workflow.

## Distribution

1. Manually trigger the workflow from [GitHub actions](https://github.com/Tzeentchnet/Flagrum/actions).
   This creates a draft in [Tzeentchnet/Flagrum/releases](https://github.com/Tzeentchnet/Flagrum/releases).
2. Ensure the draft looks correct, then release it.
3. Ensure that the locally installed copy of Flagrum correctly updates to the new version when launched.
4. Remove the previous version of Flagrum from the releases page.

## Local Smoke Test

Before triggering the release workflow, the release path can be smoke-tested locally without installing Flagrum:

1. Publish `src/Flagrum/Flagrum.csproj` in `Release` for `win-x64` as a self-contained app.
2. Copy `libs/*` into the publish directory, matching the workflow.
3. Verify the publish directory contains `Flagrum.exe`, `Flagrum.Premium.dll`, `Drautos.dll`, `steam_api64.dll`, `Steamworks.NET.dll`, and `MicrosoftEdgeWebview2Setup.exe`.
4. Run `vpk pack` from a temporary working directory against that publish directory.
5. Inspect the generated full package for `lib/app/Flagrum.Premium.dll`, `lib/app/Drautos.dll`, and `lib/app/steam_api64.dll`.
6. Start `Flagrum.exe` from the publish directory and confirm it stays running long enough to reach the WPF event loop, then close it.

The `vpk` tool version currently used by the workflow targets .NET 9. If only .NET 10 is installed, set `DOTNET_ROLL_FORWARD=Major` before running `vpk pack` so the tool can run on the installed .NET 10 runtime. This environment variable is set on the workflow packaging steps.

The installer and update flow should still be tested manually. Do not run `Setup.exe` in an automated local smoke test unless you intentionally want to modify the installed application state on that machine.