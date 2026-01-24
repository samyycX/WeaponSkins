using FluentMigrator.Runner;

using Microsoft.Extensions.Logging;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

using WeaponSkins.Econ;
using WeaponSkins.Extensions;
using WeaponSkins.Shared;

namespace WeaponSkins.Services;

public class HookInventoryUpdateService : IInventoryUpdateService
{
    private ISwiftlyCore Core { get; }
    private InventoryService InventoryService { get; }
    private PlayerService PlayerService { get; }
    private NativeService NativeService { get; }
    private ILogger<HookInventoryUpdateService> Logger { get; }
    private ItemPermissionService ItemPermissionService { get; }
    private WeaponSkinGetterAPI Api { get; }
    private DataService DataService { get; }
    private EconService EconService { get; }

    public HookInventoryUpdateService(ISwiftlyCore core,
        InventoryService inventoryService,
        WeaponSkinGetterAPI api,
        PlayerService playerService,
        NativeService nativeService,
        ILogger<HookInventoryUpdateService> logger,
        ItemPermissionService itemPermissionService,
        DataService dataService,
        EconService econService)
    {
        Core = core;
        InventoryService = inventoryService;
        Api = api;
        PlayerService = playerService;
        NativeService = nativeService;
        Logger = logger;
        ItemPermissionService = itemPermissionService;
        DataService = dataService;
        EconService = econService;

        NativeService.OnGiveNamedItemPost += OnGiveNamedItemPost;
        Core.GameEvent.HookPost<EventPlayerSpawn>(OnPlayerSpawn);

        foreach (var player in Core.PlayerManager.GetAllPlayers())
        {
            if (player.Controller is { IsValid: true, InventoryServices.IsValid: true } controller)
            {
                ApplyPlayerWeapons(player);
                ApplyPlayerGlove(player);
            }
        }
    }

    private void OnGiveNamedItemPost(CCSPlayer_ItemServices services,
        CBasePlayerWeapon weapon)
    {
        try
        {
            var ownerHandle = weapon.OwnerEntity;
            if (!ownerHandle.IsValid) return;
            var owner = ownerHandle.Value?.As<CCSPlayerPawn>();
            if (owner == null || !owner.IsValid) return;
            var controllerHandle = owner.Controller;
            if (!controllerHandle.IsValid) return;
            var controller = controllerHandle.Value;
            if (controller == null || !controller.IsValid) return;
            ApplyWeaponSkin(controller.SteamID, controller.Team, weapon);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error in GiveNamedItemPost");
        }
    }

