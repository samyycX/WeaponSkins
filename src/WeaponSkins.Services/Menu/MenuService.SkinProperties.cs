using SwiftlyS2.Shared.Menu;
using SwiftlyS2.Shared.Menu.Components;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins;

public partial class MenuService
{
    public IMenu BuildSkinPropertiesMenu(IPlayer player,
        WeaponSkinData weaponInHand)
    {
        var main = Core.Menu.CreateMenu("weaponskins.skinproperties")
            .WithTitle(LocalizationService[player].MenuTitleSkinProperties);

        void Update(Action<WeaponSkinData> apply) =>
            Api.UpdateWeaponSkin(weaponInHand.SteamID, weaponInHand.Team, weaponInHand.DefinitionIndex, apply, true);

        main.Add(CreateFloatInput(LocalizationService[player].MenuSkinPropertiesWear, weaponInHand.PaintkitWear,
            value => value is >= 0.0f and <= 1.0f,
            (_, value) =>
            {
                weaponInHand.PaintkitWear = value;
                Update(skin => skin.PaintkitWear = value);
            }));

        main.Add(CreateIntInput(LocalizationService[player].MenuSkinPropertiesSeed, weaponInHand.PaintkitSeed,
            value => value >= 0,
            (_, value) =>
            {
                weaponInHand.PaintkitSeed = value;
                Update(skin => skin.PaintkitSeed = value);
            }));

        main.Add(new InputComponent(LocalizationService[player].MenuSkinPropertiesNametag,
            weaponInHand.Nametag ?? LocalizationService[player].MenuSkinPropertiesNametagNone,
            null,
            ctx => Update(skin => skin.Nametag = ctx.NewValue))
        {
            MaxLength = 32
        });

        main.Add(CreateButton(LocalizationService[player].MenuSkinPropertiesNametagUnset,
            _ => Update(skin => skin.Nametag = null)));
        main.Add(CreateButton(LocalizationService[player].MenuSkinPropertiesSetStattrak,
            _ => Update(skin => skin.Quality = EconItemQuality.StatTrak)));
        main.Add(CreateButton(LocalizationService[player].MenuSkinPropertiesUnsetStattrak,
            _ => Update(skin => skin.Quality = EconItemQuality.Normal)));
        main.Add(CreateButton(LocalizationService[player].MenuSkinPropertiesSetSouvenir,
            _ => Update(skin => skin.Quality = EconItemQuality.Souvenir)));
        main.Add(CreateButton(LocalizationService[player].MenuSkinPropertiesUnsetSouvenir,
            _ => Update(skin => skin.Quality = EconItemQuality.Normal)));

        main.Add(CreateIntInput(LocalizationService[player].MenuSkinPropertiesStattrakCount(weaponInHand.StattrakCount),
            weaponInHand.StattrakCount,
            value => value >= 0,
            (_, value) =>
            {
                weaponInHand.StattrakCount = value;
                Update(skin => skin.StattrakCount = value);
            }));

        for (int i = 0; i < 6; i++)
        {
            var option = GetStickerPropertiesMenuSubmenuOption(player, weaponInHand, i);
            if (option != null)
            {
                main.Add(option);
            }
        }

        for (int i = 0; i < 1; i++)
        {
            var option = GetKeychainPropertiesMenuSubmenuOption(player, weaponInHand, i);
            if (option != null)
            {
                main.Add(option);
            }
        }

        return main.Build();
    }

    public IMenuComponent GetSkinPropertiesMenuSubmenuOption(IPlayer player)
    {
        if (!ItemPermissionService.CanUseWeaponSkins(player.SteamID))
        {
            return CreateDisabledOption(LocalizationService[player].MenuTitleSkinProperties);
        }

        if (!TryGetWeaponDataInHand(player, out var weaponInHand))
        {
            return CreateDisabledOption(LocalizationService[player].MenuTitleSkinProperties);
        }

        return new SubmenuComponent(LocalizationService[player].MenuTitleSkinProperties,
            _ => BuildSkinPropertiesMenu(player, weaponInHand));
    }
}