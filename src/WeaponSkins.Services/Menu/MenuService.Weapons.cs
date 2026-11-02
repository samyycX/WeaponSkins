using SwiftlyS2.Shared.Menu;
using SwiftlyS2.Shared.Menu.Components;
using SwiftlyS2.Shared.Players;

namespace WeaponSkins;

public partial class MenuService
{
    public IMenu BuildWeaponSkinMenu(IPlayer player)
    {
        var main = Core.Menu.CreateMenu("weaponskins.weapons")
            .WithTitle(LocalizationService[player].MenuTitleSkins);

        foreach (var (weapon, paintkits) in EconService.WeaponToPaintkits)
        {
            var item = EconService.Items[weapon];
            if (!Utilities.IsWeaponDefinitionIndex(item.Index))
            {
                continue;
            }

            var itemName = EconService.GetLocalizedName(item.LocalizedNames, player.PlayerLanguage.Value);
            var submenuOption = new SubmenuComponent(itemName, _ =>
            {
                var skinMenu = Core.Menu.CreateMenu("weaponskins.weapons.paintkits").WithTitle(itemName);
                skinMenu.Add(CreateButton(LocalizationService[player].MenuReset,
                    p => Api.ResetWeaponSkin(p.SteamID, p.Controller.Team, (ushort)item.Index, true)));

                foreach (var paintkit in paintkits.OrderByDescending(p => p.Rarity.Id))
                {
                    var option = CreateRarityButton(
                        EconService.GetLocalizedName(paintkit.LocalizedNames, player.PlayerLanguage.Value),
                        paintkit.Rarity.Color.HexColor,
                        p => Api.UpdateWeaponSkin(p.SteamID, p.Controller.Team, (ushort)item.Index, skin => skin.Paintkit = paintkit.Index, true));
                    option.Tag = paintkit.Index;
                    skinMenu.Add(option);
                }

                var built = skinMenu.Build();
                TrackFocus(built, p => TryGetWeaponDataInHand(p, out var inHand) ? inHand.Paintkit : null);
                return built;
            });

            submenuOption.Tag = (ushort)item.Index;
            main.Add(submenuOption);
        }

        var menu = main.Build();
        TrackFocus(menu, p => TryGetWeaponInHand(p, out var weapon) && Utilities.IsWeaponDefinitionIndex(weapon.AttributeManager.Item.ItemDefinitionIndex) ? (ushort)weapon.AttributeManager.Item.ItemDefinitionIndex : null);
        return menu;
    }

    public IMenuComponent GetWeaponSkinMenuSubmenuOption(IPlayer player)
    {
        if (!ItemPermissionService.CanUseWeaponSkins(player.SteamID))
        {
            return CreateDisabledOption(LocalizationService[player].MenuTitleSkins);
        }

        return new SubmenuComponent(LocalizationService[player].MenuTitleSkins, _ => BuildWeaponSkinMenu(player));
    }
}