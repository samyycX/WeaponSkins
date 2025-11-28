using FreeSql.DataAnnotations;

using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins.Database;

[Table(Name = "wp_player_gloves")]
[Index("steamid", "steamid,weapon_team")]
public record GloveModel
{
    [Column(Name = "steamid")] public required string SteamID { get; set; }
    [Column(Name = "weapon_team")] public required short Team { get; set; }
    [Column(Name = "weapon_defindex")] public required int DefinitionIndex { get; set; }

    public GloveData ToDataModel()
    {
        return new GloveData
        {
            SteamID = ulong.Parse(SteamID),
            Team = (Team)Team,
            DefinitionIndex = (ushort)DefinitionIndex,
        };
    }

    public static GloveModel FromDataModel(GloveData data)
    {
        return new GloveModel
        {
            SteamID = data.SteamID.ToString(),
            Team = (short)data.Team,
            DefinitionIndex = data.DefinitionIndex,
        };
    }
}

