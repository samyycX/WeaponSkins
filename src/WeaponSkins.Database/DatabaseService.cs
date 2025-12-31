using System.Data;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.VersionTableInfo;

using FreeSql;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Database;

using WeaponSkins.Shared;

namespace WeaponSkins.Database;

public partial class DatabaseService : IStorageProvider
{
    private IFreeSql fsql { get; set; }
    private ISwiftlyCore Core { get; set; }

    public string Name => "WeaponSkins.Database";

    public DatabaseService(ISwiftlyCore core)
    {
        Core = core;
    }

    public void Start(IDatabaseService dbService)
    {
        // var conn = core.Database.GetConnection("weaponskins");
        // var connString = core.Database.GetConnectionString("weaponskins");
        
        var protocol = dbService.GetConnectionInfo("weaponskins").Driver switch
        {
            "mysql" => DataType.MySql,
            "postgresql" => DataType.PostgreSQL,
            "sqlite" => DataType.Sqlite,
            _ => throw new Exception("Unsupported database driver."),
        };
        var conn = dbService.GetConnection("weaponskins");
        var connString = conn.ConnectionString;

        fsql = GetBuilder(protocol, connString).Build();

        RunMigrations(conn, protocol);
    }

    public void StartSqlite(string path)
    {
        fsql = GetSqliteBuilder(path).Build();
        RunMigrations(new SqliteConnection($"Data Source={path}"), DataType.Sqlite);
    }

    private void RunMigrations(IDbConnection dbConnection,
        DataType protocol)
    {
        var serviceProvider = new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner((rb) =>
            {
                if (protocol == DataType.MySql)
                {
                    rb.AddMySql8();
                }
                else if (protocol == DataType.PostgreSQL)
                {
                    rb.AddPostgres();
                }
                else if (protocol == DataType.Sqlite)
                {
                    rb.AddSQLite();
                }
                else throw new Exception("Unsupported database type.");

                rb.WithGlobalConnectionString(dbConnection.ConnectionString).ScanIn(typeof(DatabaseService).Assembly)
                    .For.Migrations();

                rb.Services
                    .AddTransient<IVersionTableMetaData, CustomMetadataTable>();
            })
            .AddLogging(lb => lb.AddFluentMigratorConsole())
            .BuildServiceProvider(false);

        using var scope = serviceProvider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }

    private FreeSqlBuilder GetSqliteBuilder(string path)
    {
        var builder = new FreeSqlBuilder();
        builder.UseConnectionString(DataType.Sqlite, $"Data Source={path}");
        return builder;
    }

    private FreeSqlBuilder GetBuilder(DataType protocol, string connectionString)
    {
        var builder = new FreeSqlBuilder();
        builder.UseConnectionString(protocol, connectionString);
        builder.UseAdoConnectionPool(true);
        return builder;
    }
}