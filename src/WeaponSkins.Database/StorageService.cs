using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SwiftlyS2.Shared;

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
        var config = options.CurrentValue;

        if (config.StorageBackend == "inherit")
        {
            Logger.LogInformation("Using inherited database storage backend.");
            DatabaseService.Start(core.Database);
            Provider = DatabaseService;
            DatabaseSynchronizeService.Synchronize();
        }
        else if (config.StorageBackend == "sqlite")
        {
            Logger.LogInformation("Using SQLite storage backend.");
            var path = Path.Combine(core.PluginDataDirectory, "weaponskins.db");
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
            Provider = emptyStorageProvider;
        }
        else
        {
            Logger.LogError("Invalid storage backend: {Backend}", config.StorageBackend);
            throw new InvalidOperationException($"Invalid storage backend: {config.StorageBackend}");
        }
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