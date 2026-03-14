using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Events;

using WeaponSkins.Configuration;
using WeaponSkins.Shared;

namespace WeaponSkins.Database;

public class StorageService
{
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

        options.OnChange(Configure);
    }

    public void Configure(MainConfigModel config)
    {
        if (config.StorageBackend == "inherit")
        {
            Logger.LogInformation("Using inherited database storage backend.");
            DatabaseService.Start(Core.Database);
            Provider = DatabaseService;
            DatabaseSynchronizeService.Synchronize();
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
            DatabaseSynchronizeService.Synchronize();
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

        if (config.SyncFromDatabaseWhenPlayerJoin)
        {
            Logger.LogInformation("Synchronizing data from database when player join.");
            Core.Event.OnClientPutInServer += OnClientPutInServer;
        } else {
            Logger.LogInformation("Not synchronizing data from database when player join.");
            Core.Event.OnClientPutInServer -= OnClientPutInServer;
        }
    }

    private void OnClientPutInServer(IOnClientPutInServerEvent @event)
    {
        DatabaseSynchronizeService.Synchronize();
    }

    public void Set(IStorageProvider provider)
    {
        Logger.LogInformation("Setting storage provider to {Provider}.", provider.Name);
        Provider = provider;
        DatabaseSynchronizeService.Synchronize();
        Logger.LogInformation("Data synchronized from {Provider}.", provider.Name);
    }

    public IStorageProvider Get()
    {
        return Provider;
    }
}