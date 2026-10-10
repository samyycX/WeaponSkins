using System.Globalization;

using FreeSql.DataAnnotations;

using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins.Database;

[Table(Name = "wp_player_skins")]
[Index("steamid", "steamid,weapon_team,weapon_defindex")]
public record SkinModel
{
    [Column(Name = "steamid")] public required string SteamID { get; set; }

    [Column(Name = "weapon_team")] public required short Team { get; set; }

    [Column(Name = "weapon_defindex")] public required int DefinitionIndex { get; set; }

    [Column(Name = "weapon_paint_id")] public required int PaintID { get; set; }

    [Column(Name = "weapon_wear")] public float Wear { get; set; } = 0.000001f;

    [Column(Name = "weapon_seed")] public int Seed { get; set; } = 0;

    [Column(Name = "weapon_nametag")] public string? Nametag { get; set; }

    [Column(Name = "weapon_stattrak")] public bool Stattrak { get; set; } = false;

    [Column(Name = "weapon_stattrak_count")]
    public int StattrakCount { get; set; } = 0;

    [Column(Name = "weapon_sticker_0")] public string Sticker0 { get; set; } = "0;0;0;0;0;0;0";

    [Column(Name = "weapon_sticker_1")] public string Sticker1 { get; set; } = "0;0;0;0;0;0;0";

    [Column(Name = "weapon_sticker_2")] public string Sticker2 { get; set; } = "0;0;0;0;0;0;0";

    [Column(Name = "weapon_sticker_3")] public string Sticker3 { get; set; } = "0;0;0;0;0;0;0";

    [Column(Name = "weapon_sticker_4")] public string Sticker4 { get; set; } = "0;0;0;0;0;0;0";

    [Column(Name = "weapon_sticker_5")] public string Sticker5 { get; set; } = "0;0;0;0;0;0;0";

    [Column(Name = "weapon_keychain")] public string Keychain { get; set; } = "0;0;0;0;0";

    private static StickerData? ToStickerModel(string sticker)
    {
        if (string.IsNullOrWhiteSpace(sticker)) return null;
        var parts = sticker.Split(';');
        if (parts.Length != 7) throw new FormatException("A sticker must contain seven fields.");
        var id = int.Parse(parts[0], CultureInfo.InvariantCulture);
        if (id == 0) return null;
        var scale = ParseFloat(parts[5]);
        if (scale <= 0) throw new FormatException("Sticker scale must be positive.");
        return new StickerData
        {
            Id = id,
            Schema = int.Parse(parts[1], CultureInfo.InvariantCulture),
            OffsetX = ParseFloat(parts[2]),
            OffsetY = ParseFloat(parts[3]),
            Wear = ParseFloat(parts[4]),
            Scale = scale,
            Rotation = ParseFloat(parts[6]),
        };
    }

    private static string FromStickerModel(StickerData? sticker)
    {
        if (sticker == null || sticker.Id == 0) return "0;0;0;0;0;0;0";
        return string.Create(CultureInfo.InvariantCulture,
            $"{sticker.Id};{sticker.Schema};{sticker.OffsetX};{sticker.OffsetY};{sticker.Wear};{sticker.Scale};{sticker.Rotation}");
    }

    private static KeychainData? ToKeychainModel(string keychain)
    {
        if (string.IsNullOrWhiteSpace(keychain)) return null;
        var parts = keychain.Split(';');
        if (parts.Length != 5) throw new FormatException("A keychain must contain five fields.");
        var id = int.Parse(parts[0], CultureInfo.InvariantCulture);
        if (id == 0) return null;
        return new KeychainData
        {
            Id = id,
            OffsetX = ParseFloat(parts[1]),
            OffsetY = ParseFloat(parts[2]),
            OffsetZ = ParseFloat(parts[3]),
            Seed = int.Parse(parts[4], CultureInfo.InvariantCulture),
        };
    }

    private static string FromKeychainModel(KeychainData? keychain)
    {
        if (keychain == null || keychain.Id == 0) return "0;0;0;0;0";
        return string.Create(CultureInfo.InvariantCulture,
            $"{keychain.Id};{keychain.OffsetX};{keychain.OffsetY};{keychain.OffsetZ};{keychain.Seed}");
    }

    private static float ParseFloat(string value)
    {
        // Older records used the server culture's decimal separator, without grouping.
        if (float.TryParse(value.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var result) &&
            float.IsFinite(result))
        {
            return result;
        }

        throw new FormatException("Invalid cosmetic floating-point value.");
    }

    public WeaponSkinData ToDataModel()
    {
        return new WeaponSkinData
        {
            SteamID = ulong.Parse(SteamID),
            Team = (Team)Team,
            DefinitionIndex = (ushort)DefinitionIndex,
            Paintkit = PaintID,
            PaintkitWear = Wear,
            PaintkitSeed = Seed,
            Nametag = Nametag,
            Quality = Stattrak ? EconItemQuality.StatTrak : EconItemQuality.Normal,
            StattrakCount = StattrakCount,
            Sticker0 = ToStickerModel(Sticker0),
            Sticker1 = ToStickerModel(Sticker1),
            Sticker2 = ToStickerModel(Sticker2),
            Sticker3 = ToStickerModel(Sticker3),
            Sticker4 = ToStickerModel(Sticker4),
            Sticker5 = ToStickerModel(Sticker5),
            Keychain0 = ToKeychainModel(Keychain),
        };
    }

    public static SkinModel FromDataModel(WeaponSkinData data)
    {
        return new SkinModel
        {
            SteamID = data.SteamID.ToString(),
            Team = (short)data.Team,
            DefinitionIndex = data.DefinitionIndex,
            PaintID = data.Paintkit,
            Wear = data.PaintkitWear,
            Seed = data.PaintkitSeed,
            Nametag = data.Nametag,
            Stattrak = data.Quality == EconItemQuality.StatTrak,
            StattrakCount = data.StattrakCount,
            Sticker0 = FromStickerModel(data.Sticker0),
            Sticker1 = FromStickerModel(data.Sticker1),
            Sticker2 = FromStickerModel(data.Sticker2),
            Sticker3 = FromStickerModel(data.Sticker3),
            Sticker4 = FromStickerModel(data.Sticker4),
            Sticker5 = FromStickerModel(data.Sticker5),
            Keychain = FromKeychainModel(data.Keychain0),
        };
    }

    public static SkinModel FromKnifeDataModel(KnifeSkinData data)
    {
        return new SkinModel
        {
            SteamID = data.SteamID.ToString(),
            Team = (short)data.Team,
            DefinitionIndex = data.DefinitionIndex,
            PaintID = data.Paintkit,
            Wear = data.PaintkitWear,
            Seed = data.PaintkitSeed,
            Nametag = data.Nametag,
            Stattrak = data.Quality == EconItemQuality.StatTrak,
            StattrakCount = data.StattrakCount
        };
    }

    public static SkinModel FromGloveDataModel(GloveData data)
    {
        return new SkinModel
        {
            SteamID = data.SteamID.ToString(),
            Team = (short)data.Team,
            DefinitionIndex = data.DefinitionIndex,
            PaintID = data.Paintkit,
            Wear = data.PaintkitWear,
            Seed = data.PaintkitSeed
        };
    }
}