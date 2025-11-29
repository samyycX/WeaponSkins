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
    private Dictionary<string /* language */, IMenuAPI> _cachedStickerMenus = new();
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

        foreach (var (index, stickerCollection) in EconService.StickerCollections)
        {
            var stickerMenu = Core.MenusAPI.CreateBuilder();
            stickerMenu.Design.SetMenuTitle(stickerCollection.LocalizedNames[language]);

            foreach (var sticker in stickerCollection.Stickers)
            {
                if (sticker.Index == 0) continue;
                var option = new ButtonMenuOption(HtmlGradient.GenerateGradientText(sticker.LocalizedNames[language],
                    sticker.Rarity.Color.HexColor));
                option.Click += (_,
                    args) =>
                {
                    if (!_stickerOperatingWeaponSkins.TryGetValue(args.Player.SteamID, out var dataInHand))
                    {
                        return ValueTask.CompletedTask;
                    }

                    Api.UpdateWeaponSkin(args.Player.SteamID, args.Player.Controller.Team, dataInHand.DefinitionIndex,
                        skin =>
                        {
                            var stickerData = new StickerData { Id = sticker.Index, };
                            switch (slot)
                            {
                                case 0: skin.Sticker0 = stickerData; break;
                                case 1: skin.Sticker1 = stickerData; break;
                                case 2: skin.Sticker2 = stickerData; break;
                                case 3: skin.Sticker3 = stickerData; break;
                                case 4: skin.Sticker4 = stickerData; break;
                                case 5: skin.Sticker5 = stickerData; break;
                            }
                        }, true);
                    return ValueTask.CompletedTask;
                };
                stickerMenu.AddOption(option);
            }

            main.AddOption(new SubmenuMenuOption(stickerCollection.LocalizedNames[language], stickerMenu.Build()));
        }

        return main.Build();
    }

    public IMenuAPI BuildStickerMenu(IPlayer player)
    {
        var language = GetLanguage(player);
        if (_cachedStickerMenus.TryGetValue(language, out var cachedMenu))
        {
            return cachedMenu;
        }

        var main = Core.MenusAPI.CreateBuilder();
        main.Design.SetMenuTitle(LocalizationService[player].MenuTitleStickers);
        for (int i = 0; i < 6; i++)
        {
            var title =
                $"[{i + 1}] Slot";
            main.AddOption(new SubmenuMenuOption(
                title
                , BuildStickerMenuBySlot(player, i, language, title)));
        }

        var menu = main.Build();
        _cachedStickerMenus[language] = menu;
        return menu;
    }


    public IMenuOption GetStickerMenuSubmenuOption(IPlayer player)
    {
        if (!TryGetWeaponDataInHand(player, out var dataInHand))
        {
            var option = new TextMenuOption(LocalizationService[player].MenuTitleStickers);
            option.Enabled = false;
            return option;
        }

        _stickerOperatingWeaponSkins[player.SteamID] = dataInHand;
        return new SubmenuMenuOption(LocalizationService[player].MenuTitleStickers, BuildStickerMenu(player));
    }
}