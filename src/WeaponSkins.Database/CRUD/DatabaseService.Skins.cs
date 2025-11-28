using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins.Database;

public partial class DatabaseService
{
    public async Task StoreSkin(WeaponSkinData skin)
    {
        await fsql.InsertOrUpdate<SkinModel>()
            .SetSource(SkinModel.FromDataModel(skin))
            .ExecuteAffrowsAsync();
    }

    public async Task StoreSkins(IEnumerable<WeaponSkinData> skins)
    {
        await fsql.InsertOrUpdate<SkinModel>()
            .SetSource(skins.Select(skin => SkinModel.FromDataModel(skin)))
            .ExecuteAffrowsAsync();
    }

    public async Task<WeaponSkinData?> GetSkinAsync(ulong steamId,
        Team team,
        ushort definitionIndex)
    {
        var model = await fsql.Select<SkinModel>()
            .Where(skin => skin.SteamID == steamId.ToString() && skin.Team == (short)team &&
                           skin.DefinitionIndex == definitionIndex)
            .ToOneAsync();

        return model.ToDataModel();
    }

    public WeaponSkinData? GetSkin(ulong steamId,
        Team team,
        ushort definitionIndex)
    {
        return GetSkinAsync(steamId, team, definitionIndex).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public async Task<IEnumerable<WeaponSkinData>> GetSkinsAsync(ulong steamId)
    {
        return await fsql.Select<SkinModel>()
            .Where(skin => skin.SteamID == steamId.ToString())
            .ToListAsync()
            .ContinueWith(task => task.Result.Select(skin => skin.ToDataModel()));
    }

    public IEnumerable<WeaponSkinData> GetSkins(ulong steamId)
    {
        return GetSkinsAsync(steamId).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public async Task<IEnumerable<WeaponSkinData>> GetAllSkinsAsync()
    {
        return await fsql.Select<SkinModel>().ToListAsync()
            .ContinueWith(task => task.Result.Select(skin => skin.ToDataModel()));
    }

    public IEnumerable<WeaponSkinData> GetAllSkins()
    {
        return GetAllSkinsAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    }
}