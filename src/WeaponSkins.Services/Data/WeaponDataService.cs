using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins;

public class WeaponDataService
{
    private readonly
        ConcurrentDictionary<ulong, ConcurrentDictionary<Team, ConcurrentDictionary<ushort, WeaponSkinData>>>
        _playerSkins = new();

    /// <summary>
    /// Stores a weapon skin for a player.
    /// </summary>
    /// <param name="skin">The weapon skin to store.</param>
    /// <returns>True if the skin was stored, false if the skin already exists and is the same.</returns>
    /// <returns></returns>
    public bool StoreSkin(WeaponSkinData skin)
    {
        var playerInventory = _playerSkins.GetOrAdd(skin.SteamID, _ => new());
        var teamSkins = playerInventory.GetOrAdd(skin.Team, _ => new());
        var result = teamSkins.UpdateOrAdd(skin.DefinitionIndex, skin);
        return result;
    }

    public bool TryGetSkin(ulong steamId,
        Team team,
        ushort definitionIndex,
        [MaybeNullWhen(false)] out WeaponSkinData skin)
    {
        skin = null;
        if (_playerSkins.TryGetValue(steamId, out var playerInventory))
        {
            if (playerInventory.TryGetValue(team, out var teamSkins))
            {
                if (teamSkins.TryGetValue(definitionIndex, out skin))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool TryGetSkins(ulong steamId,
        Team team,
        [MaybeNullWhen(false)] out ConcurrentDictionary<ushort, WeaponSkinData> skins)
    {
        skins = null;
        if (_playerSkins.TryGetValue(steamId, out var playerInventory))
        {
            return playerInventory.TryGetValue(team, out skins);
        }

        return false;
    }

    public bool TryGetSkins(ulong steamId,
        [MaybeNullWhen(false)] out IEnumerable<WeaponSkinData> skins)
    {
        skins = null;
        if (_playerSkins.TryGetValue(steamId, out var playerInventory))
        {
            skins = playerInventory.Values.SelectMany(teamSkins => teamSkins.Values);
            return true;
        }

        return false;
    }

    public void RemovePlayer(ulong steamId)
    {
        _playerSkins.TryRemove(steamId, out _);
    }
}
