using Microsoft.Extensions.Configuration;

namespace WeaponSkins.Configuration;

public class ItemPermissionConfig
{
    [ConfigurationKeyName("weapon_skins")]
    public string? WeaponSkins { get; set; }

    [ConfigurationKeyName("knife_skins")]
    public string? KnifeSkins { get; set; }

    [ConfigurationKeyName("glove_skins")]
    public string? GloveSkins { get; set; }

    [ConfigurationKeyName("stickers")]
    public string? Stickers { get; set; }

    [ConfigurationKeyName("keychains")]
    public string? Keychains { get; set; }
}
