using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

using WeaponSkins.Services;

namespace WeaponSkins.Extensions;

public static class PlayerExtensions
{
    [SwiftlyInject] private static ISwiftlyCore Core { get; set; } = null!;

    public static bool IsAlive(this IPlayer player)
    {
        return player.Controller is
            { IsValid: true, PawnIsAlive: true, PlayerPawn: { Value: { LifeState: (byte)LifeState_t.LIFE_ALIVE } } };
    }

    public static void RegiveWeapon(this IPlayer player,
        CBasePlayerWeapon weapon,
        ushort newIndex)
    {
        if (newIndex == Core.Helpers.GetDefinitionIndexByClassname("weapon_taser"))
        {
            player.RegiveTaser(weapon);
            return;
        }

        var name = Core.Helpers.GetClassnameByDefinitionIndex(newIndex)!;
        var clip1 = weapon.Clip1;
        var reservedAmmo = weapon.ReserveAmmo[0];
        player.PlayerPawn!.WeaponServices!.RemoveWeapon(weapon);
        var newWeapon = player.PlayerPawn!.ItemServices!.GiveItem<CBasePlayerWeapon>(name);
        newWeapon.Clip1 = clip1;
        newWeapon.ReserveAmmo[0] = reservedAmmo;
    }

    public static void RegiveTaser(this IPlayer player,
        CBasePlayerWeapon weapon)
    {
        var oldTaser = weapon.As<CWeaponTaser>();
        var clip1 = oldTaser.Clip1;
        var reservedAmmo = oldTaser.ReserveAmmo[0];
        var fireTime = oldTaser.FireTime.Value;
        var lastAttackTick = oldTaser.LastAttackTick;
        player.PlayerPawn!.WeaponServices!.RemoveWeapon(weapon);
        var newWeapon = player.PlayerPawn!.ItemServices!.GiveItem<CWeaponTaser>("weapon_taser");
        newWeapon.Clip1 = clip1;
        newWeapon.ReserveAmmo[0] = reservedAmmo;
        newWeapon.FireTime.Value = fireTime;
        newWeapon.LastAttackTick = lastAttackTick;
    }

    public static void RegiveKnife(this IPlayer player)
    {
        player.PlayerPawn!.WeaponServices!.RemoveWeaponBySlot(gear_slot_t.GEAR_SLOT_KNIFE);
        player.PlayerPawn!.ItemServices!.GiveItem("weapon_knife");
        player.PlayerPawn!.WeaponServices!.SelectWeaponBySlot(gear_slot_t.GEAR_SLOT_KNIFE);
    }

    public static void RegiveGlove(this IPlayer player,
        CCSPlayerInventory inv)
    {
        Core.Scheduler.NextWorldUpdate(() =>
        {
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
                var itemInLoadout =
                    inv.GetItemInLoadout(player.Controller.Team, loadout_slot_t.LOADOUT_SLOT_CLOTHING_HANDS)!;
                econGloves.ItemDefinitionIndex = itemInLoadout.ItemDefinitionIndex;
                econGloves.AccountID = itemInLoadout.AccountID;
                econGloves.ItemID = itemInLoadout.ItemID;
                econGloves.ItemIDHigh = itemInLoadout.ItemIDHigh;
                econGloves.ItemIDLow = itemInLoadout.ItemIDLow;
                econGloves.InventoryPosition = itemInLoadout.InventoryPosition;
                econGloves.EntityLevel = itemInLoadout.EntityLevel;
                econGloves.EntityQuality = itemInLoadout.EntityQuality;
                StaticNativeService.Service.UpdateItemView.CallOriginal(
                    econGloves.Address, 0);
                player.PlayerPawn.AcceptInput("SetBodygroup", "default_gloves,1");
            });
        });
    }
}