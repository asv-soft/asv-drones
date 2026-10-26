# Development

Read `AGENTS.md` and inspect your working tree before making changes. Preserve
unrelated changes and keep edits focused on the feature or fix you are working on.

## Project Structure

| Project | Responsibility |
|---|---|
| `Asv.Drones` | Shared application services and user interface. |
| `Asv.Drones.Api` | Public application contracts used by plugins. |
| `Asv.Drones.Desktop` | Desktop application entry point. |
| `Asv.Drones.Launcher` | Separate launcher application. |
| `Asv.Drones.Android` | Android platform project. |
| `Asv.Drones.iOS` | iOS platform project. |

The solution is `src/Asv.Drones.slnx`. Shared settings are declared in
`src/Directory.Build.props`.

## Versions and Dependencies

`ProductVersion` supplies the application and default NuGet package version.
The API project can use an explicitly supplied `ApiVersion` when packaging.
Asv and Avalonia dependency versions are also defined in `src/Directory.Build.props`.
Restore packages after changing those values.

The repository produces the `Asv.Drones` and `Asv.Drones.Api` NuGet packages.
The release workflows publish packages from version tags; ordinary documentation
builds do not publish NuGet packages or create application releases.

## Validate Desktop Changes

Run from the repository root:

```bash
dotnet restore src/Asv.Drones.Desktop/Asv.Drones.Desktop.csproj
dotnet build src/Asv.Drones.Desktop/Asv.Drones.Desktop.csproj -c Release --no-restore -p:GeneratePackageOnBuild=false
dotnet restore src/Asv.Drones.Launcher/Asv.Drones.Launcher.csproj
dotnet build src/Asv.Drones.Launcher/Asv.Drones.Launcher.csproj -c Release --no-restore -p:GeneratePackageOnBuild=false
```

The `Build and tests` workflow runs desktop and launcher checks on GitHub Actions.
There is currently no automated test project in this repository. Its test discovery
step reports that state explicitly and runs test projects when they are present.
A successful build alone does not verify device communication or user interaction.

## Coding and Documentation

Follow nearby code and the repository analyzers. Use clear English names for
public types and members. Keep application services, UI, and protocol concerns
separate. Write comments and documentation in English, and use comments to
explain intent or constraints rather than repeat the code.

Documentation changes are described in [Documentation](documentation.md).
