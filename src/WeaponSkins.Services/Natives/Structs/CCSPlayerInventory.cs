using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;
using SwiftlyS2.Shared.SteamAPI;

using WeaponSkins.Shared;

namespace WeaponSkins;

public class CCSPlayerInventory : INativeHandle
{
    [SwiftlyInject] private static ISwiftlyCore Core { get; set; } = null!;

    public nint Address { get; set; }
    public bool IsValid => Address != 0 && SOCache.IsValid;

    public ulong SteamID => SOCache.Owner.SteamID;

    private NativeService NativeService => StaticNativeService.Service;
    private CCSPlayerInventory_Loadouts _defaultLoadouts { get; init; }

    public CCSPlayerInventory(nint address)
    {
        Address = address;
        _defaultLoadouts = Loadouts;
    }

    public CGCClientSharedObjectCache SOCache =>
        new CGCClientSharedObjectCache(Address.Read<nint>(NativeService.CCSPlayerInventory_m_pSOCacheOffset));

    public void SODestroyed(ulong steamid,
        CEconItem item)
    {
        var soid = new SOID_t(steamid);
        NativeService.SODestroyed(this, soid, item);
    }

    public void SOCreated(ulong steamid,
        CEconItem item)
    {
        var soid = new SOID_t(steamid);
        NativeService.SOCreated(this, soid, item);
    }

    public void SOUpdated(ulong steamid,
        CEconItem item)
    {
        var soid = new SOID_t(steamid);
        NativeService.SOUpdated(this, soid, item);
    }

    public bool TryGetEconItemByItemID(ulong itemid,
        [MaybeNullWhen(false)] out CEconItem item)
    {
        var ptr = NativeService.GetEconItemByItemID.Call(Address, itemid);
        if (ptr == 0)
        {
            item = null;
            return false;
        }

        item = new CEconItem(ptr);
        return true;
    }

    public ref CUtlVector<PointerTo<CEconItemView>> Items =>
        ref Address.AsRef<CUtlVector<PointerTo<CEconItemView>>>(NativeService.CCSPlayerInventory_m_ItemsOffset);

    public ref CCSPlayerInventory_Loadouts Loadouts =>
        ref Address.AsRef<CCSPlayerInventory_Loadouts>(NativeService.CCSPlayerInventory_LoadoutsOffset);

