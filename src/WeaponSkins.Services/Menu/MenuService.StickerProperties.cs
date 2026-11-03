using SwiftlyS2.Shared.Menu;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins;

public partial class MenuService
{
    public IMenu BuildStickerPropertiesMenu(IPlayer player,
        Team team,
        ushort definition,
        int slot)
    {
        var loc = LocalizationService[player];
        var main = CreateMenu($"weaponskins.stickerproperties.slot{slot}")
            .WithTitle(loc.MenuTitleStickerProperties);

        if (!Api.TryGetWeaponSkin(player.SteamID, team, definition, out var data) || data.GetSticker(slot) is not { } sticker)
        {
            AddEmptyNotice(main, loc);
            return main.Build();
        }

        void Update(Action<WeaponSkinData> apply) =>
            Api.UpdateWeaponSkin(player.SteamID, team, definition, apply, true);

        main.Add(CreateFloatInput(loc.MenuSkinPropertiesWear, sticker.Wear,
            value => value is >= 0.0f and <= 1.0f,
            (_, value) => Update(skin => skin.GetSticker(slot)?.Wear = value)));

        main.Add(CreateFloatInput(loc.MenuStickerPropertiesOffsetX, sticker.OffsetX,
            float.IsFinite,
            (_, value) => Update(skin =>
            {
                skin.GetSticker(slot)?.OffsetX = value;
                skin.GetSticker(slot)?.Schema = 0;
            })));

        main.Add(CreateFloatInput(loc.MenuStickerPropertiesOffsetY, sticker.OffsetY,
            float.IsFinite,
            (_, value) => Update(skin =>
            {
                skin.GetSticker(slot)?.OffsetY = value;
                skin.GetSticker(slot)?.Schema = 0;
            })));

        main.Add(CreateButton(loc.MenuStickerPropertiesResetOffset, _ => Update(skin =>
        {
            skin.GetSticker(slot)?.OffsetX = 0f;
            skin.GetSticker(slot)?.OffsetY = 0f;
            skin.GetSticker(slot)?.Schema = 1337;
        })));

        return main.Build();
    }
}