using SwiftlyS2.Shared.Menu;
using SwiftlyS2.Shared.Menu.Components;
using SwiftlyS2.Shared.Players;

namespace WeaponSkins;

public partial class MenuService
{
    public IMenu BuildKnifeSkinMenu(IPlayer player)
    {
        var main = CreateMenu("weaponskins.knifes")
            .WithTitle(LocalizationService[player].MenuTitleKnifes);

        foreach (var (knife, paintkits) in EconService.WeaponToPaintkits)
        {
            var item = EconService.Items[knife];
            if (!Utilities.IsKnifeDefinitionIndex(item.Index))
            {
                continue;
            }

            var itemName = EconService.GetLocalizedName(item.LocalizedNames, player.PlayerLanguage.Value);
            var submenuOption = CreateSubmenu(itemName, _ =>
            {
                var skinMenu = CreateMenu("weaponskins.knifes.paintkits").WithTitle(itemName);
                skinMenu.Add(CreateButton(LocalizationService[player].MenuReset,
                    p => Api.ResetKnifeSkin(p.SteamID, p.Controller.Team, true)));

                foreach (var paintkit in paintkits.OrderByDescending(p => p.Rarity.Id))
                {
                    var option = CreateRarityButton(
                        EconService.GetLocalizedName(paintkit.LocalizedNames, player.PlayerLanguage.Value),
                        paintkit.Rarity.Color.HexColor,
                        p => Api.UpdateKnifeSkin(p.SteamID, p.Controller.Team, knife =>
                        {
                            knife.DefinitionIndex = (ushort)item.Index;
                            knife.Paintkit = paintkit.Index;
                        }, true),
                        PaintkitImageKey(knife, paintkit),
                        itemName);
                    option.Tag = paintkit.Index;
                    skinMenu.Add(option);
                }

                var built = skinMenu.Build();
                TrackFocus(built, p => TryGetKnifeDataInHand(p, out var inHand) ? inHand.Paintkit : null);
                return built;
            }, BaseWeaponImageKey(knife));

            submenuOption.Tag = (ushort)item.Index;
            main.Add(submenuOption);
        }

        var menu = main.Build();
        TrackFocus(menu, p => TryGetKnifeDataInHand(p, out var inHand) ? inHand.DefinitionIndex : null);
        return menu;
    }

    public IMenuComponent GetKnifeSkinMenuSubmenuOption(IPlayer player)
    {
        if (!ItemPermissionService.CanUseKnifeSkins(player.SteamID))
        {
            return CreateDisabledOption(LocalizationService[player].MenuTitleKnifes);
        }

        return new SubmenuComponent(LocalizationService[player].MenuTitleKnifes, _ => BuildKnifeSkinMenu(player));
    }
}