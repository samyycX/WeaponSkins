using SwiftlyS2.Shared.Menu;
using SwiftlyS2.Shared.Menu.Components;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins;

public partial class MenuService
{
    // Weapons carry one charm.
    private const int KeychainSlot = 0;

    public IMenu BuildKeychainMenu(IPlayer player,
        Team team,
        ushort definition)
    {
        var loc = LocalizationService[player];
        var language = player.PlayerLanguage.Value;
        var main = CreateMenu("weaponskins.keychains")
            .WithTitle(loc.MenuTitleKeychains);

        void Apply(KeychainData? keychain) =>
            Api.UpdateWeaponSkin(player.SteamID, team, definition, skin => skin.SetKeychain(KeychainSlot, keychain), true);

        main.Add(CreateButton(loc.MenuReset, _ => Apply(null)));
        main.Add(new SubmenuComponent(loc.MenuTitleKeychainProperties,
            _ => BuildKeychainPropertiesMenu(player, team, definition, KeychainSlot))
        {
            VisibleWhen = _ => Api.TryGetWeaponSkin(player.SteamID, team, definition, out var skin) && skin.HasKeychain(KeychainSlot)
        });

        // A row's Tag is the item it stands for, so any renderer can identify it.
        ButtonComponent CreateCharmButton(KeychainDefinition keychain)
        {
            var keychainIndex = keychain.Index;
            var button = CreateRarityButton(
                EconService.GetLocalizedName(keychain.LocalizedNames, language),
                keychain.Rarity.Color.HexColor,
                _ => Apply(new KeychainData { Id = keychainIndex }));
            button.Tag = keychain;
            return button;
        }

        var grouped = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (_, collection) in EconService.KeychainCollections)
        {
            var collectionName = EconService.GetLocalizedName(collection.LocalizedNames, language);
            grouped.UnionWith(collection.Keychains.Select(keychain => keychain.Name));
            var submenu = CreateSubmenu(collectionName, _ =>
            {
                var collectionMenu = CreateMenu("weaponskins.keychains.collection")
                    .WithTitle(collectionName);

                foreach (var keychain in collection.Keychains.OrderByDescending(k => k.Rarity.Id))
                {
                    collectionMenu.Add(CreateCharmButton(keychain));
                }

                return collectionMenu.Build();
            });
            // The name, not the definition: KeychainCollectionDefinition is not part of WeaponSkins.Shared.
            submenu.Tag = collection.Name;
            main.Add(submenu);
        }

        // A charm no collection lists stays reachable here.
        foreach (var (_, keychain) in EconService.Keychains.OrderByDescending(k => k.Value.Rarity.Id))
        {
            if (!grouped.Contains(keychain.Name))
            {
                main.Add(CreateCharmButton(keychain));
            }
        }

        return main.Build();
    }
}