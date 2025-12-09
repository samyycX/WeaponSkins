using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins;

public partial class MenuService
{
    public IMenuAPI BuildGlovePropertiesMenu(IPlayer player,
        GloveData gloveInHand)
    {
        var main = Core.MenusAPI.CreateBuilder();
        main.Design.SetMenuTitle(LocalizationService[player].MenuTitleGloveProperties);

        var wearOption = new InputMenuOption(
            LocalizationService[player].MenuSkinPropertiesWear,
            validator: (value) =>
            {
                if (float.TryParse(value, out var result))
                {
                    return result is >= 0.0f and <= 1.0f;
                }

                return false;
            }
        );
        wearOption.SetValue(player, gloveInHand.PaintkitWear.ToString());
        wearOption.ValueChanged += (_,
            args) =>
        {
            gloveInHand.PaintkitWear = float.Parse(args.NewValue);
            Api.UpdateGloveSkin(gloveInHand.SteamID, gloveInHand.Team, skin =>
            {
                skin.PaintkitWear = gloveInHand.PaintkitWear;
            }, true);
        };

        main.AddOption(wearOption);

        var seedOption = new InputMenuOption(
            LocalizationService[player].MenuSkinPropertiesSeed,
            validator: (value) =>
            {
                if (int.TryParse(value, out var result))
                {
                    return result >= 0;
                }

                return false;
            }
        );

        seedOption.SetValue(player, gloveInHand.PaintkitSeed.ToString());

        seedOption.ValueChanged += (_,
            args) =>
        {
            gloveInHand.PaintkitSeed = int.Parse(args.NewValue);

            Api.UpdateGloveSkin(gloveInHand.SteamID, gloveInHand.Team, skin =>
            {
                skin.PaintkitSeed = gloveInHand.PaintkitSeed;
            }, true);
        };

        main.AddOption(seedOption);

        return main.Build();
    }

    public IMenuOption GetGlovePropertiesMenuSubmenuOption(IPlayer player)
    {
        if (!ItemPermissionService.CanUseGloveSkins(player.SteamID))
        {
            return CreateDisabledOption(LocalizationService[player].MenuTitleGloveProperties);
        }

        if (!TryGetGloveDataInHand(player, out var dataInHand))
        {
            return CreateDisabledOption(LocalizationService[player].MenuTitleGloveProperties);
        }

        return new SubmenuMenuOption(LocalizationService[player].MenuTitleGloveProperties,
            () => Task.FromResult(BuildGlovePropertiesMenu(player, dataInHand)));
    }
}
