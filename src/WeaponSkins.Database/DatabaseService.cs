using System.Data;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.VersionTableInfo;

using FreeSql;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SwiftlyS2.Shared;

namespace WeaponSkins.Database;

public partial class DatabaseService
{
    private IFreeSql fsql { get; set; }
    private ISwiftlyCore Core { get; set; }

    public DatabaseService(ISwiftlyCore core)
    {
        Core = core;

        var conn = core.Database.GetConnection("weaponskins");
        var connString = core.Database.GetConnectionString("weaponskins");
        fsql = GetBuilder(connString).Build();

        RunMigrations(conn, connString);


        var skins = fsql.Select<SkinModel>().ToList();
        Console.WriteLine("SKINS: {0}", skins.Count);
    }

    private void RunMigrations(IDbConnection dbConnection,
        string dbConnString)
    {
        var serviceProvider = new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner((rb) =>
            {
                if (dbConnString.StartsWith("mysql", StringComparison.OrdinalIgnoreCase))
                {
                    rb.AddMySql8();
                }
                else if (dbConnString.StartsWith("postgresql", StringComparison.OrdinalIgnoreCase))
                {
                    rb.AddPostgres();
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

    private FreeSqlBuilder GetBuilder(string connectionString)
    {
        var builder = new FreeSqlBuilder();

        var protoEnd = connectionString.IndexOf("://");
        var lastAt = connectionString.LastIndexOf('@');
        var slash = connectionString.IndexOf('/', lastAt);

        if (protoEnd < 0 || lastAt < protoEnd || slash < 0)
            throw new FormatException("Expected format: protocol://user:password@host:port/database");

        var protocol = connectionString[..protoEnd];
        var credentials = connectionString[(protoEnd + 3)..lastAt];
        var userEnd = credentials.IndexOf(':');

        if (userEnd < 0)
            throw new FormatException("Expected format: protocol://user:password@host:port/database");

        var defaultPort = protocol switch
        {
            "mysql" => "3306",
            "postgresql" => "5432",
            _ => throw new NotSupportedException($"Unsupported protocol: {protocol}")
        };

        var hostPort = connectionString[(lastAt + 1)..slash];
        var portColon = hostPort.LastIndexOf(':');
        var host = portColon > 0 ? hostPort[..portColon] : hostPort;
        var port = portColon > 0 ? hostPort[(portColon + 1)..] : defaultPort;

        var connStr = $"Server={host};"
                      + $"Port={port};"
                      + $"Database={connectionString[(slash + 1)..]};"
                      + $"User ID={credentials[..userEnd]};"
                      + $"Password={credentials[(userEnd + 1)..]}";

        if (protocol == "mysql")
        {
            builder.UseConnectionString(DataType.MySql, connStr);
        }
        else if (protocol == "postgresql")
        {
            builder.UseConnectionString(DataType.PostgreSQL, connStr);
        }

        builder
            .UseMonitorCommand(cmd => Console.WriteLine($"Sql：{cmd.CommandText}"));
        builder.UseAdoConnectionPool(true);
        return builder;
    }
}