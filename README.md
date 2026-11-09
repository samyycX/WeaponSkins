<div align="center">
  <img src="https://pan.samyyc.dev/s/VYmMXE" alt="WeaponSkins" />
  <h1><strong>WeaponSkins</strong></h1>
  <p>Players pick weapon skins, knives, gloves, stickers, charms, agents and their patches, music kits, graffiti and pins for CS2 from an in-game menu.</p>
</div>

<p align="center">
  <a href="https://github.com/samyycX/WeaponSkins/actions/workflows/build.yml"><img src="https://img.shields.io/github/actions/workflow/status/samyycX/WeaponSkins/build.yml?branch=main&style=flat&logo=github&cacheSeconds=3600" alt="Build Status"></a>
  <a href="https://github.com/samyycX/WeaponSkins/releases/latest"><img src="https://img.shields.io/github/v/release/samyycX/WeaponSkins?style=flat&logo=github&cacheSeconds=3600" alt="Latest Release"></a>
  <img src="https://img.shields.io/github/downloads/samyycX/WeaponSkins/total?style=flat&logo=github&cacheSeconds=3600" alt="Downloads">
  <img src="https://img.shields.io/github/stars/samyycX/WeaponSkins?style=flat&logo=github&cacheSeconds=3600" alt="Stars">
  <img src="https://img.shields.io/github/license/samyycX/WeaponSkins?style=flat&cacheSeconds=3600" alt="License">
  <img src="https://img.shields.io/badge/status-beta-orange?style=flat" alt="Beta">
</p>

