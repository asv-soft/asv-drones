# Documentation

The published documentation is available at [asv.me/asv-drones](https://asv.me/asv-drones/).
Its sources are maintained alongside the application.

## Source Files

| Path | Purpose |
|---|---|
| `src/Writerside/topics` | Markdown documentation topics. |
| `src/Writerside/images` | Documentation images. |
| `src/Writerside/asv-drones.tree` | Table of contents and start page. |
| `src/Writerside/writerside.cfg` | Writerside module configuration. |
| `.github/workflows/docs.yml` | Build, validation, and Pages publication. |

Create topics in English and register them in `asv-drones.tree`. Link between
topics using their Markdown filenames, for example `getting-started.md`.
Use the `c#` language identifier for C# code blocks.

## Version Display

The documentation workflow reads `ProductVersion` from `src/Directory.Build.props`
and sets the Writerside instance version before building. The published version
therefore follows the application version without a second manually maintained
version in the documentation sources.

Changes to `src/Directory.Build.props` trigger the documentation workflow, including
version-only changes.

## Validation and Publication

The [Publish documentation workflow](https://github.com/asv-soft/asv-drones/actions/workflows/docs.yml)
builds the `Writerside/asv-drones` instance with the Writerside builder and checks
the build report before uploading the website artifact.

- Pull requests build and validate the documentation without deploying it.
- Documentation or version changes pushed to `main` build, validate, and publish
  the website through GitHub Pages.
- A manual run is available from the workflow's Actions page. Only runs on `main`
  deploy the website; runs on other branches validate the sources.

GitHub Pages uses GitHub Actions as its publication source. The documentation
workflow is separate from package and application release workflows.

## Local Review

Open `src/Writerside` in a Writerside-compatible JetBrains IDE to preview topics.
Check the table of contents, images, and local links before opening a pull request.
For the reproducible builder command and current builder image version, use
`.github/workflows/docs.yml` as the source of truth. That command requires Docker.

The CI build is the final check for Writerside inspections; a Markdown preview or
successful application build does not replace it.
