using System.Diagnostics.CodeAnalysis;

using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

using WeaponSkins.Econ;
using WeaponSkins.Shared;

namespace WeaponSkins;

public partial class MenuService
{
    private Dictionary<int, StickerDefinition> StickerDefinitions { get; set; } = new();
    private Dictionary<ulong /* steamid */, WeaponSkinData> _stickerOperatingWeaponSkins = new();

    private string? GetStickerName(StickerData? data,
        string language)
    {
        if (data == null) return null;
        if (data.Id == 0) return null;
        if (!StickerDefinitions.TryGetValue(data.Id, out var definition))
        {
            foreach (var (_, stickers) in EconService.StickerCollections)
            {
                foreach (var sticker in stickers.Stickers)
                {
                    if (sticker.Index == data.Id)
                    {
                        definition = sticker;
                        StickerDefinitions[data.Id] = definition;
                        break;
                    }
                }
            }
        }

        if (definition == null) return null;

        return definition.LocalizedNames[language];
    }

    private IMenuAPI BuildStickerMenuBySlot(IPlayer player,
        int slot,
        string language,
        string title)
    {
        var main = Core.MenusAPI.CreateBuilder();

        main.Design.SetMenuTitle(title);
        var resetOption = new ButtonMenuOption(LocalizationService[player].MenuReset);
        resetOption.Click += (_,
            args) =>
        {
            if (!_stickerOperatingWeaponSkins.TryGetValue(args.Player.SteamID, out var dataInHand))
            {
                return ValueTask.CompletedTask;
            }

            Api.UpdateWeaponSkin(args.Player.SteamID, args.Player.Controller.Team, dataInHand.DefinitionIndex,
                skin =>
                {
                    skin.SetSticker(slot, null);
                }, true);
            return ValueTask.CompletedTask;
        };

        main.AddOption(resetOption);
        foreach (var (_, stickerCollection) in EconService.StickerCollections)
        {
            main.AddOption(new SubmenuMenuOption(stickerCollection.LocalizedNames[language], () =>
            {
                var stickerMenu = Core.MenusAPI.CreateBuilder();
                stickerMenu.Design.SetMenuTitle(stickerCollection.LocalizedNames[language]);

                foreach (var sticker in stickerCollection.Stickers)
                {
                    if (sticker.Index == 0) continue;
                    var option = new ButtonMenuOption(HtmlGradient.GenerateGradientText(
                        sticker.LocalizedNames[language],
                        sticker.Rarity.Color.HexColor));
                    option.Click += (_,
                        args) =>
                    {
                        if (!_stickerOperatingWeaponSkins.TryGetValue(args.Player.SteamID, out var dataInHand))
                        {
                            return ValueTask.CompletedTask;
                        }

                        Api.UpdateWeaponSkin(args.Player.SteamID, args.Player.Controller.Team,
                            dataInHand.DefinitionIndex,
                            skin =>
                            {
                                skin.SetSticker(slot, new StickerData { Id = sticker.Index, });
                            }, true);
                        return ValueTask.CompletedTask;
                    };
                    stickerMenu.AddOption(option);
                }

                return Task.FromResult(stickerMenu.Build());
            }));
        }

        return main.Build();
    }

    public IMenuAPI BuildStickerMenu(IPlayer player)
    {
        var language = GetLanguage(player);

        var main = Core.MenusAPI.CreateBuilder();
        main.Design.SetMenuTitle(LocalizationService[player].MenuTitleStickers);

        for (int i = 0; i < 6; i++)
        {
            var title =
                $"[{i + 1}] Slot";
            var slot = i;
            main.AddOption(new SubmenuMenuOption(title,
                () => Task.FromResult(BuildStickerMenuBySlot(player, slot, language, title))));
        }

        var menu = main.Build();
        return menu;
    }


    public IMenuOption GetStickerMenuSubmenuOption(IPlayer player)
    {
        if (!ItemPermissionService.CanUseStickers(player.SteamID))
        {
            return CreateDisabledOption(LocalizationService[player].MenuTitleStickers);
        }

        if (!TryGetWeaponDataInHand(player, out var dataInHand))
        {
            return CreateDisabledOption(LocalizationService[player].MenuTitleStickers);
        }

        _stickerOperatingWeaponSkins[player.SteamID] = dataInHand;
        return new SubmenuMenuOption(LocalizationService[player].MenuTitleStickers,
            () => Task.FromResult(BuildStickerMenu(player)));
    }
}
