using SwiftlyS2.Shared.Menu;
using SwiftlyS2.Shared.Menu.Components;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins;

public partial class MenuService
{
    // Weapons only, never knives or gloves, that the player has a skin applied to on their current team.
    public IMenu BuildWeaponPropertiesMenu(IPlayer player)
    {
        var loc = LocalizationService[player];
        var team = player.Controller.Team;
        var language = player.PlayerLanguage.Value;
        var main = CreateMenu("weaponskins.weaponproperties")
            .WithTitle(loc.MenuTitleWeaponProperties);

        var skins = Api.TryGetWeaponSkins(player.SteamID, out var stored)
            ? stored.Where(skin => skin.Team == team && Utilities.IsWeaponDefinitionIndex(skin.DefinitionIndex))
                .OrderBy(skin => skin.DefinitionIndex)
                .ToList()
            : [];

        var added = 0;
        foreach (var skin in skins)
        {
            var (weapon, item) = EconService.Items.FirstOrDefault(entry => entry.Value.Index == skin.DefinitionIndex);
            if (item is null)
            {
                continue;
            }

            var weaponName = EconService.GetLocalizedName(item.LocalizedNames, language);
            var paintkit = EconService.WeaponToPaintkits.TryGetValue(weapon, out var paintkits)
                ? paintkits.FirstOrDefault(p => p.Index == skin.Paintkit)
                : null;
            var title = paintkit is null
                ? weaponName
                : $"{weaponName} | {EconService.GetLocalizedName(paintkit.LocalizedNames, language)}";
            var definition = skin.DefinitionIndex;

            var row = CreateSubmenu(title, _ => BuildSkinPropertiesMenu(player, team, definition, title),
                paintkit?.Rarity.Color.HexColor);
            // A row's Tag is the item it stands for, so any renderer can identify it.
            row.Tag = definition;
            main.Add(row);
            added++;
        }

        if (added == 0)
        {
            AddEmptyNotice(main, loc);
        }

        return main.Build();
    }

    public IMenu BuildSkinPropertiesMenu(IPlayer player,
        Team team,
        ushort definition,
        string title)
    {
        var loc = LocalizationService[player];
        var steamId = player.SteamID;
        var main = CreateMenu("weaponskins.skinproperties")
            .WithTitle(title);

        if (!Api.TryGetWeaponSkin(steamId, team, definition, out var skin))
        {
            AddEmptyNotice(main, loc);
            return main.Build();
        }

        void Update(Action<WeaponSkinData> apply) =>
            Api.UpdateWeaponSkin(steamId, team, definition, apply, true);

        main.Add(CreateHero(player.PlayerLanguage.Value, definition, skin.Paintkit));
        AddNametag(main, loc, skin.Nametag, nametag => Update(s => s.Nametag = nametag));
        AddWear(main, loc, skin.PaintkitWear, wear => Update(s => s.PaintkitWear = wear));
        main.Add(CreateSeedInput(loc, skin.PaintkitSeed, seed => Update(s => s.PaintkitSeed = seed)));
        AddStattrak(main, loc, skin.StattrakCount, skin.Quality,
            count => Update(s => s.StattrakCount = count),
            quality => Update(s => s.Quality = quality),
            withSouvenir: true);

        main.Add(CreateCategory(ItemPermissionService.CanUseStickers(steamId), loc.MenuTitleStickers,
            "weaponskins.stickers", () => BuildStickerMenu(player, team, definition)));
        main.Add(CreateCategory(ItemPermissionService.CanUseKeychains(steamId), loc.MenuTitleKeychains,
            "weaponskins.keychains", () => BuildKeychainMenu(player, team, definition)));

        return main.Build();
    }
}