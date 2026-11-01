using SwiftlyS2.Shared.Menu;
using SwiftlyS2.Shared.Menu.Components;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins;

public partial class MenuService
{
    public IMenu BuildKeychainPropertiesMenu(IPlayer player,
        WeaponSkinData data,
        int slot)
    {
        var title = LocalizationService[player].MenuTitleKeychainProperties;
        var keychain = data.GetKeychain(slot);
        if (keychain == null) return BuildEmptyMenu(Core, $"weaponskins.keychainproperties.slot{slot}", title);

        var main = Core.Menu.CreateMenu($"weaponskins.keychainproperties.slot{slot}")
            .WithTitle(title);

        void Update(Action<WeaponSkinData> apply) =>
            Api.UpdateWeaponSkin(data.SteamID, data.Team, data.DefinitionIndex, apply, true);

        main.Add(CreateIntInput(LocalizationService[player].MenuSkinPropertiesSeed, keychain.Seed,
            value => value >= 0,
            (_, value) => Update(skin => skin.GetKeychain(slot)?.Seed = value)));

        main.Add(CreateFloatInput(LocalizationService[player].MenuKeychainPropertiesOffsetX, keychain.OffsetX,
            float.IsFinite,
            (_, value) => Update(skin => skin.GetKeychain(slot)?.OffsetX = value)));

        main.Add(CreateFloatInput(LocalizationService[player].MenuKeychainPropertiesOffsetY, keychain.OffsetY,
            float.IsFinite,
            (_, value) => Update(skin => skin.GetKeychain(slot)?.OffsetY = value)));

        main.Add(CreateFloatInput(LocalizationService[player].MenuKeychainPropertiesOffsetZ, keychain.OffsetZ,
            float.IsFinite,
            (_, value) => Update(skin => skin.GetKeychain(slot)?.OffsetZ = value)));

        main.Add(CreateButton(LocalizationService[player].MenuKeychainPropertiesResetOffset, _ => Update(skin =>
        {
            skin.GetKeychain(slot)?.OffsetX = 0f;
            skin.GetKeychain(slot)?.OffsetY = 0f;
            skin.GetKeychain(slot)?.OffsetZ = 0f;
        })));

        return main.Build();
    }

    public IMenuComponent? GetKeychainPropertiesMenuSubmenuOption(IPlayer player,
        WeaponSkinData data,
        int slot)
    {
        if (!ItemPermissionService.CanUseKeychains(player.SteamID)) return null;
        if (!data.HasKeychain(slot)) return null;
        var keychain = data.GetKeychain(slot);
        if (keychain == null) return null;
        var keychainName = GetKeychainName(keychain, player.PlayerLanguage.Value);
        if (keychainName == null) return null;
        return new SubmenuComponent(LocalizationService[player].MenuSkinPropertiesSetKeychain(slot, keychainName),
            _ => BuildKeychainPropertiesMenu(player, data, slot));
    }
}