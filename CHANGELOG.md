# Changelog

Notable changes to WeaponSkins, newest first.

<!-- The release workflow takes the version from the TOP "## [x.y.z]" heading below and
     refuses to publish unless it matches PluginMetadata.Version in src/WeaponSkins.cs.
     A version with a suffix, such as 0.8.0-beta.1, is published as a pre-release.

     Shape:

     ## [1.0.0] - YYYY-MM-DD

     Optional one-line summary.

     - What changed, from the server admin's side.

     See .github/instructions/changelog.instructions.md for the full style. -->

---

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

---

Releases up to `v0.6.0-ci.6` predate this file; their notes are on the
[releases page](https://github.com/samyycX/WeaponSkins/releases).
