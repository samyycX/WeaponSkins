using FreeSql.DataAnnotations;

using SwiftlyS2.Shared.Players;

namespace WeaponSkins.Database;

[Table(Name = "wp_player_pins")]
public class PinModel
{
    [Column(Name = "steamid", IsPrimary = true)]
    public string SteamID { get; set; } = string.Empty;

    [Column(Name = "weapon_team", IsPrimary = true)]
    public int Team { get; set; }

    [Column(Name = "id")]
    public int PinID { get; set; }

    public static PinModel FromDataModel(ulong steamId, Team team, int pinIndex)
    {
        return new PinModel
        {
            SteamID = steamId.ToString(),
            Team = (int)team,
            PinID = pinIndex
        };
    }

    public (ulong SteamID, int PinIndex) ToDataModel()
    {
        return (ulong.Parse(SteamID), PinID);
    }
}
