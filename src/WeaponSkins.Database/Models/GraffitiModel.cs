using FreeSql.DataAnnotations;

namespace WeaponSkins.Database;

[Table(Name = "wp_player_graffiti")]
public class GraffitiModel
{
    [Column(Name = "steamid", IsPrimary = true)]
    public string SteamID { get; set; } = string.Empty;

    [Column(Name = "graffiti_id")]
    public int GraffitiID { get; set; }

    [Column(Name = "tint_id")]
    public int TintID { get; set; }

    public static GraffitiModel FromDataModel(ulong steamId, int graffitiIndex, int tintIndex)
    {
        return new GraffitiModel
        {
            SteamID = steamId.ToString(),
            GraffitiID = graffitiIndex,
            TintID = tintIndex
        };
    }

    public (ulong SteamID, int GraffitiIndex, int TintIndex) ToDataModel()
    {
        return (ulong.Parse(SteamID), GraffitiID, TintID);
    }
}
