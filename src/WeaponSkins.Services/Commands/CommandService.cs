using Microsoft.Extensions.Logging;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Commands;

using WeaponSkins.Database;

namespace WeaponSkins;

public partial class CommandService
{

    private ISwiftlyCore Core { get; init; }
    private ILogger Logger { get; init; }
    private MenuService MenuService { get; init; }
    private StorageService StorageService { get; init; }

    public CommandService(ISwiftlyCore core,
        ILogger<CommandService> logger,
        MenuService menuService,
        StorageService storageService)
    {
        Core = core;
        Logger = logger;
        MenuService = menuService;
        StorageService = storageService;

        RegisterCommands();
    }
    public void RegisterCommands()
    {
        Core.Command.RegisterCommand("ws", CommandSkin);
        Core.Command.RegisterCommand("wp", CommandReload);
        Core.Command.RegisterCommand("wp_admin", CommandReloadBySteamId);
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

    // Server-console/RCON only. Reloads the same data as !wp for one connected player.
    private void CommandReloadBySteamId(ICommandContext context)
    {
        if (context.IsSentByPlayer)
        {
            context.Reply("This command can only be used from the server console.");
            return;
        }

        if (context.Args.Length != 1 ||
            !ulong.TryParse(context.Args[0], out var steamId) ||
            steamId == 0)
        {
            context.Reply("Usage: sw_wp_admin <SteamID64>");
            return;
        }

        var player = Core.PlayerManager.GetAllPlayers()
            .FirstOrDefault(p => p.IsValid && p.SteamID == steamId);

        if (player == null)
        {
            context.Reply("Player is not connected. Saved skins will load on next join.");
            return;
        }

        StorageService.ReloadPlayer(player);
        context.Reply($"Skin reload requested for {steamId}.");
    }

    private void CommandReload(ICommandContext context)
    {
        if (!context.IsSentByPlayer || context.Sender is not { IsValid: true } player)
        {
            context.Reply("This command can only be used by players.");
            return;
        }

        StorageService.ReloadPlayer(player, context.Reply);
    }
}
