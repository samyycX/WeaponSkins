using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using SwiftlyS2.Shared.Players;

namespace WeaponSkins.Shared;

public interface IWeaponSkinAPI
{
    /// <summary>
    /// All item definitions indexed by name.
    /// </summary>
    IReadOnlyDictionary<string, ItemDefinition> Items { get; }

    /// <summary>
    /// Paintkits grouped by weapon name.
    /// </summary>
    IReadOnlyDictionary<string, List<PaintkitDefinition>> WeaponToPaintkits { get; }

    /// <summary>
    /// All sticker collections indexed by name.
    /// </summary>
    IReadOnlyDictionary<string, StickerCollectionDefinition> StickerCollections { get; }

    /// <summary>
    /// All keychain definitions indexed by name.
    /// </summary>
    IReadOnlyDictionary<string, KeychainDefinition> Keychains { get; }

    /// <summary>
    /// All agent definitions indexed by name.
    /// </summary>
    IReadOnlyDictionary<string, AgentDefinition> Agents { get; }

    /// <summary>
    /// All music kit definitions indexed by name.
    /// </summary>
    IReadOnlyDictionary<string, MusicKitDefinition> MusicKits { get; }

    /// <summary>
    /// All graffiti definitions indexed by name.
    /// </summary>
    IReadOnlyDictionary<string, GraffitiDefinition> Graffiti { get; }

    /// <summary>
    /// The colours a graffiti whose Tintable is true can be sprayed in, indexed by name. Graffiti art is untinted: the game washes the image in the tint's HexColor (Panorama wash-color).
    /// </summary>
    IReadOnlyDictionary<string, GraffitiTintDefinition> GraffitiTints { get; }

    /// <summary>
    /// The game's inventory image for an item: a path under panorama/images/econ/, without extension, or null when the game ships none.
    /// </summary>
    /// <param name="kind">One of agent, music, sticker, capsule, keychain, keychaincollection, graffiti, graffititournament.</param>
    /// <param name="name">The definition's Name; for keychaincollection, the collection's name; for graffititournament, the tournament's event id.</param>
    string? GetImageKey(string kind,
        string name);

    /// <summary>
    /// The music kit the player has set, if any.
    /// </summary>
    bool TryGetMusicKit(ulong steamid,
        out int musicKitIndex);

    /// <summary>
    /// The graffiti the player has chosen, if any and if they may use graffiti. The tint is 0 for a graffiti that takes none.
    /// </summary>
    bool TryGetGraffiti(ulong steamid,
        out int graffitiIndex,
        out int tintIndex);

    void SetWeaponSkins(IEnumerable<WeaponSkinData> skins,
        bool permanent = false);

    void SetKnifeSkins(IEnumerable<KnifeSkinData> knives,
        bool permanent = false);

    void SetGloveSkins(IEnumerable<GloveData> gloves,
        bool permanent = false);

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

    bool TryGetWeaponSkins(ulong steamid,
        [MaybeNullWhen(false)] out IEnumerable<WeaponSkinData> result);

    bool TryGetKnifeSkin(ulong steamid,
    Team team,
    [MaybeNullWhen(false)] out KnifeSkinData knife);

    bool TryGetKnifeSkins(ulong steamid,
        [MaybeNullWhen(false)] out IEnumerable<KnifeSkinData> result);

    

    bool TryGetGloveSkin(ulong steamid,
        Team team,
        [MaybeNullWhen(false)] out GloveData glove);

    bool TryGetGloveSkins(ulong steamid,
        [MaybeNullWhen(false)] out IEnumerable<GloveData> result);

    

    void ResetWeaponSkin(ulong steamid,
        Team team,
        ushort definitionIndex,
        bool permanent = false);

    void ResetKnifeSkin(ulong steamid,
        Team team,
        bool permanent = false);

    void ResetGloveSkin(ulong steamid,
        Team team,
        bool permanent = false);

    bool TryGetAgentSkin(ulong steamid,
        Team team,
        out int agentIndex);

    bool TryGetAgentSkins(ulong steamid,
        [MaybeNullWhen(false)] out IEnumerable<(Team Team, int AgentIndex)> result);

    void UpdateAgentSkin(ulong steamid,
        Team team,
        int agentIndex,
        bool permanent = false);

    void ResetAgentSkin(ulong steamid,
        Team team,
        bool permanent = false);

    void SetExternalStorageProvider(IStorageProvider provider);
}
