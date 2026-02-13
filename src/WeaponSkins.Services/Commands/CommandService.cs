using Microsoft.Extensions.Logging;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Commands;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
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
        Core.Command.RegisterCommand("skins", CommandSkins);
        Core.Command.RegisterCommand("skin", CommandSkins);
        Core.Command.RegisterCommand("kf", CommandKnife);
        Core.Command.RegisterCommand("knife", CommandKnife);
        Core.Command.RegisterCommand("knifes", CommandKnife);
        Core.Command.RegisterCommand("knives", CommandKnife);
        Core.Command.RegisterCommand("glove", CommandGloves);
        Core.Command.RegisterCommand("gloves", CommandGloves);
        Core.Command.RegisterCommand("sticker", CommandStickers);
        Core.Command.RegisterCommand("stickers", CommandStickers);
        Core.Command.RegisterCommand("keychain", CommandKeychains);
        Core.Command.RegisterCommand("keychains", CommandKeychains);
        Core.Command.RegisterCommand("agent", CommandAgents);
        Core.Command.RegisterCommand("agents", CommandAgents);
        Core.Command.RegisterCommand("skinproperties", CommandSkinProperties);
        Core.Command.RegisterCommand("skinprops", CommandSkinProperties);
        Core.Command.RegisterCommand("st", CommandSkinProperties);
        Core.Command.RegisterCommand("stattrak", CommandSkinProperties);
        Core.Command.RegisterCommand("knifeproperties", CommandKnifeProperties);
        Core.Command.RegisterCommand("knifeprops", CommandKnifeProperties);
        Core.Command.RegisterCommand("gloveproperties", CommandGloveProperties);
        Core.Command.RegisterCommand("gloveprops", CommandGloveProperties);
    }

    private void CommandSkin(ICommandContext context)
    {
        if (!TryGetPlayer(context, out var player))
        {
            return;
        }

        MenuService.OpenMainMenu(player);
    }

    private void CommandKnife(ICommandContext context)
    {
        if (!TryGetPlayer(context, out var player))
        {
            return;
        }

        if (!MenuService.TryOpenKnifeSkinMenu(player))
        {
            context.Reply("Unable to open this menu right now.");
        }
    }

    private void CommandSkins(ICommandContext context)
    {
        if (!TryGetPlayer(context, out var player))
        {
            return;
        }

        if (!MenuService.TryOpenWeaponSkinMenu(player))
        {
            context.Reply("Unable to open this menu right now.");
        }
    }

    private void CommandGloves(ICommandContext context)
    {
        if (!TryGetPlayer(context, out var player))
        {
            return;
        }

        if (!MenuService.TryOpenGloveSkinMenu(player))
        {
            context.Reply("Unable to open this menu right now.");
        }
    }

    private void CommandStickers(ICommandContext context)
    {
        if (!TryGetPlayer(context, out var player))
        {
            return;
        }

        if (!MenuService.TryOpenStickerMenu(player))
        {
            context.Reply("Unable to open this menu right now.");
        }
    }

    private void CommandKeychains(ICommandContext context)
    {
        if (!TryGetPlayer(context, out var player))
        {
            return;
        }

        if (!MenuService.TryOpenKeychainMenu(player))
        {
            context.Reply("Unable to open this menu right now.");
        }
    }

    private void CommandAgents(ICommandContext context)
    {
        if (!TryGetPlayer(context, out var player))
        {
            return;
        }

        if (!MenuService.TryOpenAgentMenu(player))
        {
            context.Reply("Unable to open this menu right now.");
        }
    }

    private void CommandSkinProperties(ICommandContext context)
    {
        if (!TryGetPlayer(context, out var player))
        {
            return;
        }

        if (!MenuService.TryOpenSkinPropertiesMenu(player))
        {
            context.Reply("Unable to open this menu right now.");
        }
    }

    private void CommandKnifeProperties(ICommandContext context)
    {
        if (!TryGetPlayer(context, out var player))
        {
            return;
        }

        if (!MenuService.TryOpenKnifePropertiesMenu(player))
        {
            context.Reply("Unable to open this menu right now.");
        }
    }

    private void CommandGloveProperties(ICommandContext context)
    {
        if (!TryGetPlayer(context, out var player))
        {
            return;
        }

        if (!MenuService.TryOpenGlovePropertiesMenu(player))
        {
            context.Reply("Unable to open this menu right now.");
        }
    }

    private static bool TryGetPlayer(ICommandContext context, out IPlayer player)
    {
        player = null!;
        if (!context.IsSentByPlayer || context.Sender == null)
        {
            context.Reply("This command can only be used by players.");
            return false;
        }

        player = context.Sender;
        return true;
    }
}
