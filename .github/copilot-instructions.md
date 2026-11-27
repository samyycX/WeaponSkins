# WeaponSkins — Copilot Instructions

> A SwiftlyS2 plugin (C#, .NET 10) for Counter-Strike 2: players pick weapon skins,
> knives, gloves, stickers, charms, agents and their patches, music kits, graffiti and pins
> from the `!ws` menu.
> Upstream is `samyycX/WeaponSkins`; contributions arrive as pull requests.

## Where things are documented

| Topic | Lives in |
|---|---|
| What admins do: install, storage setup, commands, settings, troubleshooting, API overview | `README.md` |
| What changed in each release | `CHANGELOG.md` — style in `.github/instructions/changelog.instructions.md` |
| Building, packaging, cutting a release | `.github/instructions/release.instructions.md` |
| How the code fits together | this file |

Keep one fact in one place: link to the README or the instruction files rather than
restating them here.

## Repository layout

```
WeaponSkins.sln / WeaponSkins.csproj    the plugin
src/
  WeaponSkins.cs                        entrypoint: PluginMetadata (the only version), DI, shared interface
  WeaponSkins.Injections/               one Add*/Use* extension pair per service
  WeaponSkins.Api/                      WeaponSkinAPI (IWeaponSkinAPI) and WeaponSkinGetterAPI (permission-filtered reads)
  WeaponSkins.Configuration/            MainConfigModel, ItemPermissionConfig, ItemPermission — config.jsonc, section "Main"; TomlConfigMigration
  WeaponSkins.Database/                 StorageService, DatabaseService (FreeSql + FluentMigrator), sync, models, migrations
  WeaponSkins.Econ/                     EconService: the item catalog, parsed from the game's own files
  WeaponSkins.Services/                 Commands, Data, Graffiti, Inventory, Menu, Permissions, Player, Stattrak, Localization
  WeaponSkins.Extensions/               PlayerExtensions (RegiveWeapon / RegiveKnife), dictionary helpers
  WeaponSkins.Utilities/                definition-index classifiers (knife / glove / weapon), a gun's WeaponCategory
WeaponSkins.Shared/                     the public contract, namespace WeaponSkins.Shared
resources/translations/*.jsonc          menu strings; en.jsonc is the reference
SwiftlyS2.CS2.dll                       not referenced by the build; SwiftlyS2.CS2 comes from NuGet
```

`WeaponSkins.Shared/` sits inside the plugin's project directory, and
`WeaponSkins.csproj` removes `WeaponSkins.Shared/**` from its own compile items. Contract
types go under `WeaponSkins.Shared/src/` and are compiled only by `WeaponSkins.Shared.csproj`.

Folder names follow `src/WeaponSkins.<Area>/`, but several services sit in the root
`WeaponSkins` namespace (`CommandService`, `MenuService`, the data services). Match the
neighbouring files rather than the folder.

## Composition

`WeaponSkins.Load` builds a private `ServiceCollection`: `AddSwiftly(Core)`, one
`AddXxx()` per service, then the matching `UseXxx()` calls on the built provider.

- Every service is a singleton, and `UseXxx()` exists only to resolve it. Constructors
  subscribe to game events and register commands, so **a service nobody resolves silently
  does nothing.** A new service gets its own `src/WeaponSkins.Injections/XxxInjection.cs`
  with both halves, and both are added to the chains in `WeaponSkins.cs`.
- `Unload` disposes the provider. `HookInventoryUpdateService` and `PinService` restore
  players' original music kits and pins there, and `StorageService` cancels in-flight loads;
  a new service holding per-player game state does the same.
- `ConfigureSharedInterface` publishes `WeaponSkinAPI` under the key `WeaponSkins.API`.

## State and storage

- **`DataService` is the live state**: one sub-service per item kind, concurrent
  dictionaries keyed SteamID → team → item. The data models are `record`s, so
  `StoreSkin` / `StoreKnife` / `StoreGlove` return `false` for a value equal to the stored
  one — and that result decides whether a carried item gets remade.
- `StorageService` chooses the persistence provider from `StorageBackend`: `inherit` uses
  the `Core.Database` connection named `weaponskins`, `sqlite` uses `weaponskins.db` in the
  plugin data directory, and `external` waits for another plugin to call
  `SetExternalStorageProvider` (`EmptyStorageProvider` stands in until then). It
  reconfigures on every `config.jsonc` save.
- At startup `DatabaseSynchronizeService.SynchronizeAsync` loads every saved row into
  `DataService`. `ReloadPlayerAsync` replaces one player's state; it backs `!wp`,
  `sw_wp_admin`, and joins when `SyncFromDatabaseWhenPlayerJoin` is on.
- `WeaponSkinAPI` writes to storage fire-and-forget (`Task.Run`) when `permanent: true`.
  The menu always saves; API callers choose.

## Applying cosmetics

`HookInventoryUpdateService` is the only `InventoryUpdateBackend` (`hook`). Its rules come
from how the CS2 client caches finishes, and each one fixed a visible bug:

- **Weapons are painted as they are created**: `OnEntityCreated` for `weapon_*`, applied a
  tick later once the give has bound the owner and econ item. A weapon picked up off the
  ground is not a new entity and keeps the finish it had.
- **An item a player already carries is never repainted in place to change it.** The
  client keeps the finish it already drew, so an update remakes the item — `RemakeWeapons`
  / `RemakeKnife` call `RegiveWeapon` / `RegiveKnife` with `PaintBeforeSelect`, which paints
  the new entity before it is selected or networked. `RemakeKnife` only acts when the
  player has a knife, because `RegiveKnife` always hands one out.
- Every apply writes a fresh random `ItemID`, and every networked field written is followed
  by its `*Updated()` notifier — a schema write without one never reaches clients. For the
  attribute lists that is each list's `AttributesUpdated()`, never the item's
  `AttributeListUpdated()` / `NetworkedDynamicAttributesUpdated()`: the embedded struct has
  no network encoder of its own, so marking it makes the engine log an unresolved change on
  every apply.
- Weapon wear always goes through `FinishCacheKeys.Resolve`. The client caches a finish by
  paint, seed and wear to three decimals, together with the stickers it was built with, so
  each sticker layout claims its own 0.001 wear step. Claims reset on map load.
- Gloves re-apply when `EconGlovesChanged` moves. Agents need a model swap to refresh
  (`GetRefreshModel`). Music kits live on the controller, which a reconnect or map change
  replaces, so they are re-applied every spawn.
- There are no signatures, offsets or `resources/gamedata` files, on purpose: use SwiftlyS2
  events and schema fields. A stale gamedata folder from an old install makes SwiftlyS2 log
  `Failed to load signature GiveNamedItem!`.

StatTrak™ counts on `EventPlayerDeath` against the attacker's active weapon and is saved on
every kill (`StattrakService`).

Graffiti is not an inventory item. `GraffitiService` spawns a `player_spray_decal` on the
static surface `!spray` aims at, keeps one per player and forgets them on map load. Its
trace only hits `worldent` and `prop_static` (the world reaches the trace filter as an
entity), so it passes through the sprayer, their held weapon and anything that moves. The
game's own graffiti key works only for players who own a graffiti; with
`ReplaceNativeGraffiti` its decal gets the player's `!ws` sticker kit and tint on the next
world update after it is created, once the game has filled it in. With `SprayWithUseKey`,
a USE press (`OnClientKeyStateChanged`, raised on the game thread) runs the same spray
without a reply, so its failures stay silent: report a new failure through `reply`, never
`SendChat`. It is skipped while `Core.Menu.GetSession` finds a menu open, since USE
selects rows there. SwiftlyS2's menus handle the press first, so one that closes a menu
leaves no session to find: `OnTick`, which runs before the frame's key events, records who
had a menu open, and those players are skipped too. Legacy `MenusAPI` menus are not
checked, on purpose: the API is obsolete, so calling it adds CS0618 warnings and trips
SwiftlyS2's obsolete-API scanner.

Agent patches are not an econ item either: the game draws them from the pawn's
`m_vecPlayerPatchEconIndices` (five slots), whatever model it wears. `AgentPatchService`
writes them after the agent model is set (spawn, an agent picked in `!ws`, a reload) and
when they change. A team with no chosen patches is left alone, so a player's own show.

Pins are written to the controller, not given as items: the scoreboard draws a player's pin
from `InventoryServices.Rank[5]`, which the game fills from their own inventory. `PinService`
saves that value once per session before its first write, re-asserts the chosen pin on every
spawn and reload (an equal value is not rewritten), and writes the saved value back on reset,
when the permission is gone, and on unload. A player with no chosen pin is left alone.

## Permissions

`ItemPermissionService` gates each item kind, and the name tag, wear, seed, StatTrak™ and
Souvenir settings, by `ItemPermissions` in `config.jsonc`. Each entry has `Enabled` and
`Permission`, and every `CanUse*` goes through the one check: enabled, and the permission
empty or held, so `"Enabled": false` acts everywhere as if no player had the permission.
Reads go through `WeaponSkinGetterAPI` and the `TryBuild*View` methods, which
return a clone with stickers and charms stripped, and name tag, wear and seed reset to a new
skin's defaults, when the player lacks those permissions. A StatTrak™ or Souvenir quality
falls back the same way, to the plain one storage loads (`Normal` for weapons, `Unusual` for
knives), and the StatTrak™ count to `0`. Writes go through `ApplyWeaponUpdateRules`,
`ApplyKnifeUpdateRules` and `ApplyGloveUpdateRules`, which keep the stored values in that
case; quality is one field, so an update that would set or clear StatTrak™ or Souvenir
without its permission keeps the stored one. A trimmed view never equals its save, so only
a changed save remakes a carried weapon or knife — StatTrak™ saves on every kill.
`StattrakService` counts only for players with the StatTrak™ permission, and on the stored
record itself: a trimmed view saved back would lose the stripped values and remake the
weapon on every kill. Agents and music kits are gated in the menu only, so saved ones keep
showing — the README promises that. Graffiti is gated in the menu, in `!spray`, in the
native-spray swap and in `TryGetGraffiti`; patches and pins in the menu, when applied and in
`TryGetAgentPatches` / `TryGetPin`.

## Item catalog

`EconService` reads `scripts/items/items_game.txt` and `resource/csgo_*` through
`Core.GameFileSystem`, which is why new CS2 items appear without a plugin update.

- Parsing takes seconds and a lot of memory, so the result is cached as JSON in the plugin
  data directory behind `version.lock`: the items_game version, `ItemLanguages` and
  `SchemaVersion`. **Bump `SchemaVersion` whenever a cached type, or what a parser puts in
  it, changes** — otherwise servers keep loading the old cache.
- `ItemLanguages` codes map through `LanguageCodeToTranslationKey`. An unknown code throws
  and the plugin does not load, which the README documents. The list is read once through
  `IOptions`, so changing it needs a restart.
- Item names come from `GetLocalizedName(names, player.PlayerLanguage.Value)`; it falls
  back to the primary language, then English, then any language that names the item.

## Menus

`MenuService` is one `partial` class per page, built with `Core.Menu` — SwiftlyS2's
component menus, not the legacy `MenusAPI`. Menu ids are `weaponskins.*`.

- **Row `Tag`s are a public contract.** Custom menu renderers draw the locker from them,
  and the README lists every type a `Tag` can hold. A new kind of row, or a changed `Tag`,
  updates that README paragraph in the same change.
- Main-menu categories pass the submenu's menu id as the component id (`CreateCategory`),
  so a renderer with a sidebar can mark the open page.
- `!ws` opens `weaponskins.main` for the player's team. A player on neither T nor CT gets
  `weaponskins.team` first, whose rows are buttons rather than submenus: the main menu
  they open stays a root, which renderers walk up to, and a renderer that auto-opens a
  root's first submenu would otherwise pick T unasked. The main menu's own `Tag` is the
  `Team` it was opened for. Every menu read or write takes its team from `EditedTeam`:
  the player's own while they are on T or CT, else that one.
- A row the player has no permission for is disabled, and with `HideWhenDisabled` hidden
  too (`Shown`). Hide with the static `Visible`, never `VisibleWhen`: renderers drawing off
  the game thread can't evaluate a per-session rule.
- **Weapon Skins** lists one row per `WeaponCategory` (`Utilities.GetWeaponCategory`, by
  the `Items` key; a gun it doesn't name lands in Other), each opening
  `weaponskins.weapons.<key>` with that category's guns. A category row passes its page's
  id as the component id too, so a renderer can draw the categories as tabs, and carries
  its key as the `Tag`.
- Linked controls — the wear slider and exact field, the StatTrak™ and Souvenir switches —
  set each other through `Poke`, which writes the other control's state without running
  its handler, so one change applies once.
- `TrackFocus` restores the focused row on back-navigation and focuses the item in hand on
  a fresh open.

## Threading

Storage I/O runs off the game thread. Code that comes back from it to touch entities,
players or `DataService` goes through `Core.Scheduler.NextTick` / `NextTickAsync`, then
re-finds the player by `SessionId`, since they may have left. Use the `Action` overload of
`NextTickAsync` and capture the result in a local — SwiftlyS2's obsolete-API scanner
misreads `NextTickAsync<T>(Func<T>)`.

## Database

- The tables are WeaponPaints-compatible (`wp_player_skins`, `wp_player_knife`,
  `wp_player_gloves`, `wp_player_agents`, `wp_player_music`, `wp_player_pins`) so servers can
  switch from WeaponPaints and websites can read them; `wp_player_graffiti` and
  `wp_player_agent_patches` are WeaponSkins' own.
  **Never rename or retype an existing column.**
- Schema changes are FluentMigrator migrations in `src/WeaponSkins.Database/Migrations/`,
  numbered `MigrationNNN`, tracked in `__weaponskins_version_info`. Guard a create with
  `Schema.Table(...).Exists()` — the table may already exist from WeaponPaints — and keep
  it valid on MySQL/MariaDB, PostgreSQL and SQLite.
- Stickers and charms are stored as `;`-separated fields, written and parsed with
  `CultureInfo.InvariantCulture`. A server culture with a decimal comma corrupted them
  before that.
- A new field on a data model touches its `DeepClone` (hand-written — a missed field is
  silently dropped on every update), the DB model's `ToDataModel` / `FromDataModel`, and
  usually a migration.

## Shared contract

`WeaponSkins.Shared` holds the data models, definitions, constants, `IWeaponSkinAPI`,
`IStorageProvider` and `IInventoryUpdateService`.

- It ships twice: next to `WeaponSkins.dll` and in `resources/exports/`. SwiftlyS2 binds
  every plugin to the exports copy it read at server start, so after a contract change a
  plugin reload keeps the old one — the changelog tells admins to restart.
- Change it additively. Removing or changing a member breaks every plugin compiled against
  it. `IStorageProvider` is implemented by external storage plugins, so a new member there
  needs a default body, as the graffiti, agent patch and pin members have, or those plugins
  stop loading. It also gets a real one on `DatabaseService` and `EmptyStorageProvider`.
- The README's API section lists the members; keep it in step.

## Configuration

`config.jsonc`, section `Main`, bound to `MainConfigModel`. The file is generated from the
model's defaults on first load, and every load adds the keys an existing file is missing
(`addMissingKeys`), down to an `ItemPermissions` entry's `Enabled` or `Permission`, leaving
the rest of it alone. Every key carries a one-line `[Description]`, which SwiftlyS2 writes
as the `//` comment above it in a new file or above an inserted key, so a new key gets one
in the same change. The exception is `ItemPermission`'s `Enabled` and `Permission`: the
`ItemPermissions` description explains them once, where theirs would repeat under all 15
entries. `StorageService`, `ItemPermissionService`, `GraffitiService` and `MenuService`
follow edits through `IOptionsMonitor`; `EconService` reads `ItemLanguages` once. A new,
renamed or removed key updates the README's settings block and table, and gets a changelog
bullet.

The binder silently skips a string where an object is expected, so `"KnifeSkins": "vip"`,
the old form, would open knives to everyone. `ItemPermissionConfig.ReadPlainPermissions`
reads such items after binding: in a `PostConfigure` in `Load`, and in the migration. Not a
`TypeConverter`: `TypeDescriptor` resolves the converter's type by name from the default
load context, so for a plugin, which SwiftlyS2 loads into its own, it never finds it.

Versions before 0.11.0 used `config.toml`. `TomlConfigMigration` runs first in `Load`: when
there is a `config.toml` but no `config.jsonc`, it binds `config.toml` into `MainConfigModel`
through Tomlyn and has SwiftlyS2 write a `config.<guid>.jsonc` of its own from a subclass
whose defaults are those values, so the file matches a new one but for its values. Servers
started from one install share the config folder, so that file is moved to `config.jsonc`
without overwriting; when another server got there first, the migration deletes only its own
file and goes on with theirs. (On Linux .NET checks, then renames, so a server losing by
microseconds replaces it with an identical file.) Then `config.toml` is renamed to
`config.toml.old` (`.old.1` and on if that exists): already gone means another server did
it, and one that can't be renamed gets a warning naming both files while the load goes on.
A `config.toml` that doesn't parse or bind, or a migrated file that can't be written or
moved in, stops the load instead of falling back to the defaults, and leaves `config.toml`
as it was. `config.jsonc` is never deleted. With both files there, `config.jsonc` wins and a
warning names `config.toml`. The migration, and the Tomlyn packages with it, can go in a
later version.

## Commands

`CommandService` registers `ws` (open the menu), `wp` (reload your saved skins), `spray`
(spray your graffiti) and `wp_admin <SteamID64>` (console/RCON only). Players type `!ws`,
`!wp` and `!spray`; the console form is `sw_wp_admin`. The README command table is the
user-facing list.

## Translations

Menu text resolves per player through `Core.Translation.GetPlayerLocalizer(player)` — the
player's CS2 language, the same one item names use. Don't mix in `Core.Localizer`, or one
menu ends up in two languages. Each string is a property on `PlayerLocalizationService`:
a new key adds the property and the `en.jsonc` entry together. A missing key renders as a
raw `{lang}.{key}` marker rather than falling back to English, so every shipped language
must stay key-complete.

## Packages

- `SwiftlyS2.CS2` is pinned in `WeaponSkins.csproj` (the comment there says why) and floats
  in `WeaponSkins.Shared.csproj`. Both use `ExcludeAssets="runtime"`: the host provides it,
  and so does `Microsoft.Extensions.*` where it is marked the same way.
- What SwiftlyS2 does not provide — FreeSql, FluentMigrator, the database drivers,
  ValveKeyValue — ships in the zip next to `WeaponSkins.dll`. Tomlyn and
  Tomlyn.Extensions.Configuration come with SwiftlyS2, and only the `config.toml`
  migration uses them.

## Build & release

```powershell
dotnet build WeaponSkins.sln -c Release
```

Releases are cut from `CHANGELOG.md` by a `[release]` commit on `main` or a manual run;
`build.yml` checks every push and pull request. Details in
`.github/instructions/release.instructions.md`.

## Code style

`.editorconfig` governs formatting: file-scoped namespaces, `System` usings first in their
own group, braces on a new line, four-space indents. Comments are one line and only for
what the code cannot say — client and engine behaviour, ordering constraints, deliberate
omissions. `FinishCacheKeys`, `WeaponAppearance` and `HookInventoryUpdateService` set the
tone.

## Keeping this file current

Update this file in the same change when a service is added or removed, a rule above stops
being true (how cosmetics are applied, storage backends, the shared contract, threading),
or the repository layout moves. Changes an admin sees belong in the README and
`CHANGELOG.md` instead.
