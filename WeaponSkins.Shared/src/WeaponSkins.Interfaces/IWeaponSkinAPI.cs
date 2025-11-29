using System.Diagnostics.CodeAnalysis;

using SwiftlyS2.Shared.Players;

namespace WeaponSkins.Shared;

public interface IWeaponSkinAPI
{
    void SetWeaponSkins(IEnumerable<WeaponSkinData> skins, bool permanent = false);

    void SetKnifeSkins(IEnumerable<KnifeSkinData> knives, bool permanent = false);

    void SetGloveSkins(IEnumerable<GloveData> gloves, bool permanent = false);

    void UpdateWeaponSkin(ulong steamid,
        Team team,
        ushort definitionIndex,
        Action<WeaponSkinData> action,
        bool permanent = false);

    void UpdateKnifeSkin(ulong steamid,
        Team team,
        Action<KnifeSkinData> action,
        bool permanent = false);

    void UpdateGloveSkin(ulong steamid,
        Team team,
        Action<GloveData> action,
        bool permanent = false);
    
    bool TryGetWeaponSkin(ulong steamid,
        Team team,
        ushort definitionIndex,
        [MaybeNullWhen(false)] out WeaponSkinData skin);

    bool TryGetKnifeSkin(ulong steamid,
        Team team,
        [MaybeNullWhen(false)] out KnifeSkinData knife);

    bool TryGetGloveSkin(ulong steamid,
        Team team,
        [MaybeNullWhen(false)] out GloveData glove);
}