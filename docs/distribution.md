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

## Signing Setup

Stable releases are signed through Azure Trusted Signing from the `Deploy to Releases` workflow. The workflow uses
GitHub OpenID Connect (OIDC), so it does not use the old `AZURE_CREDENTIALS` client-secret JSON secret.

Before running a signed release, configure the Azure and GitHub repository settings as follows:

1. Create or reuse an Azure app registration/service principal or user-assigned managed identity for GitHub Actions.
2. Add a federated identity credential for the `Tzeentchnet/Flagrum` repository. For the current workflow, the subject
   should target the `main` branch, `repo:Tzeentchnet/Flagrum:ref:refs/heads/main`, with audience
   `api://AzureADTokenExchange`.
3. Assign that identity the Azure Trusted Signing certificate profile signing role at the narrowest supported scope.
4. Confirm that the Trusted Signing endpoint matches the region where the signing account and certificate profile were
   created.
5. Add these GitHub Actions secrets to the repository:

| Secret | Purpose |
|--------|---------|
| `AZURE_CLIENT_ID` | Client ID of the federated Azure identity. |
| `AZURE_TENANT_ID` | Azure tenant ID. |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID. |
| `TRUSTED_SIGNING_ACCOUNT_ENDPOINT` | Region-specific Trusted Signing endpoint URL. |
| `TRUSTED_SIGNING_ACCOUNT_NAME` | Trusted Signing account name. |
| `CERTIFICATE_PROFILE_NAME` | Certificate profile name used for release signing. |

The `AZURE_CREDENTIALS` secret is no longer read by the release workflow.

## Release Modes

The release workflow has a `release_mode` input:

| Mode | Behavior |
|------|----------|
| `signed` | Default. Stable releases must sign successfully, or the workflow fails. |
| `unsigned-draft` | Skips Azure login/signing and creates unsigned draft release assets. |
| `auto` | Attempts signing for stable releases, but falls back to unsigned draft assets if signing is unavailable. |

Prerelease versions, identified by a hyphen in the resolved version, are packaged unsigned regardless of mode. Unsigned
release assets are always uploaded to a draft release and the generated release body includes a warning. Do not publish an
unsigned stable release unless the installer/update trust impact is intentional.

## Distribution

1. Manually trigger the workflow from [GitHub actions](https://github.com/Tzeentchnet/Flagrum/actions). Use `signed` for
   official stable releases, `unsigned-draft` for test packaging, or `auto` when an unsigned draft fallback is acceptable.
   This creates a draft in [Tzeentchnet/Flagrum/releases](https://github.com/Tzeentchnet/Flagrum/releases).
2. Ensure the draft looks correct, then release it.
3. Ensure that the locally installed copy of Flagrum correctly updates to the new version when launched.
4. Remove the previous version of Flagrum from the releases page.

If a draft already exists for the target version, rerunning the workflow updates that draft and replaces its assets. The
workflow will not update an already published release.

## Local Smoke Test

Before triggering the release workflow, the release path can be smoke-tested locally without installing Flagrum:

1. Publish `src/Flagrum/Flagrum.csproj` in `Release` for `win-x64` as a self-contained app.
2. Copy `libs/*` into the publish directory, matching the workflow.
3. Verify the publish directory contains `Flagrum.exe`, `Flagrum.Premium.dll`, `Drautos.dll`, `steam_api64.dll`, `Steamworks.NET.dll`, `Microsoft.Windows.SDK.NET.dll`, and `MicrosoftEdgeWebview2Setup.exe`.
4. Run `vpk pack` from a temporary working directory against that publish directory.
5. Inspect the generated full package for `lib/app/Flagrum.Premium.dll`, `lib/app/Drautos.dll`, and `lib/app/steam_api64.dll`.
6. Start `Flagrum.exe` from the publish directory and confirm it stays running long enough to reach the WPF event loop, then close it.

The `vpk` tool version currently used by the workflow targets .NET 9. If only .NET 10 is installed, set `DOTNET_ROLL_FORWARD=Major` before running `vpk pack` so the tool can run on the installed .NET 10 runtime. This environment variable is set on the workflow packaging steps.

The installer and update flow should still be tested manually. Do not run `Setup.exe` in an automated local smoke test unless you intentionally want to modify the installed application state on that machine.