using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using Microsoft.Extensions.Logging;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Menu;
using SwiftlyS2.Shared.Menu.Components;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;
using SwiftlyS2.Shared.Translation;

using WeaponSkins.Econ;
using WeaponSkins.Services;
using WeaponSkins.Shared;

namespace WeaponSkins;

public partial class MenuService
{
    private ISwiftlyCore Core { get; init; }
    private ILogger<MenuService> Logger { get; init; }
    private WeaponSkinAPI Api { get; init; }
    private EconService EconService { get; init; }
    private LocalizationService LocalizationService { get; init; }
    private ItemPermissionService ItemPermissionService { get; init; }
    private DataService DataService { get; init; }
    private AgentPatchService AgentPatchService { get; init; }

    public MenuService(ISwiftlyCore core,
        ILogger<MenuService> logger,
        WeaponSkinAPI api,
        EconService econService,
        LocalizationService localizationService,
        ItemPermissionService itemPermissionService,
        DataService dataService,
        AgentPatchService agentPatchService)
    {
        Core = core;
        Logger = logger;
        Api = api;
        EconService = econService;
        LocalizationService = localizationService;
        ItemPermissionService = itemPermissionService;
        DataService = dataService;
        AgentPatchService = agentPatchService;
    }

    private IMenuBuilder CreateMenu(string id) => Core.Menu.CreateMenu(id);

    public void OpenMainMenu(IPlayer player)
    {
        var loc = LocalizationService[player];
        var steamId = player.SteamID;
        var main = CreateMenu("weaponskins.main")
            .WithTitle(loc.MenuTitle);

        main.Add(CreateCategory(ItemPermissionService.CanUseWeaponSkins(steamId), loc.MenuTitleSkins,
            "weaponskins.weapons", () => BuildWeaponSkinMenu(player)));
        main.Add(CreateCategory(ItemPermissionService.CanUseKnifeSkins(steamId), loc.MenuTitleKnifes,
            "weaponskins.knifes", () => BuildKnifeSkinMenu(player)));
        main.Add(CreateCategory(ItemPermissionService.CanUseGloveSkins(steamId), loc.MenuTitleGloves,
            "weaponskins.gloves", () => BuildGloveSkinMenu(player)));
        main.Add(CreateCategory(ItemPermissionService.CanUseAgents(steamId), loc.MenuTitleAgents,
            "weaponskins.agents", () => BuildAgentMenu(player)));
        main.Add(CreateCategory(ItemPermissionService.CanUseMusicKits(steamId), loc.MenuTitleMusicKits,
            "weaponskins.musickits", () => BuildMusicKitMenu(player)));
        main.Add(CreateCategory(ItemPermissionService.CanUseGraffiti(steamId), loc.MenuTitleGraffiti,
            "weaponskins.graffiti", () => BuildGraffitiMenu(player)));
        main.Add(CreateCategory(ItemPermissionService.CanUsePins(steamId), loc.MenuTitlePins,
            "weaponskins.pins", () => BuildPinMenu(player)));
        main.Add(CreateCategory(ItemPermissionService.CanUseWeaponSkins(steamId), loc.MenuTitleWeaponProperties,
            "weaponskins.weaponproperties", () => BuildWeaponPropertiesMenu(player)));
        main.Add(CreateCategory(ItemPermissionService.CanUseKnifeSkins(steamId), loc.MenuTitleKnifeProperties,
            "weaponskins.knifeproperties", () => BuildKnifePropertiesMenu(player)));
        main.Add(CreateCategory(ItemPermissionService.CanUseGloveSkins(steamId), loc.MenuTitleGloveProperties,
            "weaponskins.gloveproperties", () => BuildGlovePropertiesMenu(player)));
        main.Add(CreateCategory(ItemPermissionService.CanUsePatches(steamId), loc.MenuTitleAgentProperties,
            "weaponskins.agentproperties", () => BuildAgentPropertiesMenu(player)));

        var menu = main.Build();
        TrackFocus(menu);
        menu.Open(player);
    }

    // The component id is the id of the menu it opens, so a renderer with a
    // category sidebar can mark the page the player is on.
    private static SubmenuComponent CreateCategory(bool enabled, string title, string menuId, Func<IMenu> build)
    {
        return new SubmenuComponent(title, _ => enabled ? build() : null, menuId) { Enabled = enabled };
    }

    public bool TryGetWeaponInHand(IPlayer player,
        [MaybeNullWhen(false)] out CBasePlayerWeapon weaponInHand)
    {
        weaponInHand = null;
        if (!player.IsValid)
        {
            return false;
        }

        if (!(player.PlayerPawn is { IsValid: true, LifeState: (byte)LifeState_t.LIFE_ALIVE }))
        {
            return false;
        }

        weaponInHand = player.PlayerPawn.WeaponServices!.ActiveWeapon.Value;
        if (weaponInHand == null)
        {
            return false;
        }

        return true;
    }


