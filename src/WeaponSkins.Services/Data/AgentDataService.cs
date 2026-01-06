using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using SwiftlyS2.Shared.Players;

namespace WeaponSkins;

public class AgentDataService
{
    private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<Team, int>> _playerAgents = new();
    private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<Team, string>> _playerDefaultModels = new();

    public void SetAgent(ulong steamId,
        Team team,
        int agentIndex)
    {
        var playerAgents = _playerAgents.GetOrAdd(steamId, _ => new());
        playerAgents[team] = agentIndex;
    }

    public void CaptureDefaultModel(ulong steamId,
        Team team,
        string modelPath)
    {
        if (string.IsNullOrWhiteSpace(modelPath))
        {
            return;
        }

        var playerDefaults = _playerDefaultModels.GetOrAdd(steamId, _ => new());
        playerDefaults.TryAdd(team, modelPath);
    }

    public bool TryGetDefaultModel(ulong steamId,
        Team team,
        [MaybeNullWhen(false)] out string modelPath)
    {
        modelPath = null;
        return _playerDefaultModels.TryGetValue(steamId, out var playerDefaults) &&
               playerDefaults.TryGetValue(team, out modelPath) &&
               !string.IsNullOrWhiteSpace(modelPath);
    }

    public bool TryGetAgent(ulong steamId,
        Team team,
        out int agentIndex)
    {
        agentIndex = 0;
        return _playerAgents.TryGetValue(steamId, out var playerAgents) &&
               playerAgents.TryGetValue(team, out agentIndex);
    }

    public bool TryRemoveAgent(ulong steamId,
        Team team)
    {
        return _playerAgents.TryGetValue(steamId, out var playerAgents) &&
               playerAgents.TryRemove(team, out _);
    }

    public void RemovePlayer(ulong steamId)
    {
        _playerAgents.TryRemove(steamId, out _);
        _playerDefaultModels.TryRemove(steamId, out _);
    }
}
