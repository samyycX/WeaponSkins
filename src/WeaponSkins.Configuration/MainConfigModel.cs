using System.ComponentModel;

using Microsoft.Extensions.Configuration;

namespace WeaponSkins.Configuration;

public class MainConfigModel
{
    [Description("Where skins are saved: \"inherit\" (SwiftlyS2's database) or \"sqlite\" (a local file). \"external\" is only for plugins that store skins themselves.")]
    public string StorageBackend { get; set; } = "inherit";

    [Description("Leave it as \"hook\".")]
    public string InventoryUpdateBackend { get; set; } = "hook";

    [Description("true reloads a player's skins from the database each time they join. Turn it on if several servers or a website use the same database.")]
    public bool SyncFromDatabaseWhenPlayerJoin { get; set; } = false;

    [Description("Languages for item names, e.g. [\"en\", \"ru\"]; empty loads all of them. Players with any other language see the first one. Needs a restart.")]
    public List<string> ItemLanguages { get; set; } = [];

    [Description("Seconds a player waits between graffiti sprayed with !spray. 0 turns the wait off.")]
    public int GraffitiCooldown { get; set; } = 30;

    [Description("true shows a player's !ws graffiti when they spray with the game's own graffiti key. false leaves those sprays as the game makes them.")]
    public bool ReplaceNativeGraffiti { get; set; } = true;

    [Description("true also sprays a player's graffiti when they press the use key (E), like !spray and with the same GraffitiCooldown.")]
    public bool SprayWithUseKey { get; set; } = false;

    [Description("true hides the menu options a player can't use (no permission, or \"Enabled\": false) instead of greying them out.")]
    public bool HideWhenDisabled { get; set; } = false;

    [Description("Who can use each feature: \"Enabled\": false turns it off for everyone, and \"Permission\": \"vip\" makes it VIP-only. An empty permission (\"\") means everyone.")]
    public ItemPermissionConfig ItemPermissions { get; set; } = new();
}
