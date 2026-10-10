using System.Collections.Concurrent;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Configuration;
using WeaponSkins.Shared;

namespace WeaponSkins.Database;

public class StorageService : IDisposable
{
    private readonly CancellationTokenSource lifetime = new();
    private readonly ConcurrentDictionary<ulong, byte> reloads = new();
    private readonly IDisposable? optionsSubscription;
    private IStorageProvider Provider { get; set; }
    private DatabaseService DatabaseService { get; init; }
    private ISwiftlyCore Core { get; init; }
    private ILogger<StorageService> Logger { get; init; }
    private DatabaseSynchronizeService DatabaseSynchronizeService { get; init; }

    public StorageService(IOptionsMonitor<MainConfigModel> options,
        ISwiftlyCore core,
        ILogger<StorageService> logger,
        DatabaseService databaseService,
        DatabaseSynchronizeService databaseSynchronizeService,
        EmptyStorageProvider emptyStorageProvider)
    {
        Core = core;
        Logger = logger;
        Provider = emptyStorageProvider;
        DatabaseService = databaseService;
        DatabaseSynchronizeService = databaseSynchronizeService;

        Configure(options.CurrentValue);

        optionsSubscription = options.OnChange(Configure);
    }

    public void Configure(MainConfigModel config)
    {
        if (config.StorageBackend == "inherit")
        {
            Logger.LogInformation("Using inherited database storage backend.");
            DatabaseService.Start(Core.Database);
            Provider = DatabaseService;
            _ = SynchronizeAsync();
        }
        else if (config.StorageBackend == "sqlite")
        {
            Logger.LogInformation("Using SQLite storage backend.");
            var path = Path.Combine(Core.PluginDataDirectory, "weaponskins.db");
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }

            DatabaseService.StartSqlite(path);
            Provider = DatabaseService;
            _ = SynchronizeAsync();
        }
        else if (config.StorageBackend == "external")
        {
            Logger.LogInformation("Using external storage backend.");
        }
        else
        {
            Logger.LogError("Invalid storage backend: {Backend}", config.StorageBackend);
            throw new InvalidOperationException($"Invalid storage backend: {config.StorageBackend}");
        }

        Core.Event.OnClientSteamAuthorize -= OnClientSteamAuthorize;
        if (config.SyncFromDatabaseWhenPlayerJoin)
        {
            Logger.LogInformation("Synchronizing data from database when player join.");
            Core.Event.OnClientSteamAuthorize += OnClientSteamAuthorize;
        }
        else
        {
            Logger.LogInformation("Not synchronizing data from database when player join.");
        }
    }

    private void OnClientSteamAuthorize(IOnClientSteamAuthorizeEvent @event)
    {
        var player = Core.PlayerManager.GetPlayer(@event.PlayerId);
        if (player is { IsValid: true } && player.SteamID != 0) ReloadPlayer(player);
    }

    public void Set(IStorageProvider provider)
    {
        Logger.LogInformation("Setting storage provider to {Provider}.", provider.Name);
        Provider = provider;
        _ = SynchronizeAsync();
    }

    public IStorageProvider Get()
    {
        return Provider;
    }

    public void ReloadPlayer(IPlayer player,
        Action<string>? reply = null)
    {
        if (lifetime.IsCancellationRequested || !player.IsValid || player.SteamID == 0) return;
        var storage = Provider;
        if (storage is EmptyStorageProvider)
        {
            reply?.Invoke("Skin storage is not ready. Try again later.");
            return;
        }

        if (!reloads.TryAdd(player.SteamID, 0))
        {
            reply?.Invoke("Your skins are already being reloaded.");
            return;
        }

        reply?.Invoke("Reloading your skins from storage.");
        _ = ReloadPlayerAsync(storage, player.SteamID, player.SessionId, reply);
    }

    private async Task ReloadPlayerAsync(IStorageProvider storage,
        ulong steamId,
        ulong sessionId,
        Action<string>? reply)
    {
        var stopping = lifetime.Token;
        try
        {
            string message;
            try
            {
                if (!await DatabaseSynchronizeService.ReloadPlayerAsync(storage, steamId, sessionId, stopping)) return;
                message = "Your skins have been reloaded.";
            }
            catch (OperationCanceledException) when (stopping.IsCancellationRequested)
            {
                return;
            }
            catch (Exception error)
            {
                Logger.LogError(error, "Failed to reload player cosmetics from {Provider}", storage.Name);
                message = "Could not reload your skins. Check the server logs.";
            }

            if (reply == null || stopping.IsCancellationRequested) return;
            await Core.Scheduler.NextTickAsync(() =>
            {
                if (stopping.IsCancellationRequested) return;
                var player = Core.PlayerManager.GetPlayerFromSessionId(sessionId);
                if (player is { IsValid: true } && player.SteamID == steamId) reply(message);
            }).WaitAsync(stopping);
        }
        catch (OperationCanceledException) when (stopping.IsCancellationRequested)
        {
        }
        catch (Exception error)
        {
            Logger.LogError(error, "Failed to complete a player cosmetic reload");
        }
        finally
        {
            reloads.TryRemove(steamId, out _);
        }
    }

    private async Task SynchronizeAsync()
    {
        var storage = Provider;
        var stopping = lifetime.Token;
        try
        {
            await DatabaseSynchronizeService.SynchronizeAsync(storage, stopping);
            Logger.LogInformation("Data synchronized from {Provider}.", storage.Name);
        }
        catch (OperationCanceledException) when (stopping.IsCancellationRequested)
        {
        }
        catch (Exception error)
        {
            Logger.LogError(error, "Failed to synchronize cosmetics from {Provider}", storage.Name);
        }
    }

    public void Dispose()
    {
        lifetime.Cancel();
        optionsSubscription?.Dispose();
        Core.Event.OnClientSteamAuthorize -= OnClientSteamAuthorize;
        lifetime.Dispose();
    }
}