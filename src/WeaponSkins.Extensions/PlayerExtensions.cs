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
        return player.IsAlive;
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
}