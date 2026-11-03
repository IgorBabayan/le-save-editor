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

## Inventory
- Click an equipment slot, a backpack item or an idol to edit it in the item inspector; drag backpack items and idols
  to move them inside their grid.
- Right-click an empty backpack cell to build a new item there (equipment or an idol), or an empty cell of the idol
  grid to build a new idol. The idol grid is 5 × 5 without its corners; idols stored over a corner are left untouched.
- Crafting materials: the "Crafting materials" box shows the material storage of the character's stash, like the
  game's Crafting Materials panel (shards, runes, glyphs). Each kind has its own "Edit" action: a dialog lists the
  materials of that kind with their amounts; change an amount, remove a material (or set it to 0), or add one picked
  from the game database. The changes are written to the stash file when the character is saved (the stash file is
  backed up like character saves). Characters that share a stash (e.g. softcore characters of a cycle) share the
  changes. The stash of a character is found by name: `STASH_CYCLE_<cycle>_<type>` (type 0 = softcore,
  1 = hardcore) or `STASH_CYCLE_<cycle>_2_<character id>` for a solo challenge character; legacy characters use
  `STASH_<type>` (`STASH_2_<id>`). Affix shards are stored in `savedShards` (`{"shardType": affixId, "quantity": n}`),
  runes and glyphs in `materialsList` (an item `[2, 102 | 103, subTypeID]` with a quantity).
- Resources: the "Resources" section below the inventory shows the keys and consumables of the character's stash,
  laid out like the game's Resources tab — keys (Harbinger Eye; Temporal Sanctum, Lightless Arbor and Soulfire Bastion
  keys; Arena Key of Memory and Arena Key) and consumables (Portal and Scale charms; Crystallized Heart and the
  primordial Horn, Fang, Feather and Petal; Temporal Keystones and Timeglass Fragments). Type an amount into a slot
  (0 removes the resource); empty slots are dimmed. Resources the game's tab does not show (lizard tails, tokens, ...)
  are listed under "Other resources" while the stash holds some, and "Edit all…" edits every resource, or adds any
  resource of the game database, in the material storage dialog. Resources are stored in the stash's `keysList` as
  the game stores them: an item in format 6 (`[6, id, id, id, id, 104, subTypeID]`) in container 100 at position
  x 0, y = subTypeID, with a quantity. They are written with the other stash changes when the character is saved;
  resource entries in another shape (written by older editor versions) are dropped by the game, so the editor
  rewrites them into this shape when it saves the stash.
- Runes and glyphs lying in the backpack: click one, or right-click it and choose "Edit…", to turn it into another
  material of the same kind; "Remove" deletes it on the next save. They take one cell and can be dragged like other
  items. Their names, descriptions and icons come from the game database; materials it does not know are shown as
  "Rune" or "Glyph" placeholders and stay in the save untouched. A rune or glyph item is stored as
  `[2, baseTypeID, subTypeID]` (rune = 102, glyph = 103). Resources (keys, charms, ... — `baseTypeID` 104) lying in
  the backpack are shown and changed the same way.
- Blessings: the "Blessings" section (after "Woven echoes") lists every Monolith timeline echo with the blessing the
  character holds for it. Select an echo to see the blessings its timeline grants — normal ones, then the Grand ones of
  the empowered timeline — with their stat ranges; pick one to put it in the slot at its best roll and adjust the roll
  with the slider, or "Remove" it. A blessing is a plain item of type 34 (`[2, 34, subTypeID, 0, 0, roll, roll, roll,
  0, 0, 0]`) in `savedItems` container 32 + timelineID (33 = Fall of the Outcasts … 42 = Spirits of Fire). The
  timelines and their blessings come from the `Timelines` table of the game database; a database made before it shows
  a hint to update it, and the blessings in the save stay untouched. Blessings of another timeline, or unknown to the
  database, stay in their slot until another one is picked.
  The game shows a blessing as "Undiscovered" unless the character's stash lists it in `unlockedBlessings`
  (blessing subTypeIDs): saving the character adds every blessing it holds that the stash does not list yet, and the
  section warns about such blessings until then.
- Remove an item with the trash button in the inspector ("Remove item" in the full builder), or right-click an
  equipped item, a backpack item or an idol and choose "Remove". It is deleted from the save on the next save.
