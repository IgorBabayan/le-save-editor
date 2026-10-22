# le-save-editor
Save editor for Last Epoch offline characters

## Releases
Every pull request merged into the `release` branch triggers the
**Release - Windows installer** GitHub Action. It publishes a self-contained
win-x64 build, packs it with Inno Setup (`installer/LE.SaveEditor.iss`) and
creates a GitHub Release `vX.Y.<build>` with `LE.SaveEditor-Setup-X.Y.<build>.exe`
attached. The newest one is shown as **Latest** in the repository's Releases panel.

`X.Y` comes from `<VersionPrefix>` in `LE.SaveEditor/LE.SaveEditor.csproj`.
