using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins;

public interface IInventoryUpdateService
{
    void UpdateWeaponSkins(IEnumerable<WeaponSkinData> skins);
    void UpdateKnifeSkins(IEnumerable<KnifeSkinData> knives);
    void UpdateGloveSkins(IEnumerable<GloveData> gloves);
    void ResetWeaponSkin(ulong steamid, Team team, ushort definitionIndex);
    void ResetKnifeSkin(ulong steamid, Team team);
    void ResetGloveSkin(ulong steamid, Team team);
}

