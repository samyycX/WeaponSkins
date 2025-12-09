using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins;

public partial class MenuService
{
    public IMenuAPI BuildKeychainPropertiesMenu(IPlayer player,
        WeaponSkinData data,
        int slot)
    {
        var main = Core.MenusAPI.CreateBuilder();
        main.Design.SetMenuTitle(LocalizationService[player].MenuTitleKeychainProperties);

        var keychain = data.GetKeychain(slot);

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
        seedOption.SetValue(player, keychain.Seed.ToString());
        seedOption.ValueChanged += (_,
            args) =>
        {
            var value = int.Parse(args.NewValue);
            Api.UpdateWeaponSkin(data.SteamID, data.Team, data.DefinitionIndex, skin =>
            {
                skin.GetKeychain(slot)?.Seed = value;
            }, true);
        };
        main.AddOption(seedOption);

        var offsetXOption = new InputMenuOption(
            LocalizationService[player].MenuKeychainPropertiesOffsetX,
            validator: (value) =>
            {
                if (float.TryParse(value, out var result))
                {
                    return true;
                }

                return false;
            }
        );
        offsetXOption.SetValue(player, keychain.OffsetX.ToString());
        offsetXOption.ValueChanged += (_,
            args) =>
        {
            var value = float.Parse(args.NewValue);
            Api.UpdateWeaponSkin(data.SteamID, data.Team, data.DefinitionIndex, skin =>
            {
                skin.GetKeychain(slot)?.OffsetX = value;
            }, true);
        };
        main.AddOption(offsetXOption);

        var offsetYOption = new InputMenuOption(
            LocalizationService[player].MenuKeychainPropertiesOffsetY,
            validator: (value) =>
            {
                if (float.TryParse(value, out var result))
                {
                    return true;
                }

                return false;
            }
        );
        offsetYOption.SetValue(player, keychain.OffsetY.ToString());
        offsetYOption.ValueChanged += (_,
            args) =>
        {
            var value = float.Parse(args.NewValue);
            Api.UpdateWeaponSkin(data.SteamID, data.Team, data.DefinitionIndex, skin =>
            {
                skin.GetKeychain(slot)?.OffsetY = value;
            }, true);
        };
        main.AddOption(offsetYOption);

        var offsetZOption = new InputMenuOption(
            LocalizationService[player].MenuKeychainPropertiesOffsetZ,
            validator: (value) =>
            {
                if (float.TryParse(value, out var result))
                {
                    return true;
                }

                return false;
            }
        );
        offsetZOption.SetValue(player, keychain.OffsetZ.ToString());
        offsetZOption.ValueChanged += (_,
            args) =>
        {
            var value = float.Parse(args.NewValue);
            Api.UpdateWeaponSkin(data.SteamID, data.Team, data.DefinitionIndex, skin =>
            {
                skin.GetKeychain(slot)?.OffsetZ = value;
            }, true);
        };
        main.AddOption(offsetZOption);

        var resetOffsetOption = new ButtonMenuOption(LocalizationService[player].MenuKeychainPropertiesResetOffset);
        resetOffsetOption.Click += (_,
            args) =>
        {
            Api.UpdateWeaponSkin(data.SteamID, data.Team, data.DefinitionIndex, skin =>
            {
                skin.GetKeychain(slot)?.OffsetX = 0f;
                skin.GetKeychain(slot)?.OffsetY = 0f;
                skin.GetKeychain(slot)?.OffsetZ = 0f;
            }, true);
            return ValueTask.CompletedTask;
        };
        main.AddOption(resetOffsetOption);

        return main.Build();
    }

    public IMenuOption? GetKeychainPropertiesMenuSubmenuOption(IPlayer player,
        WeaponSkinData data,
        int slot)
    {
        if (!ItemPermissionService.CanUseKeychains(player.SteamID)) return null;
        if (!data.HasKeychain(slot)) return null;
        var keychain = data.GetKeychain(slot);
        var keychainName = GetKeychainName(keychain, GetLanguage(player));
        if (keychainName == null) return null;
        return new SubmenuMenuOption(LocalizationService[player].MenuSkinPropertiesSetKeychain(slot, keychainName),
            () => Task.FromResult(BuildKeychainPropertiesMenu(player, data, slot)));
    }
}
