using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using SwiftlyS2.Shared.Players;

namespace WeaponSkins.Services;

public class AgentPatchDataService
{
    // CCSPlayerPawn.m_vecPlayerPatchEconIndices holds five.
    public const int Slots = 5;

    // Replaced, never written in place, so a reader never sees a half-applied change.
    private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<Team, int[]>> _patches = new();

    public bool TryGetPatches(ulong steamId,
        Team team,
        [MaybeNullWhen(false)] out IReadOnlyList<int> patches)
    {
        patches = null;
        if (!_patches.TryGetValue(steamId, out var teams) || !teams.TryGetValue(team, out var stored))
        {
            return false;
        }

        patches = stored;
        return true;
    }

    // A patch index of 0 empties the slot; a team left with no patches is dropped.
    public void SetPatch(ulong steamId,
        Team team,
        int slot,
        int patchIndex)
    {
        if (slot is < 0 or >= Slots)
        {
            return;
        }

        var teams = _patches.GetOrAdd(steamId, _ => new());
        var patches = teams.TryGetValue(team, out var stored) ? (int[])stored.Clone() : new int[Slots];
        patches[slot] = patchIndex;
        if (patches.All(index => index == 0))
        {
            teams.TryRemove(team, out _);
        }
        else
        {
            teams[team] = patches;
        }
    }

    public void RemovePatches(ulong steamId,
        Team team)
    {
        if (_patches.TryGetValue(steamId, out var teams))
        {
            teams.TryRemove(team, out _);
        }
    }

    public void RemovePlayer(ulong steamId)
    {
        _patches.TryRemove(steamId, out _);
    }
}
