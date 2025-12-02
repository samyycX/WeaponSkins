using System.Diagnostics.CodeAnalysis;

using SwiftlyS2.Shared.Players;

using WeaponSkins.Database;
using WeaponSkins.Econ;
using WeaponSkins.Services;
using WeaponSkins.Shared;

namespace WeaponSkins;

public class WeaponSkinAPI : IWeaponSkinAPI
{
    private InventoryUpdateService InventoryUpdateService { get; init; }
    private InventoryService InventoryService { get; init; }
    private DataService DataService { get; init; }
    private DatabaseService DatabaseService { get; init; }
    private EconService EconService { get; init; }

    private readonly Lazy<IReadOnlyDictionary<string, IReadOnlyList<PaintkitDefinition>>> _lazyWeaponToPaintkits;

    public IReadOnlyDictionary<string, ItemDefinition> Items => EconService.Items;

    public IReadOnlyDictionary<string, IReadOnlyList<PaintkitDefinition>> WeaponToPaintkits => _lazyWeaponToPaintkits.Value;

    public IReadOnlyDictionary<string, StickerCollectionDefinition> StickerCollections => EconService.StickerCollections;

    public IReadOnlyDictionary<string, KeychainDefinition> Keychains => EconService.Keychains;

    public WeaponSkinAPI(InventoryUpdateService inventoryUpdateService,
        InventoryService inventoryService,
        DataService dataService,
        DatabaseService databaseService,
        EconService econService)
    {
        InventoryUpdateService = inventoryUpdateService;
        InventoryService = inventoryService;
        DataService = dataService;
        DatabaseService = databaseService;
        EconService = econService;

        _lazyWeaponToPaintkits = new Lazy<IReadOnlyDictionary<string, IReadOnlyList<PaintkitDefinition>>>(() =>
        {
            return EconService.WeaponToPaintkits.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyList<PaintkitDefinition>)kvp.Value);
        });
    }

    public void SetWeaponSkins(IEnumerable<WeaponSkinData> skins,
        bool permanent = false)
    {
        InventoryUpdateService.UpdateWeaponSkins(skins);
        if (permanent)
        {
            var _ = Task.Run(async () => await DatabaseService.StoreSkins(skins));
        }
    }

    public void SetKnifeSkins(IEnumerable<KnifeSkinData> knives,
        bool permanent = false)
    {
        InventoryUpdateService.UpdateKnifeSkins(knives);
        if (permanent)
        {
            var _ = Task.Run(async () => await DatabaseService.StoreKnifes(knives));
        }
    }

    public void SetGloveSkins(IEnumerable<GloveData> gloves,
        bool permanent = false)
    {
        InventoryUpdateService.UpdateGloveSkins(gloves);
        if (permanent)
        {
            var _ = Task.Run(async () => await DatabaseService.StoreGloves(gloves));
        }
    }

    public void UpdateWeaponSkin(ulong steamid,
        Team team,
        ushort definitionIndex,
        Action<WeaponSkinData> action,
        bool permanent = false)
    {
        if (!DataService.WeaponDataService.TryGetSkin(steamid, team, definitionIndex, out var skin))
        {
            skin = new WeaponSkinData { SteamID = steamid, Team = team, DefinitionIndex = definitionIndex };
        }

        var newSkin = skin.DeepClone();
        action(newSkin);
        SetWeaponSkins([newSkin], permanent);
    }

    public void UpdateKnifeSkin(ulong steamid,
        Team team,
        Action<KnifeSkinData> action,
        bool permanent = false)
    {
        if (!DataService.KnifeDataService.TryGetKnife(steamid, team, out var knife))
        {
            knife = new KnifeSkinData { SteamID = steamid, Team = team, DefinitionIndex = 0 };
        }

        var newKnife = knife.DeepClone();
        action(newKnife);
        if (newKnife.DefinitionIndex == 0)
        {
            return;
        }

        SetKnifeSkins([newKnife], permanent);
    }

    public void UpdateGloveSkin(ulong steamid,
        Team team,
        Action<GloveData> action,
        bool permanent = false)
    {
        if (!DataService.GloveDataService.TryGetGlove(steamid, team, out var glove))
        {
            glove = new GloveData { SteamID = steamid, Team = team, DefinitionIndex = 0 };
        }

        var newGlove = glove.DeepClone();
        action(newGlove);
        if (newGlove.DefinitionIndex == 0)
        {
            return;
        }

        SetGloveSkins([newGlove], permanent);
    }

    public bool TryGetWeaponSkin(ulong steamid,
        Team team,
        ushort definitionIndex,
        [MaybeNullWhen(false)] out WeaponSkinData skin)
    {
        if (DataService.WeaponDataService.TryGetSkin(steamid, team, definitionIndex, out skin))
        {
            skin = skin.DeepClone();
            return true;
        }

        return false;
    }

    public bool TryGetKnifeSkin(ulong steamid,
        Team team,
        [MaybeNullWhen(false)] out KnifeSkinData knife)
    {
        if (DataService.KnifeDataService.TryGetKnife(steamid, team, out knife))
        {
            knife = knife.DeepClone();
            return true;
        }

        return false;
    }

    public bool TryGetGloveSkin(ulong steamid,
        Team team,
        [MaybeNullWhen(false)] out GloveData glove)
    {
        if (DataService.GloveDataService.TryGetGlove(steamid, team, out glove))
        {
            glove = glove.DeepClone();
            return true;
        }

        return false;
    }

    public void ResetWeaponSkin(ulong steamid,
        Team team,
        ushort definitionIndex,
        bool permanent = false
    )
    {
        InventoryUpdateService.ResetWeaponSkin(steamid, team, definitionIndex);
        if (permanent)
        {
            var _ = Task.Run(async () => await DatabaseService.RemoveSkin(steamid, team, definitionIndex));
        }
    }

    public void ResetKnifeSkin(ulong steamid,
        Team team,
        bool permanent = false
    )
    {
        InventoryUpdateService.ResetKnifeSkin(steamid, team);
        if (permanent)
        {
            var _ = Task.Run(async () => await DatabaseService.RemoveKnife(steamid, team));
        }
    }

    public void ResetGloveSkin(ulong steamid,
        Team team,
        bool permanent = false)
    {
        InventoryUpdateService.ResetGloveSkin(steamid, team);
        if (permanent)
        {
            var _ = Task.Run(async () => await DatabaseService.RemoveGlove(steamid, team));
        }
    }
}