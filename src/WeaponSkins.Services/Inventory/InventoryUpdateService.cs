using Microsoft.Extensions.Logging;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

using WeaponSkins.Econ;
using WeaponSkins.Extensions;
using WeaponSkins.Shared;
using WeaponSkins.Database;

namespace WeaponSkins.Services;

public class InventoryUpdateService : IInventoryUpdateService
{
    private ISwiftlyCore Core { get; init; }
    private InventoryService InventoryService { get; init; }
    private PlayerService PlayerService { get; init; }
    private NativeService NativeService { get; init; }
    private ILogger<InventoryUpdateService> Logger { get; init; }
    private ItemPermissionService ItemPermissionService { get; init; }
    private WeaponSkinGetterAPI Api { get; init; }
    private DataService DataService { get; init; }
    private EconService EconService { get; init; }
    private StorageService StorageService { get; init; }

    public InventoryUpdateService(ISwiftlyCore core,
        InventoryService inventoryService,
        WeaponSkinGetterAPI api,
        EconService econService,
        PlayerService playerService,
        NativeService nativeService,
        ILogger<InventoryUpdateService> logger,
        ItemPermissionService itemPermissionService,
        DataService dataService,
        StorageService storageService)
    {
        Core = core;
        InventoryService = inventoryService;
        Api = api;
        EconService = econService;

        PlayerService = playerService;
        NativeService = nativeService;
        Logger = logger;
        ItemPermissionService = itemPermissionService;
        DataService = dataService;
        StorageService = storageService;
        NativeService.OnSOCacheSubscribed += OnSOCacheSubscribed;

        Core.GameEvent.HookPost<EventPlayerSpawn>(OnPlayerSpawn);

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


    private HookResult OnPlayerSpawn(EventPlayerSpawn @event)
    {
        IPlayer player = @event.UserIdPlayer;

        Core.Scheduler.NextWorldUpdate(() =>
        {
            if (Api.TryGetGloveSkins(player.SteamID, out var gloves))
            {
                foreach (var glove in gloves)
                {
                    player.RegiveGlove(InventoryService.Get(player.SteamID));
                }
            }

            ApplyPlayerAgent(player);
        });

        Core.Scheduler.DelayBySeconds(0.1f, () =>
        {
            if (Api.TryGetGloveSkins(player.SteamID, out var gloves))
            {
                foreach (var glove in gloves)
                {
                    player.RegiveGlove(InventoryService.Get(player.SteamID));
                }
            }

            ApplyPlayerAgent(player);
        });

        return HookResult.Continue;
    }

    private string? GetRefreshModel(string currentModel,
        string targetModel)
    {
        foreach (var agent in EconService.Agents.Values)
        {
            var candidate = agent.ModelPath;
            if (string.IsNullOrWhiteSpace(candidate)) continue;
            if (string.Equals(candidate, currentModel, StringComparison.OrdinalIgnoreCase)) continue;
            if (string.Equals(candidate, targetModel, StringComparison.OrdinalIgnoreCase)) continue;
            return candidate;
        }

        return null;
    }

    private void ApplyPlayerAgent(IPlayer player)
    {
        Core.Scheduler.NextWorldUpdate(() =>
        {
            if (!player.IsAlive()) return;

            var pawn = player.PlayerPawn!;
            var current = pawn.CBodyComponent!.SceneNode!.GetSkeletonInstance()
                .ModelState
                .ModelName;
            DataService.AgentDataService.CaptureDefaultModel(player.SteamID, player.Controller.Team, current);

            if (DataService.AgentDataService.TryGetAgent(player.SteamID, player.Controller.Team, out var agentIndex))
            {
                var agent = EconService.Agents.Values.FirstOrDefault(a => a.Index == agentIndex);
                if (agent != null)
                {
                    var modelPath = agent.ModelPath;
                    var refreshModel = GetRefreshModel(current, modelPath);
                    if (!string.IsNullOrWhiteSpace(refreshModel))
                    {
                        pawn.SetModel(refreshModel);
                        pawn.SetModel(current);
                    }

                    Core.Scheduler.NextWorldUpdate(() =>
                    {
                        if (!player.IsAlive()) return;
                        pawn.SetModel(modelPath);
                    });
                }
            }
            else if (DataService.AgentDataService.TryGetDefaultModel(player.SteamID, player.Controller.Team,
                         out var defaultModel))
            {
                var refreshModel = GetRefreshModel(current, defaultModel);
                if (!string.IsNullOrWhiteSpace(refreshModel))
                {
                    pawn.SetModel(refreshModel);
                    pawn.SetModel(current);
                }

                Core.Scheduler.NextWorldUpdate(() =>
                {
                    if (!player.IsAlive()) return;
                    pawn.SetModel(defaultModel);
                });
            }
        });
    }

    private void OnSOCacheSubscribed(CCSPlayerInventory inventory,
        SOID_t soid)
    {
        Task.Run(async () =>
        {
            var steamId = soid.SteamID;
            
            var skins = await StorageService.Get().GetSkinsAsync(steamId);
            foreach (var skin in skins)
            {
                DataService.WeaponDataService.StoreSkin(skin);
            }
            
            var knives = await StorageService.Get().GetKnifesAsync(steamId);
            foreach (var knife in knives)
            {
                DataService.KnifeDataService.StoreKnife(knife);
            }
            
            var gloves = await StorageService.Get().GetGlovesAsync(steamId);
            foreach (var glove in gloves)
            {
                DataService.GloveDataService.StoreGlove(glove);
            }
            
            var agents = await StorageService.Get().GetAgentsAsync(steamId);
            foreach (var agent in agents)
            {
                DataService.AgentDataService.SetAgent(agent.SteamID, agent.Team, agent.AgentIndex);
            }
            
            Core.Scheduler.NextWorldUpdate(() => Update(inventory));
        });
    }

    private void Update(CCSPlayerInventory inventory)
    {
        if (Api.TryGetWeaponSkins(inventory.SteamID, out var skins))
        {
            foreach (var skin in skins)
            {
                inventory.UpdateWeaponSkin(skin);
            }
        }

        if (Api.TryGetKnifeSkins(inventory.SteamID, out var knives))
        {
            foreach (var knife in knives)
            {
                inventory.UpdateKnifeSkin(knife);
            }
        }

        if (Api.TryGetGloveSkins(inventory.SteamID, out var gloves))
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
            if (ItemPermissionService.TryBuildWeaponSkinView(skin, out var runtimeSkin))
            {
                if (DataService.WeaponDataService.StoreSkin(skin) || !runtimeSkin.Equals(skin))
                {
                    updatedSkinMaps.GetOrAdd(skin.SteamID, () => new()).Add(runtimeSkin);
                }
            }
        }

        foreach (var (steamID, updatedSkins) in updatedSkinMaps)
        {
            InventoryService.UpdateWeaponSkins(steamID, updatedSkins);

            if (PlayerService.TryGetPlayer(steamID, out var player))
            {
                // Logger.LogInformation($"Updating skins for player {player}. IsAlive: {player.IsAlive()}");
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
            if (ItemPermissionService.TryBuildKnifeSkinView(knife, out var runtimeKnife))
            {
                if (DataService.KnifeDataService.StoreKnife(knife) || !runtimeKnife.Equals(knife))
                {
                    updatedKnifeMaps.GetOrAdd(knife.SteamID, () => new()).Add(runtimeKnife);
                }
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
                                player.RegiveKnife();
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
            if (ItemPermissionService.TryBuildGloveView(glove, out var runtimeGlove))
            {
                if (DataService.GloveDataService.StoreGlove(glove) || !runtimeGlove.Equals(glove))
                {
                    updatedGloveMaps.GetOrAdd(glove.SteamID, () => new()).Add(runtimeGlove);
                }
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
                            player.RegiveGlove(InventoryService.Get(steamID));
                        }
                    }
                }
            }
        }
    }

    public void ResetWeaponSkin(ulong steamid,
        Team team,
        ushort definitionIndex)
    {
        if (DataService.WeaponDataService.TryRemoveSkin(steamid, team, definitionIndex))
        {
            InventoryService.ResetWeaponSkin(steamid, team, definitionIndex);
            if (PlayerService.TryGetPlayer(steamid, out var player))
            {
                Core.Scheduler.NextWorldUpdate(() =>
                {
                    if (player.IsAlive())
                    {
                        player.RegiveWeapon(
                            player.PlayerPawn!.WeaponServices!.MyWeapons.FirstOrDefault(w =>
                                w.Value!.AttributeManager.Item.ItemDefinitionIndex == definitionIndex &&
                                player.Controller.Team == team).Value!, definitionIndex);
                    }
                });
            }
        }
    }

    public void ResetKnifeSkin(ulong steamid,
        Team team)
    {
        if (DataService.KnifeDataService.TryRemoveKnife(steamid, team))
        {
            InventoryService.ResetKnifeSkin(steamid, team);
            if (PlayerService.TryGetPlayer(steamid, out var player))
            {
                Core.Scheduler.NextWorldUpdate(() =>
                {
                    if (player.IsAlive())
                    {
                        player.RegiveKnife();
                    }
                });
            }
        }
    }

    public void ResetGloveSkin(ulong steamid,
        Team team)
    {
        if (DataService.GloveDataService.TryRemoveGlove(steamid, team))
        {
            InventoryService.ResetGloveSkin(steamid, team);
            if (PlayerService.TryGetPlayer(steamid, out var player))
            {
                Core.Scheduler.NextWorldUpdate(() =>
                {
                    if (player.IsAlive())
                    {
                        player.RegiveGlove(InventoryService.Get(steamid));
                    }
                });
            }
        }
    }
}