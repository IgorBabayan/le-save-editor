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
`Settings > Updates > Auto-update` (on by default). On every start an installed copy
asks GitHub for the latest release (`/releases/latest`). If its tag is newer than the
running version, the installer asset is downloaded to `%TEMP%\LE.SaveEditor\Updates`,
checked against the size and SHA-256 that GitHub publishes, and run silently with
`/RELAUNCH=1`. The editor exits, the installer replaces the files and starts the new
version. Offline, slow (> 8 s) or failed checks are ignored and the editor starts normally.
Copies that were not installed with the installer (e.g. debug builds) are never updated.
