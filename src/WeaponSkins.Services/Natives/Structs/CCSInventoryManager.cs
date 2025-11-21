using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace WeaponSkins;

public struct CCSInventoryManager : INativeHandle
{
    public nint Address { get; set; }
    public bool IsValid => Address != 0;

    private NativeService NativeService { get; init; }

    public CCSInventoryManager(nint address, NativeService nativeService)
    {
        Address = address;
        NativeService = nativeService;
    }

    private nint defaultLoadoutsStart => Address + NativeService.CCSInventoryManager_m_DefaultLoadoutsOffset;

    public IEnumerable<(loadout_slot_t, CEconItemView)> GetDefaultLoadouts(Team team)
    {
        var start = defaultLoadoutsStart;
        return Enumerable.Range(0, (int)loadout_slot_t.LOADOUT_SLOT_COUNT).Select(slot =>
        {
            return ((loadout_slot_t)slot,
                Helper.AsSchema<CEconItemView>(start + ((int)team * (int)loadout_slot_t.LOADOUT_SLOT_COUNT + slot) *
                    Helper.GetSchemaSize<CEconItemView>()));
        });
    }
}