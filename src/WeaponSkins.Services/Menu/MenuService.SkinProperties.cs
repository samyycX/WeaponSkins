using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins;

public partial class MenuService
{
    public IMenuAPI BuildSkinPropertiesMenu(IPlayer player,
        WeaponSkinData weaponInHand)
    {
        var main = Core.MenusAPI.CreateBuilder();
        main.Design.SetMenuTitle(LocalizationService[player].MenuTitleSkinProperties);

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

        wearOption.SetValue(player, weaponInHand.PaintkitWear.ToString());

        wearOption.ValueChanged += (_,
            args) =>
        {
            weaponInHand.PaintkitWear = float.Parse(args.NewValue);
            Api.UpdateWeaponSkin(weaponInHand.SteamID, weaponInHand.Team, weaponInHand.DefinitionIndex, skin =>
            {
                skin.PaintkitWear = weaponInHand.PaintkitWear;
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
        seedOption.SetValue(player, weaponInHand.PaintkitSeed.ToString());
        seedOption.ValueChanged += (_,
            args) =>
        {
            weaponInHand.PaintkitSeed = int.Parse(args.NewValue);
            Api.UpdateWeaponSkin(weaponInHand.SteamID, weaponInHand.Team, weaponInHand.DefinitionIndex, skin =>
            {
                skin.PaintkitSeed = weaponInHand.PaintkitSeed;
            }, true);
        };

        main.AddOption(seedOption);


        var nametagOption = new InputMenuOption(
            LocalizationService[player].MenuSkinPropertiesNametag
        );

        nametagOption.SetValue(player,
            weaponInHand.Nametag ?? LocalizationService[player].MenuSkinPropertiesNametagNone);

        nametagOption.ValueChanged += (_,
            args) =>
        {
            Api.UpdateWeaponSkin(weaponInHand.SteamID, weaponInHand.Team, weaponInHand.DefinitionIndex, skin =>
            {
                skin.Nametag = args.NewValue;
            }, true);
        };

        main.AddOption(nametagOption);

        var unsetNametagOption = new ButtonMenuOption(LocalizationService[player].MenuSkinPropertiesNametagUnset);
        unsetNametagOption.Click += (_,
            args) =>
        {
            Api.UpdateWeaponSkin(weaponInHand.SteamID, weaponInHand.Team, weaponInHand.DefinitionIndex, skin =>
            {
                skin.Nametag = null;
            }, true);
            return ValueTask.CompletedTask;
        };

        main.AddOption(unsetNametagOption);

        var setStattrakOption = new ButtonMenuOption(LocalizationService[player].MenuSkinPropertiesSetStattrak);
        setStattrakOption.Click += (_,
            args) =>
        {
            Api.UpdateWeaponSkin(weaponInHand.SteamID, weaponInHand.Team, weaponInHand.DefinitionIndex, skin =>
            {
                skin.Quality = EconItemQuality.StatTrak;
            }, true);
            return ValueTask.CompletedTask;
        };

        main.AddOption(setStattrakOption);

        var unsetSouvenirOption = new ButtonMenuOption(LocalizationService[player].MenuSkinPropertiesUnsetSouvenir);
        unsetSouvenirOption.Click += (_,
            args) =>
        {
            Api.UpdateWeaponSkin(weaponInHand.SteamID, weaponInHand.Team, weaponInHand.DefinitionIndex, skin =>
            {
                skin.Quality = EconItemQuality.Normal;
            }, true);
            return ValueTask.CompletedTask;
        };

        main.AddOption(unsetSouvenirOption);

        var setSouvenirOption = new ButtonMenuOption(LocalizationService[player].MenuSkinPropertiesSetSouvenir);
        setSouvenirOption.Click += (_,
            args) =>
        {
            Api.UpdateWeaponSkin(weaponInHand.SteamID, weaponInHand.Team, weaponInHand.DefinitionIndex, skin =>
            {
                skin.Quality = EconItemQuality.Souvenir;
            }, true);
            return ValueTask.CompletedTask;
        };

        main.AddOption(setSouvenirOption);

        var unsetStattrakOption = new ButtonMenuOption(LocalizationService[player].MenuSkinPropertiesUnsetStattrak);
        unsetStattrakOption.Click += (_,
            args) =>
        {
            Api.UpdateWeaponSkin(weaponInHand.SteamID, weaponInHand.Team, weaponInHand.DefinitionIndex, skin =>
            {
                skin.Quality = EconItemQuality.Normal;
            }, true);   
            return ValueTask.CompletedTask;
        };

        main.AddOption(unsetStattrakOption);

        var setStattrakCountOption = new InputMenuOption(
            LocalizationService[player].MenuSkinPropertiesStattrakCount(weaponInHand.StattrakCount),
            validator: (value) =>
            {
                if (int.TryParse(value, out var result))
                {
                    return result >= 0;
                }

                return false;
            }
        );
        setStattrakCountOption.SetValue(player, weaponInHand.StattrakCount.ToString());
        setStattrakCountOption.ValueChanged += (_,
            args) =>
        {
            weaponInHand.StattrakCount = int.Parse(args.NewValue);
            Api.UpdateWeaponSkin(weaponInHand.SteamID, weaponInHand.Team, weaponInHand.DefinitionIndex, skin =>
            {
                skin.StattrakCount = weaponInHand.StattrakCount;
            }, true);
        };
        main.AddOption(setStattrakCountOption);

        for (int i = 0; i < 6; i++)
        {
            var option = GetStickerPropertiesMenuSubmenuOption(player, weaponInHand, i);
            if (option != null)
            {
                main.AddOption(option);
            }
        }

        for (int i = 0; i < 1; i++)
        {
            var option = GetKeychainPropertiesMenuSubmenuOption(player, weaponInHand, i);
            if (option != null)
            {
                main.AddOption(option);
            }
        }

        return main.Build();
    }

    public IMenuOption GetSkinPropertiesMenuSubmenuOption(IPlayer player)
    {
        if (!ItemPermissionService.CanUseWeaponSkins(player.SteamID))
        {
            return CreateDisabledOption(LocalizationService[player].MenuTitleSkinProperties);
        }

        if (!TryGetWeaponDataInHand(player, out var weaponInHand))
        {
            return CreateDisabledOption(LocalizationService[player].MenuTitleSkinProperties);
        }

        return new SubmenuMenuOption(LocalizationService[player].MenuTitleSkinProperties,
            () => Task.FromResult(BuildSkinPropertiesMenu(player, weaponInHand)));
    }
}
