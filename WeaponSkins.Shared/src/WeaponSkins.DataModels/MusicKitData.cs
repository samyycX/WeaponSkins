namespace WeaponSkins.Shared;

public class MusicKitData
{
    public ulong SteamID { get; set; }
    public int MusicKitIndex { get; set; }
    
    public MusicKitData DeepClone()
    {
        return new MusicKitData
        {
            SteamID = SteamID,
            MusicKitIndex = MusicKitIndex
        };
    }
}
