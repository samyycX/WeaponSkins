using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins;

public class GloveDataService
{
    private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<Team, GloveData>> _playerGloves = new();

    public bool StoreGlove(GloveData glove)
    {
        var playerGloves = _playerGloves.GetOrAdd(glove.SteamID, _ => new());
        return playerGloves.UpdateOrAdd(glove.Team, glove);
    }

    public bool TryGetGlove(ulong steamId,
        Team team,
        [MaybeNullWhen(false)] out GloveData glove)
    {
        glove = null;
        if (_playerGloves.TryGetValue(steamId, out var playerGloves))
        {
            return playerGloves.TryGetValue(team, out glove);
        }

        return false;
    }

    public bool TryGetGloves(ulong steamId,
        [MaybeNullWhen(false)] out IEnumerable<GloveData> gloves)
    {
        gloves = null;
        if (_playerGloves.TryGetValue(steamId, out var playerGloves))
        {
            gloves = playerGloves.Values;
            return true;
        }

        return false;
    }

    public void RemovePlayer(ulong steamId)
    {
        _playerGloves.TryRemove(steamId, out _);
    }

    public bool TryRemoveGlove(ulong steamId,
        Team team)
    {
        return _playerGloves.TryGetValue(steamId, out var playerGloves) && playerGloves.TryRemove(team, out _);
    }
}