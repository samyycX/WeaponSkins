using SwiftlyS2.Shared.Players;

namespace WeaponSkins.Shared;

public interface IWeaponSkinAPI
{
    void SetWeaponSkins(IEnumerable<WeaponSkinData> skins);

    void SetWeaponSkinsWithoutStattrak(IEnumerable<WeaponSkinData> skins);

    void SetKnifeSkins(IEnumerable<KnifeSkinData> knives);

    void SetGloveSkins(IEnumerable<GloveData> gloves);

    void SetWeaponPaintsWithoutStattrakPermanently(IEnumerable<WeaponSkinData> skins);

    void SetWeaponSkinsPermanently(IEnumerable<WeaponSkinData> skins);

    void SetKnifeSkinsPermanently(IEnumerable<KnifeSkinData> knives);

    void SetGloveSkinsPermanently(IEnumerable<GloveData> gloves);

    void UpdateWeaponSkin(ulong steamid,
        Team team,
        ushort definitionIndex,
        Action<WeaponSkinData> action);

    void UpdateKnifeSkin(ulong steamid,
        Team team,
        Action<KnifeSkinData> action);

    void UpdateGloveSkin(ulong steamid,
        Team team,
        Action<GloveData> action);

    void UpdateWeaponSkinPermanently(ulong steamid,
        Team team,
        ushort definitionIndex,
        Action<WeaponSkinData> action);

    void UpdateKnifeSkinPermanently(ulong steamid,
        Team team,
        Action<KnifeSkinData> action);

    void UpdateGloveSkinPermanently(ulong steamid,
        Team team,
        Action<GloveData> action);
}