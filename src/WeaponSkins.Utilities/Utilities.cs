namespace WeaponSkins;

public static class Utilities
{
    public static bool IsKnifeDefinitionIndex(int def)
    {
        return def is 42 or 59 or (>= 500 and < 600);
    }

    public static bool IsGloveDefinitionIndex(int def)
    {
        return def > 5000;
    }

    public static bool IsWeaponDefinitionIndex(int def)
    {
        return !IsKnifeDefinitionIndex(def) && !IsGloveDefinitionIndex(def) && def < 100;
    }

    // By the Items key (weapon_ak47); a gun not listed here, the Zeus or one the game adds later, is Other.
    public static WeaponCategory GetWeaponCategory(string weapon)
    {
        return weapon.ToLowerInvariant() switch
        {
            "weapon_ak47" or "weapon_aug" or "weapon_famas" or "weapon_galilar" or "weapon_m4a1"
                or "weapon_m4a1_silencer" or "weapon_sg556" => WeaponCategory.Rifles,
            "weapon_cz75a" or "weapon_deagle" or "weapon_elite" or "weapon_fiveseven" or "weapon_glock"
                or "weapon_hkp2000" or "weapon_p250" or "weapon_revolver" or "weapon_tec9"
                or "weapon_usp_silencer" => WeaponCategory.Pistols,
            "weapon_bizon" or "weapon_mac10" or "weapon_mp5sd" or "weapon_mp7" or "weapon_mp9"
                or "weapon_p90" or "weapon_ump45" => WeaponCategory.Smgs,
            "weapon_mag7" or "weapon_nova" or "weapon_sawedoff" or "weapon_xm1014" => WeaponCategory.Shotguns,
            "weapon_awp" or "weapon_g3sg1" or "weapon_scar20" or "weapon_ssg08" => WeaponCategory.Snipers,
            "weapon_m249" or "weapon_negev" => WeaponCategory.Heavy,
            _ => WeaponCategory.Other,
        };
    }

    // A category's row Tag, the end of its page id (weaponskins.weapons.<key>) and of its translation key.
    public static string Key(this WeaponCategory category)
    {
        return category.ToString().ToLowerInvariant();
    }
}