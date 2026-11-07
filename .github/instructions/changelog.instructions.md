---
description: 'How to write CHANGELOG.md entries for WeaponSkins. Trigger phrases: changelog, release notes, new release, version bump, what changed.'
applyTo: 'CHANGELOG.md'
---

# Changelog style

The changelog **is** the release body. `.github/workflows/release.yml` pulls the
section whose heading matches `PluginMetadata.Version` in `src/WeaponSkins.cs` and
publishes it verbatim, so write for the person reading the GitHub release page.

## Audience

A server admin deciding two things: *do I need this update*, and *what do I re-test
after installing it*. Not a reviewer, and not your future self debugging the fix.

Developers of plugins that use `IWeaponSkinAPI` are a second, smaller audience. Give
them their own bullet, starting *"For plugin developers:"*, instead of mixing contract
details into admin bullets.

## Shape

```markdown
## [0.8.0] - 2026-12-01

One-line summary, only when the release has a theme or a reason to hurry.

- What changed, from the admin's side.
- Another change.
```

- **Heading is `## [x.y.z] - YYYY-MM-DD`.** The version must equal
  `PluginMetadata.Version` exactly or the release fails — that check is deliberate.
- A suffix such as `0.8.0-beta.1` publishes a pre-release.
- **Newest section first**, directly under the header comment.
- The optional lead sentence earns its place when it tells someone whether this
  release is for them: *"Worth updating if players use stickers."*
- Plain `-` bullets. No `Added` / `Changed` / `Fixed` sections, no bold lead-in labels.
- Wrap around 90 columns.
- Leave the `---` + older-releases note at the bottom of the file.

## Content

1. **Outcome, not mechanism.** "Stickers added to a skin did not show on the weapon in
   hand" — not how the client caches finishes. Internals belong in code comments.
2. **Name the symptom they saw.** The command (`!ws`, `!wp`, `sw_wp_admin`), the
   `config.toml` key, the console error or the menu entry, in `backticks` or **bold**.
3. **One bullet per change.** If a bullet needs a semicolon and a subclause, it is two
   bullets, or it is too much detail.
4. **Call out anything that needs admin action:**
   - files that must be deleted before unzipping (the update steps assume a clean folder);
   - a new, renamed or removed `config.toml` key;
   - a new migration, especially one touching the WeaponPaints-shared `wp_player_*`
     tables that websites and other tools also read;
   - a `WeaponSkins.Shared` change, which needs a server restart rather than a plugin
     reload before other plugins see it;
   - an `EconService.SchemaVersion` bump, which makes the first start re-read the
     game's item list;
   - renamed or removed translation keys.
5. **Say when a version was folded in**, e.g. *"Includes the work versioned 0.7.1,
   which was never released on its own."*
6. Link an issue as `([#N](https://github.com/samyycX/WeaponSkins/issues/N))` **only
   when that issue exists**. Never invent one.
7. Skip anything with no user-visible effect — refactors, comments, CI. A release with
   nothing worth saying does not need a release.

## Before committing

Read the bullets back as an admin. If one explains why the bug existed at the code
level, cut it to the symptom and the fix.