> [!NOTE]
> **Switching from WeaponPaints?** Your players keep their skins, knives, gloves and pins. Follow [Switching from WeaponPaints](#-switching-from-weaponpaints) **before** you start the server with WeaponSkins.

---

## ✨ Features

- 🎨 **Skins, knives and gloves** — plus stickers, charms, agents and their patches, music kits, graffiti and pins
- 🔧 **Fine-tune each item** — wear, pattern, name tag, souvenir, and StatTrak™ that counts kills
- 👥 **Separate T and CT loadouts**
- 🌍 **Item names in each player's own game language**
- 🔒 **VIP-only options** — e.g. knives and gloves only for players with a permission
- 💾 **Saves to MySQL, MariaDB, PostgreSQL or a simple file**
- 📦 **New CS2 items appear automatically** — read straight from the game, with no gamedata files to update
- 🧩 **Works with other plugins** — they can read and change players' skins

📺 **[Watch the showcase](https://youtu.be/MRa8JIRLysE)**

## 📦 Installation

1. Install [SwiftlyS2](https://swiftlys2.net/docs/installation).
2. Download the latest `WeaponSkins-v….zip` from [Releases](https://github.com/samyycX/WeaponSkins/releases/latest).
3. Unzip it into `game/csgo/addons/swiftlys2/plugins/`, so you get a `plugins/WeaponSkins/` folder.
4. Start the server once to create WeaponSkins' settings file. The first start takes a little longer. If no database is set up yet, WeaponSkins shows a database error and doesn't load — the next step fixes that.
5. Set up [storage](#-storage-setup).

**Updating:** check the [changelog](CHANGELOG.md) for anything to do first, delete the old `plugins/WeaponSkins/` folder, then unzip the new version. Your settings and saved skins are stored elsewhere, so they stay.

## 💾 Storage setup

There are three ways to store players' skins. Pick one and follow its steps:

1. **[Locally](#1-locally-sqlite)** — no database needed
2. **[Per server](#2-per-server-default-database)** — uses your server's own SwiftlyS2 database
3. **[Dedicated skins database](#3-dedicated-skins-database)** — a separate database just for skins, which several servers can share

### 1. Locally (SQLite)

No database needed.

1. Open `addons/swiftlys2/configs/plugins/WeaponSkins/config.toml`.
2. Change `StorageBackend` to `"sqlite"`:

   ```toml
   StorageBackend = "sqlite"
   ```

3. Save the file and restart the server.

### 2. Per server (default database)

Uses this server's own SwiftlyS2 database — the `default` connection.

1. Open `addons/swiftlys2/configs/database.jsonc`.
2. Replace the `"default"` line with your database's details, like this, and save:

   ```jsonc
   {
       "default_connection": "default",
       "connections": {
           "default": {
               "driver": "mysql",
               "host": "localhost",
               "database": "swiftlys2",
               "user": "root",
               "pass": "",
               "port": 3306,
               "timeout": 30
           },
           // ...your other connections stay as they are...
       }
   }
   ```

   Put your database's address, name, login and password in `host`, `database`, `user` and `pass`. For MariaDB keep `"driver": "mysql"`; for PostgreSQL use `"postgresql"` and port `5432`.

3. In WeaponSkins' `config.toml`, keep `StorageBackend = "inherit"` — it's the default.
4. Restart the server.

### 3. Dedicated skins database

A separate database just for skins, which several servers can share.

1. Create an empty database in your hosting or database panel, for example `weaponskins`.
2. Open `addons/swiftlys2/configs/database.jsonc`. Right after the `"connections": {` line, add a `weaponskins` connection with that database's details, and save:

   ```jsonc
   {
       "default_connection": "default",
       "connections": {
           "weaponskins": {
               "driver": "mysql",
               "host": "localhost",
               "database": "weaponskins",
               "user": "root",
               "pass": "",
               "port": 3306,
               "timeout": 30
           },
           // ...your other connections stay as they are...
       }
   }
   ```

   Put your database's address, name, login and password in `host`, `database`, `user` and `pass`. For MariaDB keep `"driver": "mysql"`; for PostgreSQL use `"postgresql"` and port `5432`. Write the name `weaponskins` exactly like that, in lowercase.

3. In WeaponSkins' `config.toml`, keep `StorageBackend = "inherit"` — it's the default.
4. Restart the server.

**Same skins on several servers?** Add the same `weaponskins` connection on each server, and set `SyncFromDatabaseWhenPlayerJoin = true` on each, so a player's changes on one server show up on the others.

If you switch to another setup later, skins saved with the old one aren't copied over.

## 🔄 Switching from WeaponPaints

Players keep their saved skins, knives, gloves and pins. Agents and music kits aren't carried over, so players pick those again.

Follow these steps in order. Steps 1 and 2 use phpMyAdmin, or your host's database tool.

1. **Back up your WeaponPaints database.** Select it, click **Export**, then **Go**.
2. **Prepare the database.** Select the WeaponPaints database, open the **SQL** tab, paste this and click **Go**:

   ```sql
   ALTER TABLE wp_player_skins ADD COLUMN weapon_sticker_5 VARCHAR(128) NOT NULL DEFAULT '0;0;0;0;0;0;0';
   RENAME TABLE wp_player_agents TO wp_player_agents_weaponpaints;
   RENAME TABLE wp_player_music TO wp_player_music_weaponpaints;
   ```

3. **Install WeaponSkins** — [Installation](#-installation) steps 1–3. Don't start the server yet.
4. **Make WeaponSkins use the WeaponPaints database.** If it's already your server's default connection, there's nothing to do — that's [setup 2](#2-per-server-default-database). Otherwise, follow [setup 3](#3-dedicated-skins-database), skipping its step 1 since the database already exists, with the details from `addons/counterstrikesharp/configs/plugins/WeaponPaints/WeaponPaints.json`:

   | In `WeaponPaints.json` | In `database.jsonc` |
   |------------------------|---------------------|
   | `DatabaseHost` | `host` |
   | `DatabaseName` | `database` |
   | `DatabaseUser` | `user` |
   | `DatabasePassword` | `pass` |
   | `DatabasePort` | `port` |

5. **Remove WeaponPaints** — delete the `addons/counterstrikesharp/plugins/WeaponPaints/` folder.
6. **Start the server.**

> [!WARNING]
> **Started WeaponSkins on the WeaponPaints database before step 2?** It won't load. Do step 2, then run `DELETE FROM __weaponskins_version_info WHERE version = 3;` the same way, and restart the server.

## 💬 Commands

| Command | Who | What it does |
|---------|-----|--------------|
| `!ws` | Players | Opens the skins menu |
| `!wp` | Players | Reloads your saved skins |
| `!spray` | Players | Sprays your graffiti on the wall or floor you aim at |
| `sw_wp_admin <SteamID64>` | Server console / RCON | Reloads a connected player's saved skins |

In the `!ws` menu, players choose weapon skins, knives, gloves, agents, music kits, graffiti and pins. **Weapon Skins** sorts the guns into Rifles, Pistols, SMGs, Shotguns, Snipers, Heavy and Other (the Zeus x27). Wear, pattern, name tag, StatTrak™, stickers and charms are under **Weapon Properties**, **Knife Properties** and **Glove Properties**, and patches under **Agent Properties**. Choices are saved straight away, separately for T and CT — music kits, graffiti and pins count for both. Options a player has no permission for are greyed out.

Players spray their graffiti with `!spray` while aiming at a nearby wall or floor, and can put it on a key from their console, e.g. `bind t sw_spray` — that replaces the game's own graffiti key. Each player has one `!spray` graffiti on the map at a time: a new one replaces the last. Players who own a graffiti in their Steam inventory can also use the game's graffiti key: their spray shows the graffiti chosen in `!ws`.

**Agent Properties** has five patch slots, filled from the game's patch packs. The patches go on the agent the player wears on that team, whether it was chosen in `!ws` or comes from their own inventory.

A collectible pin chosen under **Pins** shows next to the player's name on the scoreboard, for everyone. **Reset** brings back the pin from their own inventory.

## ⚙️ Settings

All settings are in `addons/swiftlys2/configs/plugins/WeaponSkins/config.toml`. Changes apply when you save the file, except `ItemLanguages`, which needs a restart. The defaults:

```toml
[Main]
StorageBackend = "inherit"
InventoryUpdateBackend = "hook"
SyncFromDatabaseWhenPlayerJoin = false
ItemLanguages = []
GraffitiCooldown = 30
ReplaceNativeGraffiti = true

[Main.ItemPermissions]
WeaponSkins = ""
KnifeSkins = ""
GloveSkins = ""
Stickers = ""
Keychains = ""   # charms
Agents = ""
MusicKits = ""
Graffiti = ""
Patches = ""
Pins = ""
Nametag = ""
Wear = ""
Seed = ""   # pattern
```

| Setting | What it does |
|---------|--------------|
| `StorageBackend` | `"sqlite"` or `"inherit"` — see [Storage setup](#-storage-setup). `"external"` is only for plugins that store skins themselves |
| `InventoryUpdateBackend` | Leave it as `"hook"` |
| `SyncFromDatabaseWhenPlayerJoin` | `true` reloads a player's skins from the database each time they join. Turn it on if several servers or a website use the same database |
| `ItemLanguages` | Languages for item names. `[]` loads all of them; to save memory (up to ~80 MB), list only the ones you need, e.g. `["en", "ru"]`. Players with any other language see the first one |
| `GraffitiCooldown` | Seconds a player waits between graffiti sprayed with `!spray`. `0` turns the wait off |
| `ReplaceNativeGraffiti` | `true` shows a player's `!ws` graffiti when they spray with the game's own graffiti key. `false` leaves those sprays as the game makes them |
| `ItemPermissions` | VIP-only features — see below |

**Language codes** — type them exactly as shown; a wrong one stops WeaponSkins from loading:
`ar` `bg` `cs` `da` `de` `el` `en` `es` `es-419` `fi` `fr` `hu` `id` `it` `ja` `ko` `nl` `no` `pl` `pt` `pt-BR` `ro` `ru` `sv` `th` `tr` `uk` `vn` `zh-CN` `zh-TW`

### VIP-only features

To limit a feature to players with a permission, put the permission next to it — for example, `KnifeSkins = "vip"` makes knives VIP-only. Empty (`""`) means everyone can use it.

Players get permissions in SwiftlyS2's `addons/swiftlys2/configs/permissions.jsonc`, by adding their SteamID64 under `"Players"`, e.g. `"76561198000000000": ["vip"]`.

Without the permission, the menu entry is greyed out, and the player's saved items of that kind stop showing until they get it back — for graffiti, `!spray` stops working and the game's graffiti key sprays their own graffiti again; for pins, the scoreboard shows their own pin again from their next spawn. Agents and music kits are the exception: those keep showing. `Patches` greys out **Agent Properties**.

`Nametag`, `Wear` and `Seed` do the same for those settings on the **Weapon Properties**, **Knife Properties** and **Glove Properties** pages. Without the permission, the setting is greyed out and the player's items show what a newly chosen skin starts with: no name tag, wear `0`, pattern `0`. Their saved values come back with the permission.

## 🩺 Troubleshooting

| Problem | Solution |
|---------|----------|
| WeaponSkins doesn't load, with a database error in the console | It can't reach its database. Check the connection details in `database.jsonc` — `default` for setup 2, `weaponskins` for setup 3 — and that the database exists. Or switch to [local storage](#1-locally-sqlite) |
| WeaponSkins stopped loading after you changed `ItemLanguages` | A language code is wrong — copy it exactly from the list in Settings |
| A properties page says *Nothing applied yet* | Choose a skin, knife or glove first. T and CT are separate |
| Menus show only 5 items per page | That's SwiftlyS2's menu limit: raise `MaxVisibleLines` in `addons/swiftlys2/configs/menus/menus.jsonc` |
| Skins changed on another server or a website don't show | Set `SyncFromDatabaseWhenPlayerJoin = true`, or type `!wp` |
| `!wp` says storage isn't ready | Set `StorageBackend` to `"inherit"` or `"sqlite"` — `"external"` only works with a plugin that provides storage |
| `Failed to load signature GiveNamedItem!` in the console | Old files are left over: delete `plugins/WeaponSkins/` and unzip the new version again |

## 🧩 API for Plugin Developers

Reference `WeaponSkins.Shared.dll` from the release's `resources/exports/` folder and request the `WeaponSkins.API` shared interface:

```csharp
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Players;
using WeaponSkins.Shared;

private IWeaponSkinAPI? _weaponSkins;

public override void UseSharedInterface(IInterfaceManager interfaceManager)
{
    interfaceManager.TryGetSharedInterface<IWeaponSkinAPI>("WeaponSkins.API", out _weaponSkins);
}

// AK-47 (definition index 7) in Case Hardened (paint kit 44) on the T side, for this session only.
_weaponSkins?.UpdateWeaponSkin(steamId, Team.T, 7, skin => skin.Paintkit = 44, permanent: false);
```

| Members | Purpose |
|---------|---------|
| `TryGet*Skin(s)`, `TryGetMusicKit`, `TryGetGraffiti`, `TryGetAgentPatches`, `TryGetPin` | Read a player's weapon, knife, glove, agent, music kit, graffiti, agent patch or pin selections. `TryGetAgentPatches` gives one patch index per slot, `0` for an empty one |
| `Set*Skins`, `Update*Skin`, `Reset*Skin` | Change or remove them; `permanent: true` also saves to storage. Item permissions still apply |
| `Items`, `WeaponToPaintkits`, `StickerCollections`, `Keychains`, `Agents`, `MusicKits`, `Graffiti`, `GraffitiTints`, `Patches`, `PatchCollections`, `Pins` | The parsed item catalog |
| `GetImageKey(kind, name)` | The game's inventory image for an item — `kind` is `agent`, `music`, `sticker`, `capsule`, `keychain`, `keychaincollection`, `graffiti`, `graffititournament` (whose `name` is the event id), `patch`, `patchcapsule` or `pin` — as a path under `panorama/images/econ/` without extension, or `null` when the game ships none. Graffiti art is untinted: the game washes it in the tint's `HexColor` (Panorama `wash-color`) |
| `SetExternalStorageProvider` | Provide the storage for `StorageBackend = "external"`. Its graffiti, agent patch and pin members are optional: without them, those aren't saved |

Menu rows carry the item they stand for as their component `Tag` — an `AgentDefinition`, `MusicKitDefinition`, `GraffitiDefinition`, `GraffitiTintDefinition`, `StickerDefinition`, `StickerCollectionDefinition`, `PatchDefinition`, `PatchCollectionDefinition`, `PinDefinition` or `KeychainDefinition`, a charm collection's name (`string`), a weapon category's key (`string`: `rifles`, `pistols`, `smgs`, `shotguns`, `snipers`, `heavy` or `other`), a graffiti tournament's event id (`int`), a sticker or patch slot (`int`, from 0), or an econ index (`ushort` for weapons, knives and gloves, `int` for paint kits) — so a custom menu renderer can draw images and selection state from the API. A row that opens a page carries that page's menu id as its component id: the main menu's entries, and the weapon categories, whose pages are `weaponskins.weapons.<key>`.

## 🛠️ Building

```powershell
dotnet build WeaponSkins.sln -c Release
dotnet publish WeaponSkins.csproj -c Release
```

The plugin builds to `build/net10.0/` and `WeaponSkins.Shared.dll` to `build/exports/net10.0/`; the build output and `dotnet publish` (`build/publish/WeaponSkins/`, zipped to `build/WeaponSkins-v<version>.zip`) both carry the contract in `resources/exports/` as well. The [build workflow](.github/workflows/build.yml) checks every push and pull request. The [release workflow](.github/workflows/release.yml) publishes a release from the newest [changelog](CHANGELOG.md) section when a commit to `main` has `[release]` in its message, or when run by hand; the version in `src/WeaponSkins.cs` must match it.

After replacing an install whose `resources/exports/WeaponSkins.Shared.dll` predates the new one, restart the server: SwiftlyS2 reads every plugin's exports once at startup, so a plugin reload keeps the old contract.

## 🤝 Support

- **Issues**: [GitHub Issues](https://github.com/samyycX/WeaponSkins/issues)
- **License**: [AGPL-3.0](LICENSE)
