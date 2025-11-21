using Microsoft.Extensions.Logging;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Events;

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

        Core.Event.OnClientDisconnected += OnClientDisconnected;
    }

    public CCSPlayerInventory Get(ulong steamid)
    {
        return SubscribedInventories[steamid];
    }

    private void OnSOCacheSubscribed(CCSPlayerInventory inventory,
        SOID_t soid)
    {
        Logger.LogInformation($"SOCacheSubscribed: {soid.SteamID}");
        SubscribedInventories[soid.SteamID] = inventory;
    }

    private void OnClientDisconnected(IOnClientDisconnectedEvent @event)
    {
        var player = Core.PlayerManager.GetPlayer(@event.PlayerId);
        if (player == null)
        {
            return;
        }

        SubscribedInventories.Remove(player.SteamID);
    }

    public void UpdateWeaponSkins(ulong steamid,
        IEnumerable<WeaponSkinData> skins)
    {
        Logger.LogInformation($"UpdateSkin: {steamid}");
        if (SubscribedInventories.TryGetValue(steamid, out var inventory))
        {
            Logger.LogInformation($"UpdateSkin: {steamid}");
            foreach (var skin in skins)
            {
                inventory.UpdateWeaponSkin(skin);
            }
        }
    }

    public void UpdateKnifeSkins(ulong steamid,
        IEnumerable<KnifeSkinData> knives)
    {
        Logger.LogInformation($"UpdateSkin: {steamid}");
        if (SubscribedInventories.TryGetValue(steamid, out var inventory))
        {
            Logger.LogInformation($"UpdateSkin: {steamid}");
            foreach (var knife in knives)
            {
                inventory.UpdateKnifeSkin(knife);
            }
        }
    }
}