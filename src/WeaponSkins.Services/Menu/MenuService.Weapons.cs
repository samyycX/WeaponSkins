using SwiftlyS2.Shared.Menu;
using SwiftlyS2.Shared.Menu.Components;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins;

public partial class MenuService
{
    public IMenu BuildWeaponSkinMenu(IPlayer player)
    {
        var loc = LocalizationService[player];
        var categories = new Dictionary<WeaponCategory, List<(ItemDefinition Item, List<PaintkitDefinition> Paintkits)>>();
        var categoryByIndex = new Dictionary<int, WeaponCategory>();
        foreach (var (weapon, paintkits) in EconService.WeaponToPaintkits)
        {
            var item = EconService.Items[weapon];
            if (!Utilities.IsWeaponDefinitionIndex(item.Index))
            {
                continue;
            }

            var category = Utilities.GetWeaponCategory(weapon);
            categories.GetOrAdd(category, () => []).Add((item, paintkits));
            categoryByIndex[item.Index] = category;
        }

        var main = CreateMenu("weaponskins.weapons")
            .WithTitle(loc.MenuTitleSkins);

        foreach (var category in Enum.GetValues<WeaponCategory>())
        {
            if (!categories.TryGetValue(category, out var weapons))
            {
                continue;
            }

            var key = category.Key();
            var title = loc.MenuSkinsCategory(key);
            var menuId = "weaponskins.weapons." + key;
            // The page's id as the component id, as CreateCategory does, so a tab strip can mark the open one.
            main.Add(new SubmenuComponent(title, _ => BuildWeaponCategoryMenu(player, menuId, title, weapons), menuId) { Tag = key });
        }

        var menu = main.Build();
        TrackFocus(menu, p => TryGetWeaponInHand(p, out var weapon)
                              && categoryByIndex.TryGetValue(weapon.AttributeManager.Item.ItemDefinitionIndex, out var category)
            ? category.Key()
            : null);
        return menu;
    }

    private IMenu BuildWeaponCategoryMenu(IPlayer player,
        string menuId,
        string title,
        List<(ItemDefinition Item, List<PaintkitDefinition> Paintkits)> weapons)
    {
        var main = CreateMenu(menuId)
            .WithTitle(title);

        foreach (var (item, paintkits) in weapons)
        {
            var itemName = EconService.GetLocalizedName(item.LocalizedNames, player.PlayerLanguage.Value);
            var submenuOption = CreateSubmenu(itemName, _ =>
            {
                var skinMenu = CreateMenu("weaponskins.weapons.paintkits").WithTitle(itemName);
                skinMenu.Add(CreateButton(LocalizationService[player].MenuReset,
                    p => Api.ResetWeaponSkin(p.SteamID, p.Controller.Team, (ushort)item.Index, true)));

                foreach (var paintkit in paintkits.OrderByDescending(p => p.Rarity.Id))
                {
                    var option = CreateRarityButton(
                        EconService.GetLocalizedName(paintkit.LocalizedNames, player.PlayerLanguage.Value),
                        paintkit.Rarity.Color.HexColor,
                        p => Api.UpdateWeaponSkin(p.SteamID, p.Controller.Team, (ushort)item.Index, skin => skin.Paintkit = paintkit.Index, true));
                    option.Tag = paintkit.Index;
                    skinMenu.Add(option);
                }

                var built = skinMenu.Build();
                TrackFocus(built, p => TryGetWeaponDataInHand(p, out var inHand) ? inHand.Paintkit : null);
                return built;
            });

            submenuOption.Tag = (ushort)item.Index;
            main.Add(submenuOption);
        }

        var menu = main.Build();
        TrackFocus(menu, p => TryGetWeaponInHand(p, out var weapon) && Utilities.IsWeaponDefinitionIndex(weapon.AttributeManager.Item.ItemDefinitionIndex) ? (ushort)weapon.AttributeManager.Item.ItemDefinitionIndex : null);
        return menu;
    }
}