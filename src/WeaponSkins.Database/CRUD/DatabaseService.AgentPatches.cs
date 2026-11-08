using SwiftlyS2.Shared.Players;

namespace WeaponSkins.Database;

public partial class DatabaseService
{
    public async Task StoreAgentPatchesAsync(IEnumerable<(ulong SteamID, Team Team, int Slot, int PatchIndex)> patches)
    {
        var models = patches.Select(p => AgentPatchModel.FromDataModel(p.SteamID, p.Team, p.Slot, p.PatchIndex)).ToList();

        await fsql.InsertOrUpdate<AgentPatchModel>()
            .SetSource(models)
            .ExecuteAffrowsAsync();
    }

    public async Task<IEnumerable<(ulong SteamID, Team Team, int Slot, int PatchIndex)>> GetAgentPatchesAsync(ulong steamId)
    {
        var models = await fsql.Select<AgentPatchModel>()
            .Where(p => p.SteamID == steamId.ToString())
            .ToListAsync();
        return models.Select(m => m.ToDataModel());
    }

    public async Task<IEnumerable<(ulong SteamID, Team Team, int Slot, int PatchIndex)>> GetAllAgentPatchesAsync()
    {
        var models = await fsql.Select<AgentPatchModel>().ToListAsync();
        return models.Select(m => m.ToDataModel());
    }

    public async Task RemoveAgentPatchAsync(ulong steamId, Team team, int slot)
    {
        var teamValue = (int)team;
        await fsql.Delete<AgentPatchModel>()
            .Where(p => p.SteamID == steamId.ToString() && p.Team == teamValue && p.Slot == slot)
            .ExecuteAffrowsAsync();
    }

    public async Task RemoveAgentPatchesAsync(ulong steamId, Team team)
    {
        var teamValue = (int)team;
        await fsql.Delete<AgentPatchModel>()
            .Where(p => p.SteamID == steamId.ToString() && p.Team == teamValue)
            .ExecuteAffrowsAsync();
    }
}
