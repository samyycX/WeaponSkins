using SwiftlyS2.Shared.Players;

namespace WeaponSkins.Shared;

public record WeaponSkinData
{
    public required ulong SteamID { get; set; }
    public required Team Team { get; init; }
    public required ushort DefinitionIndex { get; init; }

    public EconItemQuality Quality { get; set; } = EconItemQuality.Normal;

    public int Paintkit { get; set; } = 0;
    public int PaintkitSeed { get; set; } = 0;
    public float PaintkitWear { get; set; } = 0.0f;

    public string? Nametag { get; set; } = null;
    public int StattrakCount { get; set; } = 0;

    public StickerData? Sticker0 { get; set; } = null;
    public StickerData? Sticker1 { get; set; } = null;
    public StickerData? Sticker2 { get; set; } = null;
    public StickerData? Sticker3 { get; set; } = null;
    public StickerData? Sticker4 { get; set; } = null;
    public StickerData? Sticker5 { get; set; } = null;
    public KeychainData? Keychain0 { get; set; } = null;
};