# Changelog

Notable changes to WeaponSkins, newest first. Releases up to `v0.6.0-ci.6` predate this
file; their notes are on the [releases page](https://github.com/samyycX/WeaponSkins/releases).

<!-- The release workflow takes the version from the TOP "## [x.y.z]" heading below and
     refuses to publish unless it matches PluginMetadata.Version in src/WeaponSkins.cs.
     A version with a suffix, such as 0.8.0-beta.1, is published as a pre-release.

     Shape:

     ## [1.0.0] - YYYY-MM-DD

     Optional one-line summary.

     - What changed, from the server admin's side.

     See .github/instructions/changelog.instructions.md for the full style. -->

---

## [0.9.1] - 2026-11-12

- Updated pt-BR translations

## [0.9.0] - 2026-11-09

- Added pins
- Added weapon skins categories

## [0.8.0] - 2026-11-08

- Players can spray graffiti: they pick one under **Graffiti** in `!ws`, plus a colour for
  the plain ones, then type `!spray` while aiming at a nearby wall or floor. It can go on a
  key with `bind t sw_spray` in the player's console.
- Each player has one graffiti on the map at a time, and waits 30 seconds between sprays.
  To change the wait, add `GraffitiCooldown` under `[Main]` in `config.toml`.
- Players who own a graffiti in their Steam inventory can also spray with the game's
  graffiti key, and it shows the graffiti they chose in `!ws`. To keep the game's own
  graffiti on that key, add `ReplaceNativeGraffiti = false` under `[Main]`.
- Graffiti can be made VIP-only like the other items: add `Graffiti = "vip"` under
  `[Main.ItemPermissions]`.
- Agents can wear patches: **Agent Properties** in `!ws` has five patch slots per team,
  filled from the game's patch packs. To make them VIP-only, add `Patches = "vip"` under
  `[Main.ItemPermissions]`.
- Patch packs no longer show up among the sticker collections in **Weapon Properties**.
- Name tags, wear and seed can be made VIP-only, like knives or stickers. Existing
  `config.toml` files don't get the new keys on their own: add `Nametag`, `Wear` or `Seed`
  under `[Main.ItemPermissions]` with a permission, e.g. `Wear = "vip"`.
- Without that permission, the setting is greyed out on the properties pages, and the
  player's items fall back to no name tag, wear 0 or seed 0 until they get it back.
- A StatTrak™ weapon no longer re-deploys after every kill when the player lacks the
  sticker or charm permission but still has stickers or a charm saved on it.
- WeaponSkins adds `wp_player_graffiti` and `wp_player_agent_patches` tables to its
  database on the first start.
- The first start after updating takes a little longer while WeaponSkins re-reads the
  game's item list.
- If you edited the translation files, add the new `menu.graffiti.title`,
  `menu.agentproperties.*` and `graffiti.*` keys to them.
- For plugin developers: `IWeaponSkinAPI` adds `Graffiti`, `GraffitiTints`,
  `TryGetGraffiti`, `Patches`, `PatchCollections` and `TryGetAgentPatches`, and
  `GetImageKey` takes `graffiti`, `graffititournament`, `patch` and `patchcapsule`.
  Graffiti rows carry a `GraffitiDefinition` or `GraffitiTintDefinition` as their `Tag`,
  tournament rows their event id, patch rows a `PatchDefinition` or
  `PatchCollectionDefinition`, and patch slot rows the slot number. `IStorageProvider`
  gains graffiti and agent patch members with default bodies, so storage plugins keep
  loading but don't save those until they implement them. Restart the server after
  updating, rather than reloading the plugin, so other plugins see the new
  `WeaponSkins.Shared.dll`.

## [0.7.0] - 2026-11-07

Worth updating if skins stop applying after a CS2 update, or players see an old finish on
knives, gloves or stickered weapons. Covers everything since `v0.6.0-ci.6`; versions drop
the `-ci.N` suffix from here on.

- Delete the old `plugins/WeaponSkins/` folder before unzipping this release: WeaponSkins
  no longer ships `resources/gamedata`, and leftover files make the console show
  `Failed to load signature GiveNamedItem!`.
- Signatures and offsets are gone, so a CS2 update that moves them no longer stops skins
  from applying.
- Stickers added to a skin now show on the weapon in hand. To make that work, the wear a
  weapon shows can differ from the saved value in the third decimal, without changing its
  exterior.
- A changed knife or glove finish now shows instead of the one the player's game had
  already drawn.
- Clearing a knife's name tag now removes it instead of keeping the old one.
- A player's music kit now stays after they reconnect or the map changes.
- Stickers and charms moved from the main `!ws` menu into **Weapon Properties**, the page
  formerly called **Skin Properties**. Charms are grouped by collection.
- On the properties pages, StatTrak™ and Souvenir are On/Off switches, and wear has a
  slider beside the exact value.
- `MenuItemsPerPage` is removed from `config.toml`: `MaxVisibleLines` in SwiftlyS2's
  `menus.jsonc` alone decides how many rows a page shows. The old key is ignored.
- Items the game ships without a name, such as one of the music kits, now get a fallback
  name instead of an error.
- The first start after updating takes a little longer while WeaponSkins re-reads the
  game's item list.
- If you edited the translation files, redo the edits on the new ones: the menus use new
  keys, such as `menu.weaponproperties.title`, `menu.on` and `menu.off`.
- For plugin developers: `IWeaponSkinAPI` adds `Agents`, `MusicKits`, `GetImageKey` and
  `TryGetMusicKit`, and every menu row carries the item it stands for as its `Tag`, so a
  custom menu renderer can draw the locker. Restart the server after updating, rather than
  reloading the plugin, so other plugins see the new `WeaponSkins.Shared.dll`.
