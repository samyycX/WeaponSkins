using Microsoft.Extensions.Logging;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Commands;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace WeaponSkins;

public partial class CommandService
{

    private ISwiftlyCore Core { get; init; }
    private ILogger Logger { get; init; }
    private MenuService MenuService { get; init; }

    public CommandService(ISwiftlyCore core,
        ILogger<CommandService> logger,
        MenuService menuService)
    {
        Core = core;
        Logger = logger;
        MenuService = menuService;

        RegisterCommands();
    }
    public void RegisterCommands()
    {
        Core.Command.RegisterCommand("ws", CommandSkin);
    }

    private void CommandSkin(ICommandContext context)
    {
        if (!context.IsSentByPlayer)
        {
            context.Reply("This command can only be used by players.");
            return;
        }

        MenuService.OpenMainMenu(context.Sender!);
    }
}