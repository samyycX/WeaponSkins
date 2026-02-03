using FreeSql.DataAnnotations;

namespace WeaponSkins.Database;

[Table(Name = "wp_player_music")]
public class MusicKitModel
{
    [Column(Name = "steamid", IsPrimary = true)]
    public string SteamID { get; set; } = string.Empty;
    
    [Column(Name = "weapon_team", IsPrimary = true)]
    public int WeaponTeam { get; set; }
    
    [Column(Name = "music_id")]
    public int MusicID { get; set; }

    public static MusicKitModel FromDataModel(ulong steamId, int team, int musicKitIndex)
    {
        return new MusicKitModel
        {
            SteamID = steamId.ToString(),
            WeaponTeam = team,
            MusicID = musicKitIndex
        };
    }
}
