using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

using WeaponSkins.Econ;
using WeaponSkins.Shared;

namespace WeaponSkins;

public partial class MenuService
{
    private Dictionary<int, KeychainDefinition> KeychainDefinitions { get; set; } = new();
    private Dictionary<ulong /* steamid */, WeaponSkinData> _keychainOperatingWeaponSkins = new();

    private string? GetKeychainName(KeychainData data,
        string language)
    {
        if (data == null) return null;
        if (data.Id == 0) return null;
        if (!KeychainDefinitions.TryGetValue(data.Id, out var definition))
        {
            definition = EconService.Keychains.Where(k => k.Value.Index == data.Id).FirstOrDefault().Value;
            KeychainDefinitions[data.Id] = definition;
        }

        return definition.LocalizedNames[language];
    }

    private IMenuAPI BuildKeychainMenuBySlot(IPlayer player,
        int slot,
        string language,
        string title)
    {
        var main = Core.MenusAPI.CreateBuilder();

        main.Design.SetMenuTitle(title);

        var resetOption = new ButtonMenuOption(LocalizationService[player].MenuReset);
        resetOption.Click += (_,
            args) =>
        {
            if (!_keychainOperatingWeaponSkins.TryGetValue(args.Player.SteamID, out var dataInHand))
            {
                return ValueTask.CompletedTask;
            }

            Api.UpdateWeaponSkin(args.Player.SteamID, args.Player.Controller.Team, dataInHand.DefinitionIndex, skin =>
            {
                skin.SetKeychain(slot, null);
            }, true);
            return ValueTask.CompletedTask;
        };
        main.AddOption(resetOption);

        var sorted = EconService.Keychains.OrderByDescending(k => k.Value.Rarity.Id).ToList();
        foreach (var (index, keychain) in sorted)
        {
            main.Design.SetMenuTitle(keychain.LocalizedNames[language]);
            var option = new ButtonMenuOption(HtmlGradient.GenerateGradientText(keychain.LocalizedNames[language],
                keychain.Rarity.Color.HexColor));
            option.Click += (_,
                args) =>
            {
                if (!_keychainOperatingWeaponSkins.TryGetValue(args.Player.SteamID, out var dataInHand))
                {
                    return ValueTask.CompletedTask;
                }

                Api.UpdateWeaponSkin(args.Player.SteamID, args.Player.Controller.Team, dataInHand.DefinitionIndex,
                    skin =>
                    {
                        var keychainData = new KeychainData { Id = keychain.Index, };
                        switch (slot)
                        {
                            case 0: skin.Keychain0 = keychainData; break;
                        }
                    }, true);
                return ValueTask.CompletedTask;
            };
            main.AddOption(option);
        }

        return main.Build();
    }

    public IMenuAPI BuildKeychainMenu(IPlayer player)
    {
        var language = GetLanguage(player);

        var main = Core.MenusAPI.CreateBuilder();
        main.Design.SetMenuTitle(LocalizationService[player].MenuTitleKeychains);
        for (int i = 0; i < 1; i++)
        {
            var title =
                $"[{i + 1}] Slot";
            var slot = i;
            main.AddOption(new SubmenuMenuOption(title,
                () => Task.FromResult(BuildKeychainMenuBySlot(player, slot, language, title))));
        }

        var menu = main.Build();
        return menu;
    }

    public IMenuOption GetKeychainMenuSubmenuOption(IPlayer player)
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
        return new SubmenuMenuOption(LocalizationService[player].MenuTitleKeychains,
            () => Task.FromResult(BuildKeychainMenu(player)));
    }
}
