using SwiftlyS2.Shared.Menu;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins;

public partial class MenuService
{
    public IMenu BuildKeychainPropertiesMenu(IPlayer player,
        Team team,
        ushort definition,
        int slot)
    {
        var loc = LocalizationService[player];
        var main = CreateMenu($"weaponskins.keychainproperties.slot{slot}")
            .WithTitle(loc.MenuTitleKeychainProperties);

        if (!Api.TryGetWeaponSkin(player.SteamID, team, definition, out var data) || data.GetKeychain(slot) is not { } keychain)
        {
            AddEmptyNotice(main, loc);
            return main.Build();
        }

        void Update(Action<WeaponSkinData> apply) =>
            Api.UpdateWeaponSkin(player.SteamID, team, definition, apply, true);

        main.Add(CreateIntInput(loc.MenuSkinPropertiesSeed, keychain.Seed,
            value => value >= 0,
            (_, value) => Update(skin => skin.GetKeychain(slot)?.Seed = value)));

        main.Add(CreateFloatInput(loc.MenuKeychainPropertiesOffsetX, keychain.OffsetX,
            float.IsFinite,
            (_, value) => Update(skin => skin.GetKeychain(slot)?.OffsetX = value)));

        main.Add(CreateFloatInput(loc.MenuKeychainPropertiesOffsetY, keychain.OffsetY,
            float.IsFinite,
            (_, value) => Update(skin => skin.GetKeychain(slot)?.OffsetY = value)));

        main.Add(CreateFloatInput(loc.MenuKeychainPropertiesOffsetZ, keychain.OffsetZ,
            float.IsFinite,
            (_, value) => Update(skin => skin.GetKeychain(slot)?.OffsetZ = value)));

        main.Add(CreateButton(loc.MenuKeychainPropertiesResetOffset, _ => Update(skin =>
        {
            skin.GetKeychain(slot)?.OffsetX = 0f;
            skin.GetKeychain(slot)?.OffsetY = 0f;
            skin.GetKeychain(slot)?.OffsetZ = 0f;
        })));

        return main.Build();
    }
}