using System.Globalization;

using SwiftlyS2.Shared.Menu;
using SwiftlyS2.Shared.Menu.Components;

using WeaponSkins.Shared;

namespace WeaponSkins;

// Rows shared by the weapon, knife and glove properties pages.
public partial class MenuService
{
    private const int MaxSeed = 999;

    private TextComponent CreateHero(string language, ushort definitionIndex, int paintkitIndex)
    {
        var (weapon, item) = EconService.Items.FirstOrDefault(entry => entry.Value.Index == definitionIndex);
        if (item is null) return new TextComponent(string.Empty);

        var name = EconService.GetLocalizedName(item.LocalizedNames, language);
        var paintkit = EconService.WeaponToPaintkits.TryGetValue(weapon, out var paintkits)
            ? paintkits.FirstOrDefault(p => p.Index == paintkitIndex)
            : null;
        if (paintkit is null) return new TextComponent(name);

        return new TextComponent($"{name} | {EconService.GetLocalizedName(paintkit.LocalizedNames, language)}")
        {
            Style = RarityStyle(paintkit.Rarity.Color.HexColor),
        };
    }

    private void AddNametag(IMenuBuilder menu, PlayerLocalizationService loc, bool enabled, string? current, Action<string?> apply)
    {
        // A chat capture refuses an empty message, so clearing needs its own control.
        var input = new InputComponent(loc.MenuSkinPropertiesNametag, current ?? "", null, ctx => apply(ctx.NewValue))
        {
            MaxLength = 32,
            EmptyText = loc.MenuSkinPropertiesNametagNone,
            Enabled = enabled,
            Visible = Shown(enabled)
        };
        menu.Add(input);
        menu.Add(new ButtonComponent(loc.MenuSkinPropertiesNametagUnset, ctx =>
        {
            apply(null);
            Poke(ctx.Session, input, string.Empty);
            ctx.Session.Invalidate();
            return ValueTask.CompletedTask;
        })
        {
            Visible = input.Visible,
            VisibleWhen = session => !string.IsNullOrEmpty(input.GetValue(session))
        });
    }

    // Each control writes the other's state directly, so a change runs one OnChanged and applies once.
    private void AddWear(IMenuBuilder menu, PlayerLocalizationService loc, bool enabled, float wear, Action<float> apply)
    {
        InputComponent exact = null!;
        // Coarse on purpose: the exact field above it takes any value.
        var track = new SliderComponent(loc.MenuSkinPropertiesWear, 0f, 1f, wear, 0.01f, ctx =>
        {
            apply(ctx.NewValue);
            Poke(ctx.Session, exact, FormatWear(ctx.NewValue));
        })
        {
            ValueFormat = "F2",
            WrapAround = false,
            Enabled = enabled,
            Visible = Shown(enabled)
        };
        exact = new InputComponent(loc.MenuSkinPropertiesWear, FormatWear(wear),
            input => TryParseFloat(input, out var value) && value is >= 0f and <= 1f,
            ctx =>
            {
                if (!TryParseFloat(ctx.NewValue, out var value)) return;
                apply(value);
                Poke(ctx.Session, track, value);
            })
        {
            Enabled = enabled,
            Visible = Shown(enabled)
        };
        menu.Add(exact);
        menu.Add(track);
    }

    private static string FormatWear(float wear) => wear.ToString("0.######", CultureInfo.InvariantCulture);

    private InputComponent CreateSeedInput(PlayerLocalizationService loc, bool enabled, int seed, Action<int> apply)
    {
        var input = CreateIntInput(loc.MenuSkinPropertiesSeed, seed, value => value is >= 0 and <= MaxSeed, (_, value) => apply(value));
        // Four, not three: a seed of 1000 written by another tool still displays whole.
        input.MaxLength = 4;
        input.Enabled = enabled;
        input.Visible = Shown(enabled);
        return input;
    }

    // Quality is one field: Poke shows the other switch off without its handler resetting the quality.
    private void AddStattrak(IMenuBuilder menu,
        PlayerLocalizationService loc,
        bool stattrakEnabled,
        bool souvenirEnabled,
        int count,
        EconItemQuality quality,
        Action<int> setCount,
        Action<EconItemQuality> setQuality,
        bool withSouvenir)
    {
        var counter = CreateIntInput(loc.MenuSkinPropertiesStattrakCount(count), count, value => value >= 0, (_, value) => setCount(value));
        counter.Enabled = stattrakEnabled;
        counter.Visible = Shown(stattrakEnabled);
        menu.Add(counter);

        ToggleComponent? souvenir = null;
        var stattrak = CreateToggle(loc.MenuSkinPropertiesStattrak, quality == EconItemQuality.StatTrak, loc, ctx =>
        {
            if (ctx.NewValue && souvenir != null)
            {
                Poke(ctx.Session, souvenir, false);
            }

            setQuality(ctx.NewValue ? EconItemQuality.StatTrak : EconItemQuality.Normal);
        });
        stattrak.Enabled = stattrakEnabled;
        stattrak.Visible = Shown(stattrakEnabled);
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
        souvenir.Enabled = souvenirEnabled;
        souvenir.Visible = Shown(souvenirEnabled);
        menu.Add(souvenir);
    }

    private static void Poke<T>(IMenuSession session, MenuValueComponent<T> component, T value)
    {
        var state = session.GetState<MenuValueState<T>>(component);
        state.Value = value;
        state.Initialised = true;
    }
}
