using System.Diagnostics.CodeAnalysis;

using SwiftlyS2.Shared.Players;

using WeaponSkins.Database;
using WeaponSkins.Econ;
using WeaponSkins.Services;
using WeaponSkins.Shared;

namespace WeaponSkins;

public class WeaponSkinAPI : IWeaponSkinAPI
{
    private IInventoryUpdateService InventoryUpdateService { get; init; }
    private InventoryService InventoryService { get; init; }
    private DataService DataService { get; init; }
    private StorageService StorageService { get; init; }
    private EconService EconService { get; init; }
    private ItemPermissionService ItemPermissionService { get; init; }
    private WeaponSkinGetterAPI WeaponSkinGetterAPI { get; init; }

    public IReadOnlyDictionary<string, ItemDefinition> Items => EconService.Items.AsReadOnly();

    public IReadOnlyDictionary<string, List<PaintkitDefinition>> WeaponToPaintkits =>
        EconService.WeaponToPaintkits.AsReadOnly();

    public IReadOnlyDictionary<string, StickerCollectionDefinition> StickerCollections =>
        EconService.StickerCollections.AsReadOnly();

    public IReadOnlyDictionary<string, KeychainDefinition> Keychains => EconService.Keychains.AsReadOnly();

    public WeaponSkinAPI(IInventoryUpdateService inventoryUpdateService,
        WeaponSkinGetterAPI weaponSkinGetterAPI,
        InventoryService inventoryService,
        DataService dataService,
        StorageService storageService,
        EconService econService,
        ItemPermissionService itemPermissionService
    )
    {
        InventoryUpdateService = inventoryUpdateService;
        InventoryService = inventoryService;
        DataService = dataService;
        StorageService = storageService;
        EconService = econService;
        ItemPermissionService = itemPermissionService;
        WeaponSkinGetterAPI = weaponSkinGetterAPI;
    }

    public void SetWeaponSkins(IEnumerable<WeaponSkinData> skins,
        bool permanent = false)
    {
        InventoryUpdateService.UpdateWeaponSkins(skins);
        if (permanent)
        {
            var _ = Task.Run(async () => await StorageService.Get().StoreSkinsAsync(skins));
        }
    }

    public void SetKnifeSkins(IEnumerable<KnifeSkinData> knives,
        bool permanent = false)
    {
        InventoryUpdateService.UpdateKnifeSkins(knives);
        if (permanent)
        {
            var _ = Task.Run(async () => await StorageService.Get().StoreKnifesAsync(knives));
        }
    }

    public void SetGloveSkins(IEnumerable<GloveData> gloves,
        bool permanent = false)
    {
        InventoryUpdateService.UpdateGloveSkins(gloves);
        if (permanent)
        {
            var _ = Task.Run(async () => await StorageService.Get().StoreGlovesAsync(gloves));
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
        var constrained = ItemPermissionService.ApplyWeaponUpdateRules(skin, newSkin);
        SetWeaponSkins([constrained], permanent);
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

        if (!ItemPermissionService.CanUseKnifeSkins(steamid))
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

        if (!ItemPermissionService.CanUseGloveSkins(steamid))
        {
            return;
        }

        SetGloveSkins([newGlove], permanent);
    }

    public bool TryGetWeaponSkin(ulong steamid,
        Team team,
        ushort definitionIndex,
        [MaybeNullWhen(false)] out WeaponSkinData skin) =>
        WeaponSkinGetterAPI.TryGetWeaponSkin(steamid, team, definitionIndex, out skin);

    public bool TryGetWeaponSkins(ulong steamid,
        [MaybeNullWhen(false)] out IEnumerable<WeaponSkinData> result) =>
        WeaponSkinGetterAPI.TryGetWeaponSkins(steamid, out result);

    public bool TryGetKnifeSkin(ulong steamid,
        Team team,
        [MaybeNullWhen(false)] out KnifeSkinData knife) =>
        WeaponSkinGetterAPI.TryGetKnifeSkin(steamid, team, out knife);

    public bool TryGetKnifeSkins(ulong steamid,
        [MaybeNullWhen(false)] out IEnumerable<KnifeSkinData> result) =>
        WeaponSkinGetterAPI.TryGetKnifeSkins(steamid, out result);

    public bool TryGetGloveSkin(ulong steamid,
        Team team,
        [MaybeNullWhen(false)] out GloveData glove) =>
        WeaponSkinGetterAPI.TryGetGloveSkin(steamid, team, out glove);

    public bool TryGetGloveSkins(ulong steamid,
        [MaybeNullWhen(false)] out IEnumerable<GloveData> result) =>
        WeaponSkinGetterAPI.TryGetGloveSkins(steamid, out result);

    public bool TryGetAgentSkin(ulong steamid,
        Team team,
        out int agentIndex) =>
        DataService.AgentDataService.TryGetAgent(steamid, team, out agentIndex);

    public bool TryGetAgentSkins(ulong steamid,
        [MaybeNullWhen(false)] out IEnumerable<(Team Team, int AgentIndex)> result)
    {
        var agents = new List<(Team, int)>();
        
        if (DataService.AgentDataService.TryGetAgent(steamid, Team.T, out var tIndex))
        {
            agents.Add((Team.T, tIndex));
        }
        
        if (DataService.AgentDataService.TryGetAgent(steamid, Team.CT, out var ctIndex))
        {
            agents.Add((Team.CT, ctIndex));
        }
        
        if (agents.Count > 0)
        {
            result = agents;
            return true;
        }
        
        result = null;
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
            var _ = Task.Run(async () => await StorageService.Get().RemoveSkinAsync(steamid, team, definitionIndex));
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
            var _ = Task.Run(async () => await StorageService.Get().RemoveKnifeAsync(steamid, team));
        }
    }

    public void ResetGloveSkin(ulong steamid,
        Team team,
        bool permanent = false)
    {
        InventoryUpdateService.ResetGloveSkin(steamid, team);
        if (permanent)
        {
            var _ = Task.Run(async () => await StorageService.Get().RemoveGloveAsync(steamid, team));
        }
    }

    public void UpdateAgentSkin(ulong steamid,
        Team team,
        int agentIndex,
        bool permanent = false)
    {
        DataService.AgentDataService.SetAgent(steamid, team, agentIndex);
        if (permanent)
        {
            var _ = Task.Run(async () => await StorageService.Get().StoreAgentsAsync(new[] { (steamid, team, agentIndex) }));
        }
    }

    public void ResetAgentSkin(ulong steamid,
        Team team,
        bool permanent = false)
    {
        DataService.AgentDataService.TryRemoveAgent(steamid, team);
        if (permanent)
        {
            var _ = Task.Run(async () => await StorageService.Get().RemoveAgentAsync(steamid, team));
        }
    }

    public void SetExternalStorageProvider(IStorageProvider provider)
    {
        StorageService.Set(provider);
    }
}