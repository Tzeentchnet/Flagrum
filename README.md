# Flagrum

Flagrum is an all-in-one modding tool for Final Fantasy XV. It helps modders, artists, and players browse game assets, convert/export files, manage gameplay mods, and work with Steam Workshop content.

This repository is a community fork of the original Flagrum project by [Kizari](https://github.com/Kizari).


## Features

### Asset Management

Browse and preview game files without extracting them, convert a broad range of FFXV asset formats, and export environments or terrain for further editing.

![Asset Management](.github/images/readme/asset.jpg)

### Gameplay Mods

Install, manage, create, edit, and toggle gameplay mods from one desktop application. Flagrum also includes tooling for packaging and distributing mods.

![Gameplay Mods](.github/images/readme/gameplay.jpg)

### Steam Workshop Mods

View and manage Steam Workshop mods, inspect workshop limits, adjust downloaded outfit stats, and create your own Steam Workshop mods.

![Steam Workshop Mods](.github/images/readme/workshop.jpg)

## Changes From Upstream

This fork keeps the core Flagrum experience intact while modernizing the project and making it easier to build and distribute from this repository.

- Upgraded the solution to the .NET 10 SDK and current project target frameworks.
- Added fork-owned validation and release workflow updates for `Tzeentchnet/Flagrum`.
- Replaced the unavailable private premium package with a local free-mode stub so the solution builds openly.
- Cleaned up fork distribution docs and removed legacy donation-provider branding from the app and repository assets.

## Getting Started

Download the latest release from the [Tzeentchnet/Flagrum releases page](https://github.com/Tzeentchnet/Flagrum/releases/latest).

The [Flagrum Blender companion add-on](https://github.com/Tzeentchnet/Flagrum-Blender/releases/latest) is required for certain asset imports and Steam Workshop mod creation.

Flagrum is a Windows desktop application. Running it on Linux via Proton may still be possible; the original project has notes on the [Linux wiki page](https://github.com/Kizari/Flagrum/wiki/Running-Flagrum-on-Linux).

## Building From Source

Install the .NET 10 SDK, then build the solution from the repository root:

```powershell
dotnet restore Flagrum.slnx
dotnet build Flagrum.slnx -c Debug --no-restore
```

Run the current test project with:

```powershell
dotnet test tests/Flagrum.Test/Flagrum.Test.csproj -c Debug --no-build
```

Release and smoke-test notes are in [docs/distribution.md](docs/distribution.md).

## Documentation

- [Distribution workflow](docs/distribution.md)
- [Code review and dependency analysis](docs/code-review-dependency-analysis-2026-05-17.md)
- [Attribution](ATTRIBUTION.md)

## Support Original Development

Flagrum and the wider FFXV tooling ecosystem represent a large amount of original work. To support the original maintainer, you can [buy Kizari a coffee](https://buymeacoffee.com/Kizari).

<a href="https://buymeacoffee.com/Kizari"><img height="40" src=".github/images/readme/bmc-button.png" alt="Buy Kizari a coffee" /></a>

## Credits

Flagrum was created and primarily developed by [Kizari](https://github.com/Kizari), with contributions from Alex Cup, Rinual, AsteriskAmpersand, ImpatientTraveler, Yretenai, Katelynn Kittaly, Chisa, Sai, and other community members.

Additional acknowledgements and third-party attribution details are maintained in [ATTRIBUTION.md](ATTRIBUTION.md).