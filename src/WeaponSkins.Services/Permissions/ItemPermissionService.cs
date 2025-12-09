using Microsoft.Extensions.Options;

using SwiftlyS2.Shared;

using WeaponSkins.Configuration;
using WeaponSkins.Shared;

namespace WeaponSkins.Services;

public class ItemPermissionService
{
    private readonly ISwiftlyCore Core;
    private ItemPermissionConfig Config;

    public ItemPermissionService(ISwiftlyCore core,
        IOptionsMonitor<MainConfigModel> options)
    {
        Core = core;
        Config = CloneConfig(options.CurrentValue.ItemPermissions);
        options.OnChange(model => { Config = CloneConfig(model.ItemPermissions); });
    }

    private static ItemPermissionConfig CloneConfig(ItemPermissionConfig? source)
    {
        if (source == null)
        {
            return new ItemPermissionConfig();
        }

        return new ItemPermissionConfig
        {
            WeaponSkins = source.WeaponSkins,
            KnifeSkins = source.KnifeSkins,
            GloveSkins = source.GloveSkins,
            Stickers = source.Stickers,
            Keychains = source.Keychains
        };
    }

    public WeaponSkinData BuildWeaponSkinView(WeaponSkinData data)
    {
        if (!CanUseWeaponSkins(data.SteamID))
        {
            return new WeaponSkinData
            {
                SteamID = data.SteamID,
                Team = data.Team,
                DefinitionIndex = data.DefinitionIndex
            };
        }

        var result = data.DeepClone();
        if (!CanUseStickers(data.SteamID))
        {
            for (var slot = 0; slot < 6; slot++)
            {
                result.SetSticker(slot, null);
            }
        }

        if (!CanUseKeychains(data.SteamID))
        {
            result.Keychain0 = null;
        }

        return result;
    }

    public KnifeSkinData BuildKnifeSkinView(KnifeSkinData data)
    {
        if (!CanUseKnifeSkins(data.SteamID))
        {
            return new KnifeSkinData
            {
                SteamID = data.SteamID,
                Team = data.Team,
                DefinitionIndex = 0
            };
        }

        return data.DeepClone();
    }

    public GloveData BuildGloveView(GloveData data)
    {
        if (!CanUseGloveSkins(data.SteamID))
        {
            return new GloveData
            {
                SteamID = data.SteamID,
                Team = data.Team,
                DefinitionIndex = 0
            };
        }

        return data.DeepClone();
    }

    public WeaponSkinData ApplyWeaponUpdateRules(WeaponSkinData current,
        WeaponSkinData updated)
    {
        if (!CanUseWeaponSkins(current.SteamID))
        {
            return current.DeepClone();
        }

        var result = updated.DeepClone();
        if (!CanUseStickers(current.SteamID))
        {
            for (var slot = 0; slot < 6; slot++)
            {
                result.SetSticker(slot, current.GetSticker(slot)?.DeepClone());
            }
        }

        if (!CanUseKeychains(current.SteamID))
        {
            result.Keychain0 = current.Keychain0?.DeepClone();
        }

        return result;
    }

    public bool CanUseWeaponSkins(ulong steamId) => HasPermission(steamId, Config.WeaponSkins);

    public bool CanUseKnifeSkins(ulong steamId) => HasPermission(steamId, Config.KnifeSkins);

    public bool CanUseGloveSkins(ulong steamId) => HasPermission(steamId, Config.GloveSkins);

    public bool CanUseStickers(ulong steamId) => HasPermission(steamId, Config.Stickers);

    public bool CanUseKeychains(ulong steamId) => HasPermission(steamId, Config.Keychains);

    private bool HasPermission(ulong steamId,
        string? permission)
    {
        if (string.IsNullOrWhiteSpace(permission)) return true;
        return Core.Permission != null && Core.Permission.HasPermission(steamId, permission);
    }
}
