<div align="center">
  <img src="https://pan.samyyc.dev/s/VYmMXE" />
  <h2><strong>WeaponSkins (Beta)</strong></h2>
  <h3>A powerful swiftlys2 plugin to change player's weapon skins, knifes and gloves.</h3>
</div>

<p align="center">
  <img src="https://img.shields.io/badge/build-passing-brightgreen" alt="Build Status">
  <img src="https://img.shields.io/github/downloads/samyycX/WeaponSkins/total" alt="Downloads">
  <img src="https://img.shields.io/github/stars/samyycX/WeaponSkins?style=flat&logo=github" alt="Stars">
  <img src="https://img.shields.io/github/license/samyycX/WeaponSkins" alt="License">
</p>

## Features
- MySQL & sqlite database support
- Fully functioning in-game skin menu
- Compatible with CounterStrikeSharp WeaponPaints database
- Long-term stattrak tracking
- Player-based skin name localization
- Completely game-based econ data dumping (no network required)

## Item Permissions
在 `config.toml` 中为贴纸或挂件指定权限即可限制可用玩家：
```toml
[item_permissions.stickers]
1001 = "vip.sticker"

[item_permissions.keychains]
2001 = "vip.keychain"
```
当玩家缺少对应权限时，配置项会自动隐藏或移除相关饰品。

## Showcase
[Youtube](https://youtu.be/MRa8JIRLysE)
  
## Building

- Open the project in your preferred .NET IDE (e.g., Visual Studio, Rider, VS Code).
- Build the project. The output DLL and resources will be placed in the `build/` directory.
- The publish process will also create a zip file for easy distribution.

## Publishing

- Use the `dotnet publish -c Release` command to build and package your plugin.
- Distribute the generated zip file or the contents of the `build/publish` directory.
