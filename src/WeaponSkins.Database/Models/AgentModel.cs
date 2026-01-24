using FreeSql.DataAnnotations;

using SwiftlyS2.Shared.Players;

namespace WeaponSkins.Database;

[Table(Name = "wp_player_agents")]
[Index("steamid", "steamid,weapon_team")]
public record AgentModel
{
    [Column(Name = "steamid")] public required string SteamID { get; set; }
    [Column(Name = "weapon_team")] public required short Team { get; set; }
    [Column(Name = "agent_index")] public required int AgentIndex { get; set; }

    public static AgentModel FromDataModel(ulong steamId, Team team, int agentIndex)
    {
        return new AgentModel
        {
            SteamID = steamId.ToString(),
            Team = (short)team,
            AgentIndex = agentIndex
        };
    }
}
