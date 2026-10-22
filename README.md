# le-save-editor
Save editor for Last Epoch offline characters

## Releases
Every pull request merged into the `release` branch triggers the
**Release - Windows installer** GitHub Action. It publishes a self-contained
win-x64 build, packs it with Inno Setup (`installer/LE.SaveEditor.iss`) and
creates a GitHub Release `vX.Y.<build>` with `LE.SaveEditor-Setup-X.Y.<build>.exe`
attached. The newest one is shown as **Latest** in the repository's Releases panel.

`X.Y` comes from `<VersionPrefix>` in `LE.SaveEditor/LE.SaveEditor.csproj`.

## Auto-update
`Settings > Updates > Auto-update` (on by default). After the main window opens, an installed
copy asks GitHub in the background for the latest release (`/releases/latest`). If its tag is
newer than the running version, the installer asset is downloaded to `%TEMP%\LE.SaveEditor\Updates`
(a toast shows the progress and can cancel it) and checked against the size and SHA-256 that
GitHub publishes. A second toast then counts down `Seconds before auto-restart` (default 5,
0 restarts immediately, the countdown can be cancelled), after which the installer runs silently
with `/RELAUNCH=1`: the editor exits, the installer replaces the files and starts the new version.
Offline, slow (> 15 s) or failed startup checks are ignored.

`Check for updates` in Settings checks on demand and reports the result as a toast;
`Download and update` becomes available once a newer version was found and runs the same
download / countdown / install sequence.
Copies that were not installed with the installer (e.g. debug builds) are never updated.
