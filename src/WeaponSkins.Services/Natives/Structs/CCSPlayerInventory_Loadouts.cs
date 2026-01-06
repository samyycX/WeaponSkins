using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace WeaponSkins;

public struct LoadoutItem
{
    public ulong ItemId;
    public ushort DefinitionIndex;

    public override string ToString()
    {
        return $"ItemId: {ItemId}, DefinitionIndex: {DefinitionIndex}";
    }
}

[InlineArray(57)] /* slot */
public struct LoadoutSlots
{
    private LoadoutItem _element0;
}

[InlineArray(4)]
public struct LoadoutTeams
{
    private LoadoutSlots _element0;
}

public struct CCSPlayerInventory_Loadouts
{
    private LoadoutTeams _element0;

    [UnscopedRef] public ref LoadoutItem this[Team team, loadout_slot_t slot] => ref this[(int)team, (int)slot];

    [UnscopedRef] public ref LoadoutItem this[Team team, int slot] => ref this[(int)team, slot];

    [UnscopedRef] public ref LoadoutItem this[int team, loadout_slot_t slot] => ref this[team, (int)slot];

    [UnscopedRef] public ref LoadoutItem this[int team, int slot] => ref _element0[team][slot];


    public void DebugPrint()
    {
        for (var team = 0; team < 4; team++)
        {
            for (var slot = 0; slot < 57; slot++)
            {
                Console.WriteLine(
                    $"{(Team)team} {(loadout_slot_t)slot} {this[team, slot].ItemId} {this[team, slot].DefinitionIndex}");
            }
        }
    }
}