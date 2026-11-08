using Microsoft.Extensions.Configuration;

namespace WeaponSkins.Configuration;

public class MainConfigModel
{
    public string StorageBackend { get; set; } = "inherit";

    public string InventoryUpdateBackend { get; set; } = "hook";

    public bool SyncFromDatabaseWhenPlayerJoin { get; set; } = false;

    public List<string> ItemLanguages { get; set; } = [];

    public int GraffitiCooldown { get; set; } = 30;

    public bool ReplaceNativeGraffiti { get; set; } = true;

    public ItemPermissionConfig ItemPermissions { get; set; } = new();
}
