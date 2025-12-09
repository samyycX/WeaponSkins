using System.Collections.Generic;

using Microsoft.Extensions.Configuration;

namespace WeaponSkins.Configuration;

public class ItemPermissionConfig
{
    [ConfigurationKeyName("stickers")]
    public Dictionary<int, string> Stickers { get; set; } = new();

    [ConfigurationKeyName("keychains")]
    public Dictionary<int, string> Keychains { get; set; } = new();
}
