using SwiftlyS2.Shared.Menu;
using SwiftlyS2.Shared.Menu.Components;
using SwiftlyS2.Shared.Players;

namespace WeaponSkins;

public partial class MenuService
{
    public IMenu BuildGloveSkinMenu(IPlayer player)
    {
        var main = CreateMenu("weaponskins.gloves")
            .WithTitle(LocalizationService[player].MenuTitleGloves);

        foreach (var (glove, paintkits) in EconService.WeaponToPaintkits)
        {
            var item = EconService.Items[glove];
            if (!Utilities.IsGloveDefinitionIndex(item.Index))
            {
                continue;
            }

            var itemName = EconService.GetLocalizedName(item.LocalizedNames, player.PlayerLanguage.Value);
            var submenuOption = CreateSubmenu(itemName, _ =>
            {
                var skinMenu = CreateMenu("weaponskins.gloves.paintkits").WithTitle(itemName);
                skinMenu.Add(CreateButton(LocalizationService[player].MenuReset,
                    p => Api.ResetGloveSkin(p.SteamID, p.Controller.Team, true)));

                foreach (var paintkit in paintkits.OrderByDescending(p => p.Rarity.Id))
                {
                    var option = CreateRarityButton(
                        EconService.GetLocalizedName(paintkit.LocalizedNames, player.PlayerLanguage.Value),
                        paintkit.Rarity.Color.HexColor,
                        p => Api.UpdateGloveSkin(p.SteamID, p.Controller.Team, glove =>
                        {
                            glove.DefinitionIndex = (ushort)item.Index;
                            glove.Paintkit = paintkit.Index;
                        }, true));
                    option.Tag = paintkit.Index;
                    skinMenu.Add(option);
                }

                var built = skinMenu.Build();
                TrackFocus(built, p => TryGetGloveDataInHand(p, out var inHand) && Utilities.IsGloveDefinitionIndex(inHand.DefinitionIndex) ? inHand.Paintkit : null);
                return built;
            });

            submenuOption.Tag = (ushort)item.Index;
            main.Add(submenuOption);
        }

        var menu = main.Build();
        TrackFocus(menu, p => TryGetGloveDataInHand(p, out var inHand) ? inHand.DefinitionIndex : null);
        return menu;
    }
}