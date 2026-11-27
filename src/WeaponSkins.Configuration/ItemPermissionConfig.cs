using System.ComponentModel;

using Microsoft.Extensions.Configuration;

namespace WeaponSkins.Configuration;

public class ItemPermissionConfig
{
    [Description("Weapon skins.")]
    public ItemPermission WeaponSkins { get; set; } = new();

    [Description("Knives.")]
    public ItemPermission KnifeSkins { get; set; } = new();

    [Description("Gloves.")]
    public ItemPermission GloveSkins { get; set; } = new();

    [Description("Stickers.")]
    public ItemPermission Stickers { get; set; } = new();

    [Description("Charms.")]
    public ItemPermission Keychains { get; set; } = new();

    [Description("Agents.")]
    public ItemPermission Agents { get; set; } = new();

    [Description("Music kits.")]
    public ItemPermission MusicKits { get; set; } = new();

    [Description("Graffiti.")]
    public ItemPermission Graffiti { get; set; } = new();

    [Description("Agent patches.")]
    public ItemPermission Patches { get; set; } = new();

    [Description("Pins.")]
    public ItemPermission Pins { get; set; } = new();

    [Description("Name tags.")]
    public ItemPermission Nametag { get; set; } = new();

    [Description("Changing the wear.")]
    public ItemPermission Wear { get; set; } = new();

    [Description("Changing the pattern (seed).")]
    public ItemPermission Seed { get; set; } = new();

    [Description("StatTrak™.")]
    public ItemPermission StatTrak { get; set; } = new();

    [Description("Souvenir.")]
    public ItemPermission Souvenir { get; set; } = new();

    // The binder skips a string where an object is expected, so an item given as just its permission, the form before 0.11.0, would be open to everyone.
    public void ReadPlainPermissions(IConfiguration section)
    {
        foreach (var property in typeof(ItemPermissionConfig).GetProperties())
        {
            if (property.PropertyType == typeof(ItemPermission) && section[property.Name] is { } permission)
            {
                property.SetValue(this, new ItemPermission { Permission = permission });
            }
        }
    }
}
