using Microsoft.Extensions.Logging;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Players;
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

        foreach (var player in Core.PlayerManager.GetAllPlayers())
        {
            if (player.Controller is { IsValid: true, InventoryServices.IsValid: true } controller)
            {
                if (InventoryService.TryInitializeInventory(controller.InventoryServices!, out var inventory))
                {
                    Update(inventory);
                }
            }
        }
    }

    private void OnSOCacheSubscribed(CCSPlayerInventory inventory,
        SOID_t soid)
    {
        Update(inventory);
    }

    private void Update(CCSPlayerInventory inventory)
    {
        if (DataService.WeaponDataService.TryGetSkins(inventory.SteamID, out var skins))
        {
            foreach (var skin in skins)
            {
                inventory.UpdateWeaponSkin(skin);
            }
        }

        if (DataService.KnifeDataService.TryGetKnives(inventory.SteamID, out var knives))
        {
            foreach (var knife in knives)
            {
                inventory.UpdateKnifeSkin(knife);
            }
        }

        if (DataService.GloveDataService.TryGetGloves(inventory.SteamID, out var gloves))
        {
            foreach (var glove in gloves)
            {
                inventory.UpdateGloveSkin(glove);
            }
        }
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
                Logger.LogInformation($"Updating skins for player {player}. IsAlive: {player.IsAlive()}");
                if (player.IsAlive())
                {
                    foreach (var skin in updatedSkins)
                    {
                        foreach (var weapon in player.PlayerPawn!.WeaponServices!.MyWeapons)
                        {
                            if (weapon.Value!.AttributeManager.Item.ItemDefinitionIndex == skin.DefinitionIndex &&
                                player.Controller.Team == skin.Team)
                            {
                                    Core.Scheduler.NextWorldUpdate(() =>
                                    {
                                        player.RegiveWeapon(weapon.Value, skin.DefinitionIndex);
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
                            Core.Scheduler.NextWorldUpdate(() =>
                            {
                                player.RegiveKnife(knife.DefinitionIndex);
                            });
                        }
                    }
                }
            }
        }
    }

    public void UpdateGloveSkins(IEnumerable<GloveData> gloves)
    {
        Dictionary<ulong, List<GloveData>> updatedGloveMaps = new();
        foreach (var glove in gloves)
        {
            if (DataService.GloveDataService.StoreGlove(glove))
            {
                updatedGloveMaps.GetOrAdd(glove.SteamID, () => new()).Add(glove);
            }
        }

        foreach (var (steamID, updatedGloves) in updatedGloveMaps)
        {
            InventoryService.UpdateGloveSkins(steamID, updatedGloves);

            if (PlayerService.TryGetPlayer(steamID, out var player))
            {
                if (player.IsAlive())
                {
                    foreach (var glove in updatedGloves)
                    {
                        if (player.Controller.Team == glove.Team)
                        {
                            Core.Scheduler.NextWorldUpdate(() =>
                            {
                                Console.WriteLine("UpdateGloveSkins!!: Updating glove: {0}", glove.ToString());
                                var model = player.PlayerPawn!.CBodyComponent!.SceneNode.GetSkeletonInstance()
                                    .ModelState
                                    .ModelName;
                                player.PlayerPawn.SetModel("characters/models/tm_jumpsuit/tm_jumpsuit_varianta.vmdl");
                                player.PlayerPawn.SetModel(model);
                                var econGloves = player.PlayerPawn.EconGloves;
                                // player.PlayerPawn.EconGloves.Initialized = false;
                                // player.PlayerPawn.EconGloves.InitializedUpdated();
                                econGloves.Initialized = true;

                                Core.Scheduler.NextWorldUpdate(() =>
                                {
                                    var itemInLoadout = InventoryService.Get(player.SteamID).GetItemInLoadout(glove.Team, loadout_slot_t.LOADOUT_SLOT_CLOTHING_HANDS)!;
                                    econGloves.ItemDefinitionIndex = itemInLoadout.ItemDefinitionIndex;
                                    econGloves.AccountID = itemInLoadout.AccountID;
                                    econGloves.ItemID = itemInLoadout.ItemID;
                                    econGloves.ItemIDHigh = itemInLoadout.ItemIDHigh;
                                    econGloves.ItemIDLow = itemInLoadout.ItemIDLow;
                                    econGloves.InventoryPosition = itemInLoadout.InventoryPosition;
                                    econGloves.EntityLevel = itemInLoadout.EntityLevel;
                                    econGloves.EntityQuality = itemInLoadout.EntityQuality;
                                    NativeService.UpdateItemView.CallOriginal(
                                        econGloves.Address, 0);
                                    player.PlayerPawn.AcceptInput("SetBodygroup", "default_gloves,1");
                                });
                            });
                        }
                    }
                }
            }
        }
    }
}