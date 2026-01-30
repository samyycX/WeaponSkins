using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins.Database;

public partial class DatabaseService
{
    public async Task StoreSkinsAsync(IEnumerable<WeaponSkinData> skins)
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

    public async Task<IEnumerable<WeaponSkinData>> GetSkinsAsync(ulong steamId)
    {
        return await fsql.Select<SkinModel>()
            .Where(skin => skin.SteamID == steamId.ToString())
            .ToListAsync()
            .ContinueWith(task => 
                task.Result
                    .Where(skin => Utilities.IsWeaponDefinitionIndex(skin.DefinitionIndex))
                    .Select(skin => skin.ToDataModel())
                );
    }

    public async Task<IEnumerable<WeaponSkinData>> GetAllSkinsAsync()
    {
        return await fsql.Select<SkinModel>().ToListAsync()
            .ContinueWith(task => 
                task.Result
                    .Where(skin => Utilities.IsWeaponDefinitionIndex(skin.DefinitionIndex))
                    .Select(skin => skin.ToDataModel())
                );
    }

    public async Task RemoveSkinAsync(ulong steamId,
        Team team,
        ushort definitionIndex)
    {
        await fsql.Delete<SkinModel>()
            .Where(skin => skin.SteamID == steamId.ToString() && skin.Team == (short)team &&
                           skin.DefinitionIndex == definitionIndex)
            .ExecuteAffrowsAsync();
    }

    public async Task RemoveSkinsAsync(ulong steamId)
    {
        await fsql.Delete<SkinModel>()
            .Where(skin => skin.SteamID == steamId.ToString())
            .ExecuteAffrowsAsync();
    }
}