using System.Diagnostics.CodeAnalysis;

using SwiftlyS2.Shared.Players;

using WeaponSkins.Services;
using WeaponSkins.Shared;

namespace WeaponSkins;

public class WeaponSkinGetterAPI
{

    private DataService DataService { get; init; }
    private ItemPermissionService ItemPermissionService { get; init; }

    public WeaponSkinGetterAPI(DataService dataService, ItemPermissionService itemPermissionService)
    {
        DataService = dataService;
        ItemPermissionService = itemPermissionService;
    }


    public bool TryGetWeaponSkin(ulong steamid,
        Team team,
        ushort definitionIndex,
        [MaybeNullWhen(false)] out WeaponSkinData skin)
    {
        if (DataService.WeaponDataService.TryGetSkin(steamid, team, definitionIndex, out skin))
        {
            return ItemPermissionService.TryBuildWeaponSkinView(skin, out skin);
        }

        return false;
    }

    public bool TryGetWeaponSkins(ulong steamid,
        [MaybeNullWhen(false)] out IEnumerable<WeaponSkinData> result)
    {
        result = null;
        if (DataService.WeaponDataService.TryGetSkins(steamid, out var skins))
        {
            return ItemPermissionService.TryBuildWeaponSkinsView(skins, out result);
        }

        return false;
    }

    public bool TryGetKnifeSkin(ulong steamid,
        Team team,
        [MaybeNullWhen(false)] out KnifeSkinData knife)
    {
        if (DataService.KnifeDataService.TryGetKnife(steamid, team, out knife))
        {
            return ItemPermissionService.TryBuildKnifeSkinView(knife, out knife);
        }

        return false;
    }

    public bool TryGetKnifeSkins(ulong steamid,
        [MaybeNullWhen(false)] out IEnumerable<KnifeSkinData> result)
    {
        result = null;
        if (DataService.KnifeDataService.TryGetKnives(steamid, out var knives2))
        {
            return ItemPermissionService.TryBuildKnifeSkinsView(knives2, out result);
        }

        return false;
    }

    public bool TryGetGloveSkin(ulong steamid,
        Team team,
        [MaybeNullWhen(false)] out GloveData glove)
    {
        if (DataService.GloveDataService.TryGetGlove(steamid, team, out glove))
        {
            return ItemPermissionService.TryBuildGloveView(glove, out glove);
        }

        return false;
    }

    public bool TryGetGloveSkins(ulong steamid,
        [MaybeNullWhen(false)] out IEnumerable<GloveData> result)
    {
        result = null;
        if (DataService.GloveDataService.TryGetGloves(steamid, out var gloves))
        {
            return ItemPermissionService.TryBuildGlovesView(gloves, out result);
        }

        return false;
    }
}

