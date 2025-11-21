using WeaponSkins.Services;
using WeaponSkins.Shared;

namespace WeaponSkins;

public class WeaponSkinAPI : IWeaponSkinAPI
{
    private InventoryUpdateService InventoryUpdateService { get; init; }

    public WeaponSkinAPI(InventoryUpdateService inventoryUpdateService)
    {
        InventoryUpdateService = inventoryUpdateService;
    }

    public void UpdateWeaponSkins(IEnumerable<WeaponSkinData> skins)
    {
        InventoryUpdateService.UpdateWeaponSkins(skins);
    }

    public void UpdateKnifeSkins(IEnumerable<KnifeSkinData> knives)
    {
        InventoryUpdateService.UpdateKnifeSkins(knives);
    }
}