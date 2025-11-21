using SwiftlyS2.Shared.Natives;

namespace WeaponSkins;

public struct CGCClientSharedObjectCache(nint address, NativeService nativeService) : INativeHandle
{
    public nint Address { get; set; } = address;
    public bool IsValid => Address != 0;

    private NativeService NativeService { get; } = nativeService;

    public void AddObject(CEconItem item)
    {
        NativeService.SOCache_AddObject.Call(Address, item.Address);
    }

    public void RemoveObject(CEconItem item)
    {
        NativeService.SOCache_RemoveObject.Call(Address, item.Address);
    }

    public SOID_t Owner => Address.AsRef<SOID_t>(NativeService.CGCClientSharedObjectCache_m_OwnerOffset);
}