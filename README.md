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

## Game database (`LE.SaveEditor.DB`)
Console tool that builds the editor's game data from maxroll's planner data
(`https://assets-ng.maxroll.gg/leplanner/game/data.json`) into the editor's config folder
`%APPDATA%\LE.SaveEditor` (next to `settings.json`):

- `base.db` — SQLite (EF Core) with the tables `Classes`, `Affixes`, `ItemTypes`,
  `ItemCategories`, `Uniques`, `Stats`, `Icons`. Every row keeps its element as JSON (`Json`)
  plus `Key`, `Name` and a SHA-256 `Hash`; `__DataMigrations` records every build/update.
  `Icons` has one row per image (`Group` + `Name`, `Path` = `uniques/spirit_xylem`).
- `Icons/<group>/<name>.png` — from `https://assets-ng.maxroll.gg/last-epoch/webp/<group>/<name>.webp`,
  converted to PNG because WPF cannot decode WebP on its own.

Run it and answer `B` (build from scratch), `U` (update) or `C` (close). Update compares the new data
with the database by key and hash, applies the differences as one data migration (in a transaction) and
re-downloads only new or changed icons. A database made by an older generator version is rebuilt.
Ctrl+C cancels the running step. The editor loads icons through `IIconProvider`.
Pass `--config <folder>` to write somewhere else.
