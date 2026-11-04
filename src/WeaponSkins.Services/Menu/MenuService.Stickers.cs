using System.Collections.Concurrent;

using SwiftlyS2.Shared.Menu;
using SwiftlyS2.Shared.Menu.Components;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Econ;
using WeaponSkins.Shared;

namespace WeaponSkins;

public partial class MenuService
{
    private const int StickerSlots = 6;

    private ConcurrentDictionary<int, StickerDefinition> StickerDefinitions { get; set; } = new();

    private StickerDefinition? FindSticker(StickerData? data)
    {
        if (data == null) return null;
        if (data.Id == 0) return null;
        if (StickerDefinitions.TryGetValue(data.Id, out var definition)) return definition;

        foreach (var (_, stickers) in EconService.StickerCollections)
        {
            foreach (var sticker in stickers.Stickers)
            {
                if (sticker.Index == data.Id)
                {
                    StickerDefinitions[data.Id] = sticker;
                    return sticker;
                }
            }
        }

        return null;
    }

    public IMenu BuildStickerMenu(IPlayer player,
        Team team,
        ushort definition)
    {
        var loc = LocalizationService[player];
        var language = player.PlayerLanguage.Value;
        var main = CreateMenu("weaponskins.stickers")
            .WithTitle(loc.MenuTitleStickers);
        IMenu slots = null!;

        for (var i = 0; i < StickerSlots; i++)
        {
            var slot = i;
            StickerDefinition? Applied() => Api.TryGetWeaponSkin(player.SteamID, team, definition, out var skin)
                ? FindSticker(skin.GetSticker(slot))
                : null;
            // Read on every draw: the page this opens changes the sticker it names.
            string Title() => loc.MenuSkinPropertiesSetSticker(slot + 1,
                (Applied() is { } sticker ? EconService.GetLocalizedName(sticker.LocalizedNames, language) : null)
                ?? loc.MenuSkinPropertiesNametagNone);

            // A row's Tag is the item it stands for, so any renderer can identify it.
            main.Add(new SubmenuComponent(Title(), _ => BuildStickerSlotMenu(player, team, definition, slot, Title(), slots))
            {
                TextProvider = _ => Title(),
                Tag = slot
            });
        }

        slots = main.Build();
        TrackFocus(slots);
        return slots;
    }

    private IMenu BuildStickerSlotMenu(IPlayer player,
        Team team,
        ushort definition,
        int slot,
        string title,
        IMenu slots)
    {
        var loc = LocalizationService[player];
        var language = player.PlayerLanguage.Value;
        var main = CreateMenu($"weaponskins.stickers.slot{slot}")
            .WithTitle(title);

        // Picking or clearing the sticker finishes the slot, so the player lands back on the slot list.
        // Open rather than Close: Close would reopen the capsule list in between.
        void Apply(StickerData? sticker)
        {
            Api.UpdateWeaponSkin(player.SteamID, team, definition, skin => skin.SetSticker(slot, sticker), true);
            _ = slots.Open(player);
        }

        main.Add(CreateButton(loc.MenuReset, _ => Apply(null)));
        main.Add(new SubmenuComponent(loc.MenuTitleStickerProperties,
            _ => BuildStickerPropertiesMenu(player, team, definition, slot))
        {
            VisibleWhen = _ => Api.TryGetWeaponSkin(player.SteamID, team, definition, out var skin) && skin.HasSticker(slot)
        });

        foreach (var (_, stickerCollection) in EconService.StickerCollections)
        {
            var collectionName = EconService.GetLocalizedName(stickerCollection.LocalizedNames, language);
            var capsule = CreateSubmenu(collectionName, _ =>
            {
                var stickerMenu = CreateMenu($"weaponskins.stickers.slot{slot}.collection")
                    .WithTitle(collectionName);

                foreach (var sticker in stickerCollection.Stickers)
                {
                    if (sticker.Index == 0) continue;
                    var stickerIndex = sticker.Index;
                    var button = CreateRarityButton(
                        EconService.GetLocalizedName(sticker.LocalizedNames, language),
                        sticker.Rarity.Color.HexColor,
                        _ => Apply(new StickerData { Id = stickerIndex }));
                    button.Tag = sticker;
                    stickerMenu.Add(button);
                }

                return stickerMenu.Build();
            });
            capsule.Tag = stickerCollection;
            main.Add(capsule);
        }

        return main.Build();
    }
}