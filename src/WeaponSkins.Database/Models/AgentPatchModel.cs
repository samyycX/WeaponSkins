using FreeSql.DataAnnotations;

using SwiftlyS2.Shared.Players;

namespace WeaponSkins.Database;

[Table(Name = "wp_player_agent_patches")]
public class AgentPatchModel
{
    [Column(Name = "steamid", IsPrimary = true)]
    public string SteamID { get; set; } = string.Empty;

    [Column(Name = "weapon_team", IsPrimary = true)]
    public int Team { get; set; }

    [Column(Name = "slot", IsPrimary = true)]
    public int Slot { get; set; }

    [Column(Name = "patch_id")]
    public int PatchID { get; set; }

    public static AgentPatchModel FromDataModel(ulong steamId, Team team, int slot, int patchIndex)
    {
        return new AgentPatchModel
        {
            SteamID = steamId.ToString(),
            Team = (int)team,
            Slot = slot,
            PatchID = patchIndex
        };
    }

    public (ulong SteamID, Team Team, int Slot, int PatchIndex) ToDataModel()
    {
        return (ulong.Parse(SteamID), (Team)Team, Slot, PatchID);
    }
}
