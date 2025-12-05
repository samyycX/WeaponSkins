using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins.Database;

public partial class DatabaseService
{

    public async Task StoreGlovesAsync(IEnumerable<GloveData> gloves)
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

    public async Task<IEnumerable<GloveData>> GetGlovesAsync(ulong steamId)
    {
        return await fsql.Select<GloveModel>()
            .Where(glove => glove.SteamID == steamId.ToString())
            .ToListAsync()
            .ContinueWith(task => task.Result.Select(glove => glove.ToDataModel()));
    }

    public async Task<IEnumerable<GloveData>> GetAllGlovesAsync()
    {
        return await fsql.Select<GloveModel>().ToListAsync()
            .ContinueWith(task => task.Result.Select(glove => glove.ToDataModel()));
    }

    public async Task RemoveGloveAsync(ulong steamId,
        Team team)
    {
        await fsql.Delete<GloveModel>()
            .Where(glove => glove.SteamID == steamId.ToString() && glove.Team == (short)team)
            .ExecuteAffrowsAsync();
    }

    public async Task RemoveGlovesAsync(ulong steamId)
    {
        await fsql.Delete<GloveModel>()
            .Where(glove => glove.SteamID == steamId.ToString())
            .ExecuteAffrowsAsync();
    }
}       