    private HookResult OnPlayerSpawn(EventPlayerSpawn @event)
    {
        IPlayer player = @event.UserIdPlayer;

        Core.Scheduler.NextWorldUpdate(() =>
        {
            ApplyPlayerGlove(player);
            ApplyPlayerAgent(player);
        });

        Core.Scheduler.DelayBySeconds(0.1f, () =>
        {
            ApplyPlayerGlove(player);
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
        if (!player.IsAlive()) return;

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
                if (player.IsAlive())
                {
                    ApplyWeaponSkins(player, updatedSkins);

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
                    ApplyKnifeSkins(player, updatedKnives);

                    foreach (var knife in updatedKnives)
                    {
                        foreach (var weapon in player.PlayerPawn!.WeaponServices!.MyWeapons)
                        {
                            if (weapon.Value!.AttributeManager.Item.ItemDefinitionIndex == knife.DefinitionIndex &&
                                player.Controller.Team == knife.Team)
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
                    ApplyGlove(player, updatedGloves.Last());
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

    private void ApplyWeaponSkins(IPlayer player,
        IEnumerable<WeaponSkinData> skins)
    {
        var weaponMap = skins.ToDictionary(s => s.DefinitionIndex, s => s);
        foreach (var handle in player.PlayerPawn!.WeaponServices!.MyWeapons)
        {
            var weapon = handle.Value;
            if (weapon == null || !weapon.IsValid) continue;
            var def = weapon.AttributeManager.Item.ItemDefinitionIndex;
            if (weaponMap.TryGetValue(def, out var skin))
            {
                ApplyWeaponAttributes(weapon, skin);
            }
        }
    }

    private void ApplyKnifeSkins(IPlayer player,
        IEnumerable<KnifeSkinData> knives)
    {
        foreach (var handle in player.PlayerPawn!.WeaponServices!.MyWeapons)
        {
            var weapon = handle.Value;
            if (weapon == null || !weapon.IsValid) continue;
            if (!Utilities.IsKnifeDefinitionIndex(weapon.AttributeManager.Item.ItemDefinitionIndex)) continue;
            var knife = knives.LastOrDefault(k => k.Team == player.Controller.Team);
            if (knife != null)
            {
                ApplyKnifeAttributes(weapon, knife);
            }
        }
    }

    private void ApplyPlayerWeapons(IPlayer player)
    {
        if (!Api.TryGetWeaponSkins(player.SteamID, out var weaponSkins) &&
            !Api.TryGetKnifeSkins(player.SteamID, out var knifeSkins))
        {
            return;
        }

        foreach (var handle in player.PlayerPawn!.WeaponServices!.MyWeapons)
        {
            var weapon = handle.Value;
            if (weapon == null || !weapon.IsValid) continue;
            ApplyWeaponSkin(player.SteamID, player.Controller.Team, weapon);
        }
    }

    private void ApplyPlayerGlove(IPlayer player)
    {
        if (!player.IsAlive()) return;
        var pawn = player.PlayerPawn!;
        ApplyGlove(player, pawn);
    }

    private void ApplyWeaponSkin(ulong steamId,
        Team team,
        CBasePlayerWeapon weapon)
    {
        var def = weapon.AttributeManager.Item.ItemDefinitionIndex;
        if (Utilities.IsKnifeDefinitionIndex(def))
        {
            if (Api.TryGetKnifeSkin(steamId, team, out var knife))
            {
                ApplyKnifeAttributes(weapon, knife);
            }

            return;
        }

        if (Utilities.IsWeaponDefinitionIndex(def))
        {
            if (Api.TryGetWeaponSkin(steamId, team, (ushort)def, out var skin))
            {
                ApplyWeaponAttributes(weapon, skin);
            }
        }
    }

    private void ApplyWeaponAttributes(CBasePlayerWeapon weapon,
        WeaponSkinData skin)
    {
        StickerFixService.FixSticker(skin);
        var item = weapon.AttributeManager.Item;
        item.ItemDefinitionIndex = skin.DefinitionIndex;
        item.EntityQuality = (int)skin.Quality;
        item.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture prefab", skin.Paintkit);
        item.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture seed", skin.PaintkitSeed);
        item.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture wear", skin.PaintkitWear);
        item.AttributeList.SetOrAddAttribute("set item texture prefab", skin.Paintkit);
        item.AttributeList.SetOrAddAttribute("set item texture seed", skin.PaintkitSeed);
        item.AttributeList.SetOrAddAttribute("set item texture wear", skin.PaintkitWear);

        var useLegacy = EconService
            .WeaponToPaintkits[Core.Helpers.GetClassnameByDefinitionIndex(item.ItemDefinitionIndex)]
            .Where(p => p.Index == skin.Paintkit).FirstOrDefault().UseLegacyModel;
        weapon.AcceptInputAsync("SetBodygroup", value: $"body,{(useLegacy ? 1 : 0)}");

        if (skin.Quality == EconItemQuality.StatTrak)
        {
            var val = BitConverter.Int32BitsToSingle(skin.StattrakCount);
            item.AttributeList.SetOrAddAttribute("kill eater", val);
            item.AttributeList.SetOrAddAttribute("kill eater score type", 0);
            item.NetworkedDynamicAttributes.SetOrAddAttribute("kill eater", val);
            item.NetworkedDynamicAttributes.SetOrAddAttribute("kill eater score type", 0);
        }

        if (skin.Nametag != null)
        {
            item.CustomName = skin.Nametag;
        }

        for (var i = 0; i < 6; i++)
        {
            var sticker = skin.GetSticker(i);
            if (sticker == null) continue;
            item.NetworkedDynamicAttributes.SetOrAddAttribute($"sticker slot {i} id",
                BitConverter.Int32BitsToSingle(sticker.Id));
            if (sticker.Schema != 1337)
            {
                item.NetworkedDynamicAttributes.SetOrAddAttribute($"sticker slot {i} schema",
                    BitConverter.Int32BitsToSingle(sticker.Schema));
                item.NetworkedDynamicAttributes.SetOrAddAttribute($"sticker slot {i} offset x", sticker.OffsetX);
                item.NetworkedDynamicAttributes.SetOrAddAttribute($"sticker slot {i} offset y", sticker.OffsetY);
            }

            item.NetworkedDynamicAttributes.SetOrAddAttribute($"sticker slot {i} wear", sticker.Wear);
            item.NetworkedDynamicAttributes.SetOrAddAttribute($"sticker slot {i} scale", sticker.Scale);
            item.NetworkedDynamicAttributes.SetOrAddAttribute($"sticker slot {i} rotation", sticker.Rotation);
        }

        var keychain = skin.Keychain0;
        if (keychain != null)
        {
            item.NetworkedDynamicAttributes.SetOrAddAttribute("keychain slot 0 id",
                BitConverter.Int32BitsToSingle(keychain.Id));
            item.NetworkedDynamicAttributes.SetOrAddAttribute("keychain slot 0 offset x", keychain.OffsetX);
            item.NetworkedDynamicAttributes.SetOrAddAttribute("keychain slot 0 offset y", keychain.OffsetY);
            item.NetworkedDynamicAttributes.SetOrAddAttribute("keychain slot 0 offset z", keychain.OffsetZ);
            item.NetworkedDynamicAttributes.SetOrAddAttribute("keychain slot 0 seed",
                BitConverter.Int32BitsToSingle(keychain.Seed));
        }
    }

    private void ApplyKnifeAttributes(CBasePlayerWeapon weapon,
        KnifeSkinData knife)
    {
        var item = weapon.AttributeManager.Item;
        item.EntityQuality = (int)knife.Quality;

        item.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture prefab", knife.Paintkit);
        item.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture seed", knife.PaintkitSeed);
        item.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture wear", knife.PaintkitWear);
        item.AttributeList.SetOrAddAttribute("set item texture prefab", knife.Paintkit);
        item.AttributeList.SetOrAddAttribute("set item texture seed", knife.PaintkitSeed);
        item.AttributeList.SetOrAddAttribute("set item texture wear", knife.PaintkitWear);
        if (knife.Nametag != null)
        {
            item.CustomName = knife.Nametag;
        }


        if (item.ItemDefinitionIndex != knife.DefinitionIndex)
        {
            weapon.AcceptInputAsync("ChangeSubclass", knife.DefinitionIndex.ToString());
        }

        item.ItemDefinitionIndex = knife.DefinitionIndex;


        if (knife.Quality == EconItemQuality.StatTrak)
        {
            var val = BitConverter.Int32BitsToSingle(knife.StattrakCount);
            item.AttributeList.SetOrAddAttribute("kill eater", val);
            item.AttributeList.SetOrAddAttribute("kill eater score type", 0);
            item.NetworkedDynamicAttributes.SetOrAddAttribute("kill eater", val);
            item.NetworkedDynamicAttributes.SetOrAddAttribute("kill eater score type", 0);
        }
    }

    private void ApplyGlove(IPlayer player,
        GloveData glove)
    {
        if (!player.IsAlive()) return;
        ApplyGlove(player.PlayerPawn!, glove);
    }

    private void ApplyGlove(IPlayer player,
        CCSPlayerPawn pawn)
    {
        if (!Api.TryGetGloveSkin(player.SteamID, player.Controller.Team, out var glove)) return;
        ApplyGlove(pawn, glove);
    }

    private void ApplyGlove(CCSPlayerPawn pawn,
        GloveData glove)
    {
        Core.Scheduler.NextWorldUpdate(() =>
        {
            var model = pawn.CBodyComponent!.SceneNode!.GetSkeletonInstance()
                .ModelState
                .ModelName;
            pawn.SetModel("characters/models/tm_jumpsuit/tm_jumpsuit_varianta.vmdl");
            pawn.SetModel(model);

            Core.Scheduler.NextWorldUpdate(() =>
            {
                var econGloves = pawn.EconGloves;
                econGloves.ItemDefinitionIndex = glove.DefinitionIndex;
                econGloves.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture prefab", glove.Paintkit);
                econGloves.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture seed", glove.PaintkitSeed);
                econGloves.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture wear", glove.PaintkitWear);
                econGloves.AttributeList.SetOrAddAttribute("set item texture prefab", glove.Paintkit);
                econGloves.AttributeList.SetOrAddAttribute("set item texture seed", glove.PaintkitSeed);
                econGloves.AttributeList.SetOrAddAttribute("set item texture wear", glove.PaintkitWear);
                econGloves.Initialized = true;
                pawn.AcceptInput("SetBodygroup", "default_gloves,1");
            });
        });
    }
}