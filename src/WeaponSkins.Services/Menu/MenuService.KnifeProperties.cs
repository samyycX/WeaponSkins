using SwiftlyS2.Shared.Menu;
using SwiftlyS2.Shared.Menu.Components;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins;

public partial class MenuService
{
    public IMenu BuildKnifePropertiesMenu(IPlayer player,
        KnifeSkinData weaponInHand)
    {
        var main = CreateMenu("weaponskins.knifeproperties")
            .WithTitle(LocalizationService[player].MenuTitleKnifeProperties);

        void Update(Action<KnifeSkinData> apply) =>
            Api.UpdateKnifeSkin(weaponInHand.SteamID, weaponInHand.Team, apply, true);

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

        main.Add(CreateIntInput(LocalizationService[player].MenuSkinPropertiesStattrakCount(weaponInHand.StattrakCount),
            weaponInHand.StattrakCount,
            value => value >= 0,
            (_, value) =>
            {
                weaponInHand.StattrakCount = value;
                Update(skin => skin.StattrakCount = value);
            }));

        return main.Build();
    }

    public IMenuComponent GetKnifePropertiesMenuSubmenuOption(IPlayer player)
    {
        if (!ItemPermissionService.CanUseKnifeSkins(player.SteamID))
        {
            return CreateDisabledOption(LocalizationService[player].MenuTitleKnifeProperties);
        }

        if (!TryGetKnifeDataInHand(player, out var dataInHand))
        {
            return CreateDisabledOption(LocalizationService[player].MenuTitleKnifeProperties);
        }

        return new SubmenuComponent(LocalizationService[player].MenuTitleKnifeProperties,
            _ => BuildKnifePropertiesMenu(player, dataInHand));
    }
}