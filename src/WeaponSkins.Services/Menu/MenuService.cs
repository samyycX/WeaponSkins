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

    public MenuService(ISwiftlyCore core,
        ILogger<MenuService> logger,
        WeaponSkinAPI api,
        EconService econService,
        LocalizationService localizationService,
        ItemPermissionService itemPermissionService,
        DataService dataService)
    {
        Core = core;
        Logger = logger;
        Api = api;
        EconService = econService;
        LocalizationService = localizationService;
        ItemPermissionService = itemPermissionService;
        DataService = dataService;
    }

    // CustomHUD pages by MaxVisibleLines in menus.jsonc, and a scrolling layout shows the whole list; any other
    // renderer keeps core's default page, the size CenterHTML's panel holds. Core picks the renderer only in Build().
    private IMenuBuilder CreateMenu(string id)
    {
        using var probe = Core.Menu.CreateMenu(id).Build();
        var builder = Core.Menu.CreateMenu(id);
        return string.Equals(probe.Renderer.Id, CustomHudRendererId, StringComparison.OrdinalIgnoreCase)
            ? builder.WithItemsPerPage(int.MaxValue)
            : builder;
    }

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
        main.Add(CreateCategory(ItemPermissionService.CanUseWeaponSkins(steamId), loc.MenuTitleWeaponProperties,
            "weaponskins.weaponproperties", () => BuildWeaponPropertiesMenu(player)));
        main.Add(CreateCategory(ItemPermissionService.CanUseKnifeSkins(steamId), loc.MenuTitleKnifeProperties,
            "weaponskins.knifeproperties", () => BuildKnifePropertiesMenu(player)));
        main.Add(CreateCategory(ItemPermissionService.CanUseGloveSkins(steamId), loc.MenuTitleGloveProperties,
            "weaponskins.gloveproperties", () => BuildGlovePropertiesMenu(player)));

        var menu = main.Build();
        TrackFocus(menu);
        menu.Open(player);
    }

    // The component id is the id of the menu it opens: a renderer with a
    // category sidebar (CustomHUD) matches the two to mark the page a player is on.
    private static SubmenuComponent CreateCategory(bool enabled, string title, string menuId, Func<IMenu> build)
    {
        return new SubmenuComponent(title, _ => enabled ? build() : null, menuId) { Enabled = enabled };
    }

    // CenterHTML and chat would draw it as a row of dashes; CustomHUD draws it only as its locker sidebar's rule.
    private sealed class SidebarDividerComponent : DividerComponent
    {
        public override bool IsVisible(IMenuSession session)
            => base.IsVisible(session)
               && string.Equals(session.Menu.Renderer.Id, CustomHudRendererId, StringComparison.OrdinalIgnoreCase);
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

    private static ButtonComponent CreateButton(string text, Action<IPlayer> onSelect, string? imageKey = null, string? typeLabel = null,
        Func<IMenuSession, bool>? applied = null)
    {
        return new ThumbButtonComponent(text, ctx =>
        {
            onSelect(ctx.Player);
            return ValueTask.CompletedTask;
        })
        {
            ImageKey = imageKey,
            TypeLabel = typeLabel,
            Applied = applied
        };
    }

    private static ButtonComponent CreateRarityButton(string text, string hexColor, Action<IPlayer> onSelect, string? imageKey = null, string? typeLabel = null,
        Func<IMenuSession, bool>? applied = null)
    {
        var button = CreateButton(text, onSelect, imageKey, typeLabel, applied);
        button.Style = RarityStyle(hexColor);
        return button;
    }

    private static SubmenuComponent CreateSubmenu(string text, Func<IMenuSession, IMenu?> factory, string? imageKey, string? hexColor = null,
        Func<IMenuSession, bool>? applied = null)
    {
        var submenu = new ThumbSubmenuComponent(text, factory) { ImageKey = imageKey, Applied = applied };
        if (hexColor is not null) submenu.Style = RarityStyle(hexColor);
        return submenu;
    }

    // The data services, not the API's cloning views: a grid asks once per card on every draw.
    private bool HasWeaponPaint(IMenuSession session, ushort definitionIndex, int paintkit)
        => DataService.WeaponDataService.TryGetSkin(session.Player.SteamID, session.Player.Controller.Team, definitionIndex, out var skin)
           && skin.Paintkit == paintkit;

    private bool HasKnife(IMenuSession session, int definitionIndex, int? paintkit = null)
        => DataService.KnifeDataService.TryGetKnife(session.Player.SteamID, session.Player.Controller.Team, out var knife)
           && knife.DefinitionIndex == definitionIndex && (paintkit is null || knife.Paintkit == paintkit);

    private bool HasGlove(IMenuSession session, int definitionIndex, int? paintkit = null)
        => DataService.GloveDataService.TryGetGlove(session.Player.SteamID, session.Player.Controller.Team, out var glove)
           && glove.DefinitionIndex == definitionIndex && (paintkit is null || glove.Paintkit == paintkit);

    private static MenuTextStyle RarityStyle(string hexColor)
    {
        return MenuTextStyle.Default.WithSize(MenuTextSize.SmallMedium).WithColor(NormalizeHex(hexColor));
    }

    private static ToggleComponent CreateToggle(string text, bool value, PlayerLocalizationService loc, Action<MenuValueChangedContext<bool>> onChanged)
    {
        return new ToggleComponent(text, value, onChanged) { OnSymbol = loc.MenuOn, OffSymbol = loc.MenuOff };
    }

    // ImageKey is a CS2 econ texture path under panorama/images/econ/ (no extension); TypeLabel is
    // the caption drawn above the name; chosen outlines the item the player has on now. Only the
    // CustomHUD renderer reads any of them; every other renderer ignores raw nodes addressed elsewhere.
    private static MenuNode WithCustomHudMetadata(MenuNode node, string? imageKey, string? typeLabel, bool chosen = false)
    {
        var hasType = !string.IsNullOrWhiteSpace(typeLabel);
        if (imageKey is null && !hasType && !chosen) return node;

        // `;` separates directives in the payload, so a localized label must not carry one.
        var payload = string.Join(';', new[]
        {
            imageKey is null ? null : "image=" + imageKey,
            hasType ? "type=" + typeLabel!.Replace(';', ',') : null,
            chosen ? "chosen" : null
        }.Where(p => p is not null));

        return MarkForCustomHud(node, payload);
    }

    private sealed class ThumbButtonComponent(string text, Func<MenuActionContext, ValueTask> onSelect)
        : ButtonComponent(text, onSelect)
    {
        public string? ImageKey { get; init; }
        public string? TypeLabel { get; init; }

        // Read on every draw, so the outline follows the player's pick without a rebuild.
        public Func<IMenuSession, bool>? Applied { get; init; }

        public override MenuNode Render(IMenuComponentRenderContext context)
            => WithCustomHudMetadata(base.Render(context), ImageKey, TypeLabel, Applied?.Invoke(context.Session) == true);
    }

    private sealed class ThumbSubmenuComponent(string text, Func<IMenuSession, IMenu?> factory)
        : SubmenuComponent(text, factory)
    {
        public string? ImageKey { get; init; }

        // Read on every draw, like TextProvider; a null result falls back to ImageKey.
        public Func<IMenuSession, string?>? ImageKeyProvider { get; init; }

        public Func<IMenuSession, bool>? Applied { get; init; }

        public override MenuNode Render(IMenuComponentRenderContext context)
            => WithCustomHudMetadata(base.Render(context), ImageKeyProvider?.Invoke(context.Session) ?? ImageKey, null,
                Applied?.Invoke(context.Session) == true);
    }

    private static string NormalizeHex(string hex)
    {
        return hex.StartsWith('#') ? hex : "#" + hex;
    }

    // The pair EconService.ParseWeaponToPaintkits discovered, rendered at its
    // cleanest (_light) wear.
    private static string PaintkitImageKey(string weapon, PaintkitDefinition paintkit)
    {
        return $"default_generated/{weapon}_{paintkit.Name}_light";
    }

    private static string BaseWeaponImageKey(string weapon)
    {
        return $"weapons/base_weapons/{weapon}";
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
