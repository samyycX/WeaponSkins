using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins.Database;

public class DatabaseSynchronizeService
{
    private readonly SemaphoreSlim synchronization = new(1, 1);
    private DataService DataService { get; init; }
    private ISwiftlyCore Core { get; init; }
    private IInventoryUpdateService InventoryUpdateService { get; init; }

    public DatabaseSynchronizeService(DataService dataService,
        ISwiftlyCore core,
        IInventoryUpdateService inventoryUpdateService)
    {
        DataService = dataService;
        Core = core;
        InventoryUpdateService = inventoryUpdateService;
    }

    public async Task SynchronizeAsync(IStorageProvider storage,
        CancellationToken stopping)
    {
        await synchronization.WaitAsync(stopping);
        try
        {
            var skins = (await storage.GetAllSkinsAsync().WaitAsync(stopping)).ToArray();
            var knives = (await storage.GetAllKnifesAsync().WaitAsync(stopping)).ToArray();
            var gloves = (await storage.GetAllGlovesAsync().WaitAsync(stopping)).ToArray();
            var agents = (await storage.GetAllAgentsAsync().WaitAsync(stopping)).ToArray();
            var musicKits = (await storage.GetAllMusicKitsAsync().WaitAsync(stopping)).ToArray();
            await Core.Scheduler.NextTickAsync(() =>
            {
                if (stopping.IsCancellationRequested) return;
                foreach (var skin in skins) DataService.WeaponDataService.StoreSkin(skin);
                foreach (var knife in knives) DataService.KnifeDataService.StoreKnife(knife);
                foreach (var glove in gloves) DataService.GloveDataService.StoreGlove(glove);
                foreach (var agent in agents) DataService.AgentDataService.SetAgent(agent.SteamID, agent.Team, agent.AgentIndex);
                foreach (var musicKit in musicKits) DataService.MusicKitDataService.SetMusicKit(musicKit.SteamID, musicKit.MusicKitIndex);
            }).WaitAsync(stopping);
        }
        finally
        {
            synchronization.Release();
        }
    }

    public async Task<bool> ReloadPlayerAsync(IStorageProvider storage,
        ulong steamId,
        ulong sessionId,
        CancellationToken stopping)
    {
        await synchronization.WaitAsync(stopping);
        try
        {
            var skins = (await storage.GetSkinsAsync(steamId).WaitAsync(stopping)).Where(skin => skin.SteamID == steamId).ToArray();
            var knives = (await storage.GetKnifesAsync(steamId).WaitAsync(stopping)).Where(knife => knife.SteamID == steamId).ToArray();
            var gloves = (await storage.GetGlovesAsync(steamId).WaitAsync(stopping)).Where(glove => glove.SteamID == steamId).ToArray();
            var agents = (await storage.GetAgentsAsync(steamId).WaitAsync(stopping)).Where(agent => agent.SteamID == steamId).ToArray();
            var musicKits = (await storage.GetMusicKitsAsync(steamId).WaitAsync(stopping)).Where(musicKit => musicKit.SteamID == steamId).ToArray();

            return await Core.Scheduler.NextTickAsync(() =>
            {
                if (stopping.IsCancellationRequested) return false;
                var player = Core.PlayerManager.GetPlayerFromSessionId(sessionId);
                if (player is not { IsValid: true } || player.SteamID != steamId) return false;

                var hadGlove = DataService.GloveDataService.TryGetGlove(steamId, player.Controller.Team, out _);
                DataService.WeaponDataService.RemovePlayer(steamId);
                DataService.KnifeDataService.RemovePlayer(steamId);
                DataService.GloveDataService.RemovePlayer(steamId);
                DataService.AgentDataService.TryRemoveAgent(steamId, Team.T);
                DataService.AgentDataService.TryRemoveAgent(steamId, Team.CT);
                DataService.MusicKitDataService.RemoveMusicKit(steamId);

                foreach (var skin in skins) DataService.WeaponDataService.StoreSkin(skin);
                foreach (var knife in knives) DataService.KnifeDataService.StoreKnife(knife);
                foreach (var glove in gloves) DataService.GloveDataService.StoreGlove(glove);
                foreach (var agent in agents) DataService.AgentDataService.SetAgent(steamId, agent.Team, agent.AgentIndex);
                if (musicKits.Length > 0) DataService.MusicKitDataService.SetMusicKit(steamId, musicKits[^1].MusicKitIndex);

                InventoryUpdateService.RefreshPlayer(steamId, hadGlove);
                return true;
            }).WaitAsync(stopping);
        }
        finally
        {
            synchronization.Release();
        }
    }
}