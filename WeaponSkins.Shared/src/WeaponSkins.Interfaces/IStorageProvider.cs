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
}