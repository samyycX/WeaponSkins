namespace WeaponSkins.Services;

public class MusicKitDataService
{
    private readonly Dictionary<ulong, int> _musicKits = new();

    public void SetMusicKit(ulong steamId, int musicKitIndex)
    {
        _musicKits[steamId] = musicKitIndex;
    }

    public bool TryGetMusicKit(ulong steamId, out int musicKitIndex)
    {
        return _musicKits.TryGetValue(steamId, out musicKitIndex);
    }

    public bool RemoveMusicKit(ulong steamId)
    {
        return _musicKits.Remove(steamId);
    }
}
