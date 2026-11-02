using System.Collections.Concurrent;

using SwiftlyS2.Shared.Menu;
using SwiftlyS2.Shared.Menu.Components;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Econ;
using WeaponSkins.Shared;

namespace WeaponSkins;

public partial class MenuService
{
    private ConcurrentDictionary<int, StickerDefinition> StickerDefinitions { get; set; } = new();
    private readonly ConcurrentDictionary<ulong /* steamid */, WeaponSkinData> _stickerOperatingWeaponSkins = new();

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

        return EconService.GetLocalizedName(definition.LocalizedNames, language);
    }

    private void UpdateOperatingSticker(IPlayer player, int slot, StickerData? sticker)
    {
        if (!_stickerOperatingWeaponSkins.TryGetValue(player.SteamID, out var dataInHand))
        {
            return;
        }

        Api.UpdateWeaponSkin(player.SteamID, player.Controller.Team, dataInHand.DefinitionIndex,
            skin => skin.SetSticker(slot, sticker), true);
    }

    private IMenu BuildStickerMenuBySlot(IPlayer player,
        int slot,
        string language,
        string title)
    {
        var main = CreateMenu($"weaponskins.stickers.slot{slot}")
            .WithTitle(title);

        main.Add(CreateButton(LocalizationService[player].MenuReset, p => UpdateOperatingSticker(p, slot, null)));

        foreach (var (_, stickerCollection) in EconService.StickerCollections)
        {
            var collectionName = EconService.GetLocalizedName(stickerCollection.LocalizedNames, language);
            main.Add(new SubmenuComponent(collectionName, _ =>
            {
                var stickerMenu = CreateMenu($"weaponskins.stickers.slot{slot}.collection")
                    .WithTitle(collectionName);

                var typeLabel = LocalizationService[player].MenuTypeSticker;
                foreach (var sticker in stickerCollection.Stickers)
                {
                    if (sticker.Index == 0) continue;
                    var stickerIndex = sticker.Index;
                    stickerMenu.Add(CreateRarityButton(
                        EconService.GetLocalizedName(sticker.LocalizedNames, language),
                        sticker.Rarity.Color.HexColor,
                        p => UpdateOperatingSticker(p, slot, new StickerData { Id = stickerIndex }),
                        typeLabel: typeLabel));
                }

                return stickerMenu.Build();
            }));
        }

        return main.Build();
    }

    public IMenu BuildStickerMenu(IPlayer player)
    {
        var main = CreateMenu("weaponskins.stickers")
            .WithTitle(LocalizationService[player].MenuTitleStickers);

        for (int i = 0; i < 6; i++)
        {
            var title = $"[{i + 1}] Slot";
            var slot = i;
            main.Add(new SubmenuComponent(title,
                _ => BuildStickerMenuBySlot(player, slot, player.PlayerLanguage.Value, title)));
        }

        var menu = main.Build();
        TrackFocus(menu);
        return menu;
    }

    public IMenuComponent GetStickerMenuSubmenuOption(IPlayer player)
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
        return new SubmenuComponent(LocalizationService[player].MenuTitleStickers,
            _ => BuildStickerMenu(player));
    }
}