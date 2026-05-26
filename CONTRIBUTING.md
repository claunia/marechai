# Contributing to Marechai

Thank you for helping preserve and present computing history.

This project is a collaborative catalog of computers, consoles, software, magazines, people, companies, and related artifacts. Whether you are fixing a bug, improving the UI, adding metadata, or cleaning up a migration, your contribution helps keep the history accessible and well organized.

## Before you start

Please read [README.md](README.md) first so you understand the project layout, current stack, and development prerequisites.

A working local setup usually includes:

- the .NET 10 SDK
- a MariaDB or MySQL-compatible database
- ImageMagick with HEIF, WebP, AVIF, and JXL support available in PATH
- the Uno Platform tooling needed for the cross-platform app
- a terminal and editor you are comfortable with

The repository uses the solution file [Marechai.slnx](Marechai.slnx), not a legacy `.sln` file.

## Recommended workflow

1. Fork the repository and create a branch for your work.
2. Keep changes focused and easy to review.
3. Update documentation when behavior, setup, or contributor guidance changes.
4. Validate your changes with the relevant build or test commands before opening a pull request.
5. Write a clear pull request description that explains what changed and why.

A simple branch naming pattern is:

- `feature/short-description`
- `fix/short-description`
- `docs/short-description`
- `refactor/short-description`

## Local development checklist

### Restore and build

From the repository root:

```bash
dotnet restore Marechai.slnx
dotnet build -c Debug Marechai.Server/Marechai.Server.csproj
dotnet build -c Debug -f net10.0-desktop Marechai.App/Marechai.App.csproj
```

If you are only working in the web app, the Blazor project can also be built directly:

```bash
dotnet build -c Debug Marechai/Marechai.csproj
```

### Running the app locally

Start the backend API:

```bash
dotnet run --project Marechai.Server/Marechai.Server.csproj --launch-profile http
```

Run the Uno app when needed:

```bash
dotnet run -c Debug -f net10.0-browserwasm --project Marechai.App/Marechai.App.csproj
```

## Project-specific guidance

### Web UI changes

The main web UI uses Blazor Server with MudBlazor.

When changing UI code:

- prefer existing component patterns and shared layout styles
- keep layouts responsive and accessible
- avoid introducing ad hoc styling when a shared pattern already exists
- do not add unnecessary dependencies unless they are clearly justified

### API and client changes

If you change the server contract:

- update the backend DTOs and controllers as needed
- regenerate the API client carefully
- verify that generated files still match the intended contract
- avoid editing generated client code by hand unless there is a documented reason

### Database changes

If you modify the database model or schema:

- update the EF Core model and migrations consistently
- verify that seed data and migration behavior remain safe to apply repeatedly
- keep migrations focused and understandable

### Media and metadata workflows

If you touch media ingestion or image processing:

- ensure the environment supports the expected formats, including JXL
- keep conversion logic predictable and reproducible
- document any new asset requirements or assumptions

## Code and review expectations

- keep pull requests small and easy to review
- explain the problem you are solving, not just the code you changed
- include screenshots or a short description for UI changes when helpful
- avoid mixing unrelated cleanup with feature work
- write commit messages that clearly describe the change

## Good first issues

If you are new to the project, these are usually good places to start:

- small UI polish or layout fixes in the web app
- documentation improvements and README cleanup
- minor bug fixes with a clear repro
- metadata cleanup or consistency fixes
- test or validation improvements around existing features

Look for issues that are clearly scoped, low risk, and easy to verify. If something is unclear, ask for context in the issue or pull request before you start changing things.

## Troubleshooting common setup issues

### Build or restore fails unexpectedly

- verify that you are using the correct SDK version from `global.json`
- make sure the solution is restored from the repository root with `Marechai.slnx`
- confirm that your local environment matches the required tooling for image processing
- if you are working on the app, ensure the Uno Platform prerequisites are installed

### Database connection problems

- confirm the connection string in your configuration files is valid
- verify that the database server is running and reachable
- check that the expected database and user exist before starting the backend

### Image processing issues

- make sure ImageMagick is installed with HEIF, WebP, AVIF, and JXL support
- verify that the executable is available in PATH
- if conversion-related features fail, check whether the system image libraries are present

### Git signing issues

- confirm your key was generated correctly and the fingerprint is configured
- verify the GPG program path is correct on Windows
- if the commit prompt does not appear, check your Git and GPG configuration for the repository or user scope

## Commit signing

All commits must be cryptographically signed.

This protects the repository history and helps maintain trust in the contribution process.

### Windows

1. Install Git for Windows and Gpg4win.
2. Install the Kleopatra component.
3. Generate an OpenPGP key pair in Kleopatra.
4. Save the fingerprint for later.
5. Add Git’s binaries to your PATH if needed:

   ```powershell
   C:\Program Files\Git\usr\bin
   ```

6. Configure Git to sign commits:

   ```bash
   git config --global commit.gpgsign true
   git config --global user.signingkey <FINGERPRINT>
   git config --global gpg.program "C:\Program Files (x86)\GnuPG\bin\gpg.exe"
   ```

   Or, for just this repository:

   ```bash
   cd /DRIVE/PATH_TO_PROJECT
git config commit.gpgsign true
git config user.signingkey <FINGERPRINT>
git config gpg.program "C:\Program Files (x86)\GnuPG\bin\gpg.exe"
   ```

### Linux and macOS

Use your system GPG setup and configure Git similarly:

```bash
git config --global commit.gpgsign true
git config --global user.signingkey <FINGERPRINT>
```

If Git prompts for your passphrase during commit, that is expected.

### GitHub setup

After generating your key, add it to your GitHub account so signed commits are recognized correctly.

See the GitHub documentation for adding a GPG key:

- https://docs.github.com/en/authentication/managing-commit-signature-verification/adding-a-gpg-key-to-your-github-account

## Pull request checklist

Before opening a pull request, make sure:

- the change is scoped and explained clearly
- the build succeeds for the affected projects
- any config, setup, or documentation changes are reflected in the repo
- the PR description includes what changed, why it changed, and how it was validated

A good pull request usually includes:

- a short title
- a summary of the change
- any relevant screenshots or examples
- validation steps or build commands that were run

## Thank you

Thank you for contributing to Marechai. Your work helps keep a valuable part of computing history searchable, understandable, and alive for the next person who wants to explore it.