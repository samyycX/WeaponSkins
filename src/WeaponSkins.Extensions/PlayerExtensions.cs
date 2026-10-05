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
        CBasePlayerWeapon? weapon,
        ushort newIndex)
    {
        if (weapon is not { IsValid: true } || !player.IsAlive()) return;

        if (newIndex == Core.Helpers.GetDefinitionIndexByClassname("weapon_taser"))
        {
            player.RegiveTaser(weapon);
            return;
        }

        var pawn = player.PlayerPawn;
        if (pawn is not { IsValid: true }) return;
        if (pawn.WeaponServices is not { IsValid: true } weaponServices ||
            pawn.ItemServices is not { IsValid: true } itemServices) return;

        var name = Core.Helpers.GetClassnameByDefinitionIndex(newIndex);
        if (string.IsNullOrWhiteSpace(name)) return;

        var clip1 = weapon.Clip1;
        var reservedAmmo = weapon.ReserveAmmo[0];
        weaponServices.RemoveWeapon(weapon);
        var newWeapon = itemServices.GiveItem<CBasePlayerWeapon>(name);
        if (newWeapon is not { IsValid: true }) return;

        newWeapon.Clip1 = clip1;
        newWeapon.ReserveAmmo[0] = reservedAmmo;
    }

    public static void RegiveTaser(this IPlayer player,
        CBasePlayerWeapon? weapon)
    {
        if (weapon is not { IsValid: true } || !player.IsAlive()) return;

        var pawn = player.PlayerPawn;
        if (pawn is not { IsValid: true }) return;
        if (pawn.WeaponServices is not { IsValid: true } weaponServices ||
            pawn.ItemServices is not { IsValid: true } itemServices) return;

        var oldTaser = weapon.As<CWeaponTaser>();
        var clip1 = oldTaser.Clip1;
        var reservedAmmo = oldTaser.ReserveAmmo[0];
        var fireTime = oldTaser.FireTime.Value;
        var lastAttackTick = oldTaser.LastAttackTick;
        weaponServices.RemoveWeapon(weapon);
        var newWeapon = itemServices.GiveItem<CWeaponTaser>("weapon_taser");
        if (newWeapon is not { IsValid: true }) return;

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
