---
description: 'Building, packaging and publishing WeaponSkins: the CreateZip target, release.yml, build.yml, versions, pre-releases and changelog coupling. Trigger phrases: build, publish, release, workflow, zip, version bump, tag, pre-release, beta.'
applyTo: '.github/workflows/*.yml'
---

# Build & Release

### Build

```powershell
dotnet build WeaponSkins.sln -c Release
dotnet publish WeaponSkins.csproj -c Release
```

`WeaponSkins.csproj` publishes to `build/publish/WeaponSkins/` and zips it to
`build/WeaponSkins-v<version>.zip` via the `CreateZip` target, which reads
`PluginMetadata.Version` out of `src/WeaponSkins.cs` with the `ExtractPluginVersion`
inline task. The version lives in exactly one place; nothing else needs stamping.

`WeaponSkins.Shared` builds to `build/exports/net10.0/`. The `CopyContract` and
`CopyContractToPublish` targets put a copy in `resources/exports/` of both outputs, and
publish fails if the contract is missing rather than shipping a zip without it.

### Cutting a release

`.github/workflows/release.yml` runs on `workflow_dispatch`, or on a push to `main`
whose head commit message contains `[release]`. It takes no inputs.

**A run produces exactly one release**: tag and title `v<version>`, with
`WeaponSkins-v<version>.zip` attached and the matching `CHANGELOG.md` section as its
notes, plus a compare link to the previous `v*` tag.

The version, the notes and the tag all come from `CHANGELOG.md` and must match
`PluginMetadata.Version`. An existing release **fails** the run rather than being
overwritten, which is what makes an accidental re-release loud. So a release needs
**both** a version bump and a matching changelog section — a missing section fails by
design. Style rules live in `.github/instructions/changelog.instructions.md`.

### Pre-releases

A version with a suffix (`0.8.0-beta.1`) is published with `--prerelease`. The
`[release]` trigger only watches `main`, so a pre-release from `beta` is cut by hand:
put the suffixed section at the top of `beta`'s `CHANGELOG.md`, set the same
`PluginMetadata.Version`, then run the workflow with **Use workflow from: beta**.

### Build check

`.github/workflows/build.yml` publishes on every push to `main` or `beta` and on every
pull request, and never releases. It fails when the versioned zip is not produced, and
only warns when `PluginMetadata.Version` and the newest `CHANGELOG.md` section disagree,
since that is a normal state while a release is being prepared.

### Install layout

```
addons/swiftlys2/plugins/WeaponSkins/
  WeaponSkins.dll
  WeaponSkins.Shared.dll          ← local copy, for a hot install
  <FreeSql, FluentMigrator, MySql.Data, Npgsql, Microsoft.Data.Sqlite, ...>.dll
  resources/
    templates/
    translations/
    exports/WeaponSkins.Shared.dll
```

There is no `resources/gamedata/`: WeaponSkins uses no signatures or offsets, and a
leftover folder from an old install makes SwiftlyS2 log
`Failed to load signature GiveNamedItem!`.

`config.toml` is generated on first load in
`addons/swiftlys2/configs/plugins/WeaponSkins/`. The parsed item catalog and the SQLite
database (`weaponskins.db`) live in the plugin's data directory.

### Release checklist

- [ ] `PluginMetadata.Version` bumped in `src/WeaponSkins.cs`, the only place it lives
- [ ] `CHANGELOG.md` has a `## [x.y.z] - YYYY-MM-DD` section matching it, at the top
- [ ] The commit that should publish carries `[release]`, or the workflow is run by hand
- [ ] `resources/exports/WeaponSkins.Shared.dll` present in the zip, `resources/gamedata/` absent
- [ ] `WeaponSkins.Shared` changed → the changelog tells admins to restart, not reload
- [ ] Cached catalog shape changed → `EconService.SchemaVersion` bumped
- [ ] `en.jsonc` complete; every other shipped language key-complete
- [ ] README updated for any changed command, setting or API member
