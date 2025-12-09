using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins;

public partial class MenuService
{
    public IMenuAPI BuildStickerPropertiesMenu(IPlayer player,
        WeaponSkinData data,
        int slot)
    {
        var main = Core.MenusAPI.CreateBuilder();
        main.Design.SetMenuTitle(LocalizationService[player].MenuTitleStickerProperties);

        var sticker = data.GetSticker(slot);

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

        wearOption.SetValue(player, sticker.Wear.ToString());
        wearOption.ValueChanged += (_,
            args) =>
        {
            var value = float.Parse(args.NewValue);
            Api.UpdateWeaponSkin(data.SteamID, data.Team, data.DefinitionIndex, skin =>
            {
                skin.GetSticker(slot)?.Wear = value;
            }, true);
        };
        main.AddOption(wearOption);

        var offsetXOption = new InputMenuOption(
            LocalizationService[player].MenuStickerPropertiesOffsetX,
            validator: (value) =>
            {
                if (float.TryParse(value, out var result))
                {
                    return true;
                }

                return false;
            }
        );
        offsetXOption.SetValue(player, sticker.OffsetX.ToString());
        offsetXOption.ValueChanged += (_,
            args) =>
        {
            var value = float.Parse(args.NewValue);
            Api.UpdateWeaponSkin(data.SteamID, data.Team, data.DefinitionIndex, skin =>
            {
                skin.GetSticker(slot)?.OffsetX = value;
                skin.GetSticker(slot)?.Schema = 0;
            }, true);
        };
        main.AddOption(offsetXOption);

        var offsetYOption = new InputMenuOption(
            LocalizationService[player].MenuStickerPropertiesOffsetY,
            validator: (value) =>
            {
                if (float.TryParse(value, out var result))
                {
                    return true;
                }

                return false;
            }
        );
        offsetYOption.SetValue(player, sticker.OffsetY.ToString());
        offsetYOption.ValueChanged += (_,
            args) =>
        {
            var value = float.Parse(args.NewValue);
            Api.UpdateWeaponSkin(data.SteamID, data.Team, data.DefinitionIndex, skin =>
            {
                skin.GetSticker(slot)?.OffsetY = value;
                skin.GetSticker(slot)?.Schema = 0;
            }, true);
        };
        main.AddOption(offsetYOption);

        var resetOffsetOption = new ButtonMenuOption(LocalizationService[player].MenuStickerPropertiesResetOffset);
        resetOffsetOption.Click += (_,
            args) =>
        {
            Api.UpdateWeaponSkin(data.SteamID, data.Team, data.DefinitionIndex, skin =>
            {
                skin.GetSticker(slot)?.OffsetX = 0f;
                skin.GetSticker(slot)?.OffsetY = 0f;
                skin.GetSticker(slot)?.Schema = 1337;
            }, true);
            return ValueTask.CompletedTask;
        };
        main.AddOption(resetOffsetOption);

        return main.Build();
    }

    public IMenuOption? GetStickerPropertiesMenuSubmenuOption(IPlayer player,
        WeaponSkinData data,
        int slot)
    {
        if (!ItemPermissionService.CanUseStickers(player.SteamID)) return null;
        if (!data.HasSticker(slot)) return null;
        var sticker = data.GetSticker(slot);
        var stickerName = GetStickerName(sticker, GetLanguage(player));
        if (stickerName == null) return null;
        return new SubmenuMenuOption(LocalizationService[player].MenuSkinPropertiesSetSticker(slot, stickerName),
            () => Task.FromResult(BuildStickerPropertiesMenu(player, data, slot)));
    }
}