- The corruption slot offers the corruption-only affixes first, then normal affixes of the item type. The corruption
  is stored the way the game stores it (corrupted item flag, corruption affix marked in the affix count), so it shows
  as corrupted in the game, also when it is a normal affix like Cold Penetration. Adding a corruption to a unique
  makes it a legendary, as the Rune of Corruption does. Items corrupted in the game without a new affix stay corrupted.
- Items saved by older editor versions stored a normal-affix corruption as one more prefix / suffix (the game shows
  it as such). A third prefix or suffix is shown in the corruption slot; applying the item again stores it as a real
  corruption.
- Affix values are shown as the game shows them on the item: the game scales every affix by the item type
  (e.g. Increased Bow Attack Speed is 75% higher on a bow, Health 17% higher on an amulet, unless the affix is
  written for that type), so tiers and ranges in the builder and tooltip match the game.
- Rarity counts only prefixes and suffixes: a sealed affix and the corruption do not make an item rare. The tooltip
  shows the sealed affix under "Sealed prefix" or "Sealed suffix".
- Idols are Normal, Magic (one prefix and one suffix from the idol affixes) or Unique. Idol bases and unique idols
  also take a corruption (a corrupted unique idol becomes a legendary, like any corrupted unique): the corrupted idol affixes of that idol type are offered first, then its normal idol affixes; it is
  stored like an equipment corruption (corrupted item flag, corruption marked in the affix count) and does not change
  the rarity. Most corrupted idol affixes have a wrong display name in the game data (e.g. "Added Throwing Damage and
  reduced Melee Damage" for Chill Immunity), so they are named after their internal name ("Chill Immunity"). Weaver idols (Minor, Small, Humble, Stout) and Heretical idols (Grand, Large, Ornate, Huge, Adorned) are
  offered in the builder although the game data marks them as not dropping. Weaver idols also roll the Weaver affixes
  (e.g. "Vitality and Added Health Regen", "Physical Resistance and Minion Physical Resistance"), listed first in
  their prefix and suffix pickers. Heretical idols (Grand, Large, Ornate, Huge and Adorned) take one enchantment
  from the idol enchantments besides their prefix and suffix; it is shown in blue like in the game, does not change
  the rarity and is stored among the affixes (the game tells it apart by the affix). Unique idols take no enchantment.
- Added percentages are shown with the decimals the game uses for the property (its `roundingForAdded`), so e.g.
  1.79% All Resistances on a Humble idol reads "+2%" as in the game.
- Idol altars: the "Idol altar" slot left of the idol grid holds the character's altar (container 123). Click it to
  build or change the altar in the item inspector, right-click it to edit or remove it. The idol grid takes the shape
  of the altar at once: its blocked cells, and its refracted slots highlighted (idols over a refracted slot get a
  refracted frame); with an altar every slot of its shape is open, without one the grid is the standard 5 × 5 without
  corners and centre with the quest locks. Idols that no longer fit move to a free slot, then to the backpack; an
  idol that fits nowhere is left in the save where it was. Altar layouts are the ones maxroll's planner uses (the game
  data has none). An altar is always corrupted (it is saved with the corrupted item flag): it takes two prefixes and two
  suffixes from the altar affixes and a corruption (the corrupted altar affixes are offered first, then the normal
  altar affixes, e.g. Increased Effect of Suffixes for Idols in Refracted Slots); altars are 2 × 2 and can also be
  built in the backpack.
- Opening the item list in the inspector puts the cursor into its search box. In the backpack, a weapon search also
  finds off-hands (shields, quivers, catalysts).

## Saving
The save button in the title bar (or `Ctrl+S`) writes the changes of the selected character —
equipped items, idols and the backpack (edited, moved, added or removed items) — into its `1CHARACTERSLOT_*` file.
It is enabled while the character has changes; changes of several characters are kept while switching
between them, and closing the editor or reloading the save folder asks before discarding them.
The reload button in the title bar (or `F5`) reads the save files again, e.g. after playing; it asks first
when there are unsaved changes and keeps the selected character.

- Only the changed entries of `savedItems` are rewritten. Every other field, unknown properties and the
  entry order stay as the game wrote them. The JSON is written compact on one line after the `EPOCH` marker,
  like the game writes it (a file that was indented before is written compact too).
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
  `ItemCategories`, `Uniques`, `Stats`, `Properties`, `Ailments`, `AbilityProperties`, `IdolAltarProperties`, `PlayerProperties`, `Timelines`, `Icons`. Every row keeps its element as JSON (`Json`)
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
