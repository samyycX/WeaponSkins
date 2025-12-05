using System.IO.MemoryMappedFiles;

using Microsoft.Extensions.DependencyInjection;

using SwiftlyS2.Shared.Plugins;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Commands;
using SwiftlyS2.Shared.SteamAPI;

using Tomlyn.Extensions.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using WeaponSkins.Configuration;
using WeaponSkins.Injections;
using WeaponSkins.Services;
using WeaponSkins.Shared;

namespace WeaponSkins;

[PluginMetadata(Id = "WeaponSkins", Version = "0.1.0", Name = "WeaponSkins", Author = "samyyc & ELDment",
    Description = "A swiftlys2 plugin to change player's skins.")]
public partial class WeaponSkins : BasePlugin
{
    private ServiceProvider _provider;

    public WeaponSkins(ISwiftlyCore core) : base(core)
    {
    }

    public override void Load(bool hotReload)
    {
        StickerFixService.Initialize();

        Core.Configuration
            .InitializeTomlWithModel<MainConfigModel>("config.toml", "Main")
            .Configure(builder =>
            {
                builder
                    .AddTomlFile("config.toml", false, true);
            });
        Core.Configuration.Manager.RemoveUnderscores();

        var collection = new ServiceCollection()
            .AddSwiftly(Core)
            .AddDataService()
            .AddNativeService()
            .AddInventoryService()
            .AddPlayerService()
            .AddApi()
            .AddEconService()
            .AddMenuService()
            .AddStorageService()
            .AddStattrakService()
            .AddLocalizationService()
            .AddCommandService();

        collection
            .AddOptions<MainConfigModel>()
            .BindConfiguration("Main");
        
        _provider = collection.BuildServiceProvider();

        _provider
            .UseDataService()
            .UseNativeService()
            .UseInventoryService()
            .UsePlayerService()
            .UseApi()
            .UseEconService()
            .UseMenuService()
            .UseStorageService()
            .UseStattrakService()
            .UseLocalizationService()
            .UseCommandService();
    }

    public override void Unload()
    {
    }

    public override void ConfigureSharedInterface(IInterfaceManager interfaceManager)
    {
        interfaceManager.AddSharedInterface<IWeaponSkinAPI, WeaponSkinAPI>("WeaponSkins.API",
            _provider.GetRequiredService<WeaponSkinAPI>());
    }
}