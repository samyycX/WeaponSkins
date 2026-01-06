using Microsoft.Extensions.Configuration;

namespace WeaponSkins.Configuration;

public class ItemPermissionConfig
{
    public string WeaponSkins { get; set; } = "";

    public string KnifeSkins { get; set; } = "";

    public string GloveSkins { get; set; } = "";

    public string Stickers { get; set; } = "";

    public string Keychains { get; set; } = "";

    public string Agents { get; set; } = "";
}
