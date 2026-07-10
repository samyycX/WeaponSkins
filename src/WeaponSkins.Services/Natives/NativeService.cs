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
    private bool IsWindows => OperatingSystem.IsWindows();

    public event Action<CCSPlayer_ItemServices, CBasePlayerWeapon>? OnGiveNamedItemPost;

    public unsafe delegate nint GiveNamedItemDelegate(nint pItemServices,
        nint pItemName,
        nint subtype,
        nint pEconItemView,
        nint a5,
        nint a6);

    public IUnmanagedFunction<GiveNamedItemDelegate>? GiveNamedItem { get; init; }

    public NativeService(ISwiftlyCore core,
        ILogger<NativeService> logger)
    {
        Core = core;
        Logger = logger;

        try
        {
            GiveNamedItem = Core.Memory.GetUnmanagedFunctionByAddress<GiveNamedItemDelegate>(
                Core.GameData.GetSignature("GiveNamedItem")
            );
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error getting GiveNamedItem");
            GiveNamedItem = null;
        }

        if (GiveNamedItem != null)
        {
            GiveNamedItem.AddHook(next =>
            {
                return (pItemServices,
                    pItemName,
                    subtype,
                    pEconItemView,
                    a5,
                    a6) =>
                {
                    nint ret = 0;
                    try
                    {
                        ret = next()(pItemServices, pItemName, subtype, pEconItemView, a5, a6);
                        if (ret != 0)
                        {
                            var services = Helper.AsSchema<CCSPlayer_ItemServices>(pItemServices);
                            var weapon = Helper.AsSchema<CBasePlayerWeapon>(ret);
                            if (services.IsValid && weapon.IsValid)
                            {
                                OnGiveNamedItemPost?.Invoke(services, weapon);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, "Error in GiveNamedItemPost");
                    }

                    return ret;
                };
            });
        }
    }




}