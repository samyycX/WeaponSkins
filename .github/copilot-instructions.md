# WeaponSkins — Copilot Instructions

> A SwiftlyS2 plugin (C#, .NET 10) for Counter-Strike 2: players pick weapon skins,
> knives, gloves, stickers, charms, agents and their patches, music kits and graffiti from
> the `!ws` menu.
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
  WeaponSkins.Configuration/            MainConfigModel, ItemPermissionConfig — config.toml, section [Main]
  WeaponSkins.Database/                 StorageService, DatabaseService (FreeSql + FluentMigrator), sync, models, migrations
  WeaponSkins.Econ/                     EconService: the item catalog, parsed from the game's own files
  WeaponSkins.Services/                 Commands, Data, Graffiti, Inventory, Menu, Permissions, Player, Stattrak, Localization
  WeaponSkins.Extensions/               PlayerExtensions (RegiveWeapon / RegiveKnife), dictionary helpers
  WeaponSkins.Utilities/                definition-index classifiers (knife / glove / weapon)
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
- `Unload` disposes the provider. `HookInventoryUpdateService` restores players' original
  music kits there and `StorageService` cancels in-flight loads; a new service holding
  per-player game state does the same.
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
  reconfigures on every `config.toml` save.
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
  by its `*Updated()` notifier — a schema write without one never reaches clients.
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
world update after it is created, once the game has filled it in.

Agent patches are not an econ item either: the game draws them from the pawn's
`m_vecPlayerPatchEconIndices` (five slots), whatever model it wears. `AgentPatchService`
writes them after the agent model is set (spawn, an agent picked in `!ws`, a reload) and
when they change. A team with no chosen patches is left alone, so a player's own show.

## Permissions

`ItemPermissionService` gates each item kind, and the name tag, wear and seed settings, by
`[Main.ItemPermissions]` in `config.toml`; an empty string means everyone. Reads go through
`WeaponSkinGetterAPI` and the `TryBuild*View` methods, which return a clone with stickers
and charms stripped, and name tag, wear and seed reset to a new skin's defaults, when the
player lacks those permissions. Writes go through `ApplyWeaponUpdateRules`,
`ApplyKnifeUpdateRules` and `ApplyGloveUpdateRules`, which keep the stored values in that
case. A trimmed view never equals its save, so only a changed save remakes a carried
weapon or knife — StatTrak™ saves on every kill. Agents and music kits are gated in the
menu only, so saved ones keep showing — the README promises that. Graffiti is gated in the
menu, in `!spray`, in the native-spray swap and in `TryGetGraffiti`; patches in the menu,
when applied and in `TryGetAgentPatches`.

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
  `wp_player_gloves`, `wp_player_agents`, `wp_player_music`) so servers can switch from
  WeaponPaints and websites can read them; `wp_player_graffiti` and `wp_player_agent_patches`
  are WeaponSkins' own.
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
  needs a default body, as the graffiti and agent patch members have, or those plugins stop
  loading. It also gets a real one on `DatabaseService` and `EmptyStorageProvider`.
- The README's API section lists the members; keep it in step.

## Configuration

`config.toml`, section `[Main]`, bound to `MainConfigModel` through Tomlyn. The file is
generated from the model's defaults on first load. `StorageService`, `ItemPermissionService`
and `GraffitiService` follow edits through `IOptionsMonitor`; `EconService` reads
`ItemLanguages` once. A new, renamed or removed key updates the README's settings block
and table, and gets a changelog bullet.

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
- What SwiftlyS2 does not provide — FreeSql, FluentMigrator, the database drivers, Tomlyn,
  ValveKeyValue — ships in the zip next to `WeaponSkins.dll`.

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
