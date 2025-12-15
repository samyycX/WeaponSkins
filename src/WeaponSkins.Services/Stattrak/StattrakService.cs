using Microsoft.Extensions.Logging;

using Mysqlx.Session;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Misc;

using WeaponSkins.Services;

namespace WeaponSkins;

public class StattrakService
{
    private ISwiftlyCore Core { get; init; }
    private DataService DataService { get; init; }
    private WeaponSkinAPI WeaponSkinAPI { get; init; }
    private InventoryService InventoryService { get; init; }
    private ILogger<StattrakService> Logger { get; init; }

    public StattrakService(ISwiftlyCore core,
        DataService dataService,
        WeaponSkinAPI weaponSkinAPI,
        InventoryService inventoryService,
        ILogger<StattrakService> logger)
    {
        Core = core;
        DataService = dataService;
        WeaponSkinAPI = weaponSkinAPI;
        InventoryService = inventoryService;
        Logger = logger;

        Core.GameEvent.HookPost<EventPlayerDeath>(OnPlayerDeath);
    }

    public HookResult OnPlayerDeath(EventPlayerDeath @event)
    {
        var attacker = Core.PlayerManager.GetPlayer(@event.Attacker);
        if (attacker is null || attacker is { IsValid: false, IsFakeClient: false }) return HookResult.Continue;

        if (!@event.UserIdController.IsValid) return HookResult.Continue;

        var weaponHandle = attacker.PlayerPawn!.WeaponServices!.ActiveWeapon;
        if (!weaponHandle.IsValid) return HookResult.Continue;

        var weapon = weaponHandle.Value!;
        var definitionIndex = weapon!.AttributeManager.Item.ItemDefinitionIndex;
        var team = attacker.Controller.Team;

        if (Utilities.IsWeaponDefinitionIndex(definitionIndex))
        {
            if (DataService.WeaponDataService.TryGetSkin(attacker.SteamID, team, definitionIndex, out var skin))
            {
                if (skin.Quality != EconItemQuality.StatTrak) return HookResult.Continue;
                skin.StattrakCount++;
                weapon.AttributeManager.Item.AttributeList.SetOrAddAttribute("kill eater",
                    BitConverter.Int32BitsToSingle(skin.StattrakCount));
                weapon.AttributeManager.Item.AttributeList.SetOrAddAttribute("kill eater score type", 0);
                WeaponSkinAPI.SetWeaponSkins([skin], true);
                if (InventoryService.TryGet(attacker.SteamID, out var inventory))
                {
                    inventory.UpdateWeaponSkin(skin);
                }
            }
        }

        if (Utilities.IsKnifeDefinitionIndex(definitionIndex))
        {
            if (DataService.KnifeDataService.TryGetKnife(attacker.SteamID, team, out var knife))
            {
                if (knife.Quality != EconItemQuality.StatTrak) return HookResult.Continue;
                knife.StattrakCount++;
                weapon.AttributeManager.Item.AttributeList.SetOrAddAttribute("kill eater",
                    BitConverter.Int32BitsToSingle(knife.StattrakCount));
                weapon.AttributeManager.Item.AttributeList.SetOrAddAttribute("kill eater score type", 0);
                WeaponSkinAPI.SetKnifeSkins([knife], true);
                if (InventoryService.TryGet(attacker.SteamID, out var inventory))
                {
                    inventory.UpdateKnifeSkin(knife);
                }
            }
        }

        return HookResult.Continue;
    }
}