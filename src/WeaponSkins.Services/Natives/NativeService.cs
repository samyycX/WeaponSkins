using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Microsoft.Extensions.Logging;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Memory;
using SwiftlyS2.Shared.NetMessages;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.ProtobufDefinitions;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace WeaponSkins;

public class NativeService
{
    private ISwiftlyCore Core { get; init; }
    private ILogger<NativeService> Logger { get; init; }

    public unsafe delegate nint CreateCEconItemDelegate();

    public unsafe delegate byte AddObjectDelegate(nint pSOCache,
        nint pSharedObject);

    public unsafe delegate byte RemoveObjectDelegate(nint pSOCache,
        nint pSharedObject);

    public unsafe delegate void SOCreatedDelegate(nint pInventory,
        SOID_t* pSOID,
        nint pSharedObj,
        int eventType);

    public unsafe delegate void SOUpdatedDelegate(nint pInventory,
        SOID_t* pSOID,
        nint pSharedObj,
        int eventType);

    public unsafe delegate void SODestroyedDelegate(nint pInventory,
        SOID_t* pSOID,
        nint pSharedObj,
        int eventType);

    public unsafe delegate nint SOCacheSubscribedDelegate(nint pInventory,
        SOID_t* pSOID,
        nint pSOCache);

    public delegate nint GetEconItemByItemIDDelegate(nint pInventory,
        ulong itemid);

    public delegate nint CAttribute_String_NewDelegate(nint pAttributeString,
        nint pArena);

    public required IUnmanagedFunction<CreateCEconItemDelegate> CreateCEconItem { get; init; }
    public required IUnmanagedFunction<AddObjectDelegate> SOCache_AddObject { get; init; }
    public required IUnmanagedFunction<RemoveObjectDelegate> SOCache_RemoveObject { get; init; }
    public required IUnmanagedFunction<SOCreatedDelegate> CPlayerInventory_SOCreated { get; init; }
    public required IUnmanagedFunction<SOUpdatedDelegate> CPlayerInventory_SOUpdated { get; init; }
    public required IUnmanagedFunction<SODestroyedDelegate> CPlayerInventory_SODestroyed { get; init; }
    public required IUnmanagedFunction<SOCacheSubscribedDelegate> CPlayerInventory_SOCacheSubscribed { get; init; }
    public required IUnmanagedFunction<GetEconItemByItemIDDelegate> GetEconItemByItemID { get; init; }
    public required IUnmanagedFunction<CAttribute_String_NewDelegate> CAttribute_String_New { get; init; }

    public required int CCSPlayerInventory_LoadoutsOffset { get; init; }
    public required int CCSInventoryManager_m_DefaultLoadoutsOffset { get; init; }
    public required int CGCClientSharedObjectCache_m_OwnerOffset { get; init; }
    public required int CCSPlayerInventory_m_pSOCacheOffset { get; init; }
    public required int CCSPlayerInventory_m_ItemsOffset { get; init; }
    public required int CCSPlayerController_InventoryServices_m_pInventoryOffset { get; init; }
    public required CCSInventoryManager CCSInventoryManager { get; init; }


    public event Action<CCSPlayerInventory, SOID_t>? OnSOCacheSubscribed;


