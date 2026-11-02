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
        ushort newIndex,
        Action<CBasePlayerWeapon>? paint = null)
    {
        if (weapon is not { IsValid: true } || !player.IsAlive()) return;

        if (newIndex == Core.Helpers.GetDefinitionIndexByClassname("weapon_taser"))
        {
            player.RegiveTaser(weapon, paint);
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
        var wasActive = weaponServices.ActiveWeapon.Value?.Address == weapon.Address;
        weaponServices.RemoveWeapon(weapon);
        var newWeapon = itemServices.GiveItem<CBasePlayerWeapon>(name);
        if (newWeapon is not { IsValid: true }) return;

        newWeapon.Clip1 = clip1;
        newWeapon.ReserveAmmo[0] = reservedAmmo;
        // Painted before it is selected or first networked: the client keeps the finish a
        // weapon was drawn with, and the give hook's own paint only lands a tick later.
        paint?.Invoke(newWeapon);
        // Dropping the held weapon switches away from it; switch back to show the new finish.
        if (wasActive) weaponServices.SelectWeapon(newWeapon);
    }

    public static void RegiveTaser(this IPlayer player,
        CBasePlayerWeapon? weapon,
        Action<CBasePlayerWeapon>? paint = null)
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
        var wasActive = weaponServices.ActiveWeapon.Value?.Address == weapon.Address;
        weaponServices.RemoveWeapon(weapon);
        var newWeapon = itemServices.GiveItem<CWeaponTaser>("weapon_taser");
        if (newWeapon is not { IsValid: true }) return;

        newWeapon.Clip1 = clip1;
        newWeapon.ReserveAmmo[0] = reservedAmmo;
        newWeapon.FireTime.Value = fireTime;
        newWeapon.LastAttackTick = lastAttackTick;
        paint?.Invoke(newWeapon);
        if (wasActive) weaponServices.SelectWeapon(newWeapon);
    }

    public static void RegiveKnife(this IPlayer player,
        Action<CBasePlayerWeapon>? paint = null)
    {
        player.PlayerPawn!.WeaponServices!.RemoveWeaponBySlot(gear_slot_t.GEAR_SLOT_KNIFE);
        // weapon_knife is the CT default.
        var knife = player.PlayerPawn!.ItemServices!.GiveItem<CBasePlayerWeapon>(
            player.Controller.Team == Team.T ? "weapon_knife_t" : "weapon_knife");
        if (knife is { IsValid: true }) paint?.Invoke(knife);
        player.PlayerPawn!.WeaponServices!.SelectWeaponBySlot(gear_slot_t.GEAR_SLOT_KNIFE);
    }
}
