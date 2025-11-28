using System.IO.MemoryMappedFiles;

using Microsoft.Extensions.DependencyInjection;

using SwiftlyS2.Shared.Plugins;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Commands;
using SwiftlyS2.Shared.SteamAPI;

using WeaponSkins.Injections;
using WeaponSkins.Services;
using WeaponSkins.Shared;

namespace WeaponSkins;

[PluginMetadata(Id = "WeaponSkins", Version = "0.1.0", Name = "WeaponSkins", Author = "samyyc",
    Description = "No description.")]
public partial class WeaponSkins : BasePlugin
{
    private ServiceProvider _provider;

    public WeaponSkins(ISwiftlyCore core) : base(core)
    {
    }

    [Command("test")]
    public void TestCommand(ICommandContext args)
    {
        var menu = _provider.GetRequiredService<MenuService>();
        menu.TestMenu(args.Sender);
    }

    public override void Load(bool hotReload)
    {
        _provider = new ServiceCollection()
            .AddSwiftly(Core)
            .AddDataService()
            .AddNativeService()
            .AddInventoryService()
            .AddPlayerService()
            .AddApi()
            .AddEconService()
            .AddMenuService()
            .AddDatabaseService()
            .AddStickerFixService()
            .AddStattrakService()
            .AddLocalizationService()
            .BuildServiceProvider();

        _provider
            .UseDataService()
            .UseNativeService()
            .UseInventoryService()
            .UsePlayerService()
            .UseApi()
            .UseEconService()
            .UseMenuService()
            .UseDatabaseService()
            .UseStickerFixService()
            .UseStattrakService()
            .UseLocalizationService();

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