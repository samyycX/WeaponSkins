using System.Text;

using WeaponSkins.Shared;

namespace WeaponSkins.Services;

// A client caches every finish it generates, for any weapon it sees, by paint, seed and wear printed to three
// decimals, and the cached finish keeps the stickers it was built with: stickers added to a finish already
// drawn never show in hand. So each sticker layout claims its own 0.001 wear step, shared by all players.
internal static class FinishCacheKeys
{
    private const int Steps = 1000;
    private static readonly float[] ExteriorCeilings = [0.07f, 0.15f, 0.38f, 0.45f];
    private static readonly Dictionary<(int Paint, int Seed, int Step), string> Claims = new();

    public static float Resolve(WeaponSkinData skin)
    {
        var wear = skin.PaintkitWear;
        if (wear is not (>= 0f and <= 1f)) return wear;
        var layout = Layout(skin);
        // Staying inside the exterior keeps the condition the player picked.
        return Claim(skin, wear, layout, sameExterior: true) ?? Claim(skin, wear, layout, sameExterior: false) ?? wear;
    }

    // Clients drop their generated finishes on map change.
    public static void Clear() => Claims.Clear();

    private static float? Claim(WeaponSkinData skin,
        float wear,
        string layout,
        bool sameExterior)
    {
        var own = Step(wear);
        for (var i = 0; i <= 2 * Steps; i++)
        {
            var step = own + (i % 2 == 1 ? (i + 1) / 2 : -(i / 2));
            if (step is < 0 or > Steps) continue;
            var candidate = InStep(wear, own, step);
            if (sameExterior && Exterior(candidate) != Exterior(wear)) continue;
            var key = (skin.Paintkit, skin.PaintkitSeed, step);
            if (Claims.TryGetValue(key, out var owner) && owner != layout) continue;
            Claims[key] = layout;
            return candidate;
        }
        return null;
    }

    private static string Layout(WeaponSkinData skin)
    {
        var layout = new StringBuilder().Append(skin.DefinitionIndex);
        WeaponAppearance.WriteStickers(skin,
            (name, value) => layout.Append('|').Append(name).Append('=').Append(BitConverter.SingleToInt32Bits(value)));
        return layout.ToString();
    }

    private static int Step(float wear) => (int)Math.Round(wear * 1000d, MidpointRounding.ToEven);

    private static float InStep(float wear,
        int own,
        int step)
    {
        if (step == own) return wear;
        var shifted = (float)(wear + (step - own) / 1000d);
        // A wear of exactly 0 is avoided, as the stored default 0.000001 is.
        return shifted > 0f && Step(shifted) == step ? shifted : Math.Max(step / 1000f, 0.000001f);
    }

    private static int Exterior(float wear)
    {
        var exterior = 0;
        while (exterior < ExteriorCeilings.Length && wear >= ExteriorCeilings[exterior]) exterior++;
        return exterior;
    }
}
