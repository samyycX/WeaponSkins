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

    public WeaponSkinData DeepClone()
    {
        return new WeaponSkinData
        {
            SteamID = SteamID,
            Team = Team,
            DefinitionIndex = DefinitionIndex,
            Quality = Quality,
            Paintkit = Paintkit,
            PaintkitSeed = PaintkitSeed,
            PaintkitWear = PaintkitWear,
            Nametag = Nametag,
            StattrakCount = StattrakCount,
            Sticker0 = Sticker0?.DeepClone(),
            Sticker1 = Sticker1?.DeepClone(),
            Sticker2 = Sticker2?.DeepClone(),
            Sticker3 = Sticker3?.DeepClone(),
            Sticker4 = Sticker4?.DeepClone(),
            Sticker5 = Sticker5?.DeepClone(),
            Keychain0 = Keychain0?.DeepClone(),
        };
    }

    public StickerData? GetSticker(int slot)
    {
        switch (slot)
        {
            case 0: return Sticker0;
            case 1: return Sticker1;
            case 2: return Sticker2;
            case 3: return Sticker3;
            case 4: return Sticker4;
            case 5: return Sticker5;
        }

        return null;
    }

    public void SetSticker(int slot,
        StickerData data)
    {
        switch (slot)
        {
            case 0: Sticker0 = data; break;
            case 1: Sticker1 = data; break;
            case 2: Sticker2 = data; break;
            case 3: Sticker3 = data; break;
            case 4: Sticker4 = data; break;
            case 5: Sticker5 = data; break;
        }
    }

    public bool HasSticker(int slot) => GetSticker(slot) != null;

    public KeychainData? GetKeychain(int slot)
    {
        switch (slot)
        {
            case 0: return Keychain0;
        }

        return null;
    }

    public void SetKeychain(int slot,
        KeychainData data)
    {
        switch (slot)
        {
            case 0: Keychain0 = data; break;
        }
    }

    public bool HasKeychain(int slot) => GetKeychain(slot) != null;
}