using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Menu;
using SwiftlyS2.Shared.Menu.Components;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;
using SwiftlyS2.Shared.Translation;

using WeaponSkins.Configuration;
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
    private IOptionsMonitor<MainConfigModel> Options { get; init; }

    public MenuService(ISwiftlyCore core,
        ILogger<MenuService> logger,
        WeaponSkinAPI api,
        EconService econService,
        LocalizationService localizationService,
        ItemPermissionService itemPermissionService,
        DataService dataService,
        IOptionsMonitor<MainConfigModel> options)
    {
        Core = core;
        Logger = logger;
        Api = api;
        EconService = econService;
        LocalizationService = localizationService;
        ItemPermissionService = itemPermissionService;
        DataService = dataService;
        Options = options;
    }

    private IMenuBuilder CreateMenu(string id)
    {
        return Core.Menu.CreateMenu(id).WithItemsPerPage(Options.CurrentValue.MenuItemsPerPage);
    }

    public void OpenMainMenu(IPlayer player)
    {
        var main = CreateMenu("weaponskins.main")
            .WithTitle(LocalizationService[player].MenuTitle);

        AddMenuOption(main, ItemPermissionService.CanUseWeaponSkins(player.SteamID),
            () => GetWeaponSkinMenuSubmenuOption(player), LocalizationService[player].MenuTitleSkins);
        AddMenuOption(main, ItemPermissionService.CanUseKnifeSkins(player.SteamID),
            () => GetKnifeSkinMenuSubmenuOption(player), LocalizationService[player].MenuTitleKnifes);
        AddMenuOption(main, ItemPermissionService.CanUseGloveSkins(player.SteamID),
            () => GetGloveSkinMenuSubmenuOption(player), LocalizationService[player].MenuTitleGloves);
        AddMenuOption(main, ItemPermissionService.CanUseStickers(player.SteamID),
            () => GetStickerMenuSubmenuOption(player), LocalizationService[player].MenuTitleStickers);
        AddMenuOption(main, ItemPermissionService.CanUseKeychains(player.SteamID),
            () => GetKeychainMenuSubmenuOption(player), LocalizationService[player].MenuTitleKeychains);
        AddMenuOption(main, ItemPermissionService.CanUseWeaponSkins(player.SteamID),
            () => GetSkinPropertiesMenuSubmenuOption(player), LocalizationService[player].MenuTitleSkinProperties);
        AddMenuOption(main, ItemPermissionService.CanUseKnifeSkins(player.SteamID),
            () => GetKnifePropertiesMenuSubmenuOption(player), LocalizationService[player].MenuTitleKnifeProperties);
        AddMenuOption(main, ItemPermissionService.CanUseGloveSkins(player.SteamID),
            () => GetGlovePropertiesMenuSubmenuOption(player), LocalizationService[player].MenuTitleGloveProperties);

        AddMenuOption(main, ItemPermissionService.CanUseAgents(player.SteamID),
            () => GetAgentMenuSubmenuOption(player), LocalizationService[player].MenuTitleAgents);
        AddMenuOption(main, ItemPermissionService.CanUseMusicKits(player.SteamID),
            () => GetMusicKitMenuSubmenuOption(player), LocalizationService[player].MenuTitleMusicKits);

        var menu = main.Build();
        TrackFocus(menu);
        menu.Open(player);
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

    private static void AddMenuOption(IMenuBuilder builder,
        bool enabled,
        Func<IMenuComponent> optionFactory,
        string title)
    {
        builder.Add(enabled ? optionFactory() : CreateDisabledOption(title));
    }

    private static TextComponent CreateDisabledOption(string title)
    {
        return new TextComponent(title) { Enabled = false };
    }

    private static IMenu BuildEmptyMenu(ISwiftlyCore core, string id, string title)
    {
        return core.Menu.CreateMenu(id).WithTitle(title).Build();
    }

    private static ButtonComponent CreateButton(string text, Action<IPlayer> onSelect, string? imageKey = null, string? typeLabel = null)
    {
        return new ThumbButtonComponent(text, ctx =>
        {
            onSelect(ctx.Player);
            return ValueTask.CompletedTask;
        })
        {
            ImageKey = imageKey,
            TypeLabel = typeLabel
        };
    }

    private static ButtonComponent CreateRarityButton(string text, string hexColor, Action<IPlayer> onSelect, string? imageKey = null, string? typeLabel = null)
    {
        var button = CreateButton(text, onSelect, imageKey, typeLabel);
        button.Style = MenuTextStyle.Default.WithSize(MenuTextSize.SmallMedium).WithColor(NormalizeHex(hexColor));
        return button;
    }

    private static SubmenuComponent CreateSubmenu(string text, Func<IMenuSession, IMenu?> factory, string? imageKey)
    {
        return new ThumbSubmenuComponent(text, factory) { ImageKey = imageKey };
    }

    // ImageKey is a CS2 econ texture path under panorama/images/econ/ (no extension); TypeLabel is
    // the caption drawn above the name. Only the CustomHUD renderer reads either; every other
    // renderer ignores raw nodes addressed elsewhere.
    private static MenuNode WithCustomHudMetadata(MenuNode node, string? imageKey, string? typeLabel)
    {
        const string customHudRendererId = "customhud.panorama";

        var hasType = !string.IsNullOrWhiteSpace(typeLabel);
        if (imageKey is null && !hasType) return node;

        // `;` separates directives in the payload, so a localized label must not carry one.
        var payload = string.Join(';', new[]
        {
            imageKey is null ? null : "image=" + imageKey,
            hasType ? "type=" + typeLabel!.Replace(';', ',') : null
        }.Where(p => p is not null));

        var raw = new MenuRawNode(customHudRendererId, payload);
        return node is MenuLineNode line
            ? line with { Children = [.. line.Children, raw] }
            : MenuLineNode.Of(node, raw);
    }

    private sealed class ThumbButtonComponent(string text, Func<MenuActionContext, ValueTask> onSelect)
        : ButtonComponent(text, onSelect)
    {
        public string? ImageKey { get; init; }
        public string? TypeLabel { get; init; }

        public override MenuNode Render(IMenuComponentRenderContext context)
            => WithCustomHudMetadata(base.Render(context), ImageKey, TypeLabel);
    }

    private sealed class ThumbSubmenuComponent(string text, Func<IMenuSession, IMenu?> factory)
        : SubmenuComponent(text, factory)
    {
        public string? ImageKey { get; init; }

        public override MenuNode Render(IMenuComponentRenderContext context)
            => WithCustomHudMetadata(base.Render(context), ImageKey, null);
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