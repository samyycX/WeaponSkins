using System.Runtime.CompilerServices;

using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;
using SwiftlyS2.Shared.SteamAPI;

using WeaponSkins.Shared;

namespace WeaponSkins;

public class CCSPlayerInventory : INativeHandle
{
    public nint Address { get; set; }
    public bool IsValid => Address != 0;

    public ulong SteamID => SOCache.Owner.SteamID;

    private NativeService NativeService { get; init; }

    public CCSPlayerInventory(nint address, NativeService nativeService)
    {
        Address = address;
        NativeService = nativeService;
    }

    public CGCClientSharedObjectCache SOCache =>
        new CGCClientSharedObjectCache(Address.Read<nint>(NativeService.CCSPlayerInventory_m_pSOCacheOffset),
            NativeService);

    public void SODestroyed(ulong steamid, CEconItem item)
    {
        var soid = new SOID_t(steamid);
        unsafe
        {
            NativeService.CPlayerInventory_SODestroyed.Call(Address, &soid, item.Address,
                4 /* eSOCacheEvent_Incremental */);
        }
    }

    public void SOCreated(ulong steamid, CEconItem item)
    {
        var soid = new SOID_t(steamid);
        unsafe
        {
            NativeService.CPlayerInventory_SOCreated.Call(Address, &soid, item.Address,
                4 /* eSOCacheEvent_Incremental */);
        }
    }

    public void SOUpdated(ulong steamid, CEconItem item)
    {
        var soid = new SOID_t(steamid);
        unsafe
        {
            NativeService.CPlayerInventory_SOUpdated.Call(Address, &soid, item.Address,
                4 /* eSOCacheEvent_Incremental */);
        }
    }

    public CEconItem? GetEconItemByItemID(ulong itemid)
    {
        unsafe
        {
            var ptr = NativeService.GetEconItemByItemID.Call(Address, itemid);
            if (ptr == 0)
            {
                return null;
            }

            return new CEconItem(ptr);
        }
    }

    public ref CUtlVector<PointerTo<CEconItemView>> Items =>
        ref Address.AsRef<CUtlVector<PointerTo<CEconItemView>>>(NativeService.CCSPlayerInventory_m_ItemsOffset);

    public ref CCSPlayerInventory_Loadouts Loadouts =>
        ref Address.AsRef<CCSPlayerInventory_Loadouts>(NativeService.CCSPlayerInventory_LoadoutsOffset);

    private bool TryGetLoadoutItem(Team team, ushort definitionIndex, out (Team team, loadout_slot_t slot) indices)
    {
        indices = default;
        for (var slot = 0; slot < (int)loadout_slot_t.LOADOUT_SLOT_COUNT; slot++)
        {
            if (Loadouts[team, slot].DefinitionIndex == definitionIndex)
            {
                Console.WriteLine("TryGetLoadoutItem: Found item in loadout slot {0}", slot);
                Console.WriteLine("TryGetLoadoutItem: Definition index {0}", definitionIndex);
                Console.WriteLine("TryGetLoadoutItem: Loadout item {0}", Loadouts[team, slot].ToString());
                indices = (team, (loadout_slot_t)slot);
                return true;
            }
        }

        // itemid ==0, defindex == 65535, meaning the loadout slot is default weapon and default skin
        // so we search in default loadouts.
        foreach (var (slot, itemView) in NativeService.CCSInventoryManager.GetDefaultLoadouts(team))
        {
            if (itemView.ItemDefinitionIndex == definitionIndex)
            {
                indices = (team, slot);
                return true;
            }
        }

        // absent in loadout
        return false;
    }

    private ulong GetHighestItemID()
    {
        return Items.Select(item => item.Value.ItemID).Where(IsValidItemID).Max();
    }

    private uint GetHighestInventoryPosition()
    {
        return Items.Max(item => item.Value.InventoryPosition);
    }

    private ulong GetNewItemID()
    {
        return GetHighestItemID() + 0x0000000100000000;
    }

    private uint GetNewInventoryPosition()
    {
        return GetHighestInventoryPosition() + 1;
    }

    private bool IsValidItemID(ulong itemID)
    {
        // 0xF00000000000000: default skin so no item id, but loadout item changed
        return itemID != 0 && itemID < 0xF000000000000000;
    }

