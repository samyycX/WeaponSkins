using Microsoft.Extensions.Logging;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.SchemaDefinitions;

using WeaponSkins.Extensions;
using WeaponSkins.Shared;

namespace WeaponSkins.Services;

public class InventoryUpdateService
{
    private ISwiftlyCore Core { get; init; }
    private InventoryService InventoryService { get; init; }
    private DataService DataService { get; init; }
    private PlayerService PlayerService { get; init; }
    private NativeService NativeService { get; init; }
    private ILogger<InventoryUpdateService> Logger { get; init; }

    public InventoryUpdateService(ISwiftlyCore core,
        InventoryService inventoryService,
        DataService dataService,
        PlayerService playerService,
        NativeService nativeService,
        ILogger<InventoryUpdateService> logger)
    {
        Core = core;
        InventoryService = inventoryService;
        DataService = dataService;

        PlayerService = playerService;
        NativeService = nativeService;
        Logger = logger;

        NativeService.OnSOCacheSubscribed += OnSOCacheSubscribed;
    }

    private void OnSOCacheSubscribed(CCSPlayerInventory inventory,
        SOID_t soid)
    {
        Core.Scheduler.NextWorldUpdate(() =>
        {
            if (DataService.WeaponDataService.TryGetSkins(soid.SteamID, out var skins))
            {
                foreach (var skin in skins)
                {
                    inventory.UpdateWeaponSkin(skin);
                }
            }

            if (DataService.KnifeDataService.TryGetKnives(soid.SteamID, out var knives))
            {
                foreach (var knife in knives)
                {
                    inventory.UpdateKnifeSkin(knife);
                }
            }
        });
    }

    public void UpdateWeaponSkins(IEnumerable<WeaponSkinData> skins)
    {
        Dictionary<ulong, List<WeaponSkinData>> updatedSkinMaps = new();

        foreach (var skin in skins)
        {
            if (DataService.WeaponDataService.StoreSkin(skin))
            {
                updatedSkinMaps.GetOrAdd(skin.SteamID, () => new()).Add(skin);
            }
        }

        foreach (var (steamID, updatedSkins) in updatedSkinMaps)
        {
            InventoryService.UpdateWeaponSkins(steamID, updatedSkins);

            if (PlayerService.TryGetPlayer(steamID, out var player))
            {
                if (player.IsAlive())
                {
                    foreach (var skin in updatedSkins)
                    {
                        foreach (var weapon in player.PlayerPawn!.WeaponServices!.MyWeapons)
                        {
                            if (weapon.Value!.AttributeManager.Item.ItemDefinitionIndex == skin.DefinitionIndex &&
                                player.Controller.Team == skin.Team)
                            {
                                Core.Scheduler.NextTick(() =>
                                {
                                    player.RegiveWeapon(weapon.Value);
                                });
                            }
                        }
                    }
                }
            }
        }
    }

    public void UpdateKnifeSkins(IEnumerable<KnifeSkinData> knives)
    {
        Dictionary<ulong, List<KnifeSkinData>> updatedKnifeMaps = new();
        foreach (var knife in knives)
        {
            if (DataService.KnifeDataService.StoreKnife(knife))
            {
                updatedKnifeMaps.GetOrAdd(knife.SteamID, () => new()).Add(knife);
            }
        }

        foreach (var (steamID, updatedKnives) in updatedKnifeMaps)
        {
            InventoryService.UpdateKnifeSkins(steamID, updatedKnives);

            if (PlayerService.TryGetPlayer(steamID, out var player))
            {
                if (player.IsAlive())
                {
                    foreach (var knife in updatedKnives)
                    {
                        if (player.Controller.Team == knife.Team)
                        {
                            Core.Scheduler.NextTick(() =>
                            {
                                player.RegiveKnife(knife.DefinitionIndex);
                            });
                        }
                    }
                }
            }
        }
    }
}