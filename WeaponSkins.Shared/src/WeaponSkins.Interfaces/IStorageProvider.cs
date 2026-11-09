using SwiftlyS2.Shared.Players;

namespace WeaponSkins.Shared;

/// <summary>
/// A storage provider for weapon skins.
/// </summary>
public interface IStorageProvider
{
    /// <summary>
    /// The name of the storage provider.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Store the skins.
    /// </summary>
    /// <param name="skins">The skins to store.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StoreSkinsAsync(IEnumerable<WeaponSkinData> skins);

    /// <summary>
    /// Get a skin.
    /// </summary>
    /// <param name="steamId">The SteamID64 of the player.</param>
    /// <param name="team">The team of the player.</param>
    /// <param name="definitionIndex">The definition index of the skin.</param>
    /// <returns>The skin data.</returns>
    Task<WeaponSkinData?> GetSkinAsync(ulong steamId,
        Team team,
        ushort definitionIndex);

    /// <summary>
    /// Get all skins.
    /// </summary>
    /// <param name="steamId">The SteamID64 of the player.</param>
    /// <returns>The skins data.</returns>
    Task<IEnumerable<WeaponSkinData>> GetSkinsAsync(ulong steamId);

    /// <summary>
    /// Get all skins.
    /// </summary>
    /// <returns>The skins data.</returns>
    Task<IEnumerable<WeaponSkinData>> GetAllSkinsAsync();