    private bool TryGetLoadoutItem(Team team,
        ushort definitionIndex,
        out (Team team, loadout_slot_t slot) indices)
    {
        indices = default;
        for (var slot = 0; slot < (int)loadout_slot_t.LOADOUT_SLOT_COUNT; slot++)
        {
            if (Loadouts[team, slot].DefinitionIndex == definitionIndex)
            {
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

    public CEconItemView? GetItemInLoadout(Team team,
        loadout_slot_t slot)
    {
        var ptr = NativeService.CPlayerInventory_GetItemInLoadout.CallOriginal(Address, (int)team, (int)slot);
        return ptr != 0 ? Helper.AsSchema<CEconItemView>(ptr) : null;
    }

    private ulong GetHighestItemID()
    {
        return Items.Select(item => item.Value.ItemID).Where(IsValidItemID).DefaultIfEmpty(0UL).Max();
    }

    private uint GetHighestInventoryPosition()
    {
        return Items.Select(item => item.Value.InventoryPosition).DefaultIfEmpty(0U).Max();
    }

    private ulong GetNewItemID()
    {
        return GetHighestItemID() + 1;
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

    public bool TryGetItemID(Team team,
        ushort definitionIndex,
        out ulong itemID)
    {
        itemID = 0;
        if (TryGetLoadoutItem(team, definitionIndex, out var indices))
        {
            ref var loadout = ref Loadouts[indices.team, indices.slot];
            itemID = loadout.ItemId;
            if (!IsValidItemID(itemID))
            {
                itemID = 0;
                return false;
            }

            return true;
        }

        return false;
    }

    public void UpdateLoadoutItem(Team team,
        ushort definitionIndex,
        ulong itemID)
    {
        if (TryGetLoadoutItem(team, definitionIndex, out var indices))
        {
            ref var loadout = ref Loadouts[indices.team, indices.slot];
            loadout.ItemId = itemID;
            loadout.DefinitionIndex = definitionIndex;
        }
        // do nothing
    }

    public void UpdateWeaponSkin(WeaponSkinData data)
    {
        var skinData = data.DeepClone();
        StickerFixService.FixSticker(skinData);
        Core.Scheduler.NextWorldUpdate(() =>
        {
            var item = NativeService.CreateCEconItemInstance();
            if (TryGetItemID(skinData.Team, skinData.DefinitionIndex, out var itemID) &&
                TryGetEconItemByItemID(itemID, out var oldItem))
            {
                item.AccountID = new CSteamID(SteamID).GetAccountID().m_AccountID;
                item.ItemID = GetNewItemID();
                item.InventoryPosition = GetNewInventoryPosition();
                if (itemID != GetDefaultWeaponSkinItemID(skinData.Team, skinData.DefinitionIndex))
                {
                    SOCache.RemoveObject(oldItem);
                }

                SODestroyed(SteamID, oldItem);

                UpdateLoadoutItem(skinData.Team, skinData.DefinitionIndex, item.ItemID);
            }
            else
            {
                item.AccountID = new CSteamID(SteamID).GetAccountID().m_AccountID;
                item.ItemID = GetNewItemID();
                item.InventoryPosition = GetNewInventoryPosition();
                UpdateLoadoutItem(skinData.Team, skinData.DefinitionIndex, item.ItemID);
            }

            item.Apply(skinData);
            SOCache.AddObject(item);
            SOCreated(SteamID, item);
            SOUpdated(SteamID, item);
        });
    }

    public void UpdateKnifeSkin(KnifeSkinData skinData)
    {
        Core.Scheduler.NextWorldUpdate(() =>
        {
            var item = NativeService.CreateCEconItemInstance();
            ref var loadout = ref Loadouts[skinData.Team, loadout_slot_t.LOADOUT_SLOT_MELEE];

            if (IsValidItemID(loadout.ItemId) && TryGetEconItemByItemID(loadout.ItemId, out var oldItem))
            {
                if (loadout.ItemId != GetDefaultKnifeSkinItemID(skinData.Team))
                {
                    SOCache.RemoveObject(oldItem);
                }

                SODestroyed(SteamID, oldItem);
            }

            item.AccountID = new CSteamID(SteamID).GetAccountID().m_AccountID;
            item.ItemID = GetNewItemID();
            item.InventoryPosition = GetNewInventoryPosition();
            loadout.ItemId = item.ItemID;
            loadout.DefinitionIndex = skinData.DefinitionIndex;
            item.Apply(skinData);
            SOCache.AddObject(item);
            SOCreated(SteamID, item);
            SOUpdated(SteamID, item);
        });
    }

    public void UpdateGloveSkin(GloveData skinData)
    {
        Core.Scheduler.NextWorldUpdate(() =>
        {
            var item = NativeService.CreateCEconItemInstance();
            ref var loadout = ref Loadouts[skinData.Team, loadout_slot_t.LOADOUT_SLOT_CLOTHING_HANDS];

            if (IsValidItemID(loadout.ItemId) && TryGetEconItemByItemID(loadout.ItemId, out var oldItem))
            {
                if (loadout.ItemId != GetDefaultGloveSkinItemID(skinData.Team))
                {
                    SOCache.RemoveObject(oldItem);
                }

                SODestroyed(SteamID, oldItem);
            }

            item.AccountID = new CSteamID(SteamID).GetAccountID().m_AccountID;
            item.ItemID = GetNewItemID();
            item.InventoryPosition = GetNewInventoryPosition();
            loadout.ItemId = item.ItemID;
            loadout.DefinitionIndex = skinData.DefinitionIndex;
            item.Apply(skinData);
            SOCache.AddObject(item);
            SOCreated(SteamID, item);
            SOUpdated(SteamID, item);
        });
    }

    public ulong GetDefaultWeaponSkinItemID(Team team,
        ushort definitionIndex)
    {
        for (var slot = 0; slot < (int)loadout_slot_t.LOADOUT_SLOT_COUNT; slot++)
        {
            if (Loadouts[team, slot].DefinitionIndex == definitionIndex)
            {
                return Loadouts[team, slot].ItemId;
            }
        }

        return 0;
    }

    public ulong GetDefaultKnifeSkinItemID(Team team)
    {
        return _defaultLoadouts[team, loadout_slot_t.LOADOUT_SLOT_MELEE].ItemId;
    }

    public ulong GetDefaultGloveSkinItemID(Team team)
    {
        return _defaultLoadouts[team, loadout_slot_t.LOADOUT_SLOT_CLOTHING_HANDS].ItemId;
    }

    public void ResetWeaponSkin(Team team,
        ushort definitionIndex)
    {
        Core.Scheduler.NextWorldUpdate(() =>
        {
            if (TryGetLoadoutItem(team, definitionIndex, out var indices))
            {
                ref var loadout = ref Loadouts[indices.team, indices.slot];
                loadout = _defaultLoadouts[indices.team, indices.slot];
                if (TryGetEconItemByItemID(loadout.ItemId, out var item))
                {
                    SOCreated(SteamID, item);
                    SOUpdated(SteamID, item);
                }
            }
        });
    }

    public void ResetKnifeSkin(Team team)
    {
        Core.Scheduler.NextWorldUpdate(() =>
        {
            ref var loadout = ref Loadouts[team, loadout_slot_t.LOADOUT_SLOT_MELEE];
            loadout = _defaultLoadouts[team, loadout_slot_t.LOADOUT_SLOT_MELEE];
            if (TryGetEconItemByItemID(loadout.ItemId, out var item))
            {
                SOCreated(SteamID, item);
                SOUpdated(SteamID, item);
            }
        });
    }

    public void ResetGloveSkin(Team team)
    {
        Core.Scheduler.NextWorldUpdate(() =>
        {
            ref var loadout = ref Loadouts[team, loadout_slot_t.LOADOUT_SLOT_CLOTHING_HANDS];
            loadout = _defaultLoadouts[team, loadout_slot_t.LOADOUT_SLOT_CLOTHING_HANDS];
            if (TryGetEconItemByItemID(loadout.ItemId, out var item))
            {
                SOCreated(SteamID, item);
                SOUpdated(SteamID, item);
            }
        });
    }
}