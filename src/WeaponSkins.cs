using System.IO.MemoryMappedFiles;

using Microsoft.Extensions.DependencyInjection;

using SwiftlyS2.Shared.Plugins;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.SchemaDefinitions;
using SwiftlyS2.Shared.Schemas;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Commands;

using ValveKeyValue;

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
            .BuildServiceProvider();

        _provider
            .UseDataService()
            .UseNativeService()
            .UseInventoryService()
            .UsePlayerService()
            .UseApi()
            .UseEconService()
            .UseMenuService();

        var dataService = _provider.GetRequiredService<DataService>();
        dataService.KnifeDataService.StoreKnife(new KnifeSkinData
        {
            SteamID = 76561199171006920,
            Team = Team.T,
            DefinitionIndex = 506,
            Paintkit = 800,
            PaintkitSeed = 1,
            PaintkitWear = 1.0f
        });

        dataService.WeaponDataService.StoreSkin(new WeaponSkinData
        {
            SteamID = 76561199171006920,
            Team = Team.T,
            DefinitionIndex = 4,
            Paintkit = 801,
            PaintkitSeed = 1,
            PaintkitWear = 1.0f,
            Sticker2 = new()
            {
                Id = 1876,
                Wear = 0.1f,
                Scale = 1.0f,
                Rotation = 0.0f,
                OffsetX = 0.01f,
                OffsetY = 0.01f,
                Schema = 1
            }
        });

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