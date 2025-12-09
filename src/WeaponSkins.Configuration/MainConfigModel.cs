using Microsoft.Extensions.Configuration;

namespace WeaponSkins.Configuration;

public class MainConfigModel
{
    [ConfigurationKeyName("storage_backend")]
    public string StorageBackend { get; set; } = "inherit";

    [ConfigurationKeyName("item_permissions")]
    public ItemPermissionConfig ItemPermissions { get; set; } = new();
}