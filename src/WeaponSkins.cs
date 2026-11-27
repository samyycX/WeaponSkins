using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Commands;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Plugins;
using SwiftlyS2.Shared.SchemaDefinitions;
using SwiftlyS2.Shared.SteamAPI;

using WeaponSkins.Configuration;
using WeaponSkins.Injections;
using WeaponSkins.Services;
using WeaponSkins.Shared;

namespace WeaponSkins;


[PluginMetadata(
    Id = "WeaponSkins",
    Version = "0.11.0",
    Name = "WeaponSkins",
    Author = "samyyc & ELDment",
    Description = "A swiftlys2 plugin to change player's skins."
)]
public partial class WeaponSkins : BasePlugin
{
    private ServiceProvider _provider = null!;

    public WeaponSkins(ISwiftlyCore core) : base(core)
    {
    }

    public override void Load(bool hotReload)
    {
        TomlConfigMigration.Run(Core);

        Core.Configuration.InitializeJsonWithModel<MainConfigModel>("config.jsonc", "Main", addMissingKeys: true)
            .Configure(builder =>
            {
                builder.AddJsonFile("config.jsonc", optional: false, reloadOnChange: true);
            });

        var collection = new ServiceCollection()
            .AddSwiftly(Core)
            .AddDataService()
            .AddInventoryService()
            .AddPlayerService()
            .AddApi()
            .AddEconService()
            .AddMenuService()
            .AddStorageService()
            .AddStattrakService()
            .AddLocalizationService()
            .AddItemPermissionService()
            .AddGraffitiService()
            .AddAgentPatchService()
            .AddPinService()
            .AddCommandService();


        collection
            .AddOptions<MainConfigModel>()
            .BindConfiguration("Main")
            .PostConfigure<IConfiguration>((model, configuration) =>
                model.ItemPermissions?.ReadPlainPermissions(configuration.GetSection("Main:ItemPermissions")));

        _provider = collection.BuildServiceProvider();

        _provider
            .UseDataService()
            .UseInventoryService()
            .UsePlayerService()
            .UseApi()
            .UseEconService()
            .UseMenuService()
            .UseStorageService()
            .UseStattrakService()
            .UseLocalizationService()
            .UseItemPermissionService()
            .UseGraffitiService()
            .UseAgentPatchService()
            .UsePinService()
            .UseCommandService();
    }

    public override void Unload()
    {
        // SwiftlyS2 also calls Unload when Load throws, which can be before the provider exists.
        _provider?.Dispose();
    }

    public override void ConfigureSharedInterface(IInterfaceManager interfaceManager)
    {
        interfaceManager.AddSharedInterface<IWeaponSkinAPI, WeaponSkinAPI>("WeaponSkins.API",
            _provider.GetRequiredService<WeaponSkinAPI>());
    }
}
