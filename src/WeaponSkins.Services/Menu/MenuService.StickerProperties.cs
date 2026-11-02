using SwiftlyS2.Shared.Menu;
using SwiftlyS2.Shared.Menu.Components;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins;

public partial class MenuService
{
    public IMenu BuildStickerPropertiesMenu(IPlayer player,
        WeaponSkinData data,
        int slot)
    {
        var title = LocalizationService[player].MenuTitleStickerProperties;
        var sticker = data.GetSticker(slot);
        if (sticker == null) return BuildEmptyMenu(Core, $"weaponskins.stickerproperties.slot{slot}", title);

        var main = CreateMenu($"weaponskins.stickerproperties.slot{slot}")
            .WithTitle(title);

        void Update(Action<WeaponSkinData> apply) =>
            Api.UpdateWeaponSkin(data.SteamID, data.Team, data.DefinitionIndex, apply, true);

        main.Add(CreateFloatInput(LocalizationService[player].MenuSkinPropertiesWear, sticker.Wear,
            value => value is >= 0.0f and <= 1.0f,
            (_, value) => Update(skin => skin.GetSticker(slot)?.Wear = value)));

        main.Add(CreateFloatInput(LocalizationService[player].MenuStickerPropertiesOffsetX, sticker.OffsetX,
            float.IsFinite,
            (_, value) => Update(skin =>
            {
                skin.GetSticker(slot)?.OffsetX = value;
                skin.GetSticker(slot)?.Schema = 0;
            })));

        main.Add(CreateFloatInput(LocalizationService[player].MenuStickerPropertiesOffsetY, sticker.OffsetY,
            float.IsFinite,
            (_, value) => Update(skin =>
            {
                skin.GetSticker(slot)?.OffsetY = value;
                skin.GetSticker(slot)?.Schema = 0;
            })));

        main.Add(CreateButton(LocalizationService[player].MenuStickerPropertiesResetOffset, _ => Update(skin =>
        {
            skin.GetSticker(slot)?.OffsetX = 0f;
            skin.GetSticker(slot)?.OffsetY = 0f;
            skin.GetSticker(slot)?.Schema = 1337;
        })));

        return main.Build();
    }

    public IMenuComponent? GetStickerPropertiesMenuSubmenuOption(IPlayer player,
        WeaponSkinData data,
        int slot)
    {
        if (!ItemPermissionService.CanUseStickers(player.SteamID)) return null;
        if (!data.HasSticker(slot)) return null;
        var sticker = data.GetSticker(slot);
        var stickerName = GetStickerName(sticker, player.PlayerLanguage.Value);
        if (stickerName == null) return null;
        return new SubmenuComponent(LocalizationService[player].MenuSkinPropertiesSetSticker(slot, stickerName),
            _ => BuildStickerPropertiesMenu(player, data, slot));
    }
}