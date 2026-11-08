using System.Collections.Concurrent;

namespace WeaponSkins.Services;

public class GraffitiDataService
{
    private readonly ConcurrentDictionary<ulong, (int GraffitiIndex, int TintIndex)> _graffiti = new();

    public void SetGraffiti(ulong steamId, int graffitiIndex, int tintIndex)
    {
        _graffiti[steamId] = (graffitiIndex, tintIndex);
    }

    public bool TryGetGraffiti(ulong steamId, out int graffitiIndex, out int tintIndex)
    {
        var found = _graffiti.TryGetValue(steamId, out var graffiti);
        (graffitiIndex, tintIndex) = graffiti;
        return found;
    }

    public bool RemoveGraffiti(ulong steamId)
    {
        return _graffiti.TryRemove(steamId, out _);
    }
}
