using FreeSql.DataAnnotations;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins.Database;

[Table(Name = "wp_player_knife")]
[Index("steamid", "steamid,weapon_team")]
public record KnifeModel
{

    [SwiftlyInject]
    private static ISwiftlyCore Core { get; set; } = null!;

    [Column(Name = "steamid")] public required string SteamID { get; set; }
    [Column(Name = "weapon_team")] public required short Team { get; set; }
    [Column(Name = "knife")] public required string Knife { get; set; }

    public KnifeSkinData ToDataModel()
    {
        return new KnifeSkinData
        {
            SteamID = ulong.Parse(SteamID),
            Team = (Team)Team,
            DefinitionIndex = (ushort)Core.Helpers.GetDefinitionIndexByClassname(Knife)!.Value,
        };
    }

    public static KnifeModel FromDataModel(KnifeSkinData data)
    {
        return new KnifeModel
        {
            SteamID = data.SteamID.ToString(),
            Team = (short)data.Team,
            Knife = Core.Helpers.GetClassnameByDefinitionIndex(data.DefinitionIndex)!,
        };
    }
}