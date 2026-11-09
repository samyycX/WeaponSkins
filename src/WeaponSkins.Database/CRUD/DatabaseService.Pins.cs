using SwiftlyS2.Shared.Players;

namespace WeaponSkins.Database;

public partial class DatabaseService
{
    public async Task StorePinsAsync(IEnumerable<(ulong SteamID, int PinIndex)> pins)
    {
        // One row per team, as WeaponPaints writes them, so its rows are replaced rather than left beside ours.
        var models = new List<PinModel>();
        foreach (var pin in pins)
        {
            models.Add(PinModel.FromDataModel(pin.SteamID, Team.T, pin.PinIndex));
            models.Add(PinModel.FromDataModel(pin.SteamID, Team.CT, pin.PinIndex));
        }

        await fsql.InsertOrUpdate<PinModel>()
            .SetSource(models)
            .ExecuteAffrowsAsync();
    }

    public async Task<IEnumerable<(ulong SteamID, int PinIndex)>> GetPinsAsync(ulong steamId)
    {
        var models = await fsql.Select<PinModel>()
            .Where(p => p.SteamID == steamId.ToString())
            .ToListAsync();
        return models.Select(m => m.ToDataModel()).Distinct();
    }

    public async Task<IEnumerable<(ulong SteamID, int PinIndex)>> GetAllPinsAsync()
    {
        var models = await fsql.Select<PinModel>().ToListAsync();
        return models.Select(m => m.ToDataModel()).Distinct();
    }

    public async Task RemovePinAsync(ulong steamId)
    {
        await fsql.Delete<PinModel>()
            .Where(p => p.SteamID == steamId.ToString())
            .ExecuteAffrowsAsync();
    }
}