    public NativeService(ISwiftlyCore core,
        ILogger<NativeService> logger)
    {
        Core = core;
        Logger = logger;

        var soCacheVtable = Core.Memory.GetVTableAddress("server", "GCSDK::CGCClientSharedObjectCache")!.Value;
        SOCache_AddObject = Core.Memory.GetUnmanagedFunctionByVTable<AddObjectDelegate>(
            soCacheVtable,
            Core.GameData.GetOffset("GCSDK::CGCClientSharedObjectCache::AddObject")
        );

        SOCache_RemoveObject = Core.Memory.GetUnmanagedFunctionByVTable<RemoveObjectDelegate>(
            soCacheVtable,
            Core.GameData.GetOffset("GCSDK::CGCClientSharedObjectCache::RemoveObject")
        );

        var playerInventoryVtable = Core.Memory.GetVTableAddress("server", "CCSPlayerInventory")!.Value;

        CPlayerInventory_SOCreated = Core.Memory.GetUnmanagedFunctionByVTable<SOCreatedDelegate>(
            playerInventoryVtable,
            Core.GameData.GetOffset("CPlayerInventory::SOCreated")
        );

        CPlayerInventory_SOUpdated = Core.Memory.GetUnmanagedFunctionByVTable<SOUpdatedDelegate>(
            playerInventoryVtable,
            Core.GameData.GetOffset("CPlayerInventory::SOUpdated")
        );

        CPlayerInventory_SODestroyed = Core.Memory.GetUnmanagedFunctionByVTable<SODestroyedDelegate>(
            playerInventoryVtable,
            Core.GameData.GetOffset("CPlayerInventory::SODestroyed")
        );

        CPlayerInventory_SOCacheSubscribed = Core.Memory.GetUnmanagedFunctionByVTable<SOCacheSubscribedDelegate>(
            playerInventoryVtable,
            Core.GameData.GetOffset("CPlayerInventory::SOCacheSubscribed")
        );

        var stringVtable = Core.Memory.GetVTableAddress("server", "CAttribute_String")!.Value;
        CAttribute_String_New = Core.Memory.GetUnmanagedFunctionByVTable<CAttribute_String_NewDelegate>(
            stringVtable,
            Core.GameData.GetOffset("CAttribute_String::New")
        );

        CreateCEconItem = Core.Memory.GetUnmanagedFunctionByAddress<CreateCEconItemDelegate>(
            Core.GameData.GetSignature("CreateCEconItem")
        );

        GetEconItemByItemID = Core.Memory.GetUnmanagedFunctionByAddress<GetEconItemByItemIDDelegate>(
            Core.GameData.GetSignature("GetEconItemByItemID")
        );

        CCSPlayerInventory_LoadoutsOffset = Core.GameData.GetOffset("CCSPlayerInventory::m_Loadouts");
        CCSInventoryManager_m_DefaultLoadoutsOffset = Core.GameData.GetOffset("CCSInventoryManager::m_DefaultLoadouts");
        CCSPlayerInventory_m_ItemsOffset = Core.GameData.GetOffset("CCSPlayerInventory::m_Items");
        CCSPlayerInventory_m_pSOCacheOffset = Core.GameData.GetOffset("CCSPlayerInventory::m_pSOCache");
        CCSPlayerController_InventoryServices_m_pInventoryOffset = Core.GameData.GetOffset("CCSPlayerController_InventoryServices::m_pInventory");
        CGCClientSharedObjectCache_m_OwnerOffset =
            Core.GameData.GetOffset("GCSDK::CGCClientSharedObjectCache::m_Owner");
        var xrefCCSInventoryManager = Core.GameData.GetSignature("CCSInventoryManager_xref");
        CCSInventoryManager = new CCSInventoryManager(Core.Memory.ResolveXrefAddress(xrefCCSInventoryManager)!, this);
        CPlayerInventory_SOCacheSubscribed.AddHook(next =>
        {
            unsafe
            {
                return (pInventory,
                    pSOID,
                    pSOCache) =>
                {
                    try
                    {
                        var ret = next()(pInventory, pSOID, pSOCache);
                        var inventory = new CCSPlayerInventory(pInventory, this);
                        var a = inventory.Loadouts[Team.CT, loadout_slot_t.LOADOUT_SLOT_C4];
                        OnSOCacheSubscribed?.Invoke(inventory, *pSOID);
                        return ret;
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, "Error in SOCacheSubscribed");
                        return 0;
                    }
                };
            }
        });

        StaticNativeService.Service = this;
    }

    public CEconItem CreateCEconItemInstance()
    {
        return new CEconItem(CreateCEconItem.Call());
    }

    public CAttribute_String CreateAttributeString()
    {
        // TODO: Use helper 

        var ret = CAttribute_String_New.Call(0, 0);
        return Helper.AsProtobuf<CAttribute_String>(ret, false);
    }
}