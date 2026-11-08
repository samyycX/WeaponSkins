namespace WeaponSkins.Database;

public partial class DatabaseService
{
    public async Task StoreGraffitiAsync(IEnumerable<(ulong SteamID, int GraffitiIndex, int TintIndex)> graffiti)
    {
        var models = graffiti.Select(g => GraffitiModel.FromDataModel(g.SteamID, g.GraffitiIndex, g.TintIndex)).ToList();

        await fsql.InsertOrUpdate<GraffitiModel>()
            .SetSource(models)
            .ExecuteAffrowsAsync();
    }

    public async Task<IEnumerable<(ulong SteamID, int GraffitiIndex, int TintIndex)>> GetGraffitiAsync(ulong steamId)
    {
        var models = await fsql.Select<GraffitiModel>()
            .Where(g => g.SteamID == steamId.ToString())
            .ToListAsync();
        return models.Select(m => m.ToDataModel());
    }

    public async Task<IEnumerable<(ulong SteamID, int GraffitiIndex, int TintIndex)>> GetAllGraffitiAsync()
    {
        var models = await fsql.Select<GraffitiModel>().ToListAsync();
        return models.Select(m => m.ToDataModel());
    }

    public async Task RemoveGraffitiAsync(ulong steamId)
    {
        await fsql.Delete<GraffitiModel>()
            .Where(g => g.SteamID == steamId.ToString())
            .ExecuteAffrowsAsync();
    }
}
