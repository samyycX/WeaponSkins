using SwiftlyS2.Shared.Menu;
using SwiftlyS2.Shared.Menu.Components;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins;

public partial class MenuService
{
    public IMenu BuildGlovePropertiesMenu(IPlayer player,
        GloveData gloveInHand)
    {
        var main = Core.Menu.CreateMenu("weaponskins.gloveproperties")
            .WithTitle(LocalizationService[player].MenuTitleGloveProperties);

        main.Add(CreateFloatInput(LocalizationService[player].MenuSkinPropertiesWear, gloveInHand.PaintkitWear,
            value => value is >= 0.0f and <= 1.0f,
            (_, value) =>
            {
                gloveInHand.PaintkitWear = value;
                Api.UpdateGloveSkin(gloveInHand.SteamID, gloveInHand.Team, skin => skin.PaintkitWear = value, true);
            }));

        main.Add(CreateIntInput(LocalizationService[player].MenuSkinPropertiesSeed, gloveInHand.PaintkitSeed,
            value => value >= 0,
            (_, value) =>
            {
                gloveInHand.PaintkitSeed = value;
                Api.UpdateGloveSkin(gloveInHand.SteamID, gloveInHand.Team, skin => skin.PaintkitSeed = value, true);
            }));

        return main.Build();
    }

    public IMenuComponent GetGlovePropertiesMenuSubmenuOption(IPlayer player)
    {
        if (!ItemPermissionService.CanUseGloveSkins(player.SteamID))
        {
            return CreateDisabledOption(LocalizationService[player].MenuTitleGloveProperties);
        }

        if (!TryGetGloveDataInHand(player, out var dataInHand))
        {
            return CreateDisabledOption(LocalizationService[player].MenuTitleGloveProperties);
        }

        return new SubmenuComponent(LocalizationService[player].MenuTitleGloveProperties,
            _ => BuildGlovePropertiesMenu(player, dataInHand));
    }
}