    public bool TryGetWeaponDataInHand(IPlayer player,
        [MaybeNullWhen(false)] out WeaponSkinData dataInHand)
    {
        dataInHand = null;
        if (!TryGetWeaponInHand(player, out var weaponInHand))
        {
            return false;
        }

        var defIndex = weaponInHand.AttributeManager.Item.ItemDefinitionIndex;
        if (!Api.TryGetWeaponSkin(player.SteamID, player.Controller.Team, defIndex, out dataInHand))
        {
            return false;
        }

        return true;
    }

    public bool TryGetKnifeDataInHand(IPlayer player,
        [MaybeNullWhen(false)] out KnifeSkinData dataInHand)
    {
        return Api.TryGetKnifeSkin(player.SteamID, player.Controller.Team, out dataInHand);
    }

    public bool TryGetGloveDataInHand(IPlayer player,
        [MaybeNullWhen(false)] out GloveData dataInHand)
    {
        return Api.TryGetGloveSkin(player.SteamID, player.Controller.Team, out dataInHand);
    }

    private string GetAgentName(AgentDefinition agent, string language)
    {
        return agent.LocalizedNames.TryGetValue(language, out var localized)
            ? localized
            : agent.LocalizedNames.GetValueOrDefault("english") ?? agent.Name;
    }

    private static void AddEmptyNotice(IMenuBuilder builder, PlayerLocalizationService loc)
    {
        builder.Add(new TextComponent(loc.MenuPropertiesEmpty) { Enabled = false });
    }

    private static ButtonComponent CreateButton(string text, Action<IPlayer> onSelect)
    {
        return new ButtonComponent(text, ctx =>
        {
            onSelect(ctx.Player);
            return ValueTask.CompletedTask;
        });
    }

    private static ButtonComponent CreateRarityButton(string text, string hexColor, Action<IPlayer> onSelect)
    {
        var button = CreateButton(text, onSelect);
        button.Style = RarityStyle(hexColor);
        return button;
    }

    private static SubmenuComponent CreateSubmenu(string text, Func<IMenuSession, IMenu?> factory, string? hexColor = null)
    {
        var submenu = new SubmenuComponent(text, factory);
        if (hexColor is not null) submenu.Style = RarityStyle(hexColor);
        return submenu;
    }

    private static MenuTextStyle RarityStyle(string hexColor)
    {
        return MenuTextStyle.Default.WithSize(MenuTextSize.SmallMedium).WithColor(NormalizeHex(hexColor));
    }

    private static ToggleComponent CreateToggle(string text, bool value, PlayerLocalizationService loc, Action<MenuValueChangedContext<bool>> onChanged)
    {
        return new ToggleComponent(text, value, onChanged) { OnSymbol = loc.MenuOn, OffSymbol = loc.MenuOff };
    }

    private static string NormalizeHex(string hex)
    {
        return hex.StartsWith('#') ? hex : "#" + hex;
    }

    private static InputComponent CreateFloatInput(string text, float value, Func<float, bool> isValid, Action<IPlayer, float> apply)
    {
        return new InputComponent(text, value.ToString(CultureInfo.InvariantCulture),
            input => TryParseFloat(input, out var parsed) && isValid(parsed),
            ctx =>
            {
                if (TryParseFloat(ctx.NewValue, out var parsed))
                {
                    apply(ctx.Player, parsed);
                }
            });
    }

    private static InputComponent CreateIntInput(string text, int value, Func<int, bool> isValid, Action<IPlayer, int> apply)
    {
        return new InputComponent(text, value.ToString(CultureInfo.InvariantCulture),
            input => int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) && isValid(parsed),
            ctx =>
            {
                if (int.TryParse(ctx.NewValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                {
                    apply(ctx.Player, parsed);
                }
            });
    }

    private static bool TryParseFloat(string input, out float value)
    {
        return float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    // Stands in for the legacy MoveToOption click handlers: back-navigation restores the last row,
    // a fresh open lands on the row whose Tag matches what the player is holding.
    private void TrackFocus(IMenu menu, Func<IPlayer, object?>? inHandTag = null)
    {
        var remembered = new ConcurrentDictionary<int, int>();

        menu.Closed += session => remembered[session.Player.PlayerID] = session.FocusedIndex;
        menu.Opened += session =>
        {
            if (remembered.TryRemove(session.Player.PlayerID, out var index))
            {
                session.SetFocus(index);
                return;
            }

            if (inHandTag == null)
            {
                return;
            }

            Core.Scheduler.NextTick(() =>
            {
                if (!session.IsOpen || !session.Player.IsValid || inHandTag(session.Player) is not { } tag)
                {
                    return;
                }

                var position = 0;
                foreach (var component in menu.GetComponents(MenuRegion.Body))
                {
                    if (!component.IsFocusable || !component.IsVisible(session))
                    {
                        continue;
                    }

                    if (component is MenuComponentBase { Tag: { } componentTag } && componentTag.Equals(tag))
                    {
                        session.SetFocus(position);
                        return;
                    }

                    position++;
                }
            });
        };
    }
}
