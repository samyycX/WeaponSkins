using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins;

public class KnifeDataService
{
    private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<Team, KnifeSkinData>> _playerKnives = new();

    /// <summary>
    /// Stores a knife skin for a player.
    /// </summary>
    /// <param name="knife">The knife skin to store.</param>
    /// <returns>True if the skin was stored, false if the skin already exists and is the same.</returns>
    public bool StoreKnife(KnifeSkinData knife)
    {
        var teamKnives = _playerKnives.GetOrAdd(knife.SteamID, _ => new());
        return teamKnives.UpdateOrAdd(knife.Team, knife);
    }

    public bool TryGetKnife(ulong steamId, Team team, [MaybeNullWhen(false)] out KnifeSkinData knife)
    {
        knife = null;
        if (_playerKnives.TryGetValue(steamId, out var teamKnives))
        {
            return teamKnives.TryGetValue(team, out knife);
        }
        return false;
    }

    public bool TryGetKnives(ulong steamId, [MaybeNullWhen(false)] out IEnumerable<KnifeSkinData> knives)
    {
        knives = null;
        if (_playerKnives.TryGetValue(steamId, out var teamKnives))
        {
            knives = teamKnives.Values;
            return true;
        }
        return false;
    }
}