    /// <summary>
    /// Remove a skin.
    /// </summary>
    /// <param name="steamId">The SteamID64 of the player.</param>
    /// <param name="team">The team of the player.</param>
    /// <param name="definitionIndex">The definition index of the skin.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveSkinAsync(ulong steamId,
        Team team,
        ushort definitionIndex);

    /// <summary>
    /// Remove all skins.
    /// </summary>
    /// <param name="steamId">The SteamID64 of the player.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveSkinsAsync(ulong steamId);

    /// <summary>
    /// Store the knives.
    /// </summary>
    /// <param name="knives">The knives to store.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StoreKnifesAsync(IEnumerable<KnifeSkinData> knives);

    /// <summary>
    /// Get a knife.
    /// </summary>
    /// <param name="steamId">The SteamID64 of the player.</param>
    /// <param name="team">The team of the player.</param>
    /// <returns>The knife data.</returns>
    Task<KnifeSkinData?> GetKnifeAsync(ulong steamId,
        Team team);

    /// <summary>
    /// Get all knives.
    /// </summary>
    /// <param name="steamId">The SteamID64 of the player.</param>
    /// <returns>The knives data.</returns>
    Task<IEnumerable<KnifeSkinData>> GetKnifesAsync(ulong steamId);

    /// <summary>
    /// Get all knives.
    /// </summary>
    /// <returns>The knives data.</returns>
    Task<IEnumerable<KnifeSkinData>> GetAllKnifesAsync();

    /// <summary>
    /// Remove a knife.
    /// </summary>
    /// <param name="steamId">The SteamID64 of the player.</param>
    /// <param name="team">The team of the player.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveKnifeAsync(ulong steamId,
        Team team);

    /// <summary>
    /// Remove all knives.
    /// </summary>
    /// <param name="steamId">The SteamID64 of the player.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveKnifesAsync(ulong steamId);

    /// <summary>
    /// Store the gloves.
    /// </summary>
    /// <param name="gloves">The gloves to store.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StoreGlovesAsync(IEnumerable<GloveData> gloves);

    /// <summary>
    /// Get a glove.
    /// </summary>
    /// <param name="steamId">The SteamID64 of the player.</param>
    /// <param name="team">The team of the player.</param>
    /// <returns>The glove data.</returns>
    Task<GloveData?> GetGloveAsync(ulong steamId,
        Team team);

    /// <summary>
    /// Get all gloves.
    /// </summary>
    /// <param name="steamId">The SteamID64 of the player.</param>
    /// <returns>The gloves data.</returns>
    Task<IEnumerable<GloveData>> GetGlovesAsync(ulong steamId);

    /// <summary>
    /// Get all gloves.
    /// </summary>
    /// <returns>The gloves data.</returns>
    Task<IEnumerable<GloveData>> GetAllGlovesAsync();

    /// <summary>
    /// Remove a glove.
    /// </summary>
    /// <param name="steamId">The SteamID64 of the player.</param>
    /// <param name="team">The team of the player.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveGloveAsync(ulong steamId,
        Team team);

    /// <summary>
    /// Remove all gloves.
    /// </summary>
    /// <param name="steamId">The SteamID64 of the player.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveGlovesAsync(ulong steamId);

    /// <summary>
    /// Store the agents.
    /// </summary>
    /// <param name="agents">The agents to store.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StoreAgentsAsync(IEnumerable<(ulong SteamID, Team Team, int AgentIndex)> agents);

    /// <summary>
    /// Get an agent.
    /// </summary>
    /// <param name="steamId">The SteamID64 of the player.</param>
    /// <param name="team">The team of the player.</param>
    /// <returns>The agent index.</returns>
    Task<int?> GetAgentAsync(ulong steamId, Team team);

    /// <summary>
    /// Get all agents.
    /// </summary>
    /// <param name="steamId">The SteamID64 of the player.</param>
    /// <returns>The agents data.</returns>
    Task<IEnumerable<(ulong SteamID, Team Team, int AgentIndex)>> GetAgentsAsync(ulong steamId);

    /// <summary>
    /// Get all agents.
    /// </summary>
    /// <returns>The agents data.</returns>
    Task<IEnumerable<(ulong SteamID, Team Team, int AgentIndex)>> GetAllAgentsAsync();

    /// <summary>
    /// Remove an agent.
    /// </summary>
    /// <param name="steamId">The SteamID64 of the player.</param>
    /// <param name="team">The team of the player.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveAgentAsync(ulong steamId, Team team);

    /// <summary>
    /// Remove all agents.
    /// </summary>
    /// <param name="steamId">The SteamID64 of the player.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveAgentsAsync(ulong steamId);

    /// <summary>
    /// Store the music kits.
    /// </summary>
    /// <param name="musicKits">The music kits to store.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StoreMusicKitsAsync(IEnumerable<(ulong SteamID, int MusicKitIndex)> musicKits);

    /// <summary>
    /// Get a music kit.
    /// </summary>
    /// <param name="steamId">The SteamID64 of the player.</param>
    /// <returns>The music kit index.</returns>
    Task<int?> GetMusicKitAsync(ulong steamId);

    /// <summary>
    /// Get all music kits.
    /// </summary>
    /// <param name="steamId">The SteamID64 of the player.</param>
    /// <returns>The music kits data.</returns>
    Task<IEnumerable<(ulong SteamID, int MusicKitIndex)>> GetMusicKitsAsync(ulong steamId);

    /// <summary>
    /// Get all music kits.
    /// </summary>
    /// <returns>The music kits data.</returns>
    Task<IEnumerable<(ulong SteamID, int MusicKitIndex)>> GetAllMusicKitsAsync();

    /// <summary>
    /// Remove a music kit.
    /// </summary>
    /// <param name="steamId">The SteamID64 of the player.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveMusicKitAsync(ulong steamId);

    // The graffiti members have default bodies so storage plugins built before them keep loading; those just don't save graffiti.

    /// <summary>
    /// Store the graffiti.
    /// </summary>
    /// <param name="graffiti">The graffiti to store, one per player. TintIndex is 0 for a graffiti that takes no tint.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StoreGraffitiAsync(IEnumerable<(ulong SteamID, int GraffitiIndex, int TintIndex)> graffiti) => Task.CompletedTask;

    /// <summary>
    /// Get a player's graffiti.
    /// </summary>
    /// <param name="steamId">The SteamID64 of the player.</param>
    /// <returns>The graffiti data.</returns>
    Task<IEnumerable<(ulong SteamID, int GraffitiIndex, int TintIndex)>> GetGraffitiAsync(ulong steamId) =>
        Task.FromResult(Enumerable.Empty<(ulong SteamID, int GraffitiIndex, int TintIndex)>());

    /// <summary>
    /// Get all graffiti.
    /// </summary>
    /// <returns>The graffiti data.</returns>
    Task<IEnumerable<(ulong SteamID, int GraffitiIndex, int TintIndex)>> GetAllGraffitiAsync() =>
        Task.FromResult(Enumerable.Empty<(ulong SteamID, int GraffitiIndex, int TintIndex)>());

    /// <summary>
    /// Remove a player's graffiti.
    /// </summary>
    /// <param name="steamId">The SteamID64 of the player.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveGraffitiAsync(ulong steamId) => Task.CompletedTask;

    // The agent patch members have default bodies too; storage plugins built before them don't save patches.

    /// <summary>
    /// Store agent patches.
    /// </summary>
    /// <param name="patches">The patches to store, one per player, team and slot. Slot is 0 to 4.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StoreAgentPatchesAsync(IEnumerable<(ulong SteamID, Team Team, int Slot, int PatchIndex)> patches) => Task.CompletedTask;

    /// <summary>
    /// Get a player's agent patches.
    /// </summary>
    /// <param name="steamId">The SteamID64 of the player.</param>
    /// <returns>The patches data.</returns>
    Task<IEnumerable<(ulong SteamID, Team Team, int Slot, int PatchIndex)>> GetAgentPatchesAsync(ulong steamId) =>
        Task.FromResult(Enumerable.Empty<(ulong SteamID, Team Team, int Slot, int PatchIndex)>());

    /// <summary>
    /// Get all agent patches.
    /// </summary>
    /// <returns>The patches data.</returns>
    Task<IEnumerable<(ulong SteamID, Team Team, int Slot, int PatchIndex)>> GetAllAgentPatchesAsync() =>
        Task.FromResult(Enumerable.Empty<(ulong SteamID, Team Team, int Slot, int PatchIndex)>());

    /// <summary>
    /// Remove the patch in one slot.
    /// </summary>
    /// <param name="steamId">The SteamID64 of the player.</param>
    /// <param name="team">The team of the player.</param>
    /// <param name="slot">The slot, 0 to 4.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveAgentPatchAsync(ulong steamId, Team team, int slot) => Task.CompletedTask;

    /// <summary>
    /// Remove a player's agent patches on a team.
    /// </summary>
    /// <param name="steamId">The SteamID64 of the player.</param>
    /// <param name="team">The team of the player.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveAgentPatchesAsync(ulong steamId, Team team) => Task.CompletedTask;

    // The pin members have default bodies too; storage plugins built before them don't save pins.

    /// <summary>
    /// Store the pins.
    /// </summary>
    /// <param name="pins">The pins to store, one per player.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StorePinsAsync(IEnumerable<(ulong SteamID, int PinIndex)> pins) => Task.CompletedTask;

    /// <summary>
    /// Get a player's pin.
    /// </summary>
    /// <param name="steamId">The SteamID64 of the player.</param>
    /// <returns>The pins data.</returns>
    Task<IEnumerable<(ulong SteamID, int PinIndex)>> GetPinsAsync(ulong steamId) =>
        Task.FromResult(Enumerable.Empty<(ulong SteamID, int PinIndex)>());

    /// <summary>
    /// Get all pins.
    /// </summary>
    /// <returns>The pins data.</returns>
    Task<IEnumerable<(ulong SteamID, int PinIndex)>> GetAllPinsAsync() =>
        Task.FromResult(Enumerable.Empty<(ulong SteamID, int PinIndex)>());

    /// <summary>
    /// Remove a player's pin.
    /// </summary>
    /// <param name="steamId">The SteamID64 of the player.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemovePinAsync(ulong steamId) => Task.CompletedTask;
}
