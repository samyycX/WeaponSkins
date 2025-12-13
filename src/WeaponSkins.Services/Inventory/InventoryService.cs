using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Logging;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

using WeaponSkins.Shared;

namespace WeaponSkins.Services;

public class InventoryService
{
    private ISwiftlyCore Core { get; init; }
    private NativeService NativeService { get; init; }
    private DataService DataService { get; init; }
    private ILogger<InventoryService> Logger { get; init; }

    private Dictionary<ulong /* steamid */, CCSPlayerInventory /* inventory */> SubscribedInventories = new();

    public InventoryService(ISwiftlyCore core,
        NativeService nativeService,
        DataService dataService,
        ILogger<InventoryService> logger)
    {
        Core = core;
        NativeService = nativeService;
        DataService = dataService;
        Logger = logger;

        NativeService.OnSOCacheSubscribed += OnSOCacheSubscribed;
        NativeService.OnSOCacheUnsubscribed += OnSOCacheUnsubscribed;
    }

    public CCSPlayerInventory Get(ulong steamid)
    {
        return SubscribedInventories[steamid];
    }

    public bool TryGet(ulong steamid,
        [MaybeNullWhen(false)] out CCSPlayerInventory inventory)
    {
        return SubscribedInventories.TryGetValue(steamid, out inventory);
    }

    private void OnSOCacheSubscribed(CCSPlayerInventory inventory,
        SOID_t soid)
    {
        // Logger.LogInformation($"SOCacheSubscribed: {soid.SteamID}");
        SubscribedInventories[soid.SteamID] = inventory;
    }

    private void OnSOCacheUnsubscribed(CCSPlayerInventory inventory,
        SOID_t soid)
    {
        SubscribedInventories.Remove(soid.SteamID);
    }

    public void UpdateWeaponSkins(ulong steamid,
        IEnumerable<WeaponSkinData> skins)
    {
        // Logger.LogInformation($"UpdateSkin: {steamid}");
        if (SubscribedInventories.TryGetValue(steamid, out var inventory))
        {
            // Logger.LogInformation($"UpdateSkin: {steamid}");
            foreach (var skin in skins)
            {
                inventory.UpdateWeaponSkin(skin);
            }
        }
    }

    public void UpdateKnifeSkins(ulong steamid,
        IEnumerable<KnifeSkinData> knives)
    {
        // Logger.LogInformation($"UpdateSkin: {steamid}");
        if (SubscribedInventories.TryGetValue(steamid, out var inventory))
        {
            // Logger.LogInformation($"UpdateSkin: {steamid}");
            foreach (var knife in knives)
            {
                inventory.UpdateKnifeSkin(knife);
            }
        }
    }

    public void UpdateGloveSkins(ulong steamid,
        IEnumerable<GloveData> gloves)
    {
        // Logger.LogInformation($"UpdateSkin: {steamid}");
        if (SubscribedInventories.TryGetValue(steamid, out var inventory))
        {
            // Logger.LogInformation($"UpdateSkin: {steamid}");
            foreach (var glove in gloves)
            {
                inventory.UpdateGloveSkin(glove);
            }
        }
    }

    public bool TryInitializeInventory(CCSPlayerController_InventoryServices service,
        [MaybeNullWhen(false)] out CCSPlayerInventory inventory)
    {
        inventory =
            new CCSPlayerInventory(service.Address +
                                   NativeService.CCSPlayerController_InventoryServices_m_pInventoryOffset);
        if (!inventory.IsValid)
        {
            inventory = null;
            return false;
        }

        SubscribedInventories[inventory.SteamID] = inventory;
        return true;
    }

    public void ResetWeaponSkin(ulong steamid,
        Team team,
        ushort definitionIndex)
    {
        if (TryGet(steamid, out var inventory))
        {
            inventory.ResetWeaponSkin(team, definitionIndex);
        }   
    }

    public void ResetKnifeSkin(ulong steamid,
        Team team)
    {
        if (TryGet(steamid, out var inventory))
        {
            inventory.ResetKnifeSkin(team);
        }
    }
    
    public void ResetGloveSkin(ulong steamid,
        Team team)
    {
        if (TryGet(steamid, out var inventory))
        {
            inventory.ResetGloveSkin(team);
        }
    }
}