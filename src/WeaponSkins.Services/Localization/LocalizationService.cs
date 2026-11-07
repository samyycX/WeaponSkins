using Microsoft.Extensions.Logging;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Translation;

namespace WeaponSkins;

public class LocalizationService
{
    private ISwiftlyCore Core { get; init; }
    private ILogger Logger { get; init; }
    private ILocalizer Localizer { get; init; }

    public LocalizationService(ISwiftlyCore core,
        ILogger<LocalizationService> logger)
    {
        Core = core;
        Logger = logger;
        Localizer = Core.Localizer;
    }

    public PlayerLocalizationService this[IPlayer player] => new(Core.Translation.GetPlayerLocalizer(player));
}

public class PlayerLocalizationService
{
    private ILocalizer Localizer { get; init; }

    public PlayerLocalizationService(ILocalizer localizer)
    {
        Localizer = localizer;
    }

    public IPlayer Player { get; init; } = null!;


    public string MenuTitle => Localizer["menu.title"];
    public string MenuTitleSkins => Localizer["menu.skins.title"];
    public string MenuTitleKnifes => Localizer["menu.knifes.title"];
    public string MenuTitleGloves => Localizer["menu.gloves.title"];
    public string MenuTitleStickers => Localizer["menu.stickers.title"];
    public string MenuTitleKeychains => Localizer["menu.keychains.title"];
    public string MenuTitleAgents => Localizer["menu.agents.title"];
    public string MenuTitleMusicKits => Localizer["menu.musickits.title"];
    public string MenuTitleWeaponProperties => Localizer["menu.weaponproperties.title"];
    public string MenuTitleKnifeProperties => Localizer["menu.knifeproperties.title"];
    public string MenuTitleGloveProperties => Localizer["menu.gloveproperties.title"];
    public string MenuTitleStickerProperties => Localizer["menu.stickerproperties.title"];
    public string MenuTitleKeychainProperties => Localizer["menu.keychainproperties.title"];
    public string MenuPropertiesEmpty => Localizer["menu.properties.empty"];
    public string MenuSkinPropertiesStattrak => Localizer["menu.skinproperties.stattrak"];
    public string MenuSkinPropertiesSouvenir => Localizer["menu.skinproperties.souvenir"];
    public string MenuSkinPropertiesSetSticker(int slot, string stickerName) => Localizer["menu.skinproperties.setsticker", slot, stickerName];
    public string MenuSkinPropertiesWear => Localizer["menu.skinproperties.wear"];
    public string MenuSkinPropertiesSeed => Localizer["menu.skinproperties.seed"];
    public string MenuSkinPropertiesNametag => Localizer["menu.skinproperties.nametag"];
    public string MenuSkinPropertiesNametagNone => Localizer["menu.skinproperties.nametagnone"];
    public string MenuSkinPropertiesNametagUnset => Localizer["menu.skinproperties.nametagunset"];
    public string MenuStickerPropertiesOffsetX => Localizer["menu.stickerproperties.offsetx"];
    public string MenuStickerPropertiesOffsetY => Localizer["menu.stickerproperties.offsety"];
    public string MenuStickerPropertiesResetOffset => Localizer["menu.stickerproperties.resetoffset"];
    public string MenuKeychainPropertiesOffsetX => Localizer["menu.keychainproperties.offsetx"];
    public string MenuKeychainPropertiesOffsetY => Localizer["menu.keychainproperties.offsety"];
    public string MenuKeychainPropertiesOffsetZ => Localizer["menu.keychainproperties.offsetz"];
    public string MenuKeychainPropertiesResetOffset => Localizer["menu.keychainproperties.resetoffset"];
    public string MenuReset => Localizer["menu.reset"];
    public string MenuOn => Localizer["menu.on"];
    public string MenuOff => Localizer["menu.off"];
    public string MenuTypeCtAgent => Localizer["menu.type.ctagent"];
    public string MenuTypeTAgent => Localizer["menu.type.tagent"];
    public string MenuTypeCharm => Localizer["menu.type.charm"];
    public string MenuTypeMusicKit => Localizer["menu.type.musickit"];
    public string MenuTypeSticker => Localizer["menu.type.sticker"];

    public string MenuSkinPropertiesStattrakCount(int stattrak) =>
        Localizer["menu.skinproperties.stattrakcount", stattrak];

    public string? MenuRarity(string rarityName) =>
        RarityNames.Contains(rarityName) ? Localizer[$"menu.rarity.{rarityName.ToLowerInvariant()}"] : null;

    // Gloves read Extraordinary in game whatever their paintkit's rarity.
    public string MenuRarityExtraordinary => Localizer["menu.rarity.extraordinary"];

    private static readonly HashSet<string> RarityNames = new(StringComparer.OrdinalIgnoreCase)
        { "common", "uncommon", "rare", "mythical", "legendary", "ancient", "immortal" };

    public string MenuUnset => Localizer["menu.unset"];
}