using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using SwiftlyS2.Shared;

using Tomlyn.Extensions.Configuration;

namespace WeaponSkins.Configuration;

// Settings were in config.toml before 0.11.0. Once servers have moved, this can go, and the Tomlyn packages with it.
internal static class TomlConfigMigration
{
    private static MainConfigModel? Migrated { get; set; }

    public static void Run(ISwiftlyCore core)
    {
        var tomlPath = core.Configuration.GetConfigPath("config.toml");
        if (!File.Exists(tomlPath)) return;

        var jsoncPath = core.Configuration.GetConfigPath("config.jsonc");
        if (File.Exists(jsoncPath))
        {
            core.Logger.LogWarning("{Toml} is ignored: WeaponSkins reads its settings from config.jsonc.", tomlPath);
            return;
        }

        var values = new MainConfigModel();
        try
        {
            var toml = new ConfigurationBuilder()
                .AddTomlFile(tomlPath, optional: false, reloadOnChange: false)
                .Build();
            // Values only, and no plain-string permission, which ReadPlainPermissions reads: a key present for a plugin type makes the binder cache that type in TypeDescriptor, which keeps the plugin's assembly from unloading.
            new ConfigurationBuilder()
                .AddInMemoryCollection(toml.AsEnumerable().Where(setting => setting.Value is not null && !string.Equals(ConfigurationPath.GetParentPath(setting.Key), "Main:ItemPermissions", StringComparison.OrdinalIgnoreCase)))
                .Build()
                .GetSection("Main")
                .Bind(values);
            values.ItemPermissions.ReadPlainPermissions(toml.GetSection("Main:ItemPermissions"));
        }
        catch (Exception) when (!File.Exists(tomlPath) && File.Exists(jsoncPath))
        {
            // Another server moved the settings after the checks above.
            return;
        }
        catch (Exception error)
        {
            // Never the defaults instead: they open every VIP-only item and can switch the storage backend.
            throw new InvalidDataException($"Could not read {tomlPath} to move its settings to config.jsonc. Fix the file and load WeaponSkins again.", error);
        }

        // Servers run from one install share this folder: each writes a file of its own and moves it in only while there is no config.jsonc, so none reads half a file or removes another's.
        var tempName = $"config.{Guid.NewGuid():N}.jsonc";
        var tempPath = core.Configuration.GetConfigPath(tempName);
        var written = false;
        try
        {
            Migrated = values;
            core.Configuration.InitializeJsonWithModel<MigratedConfigModel>(tempName, "Main", addMissingKeys: false);
            written = true;
            File.Move(tempPath, jsoncPath);
        }
        catch (Exception) when (written && File.Exists(jsoncPath))
        {
            // Another server got there first, from the same config.toml.
        }
        catch (Exception error)
        {
            throw new IOException($"Could not move the settings in {tomlPath} to {jsoncPath}.", error);
        }
        finally
        {
            Migrated = null;
            try
            {
                File.Delete(tempPath);
            }
            catch (Exception error)
            {
                core.Logger.LogWarning(error, "Could not delete {Temp}, a temporary file from moving the settings to config.jsonc. WeaponSkins ignores it.", tempPath);
            }
        }

        var backupPath = tomlPath + ".old";
        for (var i = 1; Path.Exists(backupPath); i++) backupPath = $"{tomlPath}.old.{i}";

        try
        {
            File.Move(tomlPath, backupPath);
        }
        catch (Exception error) when (File.Exists(tomlPath))
        {
            core.Logger.LogWarning(error, "The settings are now in {Jsonc}, but {Toml} could not be renamed. WeaponSkins ignores it.", jsoncPath, tomlPath);
            return;
        }
        catch (Exception)
        {
            // Another server renamed it first.
            return;
        }

        core.Logger.LogInformation("Moved the settings from config.toml to {Jsonc} and renamed config.toml to {Backup}.", jsoncPath, Path.GetFileName(backupPath));
    }

    // SwiftlyS2 writes a new file from the model's defaults, so defaults copied from config.toml give it a new file's layout and comments.
    private sealed class MigratedConfigModel : MainConfigModel
    {
        public MigratedConfigModel()
        {
            foreach (var property in typeof(MainConfigModel).GetProperties())
            {
                property.SetValue(this, property.GetValue(Migrated));
            }
        }
    }
}
