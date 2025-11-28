using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins.Database;

public partial class DatabaseService
{
    public async Task StoreKnife(KnifeSkinData knife)
    {
        await fsql.InsertOrUpdate<KnifeModel>()
            .SetSource(KnifeModel.FromDataModel(knife))
            .ExecuteAffrowsAsync();

        await fsql.InsertOrUpdate<SkinModel>()
            .SetSource(SkinModel.FromKnifeDataModel(knife))
            .ExecuteAffrowsAsync();
    }

    public async Task StoreKnifes(IEnumerable<KnifeSkinData> knives)
    {
        await fsql.InsertOrUpdate<KnifeModel>()
            .SetSource(knives.Select(knife => KnifeModel.FromDataModel(knife)))
            .ExecuteAffrowsAsync();

        await fsql.InsertOrUpdate<SkinModel>()
            .SetSource(knives.Select(knife => SkinModel.FromKnifeDataModel(knife)))
            .ExecuteAffrowsAsync();
    }

    public async Task<KnifeSkinData?> GetKnifeAsync(ulong steamId,
        Team team)
    {
        var model = await fsql.Select<KnifeModel>()
            .Where(knife => knife.SteamID == steamId.ToString() && knife.Team == (short)team)
            .ToOneAsync();
        var data = model.ToDataModel();
        var skinModel = await fsql.Select<SkinModel>()
            .Where(skin => skin.SteamID == steamId.ToString() && skin.Team == (short)team &&
                           skin.DefinitionIndex == data.DefinitionIndex)
            .ToOneAsync();

        if (skinModel != null)
        {
            data.Paintkit = skinModel.PaintID;
            data.PaintkitWear = skinModel.Wear;
            data.PaintkitSeed = skinModel.Seed;
            data.Quality = skinModel.Stattrak ? EconItemQuality.StatTrak : EconItemQuality.Normal;
            data.StattrakCount = skinModel.StattrakCount;
            data.Nametag = skinModel.Nametag;
        }

        return data;
    }

    public KnifeSkinData? GetKnife(ulong steamId,
        Team team)
    {
        return GetKnifeAsync(steamId, team).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public async Task<IEnumerable<KnifeSkinData>> GetKnifesAsync(ulong steamId)
    {
        var results = await fsql.Select<KnifeModel, SkinModel>()
            .LeftJoin((knife,
                    skin) =>
                knife.SteamID == skin.SteamID &&
                knife.Team == skin.Team)
            .ToListAsync((Knife,
                Skin) => new { Knife, Skin });

        return results
            .Where(item => item.Skin == null ||
                           item.Knife.Knife ==
                           Core.Helpers.GetClassnameByDefinitionIndex(item.Skin.DefinitionIndex))
            .Select(item =>
            {
                var data = item.Knife.ToDataModel();

                if (item.Skin != null)
                {
                    data.Paintkit = item.Skin.PaintID;
                    data.PaintkitWear = item.Skin.Wear;
                    data.PaintkitSeed = item.Skin.Seed;
                    data.Quality = item.Skin.Stattrak ? EconItemQuality.StatTrak : EconItemQuality.Normal;
                    data.StattrakCount = item.Skin.StattrakCount;
                    data.Nametag = item.Skin.Nametag;
                }

                return data;
            }).ToList();
    }

    public IEnumerable<KnifeSkinData> GetKnifes(ulong steamId)
    {
        return GetKnifesAsync(steamId).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public async Task<List<KnifeSkinData>> GetAllKnifesAsync()
    {
        var results = await fsql.Select<KnifeModel, SkinModel>()
            .LeftJoin((knife,
                    skin) =>
                knife.SteamID == skin.SteamID &&
                knife.Team == skin.Team)
            .ToListAsync((Knife,
                Skin) => new { Knife, Skin });

        return results
            .Where(item => item.Skin == null ||
                           item.Knife.Knife ==
                           Core.Helpers.GetClassnameByDefinitionIndex(item.Skin.DefinitionIndex))
            .Select(item =>
            {
                var data = item.Knife.ToDataModel();

                if (item.Skin != null)
                {
                    data.Paintkit = item.Skin.PaintID;
                    data.PaintkitWear = item.Skin.Wear;
                    data.PaintkitSeed = item.Skin.Seed;
                    data.Quality = item.Skin.Stattrak ? EconItemQuality.StatTrak : EconItemQuality.Normal;
                    data.StattrakCount = item.Skin.StattrakCount;
                    data.Nametag = item.Skin.Nametag;
                }

                return data;
            }).ToList();
    }

    public IEnumerable<KnifeSkinData> GetAllKnifes()
    {
        return GetAllKnifesAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    }
}   