using System.Collections.Concurrent;

using SwiftlyS2.Shared.Menu;
using SwiftlyS2.Shared.Menu.Components;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Econ;
using WeaponSkins.Shared;

namespace WeaponSkins;

public partial class MenuService
{
    private ConcurrentDictionary<int, KeychainDefinition> KeychainDefinitions { get; set; } = new();
    private readonly ConcurrentDictionary<ulong /* steamid */, WeaponSkinData> _keychainOperatingWeaponSkins = new();

    private string? GetKeychainName(KeychainData data,
        string language)
    {
        if (data == null) return null;
        if (data.Id == 0) return null;
        if (!KeychainDefinitions.TryGetValue(data.Id, out var definition))
        {
            definition = EconService.Keychains.Where(k => k.Value.Index == data.Id).FirstOrDefault().Value;
            if (definition == null) return null;
            KeychainDefinitions[data.Id] = definition;
        }

        return EconService.GetLocalizedName(definition.LocalizedNames, language);
    }

    private void UpdateOperatingKeychain(IPlayer player, int slot, KeychainData? keychain)
    {
        if (!_keychainOperatingWeaponSkins.TryGetValue(player.SteamID, out var dataInHand))
        {
            return;
        }

        Api.UpdateWeaponSkin(player.SteamID, player.Controller.Team, dataInHand.DefinitionIndex,
            skin => skin.SetKeychain(slot, keychain), true);
    }

    private IMenu BuildKeychainMenuBySlot(IPlayer player,
        int slot,
        string language,
        string title)
    {
        var main = Core.Menu.CreateMenu($"weaponskins.keychains.slot{slot}")
            .WithTitle(title);

        main.Add(CreateButton(LocalizationService[player].MenuReset, p => UpdateOperatingKeychain(p, slot, null)));

        foreach (var (_, keychain) in EconService.Keychains.OrderByDescending(k => k.Value.Rarity.Id))
        {
            var keychainIndex = keychain.Index;
            main.Add(CreateRarityButton(
                EconService.GetLocalizedName(keychain.LocalizedNames, language),
                keychain.Rarity.Color.HexColor,
                p => UpdateOperatingKeychain(p, slot, new KeychainData { Id = keychainIndex })));
        }

        return main.Build();
    }

    public IMenu BuildKeychainMenu(IPlayer player)
    {
        var main = Core.Menu.CreateMenu("weaponskins.keychains")
            .WithTitle(LocalizationService[player].MenuTitleKeychains);

        for (int i = 0; i < 1; i++)
        {
            var title = $"[{i + 1}] Slot";
            var slot = i;
            main.Add(new SubmenuComponent(title,
                _ => BuildKeychainMenuBySlot(player, slot, player.PlayerLanguage.Value, title)));
        }

        return main.Build();
    }

    public IMenuComponent GetKeychainMenuSubmenuOption(IPlayer player)
    {
        if (!ItemPermissionService.CanUseKeychains(player.SteamID))
        {
            return CreateDisabledOption(LocalizationService[player].MenuTitleKeychains);
        }

        if (!TryGetWeaponDataInHand(player, out var dataInHand))
        {
            return CreateDisabledOption(LocalizationService[player].MenuTitleKeychains);
        }

        _keychainOperatingWeaponSkins[player.SteamID] = dataInHand;
        return new SubmenuComponent(LocalizationService[player].MenuTitleKeychains,
            _ => BuildKeychainMenu(player));
    }
}