    public bool TryGetItemID(Team team, ushort definitionIndex, out ulong itemID)
    {
        itemID = 0;
        if (TryGetLoadoutItem(team, definitionIndex, out var indices))
        {
            ref var loadout = ref Loadouts[indices.team, indices.slot];
            Console.WriteLine("TryGetItemID: Loadout item {0}", loadout.ToString());
            itemID = loadout.ItemId;
            if (!IsValidItemID(itemID))
            {
                itemID = 0;
                return false;
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    public void UpdateLoadoutItem(Team team, ushort definitionIndex, ulong itemID)
    {
        if (TryGetLoadoutItem(team, definitionIndex, out var indices))
        {
            ref var loadout = ref Loadouts[indices.team, indices.slot];
            loadout.ItemId = itemID;
            loadout.DefinitionIndex = definitionIndex;
        }
        // do nothing
    }

    public void UpdateWeaponSkin(WeaponSkinData skinData)
    {
        Console.WriteLine("UpdateWeaponSkin: {0}", skinData.ToString());
        unsafe
        {
            Console.WriteLine("UpdateWeaponSkin: Creating item");
            var item = NativeService.CreateCEconItemInstance();
            // Already has a skin
            Console.WriteLine("UpdateWeaponSkin: Trying to get item ID");
            if (TryGetItemID(skinData.Team, skinData.DefinitionIndex, out var itemID))
            {
                Console.WriteLine("UpdateWeaponSkin: Item ID found");
                var oldItem =
                    GetEconItemByItemID(itemID); // this should never be null, since item id is already in loadouts
                if (oldItem == null)
                {
                    throw new Exception($"GetEconItemByItemID returned null for item id {itemID}");
                }

                Console.WriteLine("UpdateWeaponSkin: Item found");
                item.AccountID = oldItem.AccountID;
                item.ItemID = oldItem.ItemID;
                item.InventoryPosition = oldItem.InventoryPosition;
                item.AccountID = new CSteamID(SteamID).GetAccountID().m_AccountID;
                item.ItemID = GetNewItemID();
                Console.WriteLine("UpdateWeaponSkin: New item ID set to {0}", item.ItemID);
                item.InventoryPosition = GetNewInventoryPosition();
                Console.WriteLine("UpdateWeaponSkin: New inventory position set to {0}", item.InventoryPosition);
                Console.WriteLine("UpdateWeaponSkin: Item copied");
                SOCache.RemoveObject(oldItem);
                Console.WriteLine("UpdateWeaponSkin: Removing item");
                SODestroyed(SteamID, oldItem);
                Console.WriteLine("UpdateWeaponSkin: Item destroyed");
                UpdateLoadoutItem(skinData.Team, skinData.DefinitionIndex, item.ItemID);
            }
            else
            {
                Console.WriteLine("UpdateWeaponSkin: No item ID found");
                item.AccountID = new CSteamID(SteamID).GetAccountID().m_AccountID;
                // not sure if it will work
                Console.WriteLine("UpdateWeaponSkin: Getting new item ID");
                item.ItemID = GetNewItemID();
                item.InventoryPosition = GetNewInventoryPosition();
                Console.WriteLine("UpdateWeaponSkin: New item ID set");
                UpdateLoadoutItem(skinData.Team, skinData.DefinitionIndex, item.ItemID);
            }

            Console.WriteLine("UpdateWeaponSkin: Applying skin data");

            item.Apply(skinData);
            Console.WriteLine("UpdateWeaponSkin: Adding item to SOCache");
            SOCache.AddObject(item);
            Console.WriteLine("UpdateWeaponSkin: Item added to SOCache");
            SOCreated(SteamID, item);
            SOUpdated(SteamID, item);
            Console.WriteLine("UpdateWeaponSkin: Item updated");
        }
    }

    public void UpdateKnifeSkin(KnifeSkinData skinData)
    {
        unsafe
        {
            Console.WriteLine("UpdateKnifeSkin: {0}", skinData.ToString());
            // somehow the MELEE loadout keeps the old itemid
            // by old i means IT CAN BE A ITEM THAT I SOLD TWO WEEKS AGO

            var item = NativeService.CreateCEconItemInstance();
            ref var loadout = ref Loadouts[skinData.Team, loadout_slot_t.LOADOUT_SLOT_MELEE];
            item.AccountID = new CSteamID(SteamID).GetAccountID().m_AccountID;
            // not sure if it will work
            item.ItemID = GetNewItemID();
            item.InventoryPosition = GetNewInventoryPosition();
            loadout.ItemId = item.ItemID;
            loadout.DefinitionIndex = skinData.DefinitionIndex;
            item.Apply(skinData);
            SOCache.AddObject(item);
            SOCreated(SteamID, item);
            SOUpdated(SteamID, item);
        }
    }
}