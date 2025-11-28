using System.Diagnostics.CodeAnalysis;

using SwiftlyS2.Shared.Players;

using WeaponSkins.Database;
using WeaponSkins.Services;
using WeaponSkins.Shared;

namespace WeaponSkins;

public class WeaponSkinAPI : IWeaponSkinAPI
{
    private InventoryUpdateService InventoryUpdateService { get; init; }
    private InventoryService InventoryService { get; init; }
    private DataService DataService { get; init; }
    private DatabaseService DatabaseService { get; init; }

    public WeaponSkinAPI(InventoryUpdateService inventoryUpdateService,
        InventoryService inventoryService,
        DataService dataService,
        DatabaseService databaseService)
    {
        InventoryUpdateService = inventoryUpdateService;
        InventoryService = inventoryService;
        DataService = dataService;
        DatabaseService = databaseService;
    }

    public void SetWeaponSkins(IEnumerable<WeaponSkinData> skins)
    {
        InventoryUpdateService.UpdateWeaponSkins(skins);
    }

    public void SetWeaponSkinsWithoutStattrak(IEnumerable<WeaponSkinData> skins)
    {
        foreach (var skin in skins)
        {
            if (DataService.WeaponDataService.TryGetSkin(skin.SteamID, skin.Team, skin.DefinitionIndex,
                    out var existingSkin))
            {
                skin.StattrakCount = existingSkin.StattrakCount;
            }
        }

        SetWeaponSkins(skins);
    }

    public void SetKnifeSkins(IEnumerable<KnifeSkinData> knives)
    {
        InventoryUpdateService.UpdateKnifeSkins(knives);
    }

    public void SetGloveSkins(IEnumerable<GloveData> gloves)
    {
        InventoryUpdateService.UpdateGloveSkins(gloves);
    }

    public void UpdateWeaponSkin(ulong steamid,
        Team team,
        ushort definitionIndex,
        Action<WeaponSkinData> action)
    {
        if (DataService.WeaponDataService.TryGetSkin(steamid, team, definitionIndex, out var skin))
        {
            action(skin);
            SetWeaponSkins([skin]);
        }
    }

    public void UpdateKnifeSkin(ulong steamid,
        Team team,
        Action<KnifeSkinData> action)
    {
        if (DataService.KnifeDataService.TryGetKnife(steamid, team, out var knife))
        {
            action(knife);
            SetKnifeSkins([knife]);
        }
    }

    public void UpdateGloveSkin(ulong steamid,
        Team team,
        Action<GloveData> action)
    {
        if (DataService.GloveDataService.TryGetGlove(steamid, team, out var glove))
        {
            action(glove);
            SetGloveSkins([glove]);
        }
    }

    public void SetWeaponPaintsWithoutStattrakPermanently(IEnumerable<WeaponSkinData> skins)
    {
        SetWeaponSkinsWithoutStattrak(skins);
        var _ = Task.Run(async () => await DatabaseService.StoreSkins(skins));
    }

    public void SetWeaponSkinsPermanently(IEnumerable<WeaponSkinData> skins)
    {
        InventoryUpdateService.UpdateWeaponSkins(skins);
        var _ = Task.Run(async () => await DatabaseService.StoreSkins(skins));
    }

    public void SetKnifeSkinsPermanently(IEnumerable<KnifeSkinData> knives)
    {
        InventoryUpdateService.UpdateKnifeSkins(knives);
        var _ = Task.Run(async () => await DatabaseService.StoreKnifes(knives));
    }

    public void SetGloveSkinsPermanently(IEnumerable<GloveData> gloves)
    {
        InventoryUpdateService.UpdateGloveSkins(gloves);
        var _ = Task.Run(async () => await DatabaseService.StoreGloves(gloves));
    }

    public void UpdateWeaponSkinPermanently(ulong steamid,
        Team team,
        ushort definitionIndex,
        Action<WeaponSkinData> action)
    {
        if (DataService.WeaponDataService.TryGetSkin(steamid, team, definitionIndex, out var skin))
        {
            action(skin);
            SetWeaponSkinsPermanently([skin]);
        }
    }

    public void UpdateKnifeSkinPermanently(ulong steamid,
        Team team,
        Action<KnifeSkinData> action)
    {
        UpdateKnifeSkin(steamid, team, action);
        if (DataService.KnifeDataService.TryGetKnife(steamid, team, out var knife))
        {
            action(knife);
            SetKnifeSkinsPermanently([knife]);
        }
    }

    public void UpdateGloveSkinPermanently(ulong steamid,
        Team team,
        Action<GloveData> action)
    {
        UpdateGloveSkin(steamid, team, action);
        if (DataService.GloveDataService.TryGetGlove(steamid, team, out var glove))
        {
            action(glove);
            SetGloveSkinsPermanently([glove]);
        }
    }
}