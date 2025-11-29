using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

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
        var name = Core.Helpers.GetClassnameByDefinitionIndex(newIndex)!;
        var clip1 = weapon.Clip1;
        var reservedAmmo = weapon.ReserveAmmo[0];
        player.PlayerPawn!.WeaponServices!.RemoveWeapon(weapon);
        var newWeapon = player.PlayerPawn!.ItemServices!.GiveItem<CBasePlayerWeapon>(name);
        newWeapon.Clip1 = clip1;
        newWeapon.ReserveAmmo[0] = reservedAmmo;
    }

    public static void RegiveKnife(this IPlayer player,
        ushort defIndex)
    {
        player.PlayerPawn!.WeaponServices!.RemoveWeaponBySlot(gear_slot_t.GEAR_SLOT_KNIFE);
        player.PlayerPawn!.ItemServices!.GiveItem("weapon_knife");
        player.PlayerPawn!.WeaponServices!.SelectWeaponBySlot(gear_slot_t.GEAR_SLOT_KNIFE);
    }
}