using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins.Database;

public partial class DatabaseService
{
    public async Task StoreGlove(GloveData glove)
    {
        await fsql.InsertOrUpdate<GloveModel>()
            .SetSource(GloveModel.FromDataModel(glove))
            .ExecuteAffrowsAsync();
    }

    public async Task StoreGloves(IEnumerable<GloveData> gloves)
    {
        await fsql.InsertOrUpdate<GloveModel>()
            .SetSource(gloves.Select(glove => GloveModel.FromDataModel(glove)))
            .ExecuteAffrowsAsync();
    }

    public async Task<GloveData?> GetGloveAsync(ulong steamId,
        Team team)
    {
        var model = await fsql.Select<GloveModel>()
            .Where(glove => glove.SteamID == steamId.ToString() && glove.Team == (short)team)
            .ToOneAsync();
        return model.ToDataModel();
    }

    public GloveData? GetGlove(ulong steamId,
        Team team)
    {
        return GetGloveAsync(steamId, team).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public async Task<IEnumerable<GloveData>> GetGlovesAsync(ulong steamId)
    {
        return await fsql.Select<GloveModel>()
            .Where(glove => glove.SteamID == steamId.ToString())
            .ToListAsync()
            .ContinueWith(task => task.Result.Select(glove => glove.ToDataModel()));
    }

    public IEnumerable<GloveData> GetGloves(ulong steamId)
    {
        return GetGlovesAsync(steamId).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public async Task<IEnumerable<GloveData>> GetAllGlovesAsync()
    {
        return await fsql.Select<GloveModel>().ToListAsync()
            .ContinueWith(task => task.Result.Select(glove => glove.ToDataModel()));
    }

    public IEnumerable<GloveData> GetAllGloves()
    {
        return GetAllGlovesAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    }
}       