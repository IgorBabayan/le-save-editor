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

## Saving
The save button in the title bar (or `Ctrl+S`) writes the changes of the selected character —
equipped items and the backpack (edited, moved or removed items) — into its `1CHARACTERSLOT_*` file.
It is enabled while the character has changes; changes of several characters are kept while switching
between them, and closing the editor or reloading the save folder asks before discarding them.

- Only the changed entries of `savedItems` are rewritten. Every other field, unknown properties and the
  entry order stay as the game wrote them, in the game's layout (`EPOCH` marker, indentation, line endings).
- Rolls of an item keep their stored byte while their value is unchanged, so an item applied again
  without edits is written back byte for byte; items the editor cannot show are never touched.
- The file is written to a temporary file that then replaces the save; the previous file is kept next to it
  as `<save>.<yyyyMMdd-HHmmss>.bak` (the newest 5 are kept, and `.bak` files are never listed as characters).
- Saving is refused when the file changed after it was loaded (e.g. Last Epoch saved it): close the game,
  reload the folder and make the changes again.

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

`--run build|update` runs one operation without the menu and exits (exit code 0 = succeeded,
1 = failed, 2 = cancelled). `--machine` prints JSON lines instead of console text: `bar-start` /
`bar` / `bar-end` for every progress bar, `log` for every text line and a final `done` with the status.

The editor ships the generator in `Tools\LE.SaveEditor.DB` (local builds copy it there, the release
workflow publishes it there). The database button in the title bar (left of the notification bell)
asks for confirmation, then runs `LE.SaveEditor.DB --run update --machine` hidden — update builds a
new database when none exists — and shows the same progress bars in a dialog that cannot be closed
until the generator has exited. After OK, the generator's result lines appear in the notification center.
