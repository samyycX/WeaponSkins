using Microsoft.Extensions.Configuration;

namespace WeaponSkins.Configuration;

public class MainConfigModel
{
    public string StorageBackend { get; set; } = "inherit";

    public string InventoryUpdateBackend { get; set; } = "hook";

    public bool SyncFromDatabaseWhenPlayerJoin { get; set; } = false;

    public List<string> ItemLanguages { get; set; } = [];

    // Core also caps a page at MenuLayout.MaxVisibleLines in menus.jsonc, so raise that to match.
    public int MenuItemsPerPage { get; set; } = 12;

    public ItemPermissionConfig ItemPermissions { get; set; } = new();
}