using System.Globalization;

using SwiftlyS2.Shared.Menu;
using SwiftlyS2.Shared.Menu.Components;

using WeaponSkins.Shared;

namespace WeaponSkins;

// Properties pages as CustomHUD's card; other renderers ignore the markers and list the same rows.
public partial class MenuService
{
    private const string CustomHudRendererId = "customhud.panorama";
    private const int MaxSeed = 999;

    // Appended into the component's own line: CustomHUD splits a value off its first text node.
    private static MenuNode MarkForCustomHud(MenuNode node, string payload)
    {
        var raw = new MenuRawNode(CustomHudRendererId, payload);
        return node is MenuLineNode line
            ? line with { Children = [.. line.Children, raw] }
            : MenuLineNode.Of(node, raw);
    }

    private TextComponent CreateHero(PlayerLocalizationService loc, string language, ushort definitionIndex, int paintkitIndex, bool glove = false)
    {
        var (weapon, item) = EconService.Items.FirstOrDefault(entry => entry.Value.Index == definitionIndex);
        if (item is null) return new HeroComponent(string.Empty);

        var name = EconService.GetLocalizedName(item.LocalizedNames, language);
        var paintkit = EconService.WeaponToPaintkits.TryGetValue(weapon, out var paintkits)
            ? paintkits.FirstOrDefault(p => p.Index == paintkitIndex)
            : null;
        if (paintkit is null)
        {
            // CS2 ships no render of a bare glove type.
            return new HeroComponent(name) { ImageKey = glove ? null : BaseWeaponImageKey(weapon) };
        }

        return new HeroComponent($"{name} | {EconService.GetLocalizedName(paintkit.LocalizedNames, language)}")
        {
            ImageKey = PaintkitImageKey(weapon, paintkit),
            Rarity = glove ? loc.MenuRarityExtraordinary : loc.MenuRarity(paintkit.Rarity.Name),
            Style = RarityStyle(paintkit.Rarity.Color.HexColor),
        };
    }

    private static void AddNametag(IMenuBuilder menu, PlayerLocalizationService loc, string? current, Action<string?> apply)
    {
        // A chat capture refuses an empty message, so clearing needs its own control.
        var input = new InputComponent(loc.MenuSkinPropertiesNametag, current ?? "", null, ctx => apply(ctx.NewValue))
        {
            MaxLength = 32,
            EmptyText = loc.MenuSkinPropertiesNametagNone
        };
        menu.Add(input);
        menu.Add(new InlineButtonComponent(loc.MenuSkinPropertiesNametagUnset, ctx =>
        {
            apply(null);
            Poke(ctx.Session, input, string.Empty);
            ctx.Session.Invalidate();
            return ValueTask.CompletedTask;
        })
        {
            VisibleWhen = session => !string.IsNullOrEmpty(input.GetValue(session))
        });
    }

    // Each control writes the other's state directly, so a change runs one OnChanged and applies once.
    private static void AddWear(IMenuBuilder menu, PlayerLocalizationService loc, float wear, Action<float> apply)
    {
        InputComponent exact = null!;
        var track = new WearTrackComponent(loc.MenuSkinPropertiesWear, wear, ctx =>
        {
            apply(ctx.NewValue);
            Poke(ctx.Session, exact, FormatWear(ctx.NewValue));
        });
        exact = new InputComponent(loc.MenuSkinPropertiesWear, FormatWear(wear),
            input => TryParseFloat(input, out var value) && value is >= 0f and <= 1f,
            ctx =>
            {
                if (!TryParseFloat(ctx.NewValue, out var value)) return;
                apply(value);
                Poke(ctx.Session, track, value);
            });
        menu.Add(exact);
        menu.Add(track);
    }

    private static string FormatWear(float wear) => wear.ToString("0.######", CultureInfo.InvariantCulture);

    private static InputComponent CreateSeedInput(PlayerLocalizationService loc, int seed, Action<int> apply)
    {
        var input = CreateIntInput(loc.MenuSkinPropertiesSeed, seed, value => value is >= 0 and <= MaxSeed, (_, value) => apply(value));
        // Four, not three: a seed of 1000 written by another tool still displays whole.
        input.MaxLength = 4;
        return input;
    }

    // Quality is one field: Poke shows the other switch off without its handler resetting the quality.
    private static void AddStattrak(IMenuBuilder menu,
        PlayerLocalizationService loc,
        int count,
        EconItemQuality quality,
        Action<int> setCount,
        Action<EconItemQuality> setQuality,
        bool withSouvenir)
    {
        menu.Add(CreateIntInput(loc.MenuSkinPropertiesStattrakCount(count), count, value => value >= 0, (_, value) => setCount(value)));

        ToggleComponent? souvenir = null;
        var stattrak = new InlineToggleComponent(loc.MenuSkinPropertiesStattrak, quality == EconItemQuality.StatTrak, ctx =>
        {
            if (ctx.NewValue && souvenir != null)
            {
                Poke(ctx.Session, souvenir, false);
            }

            setQuality(ctx.NewValue ? EconItemQuality.StatTrak : EconItemQuality.Normal);
        })
        {
            OnSymbol = loc.MenuOn,
            OffSymbol = loc.MenuOff
        };
        menu.Add(stattrak);

        if (!withSouvenir)
        {
            return;
        }

        souvenir = CreateToggle(loc.MenuSkinPropertiesSouvenir, quality == EconItemQuality.Souvenir, loc, ctx =>
        {
            if (ctx.NewValue)
            {
                Poke(ctx.Session, stattrak, false);
            }

            setQuality(ctx.NewValue ? EconItemQuality.Souvenir : EconItemQuality.Normal);
        });
        menu.Add(souvenir);
    }

    private static void Poke<T>(IMenuSession session, MenuValueComponent<T> component, T value)
    {
        var state = session.GetState<MenuValueState<T>>(component);
        state.Value = value;
        state.Initialised = true;
    }

    private sealed class HeroComponent(string text) : TextComponent(text)
    {
        public string? ImageKey { get; init; }
        public string? Rarity { get; init; }

        public override MenuNode Render(IMenuComponentRenderContext context)
        {
            var payload = "hero";
            if (ImageKey is not null) payload += ";image=" + ImageKey;
            // `;` separates directives in the payload, so a localized label must not carry one.
            if (!string.IsNullOrWhiteSpace(Rarity)) payload += ";type=" + Rarity.Replace(';', ',');
            return MarkForCustomHud(base.Render(context), payload);
        }
    }

    private sealed class InlineToggleComponent(string text, bool value, Action<MenuValueChangedContext<bool>> onChanged)
        : ToggleComponent(text, value, onChanged)
    {
        public override MenuNode Render(IMenuComponentRenderContext context)
            => MarkForCustomHud(base.Render(context), "inline");
    }

    private sealed class InlineButtonComponent(string text, Func<MenuActionContext, ValueTask> onSelect)
        : ButtonComponent(text, onSelect)
    {
        public override MenuNode Render(IMenuComponentRenderContext context)
            => MarkForCustomHud(base.Render(context), "inline");
    }

    // Coarse on purpose: the exact field above it takes any value.
    private sealed class WearTrackComponent : SliderComponent
    {
        public WearTrackComponent(string text, float wear, Action<MenuValueChangedContext<float>> onChanged)
            : base(text, 0f, 1f, wear, 0.01f, onChanged)
        {
            ValueFormat = "F2";
            WrapAround = false;
        }

        public override MenuNode Render(IMenuComponentRenderContext context)
            => MarkForCustomHud(base.Render(context), "inline;track=wear");
    }
}