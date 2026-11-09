using SwiftlyS2.Shared.Menu;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins;

public partial class MenuService
{
    private IMenu BuildPinMenu(IPlayer player)
    {
        var menu = CreateMenu("weaponskins.pins")
            .WithTitle(LocalizationService[player].MenuTitlePins);

        menu.Add(CreateButton(LocalizationService[player].MenuReset, p => Api.ResetPin(p.SteamID)));

        foreach (var pin in EconService.Pins.Values.OrderBy(pin => pin.Index))
        {
            var pinName = EconService.GetLocalizedName(pin.LocalizedNames, player.PlayerLanguage.Value);
            var truncatedName = pinName.Length > 30 ? pinName.Substring(0, 27) + "..." : pinName;
            var index = pin.Index;

            var button = CreateRarityButton(truncatedName, pin.Rarity.Color.HexColor, p => Api.SetPin(p.SteamID, index));
            // A row's Tag is the item it stands for, so any renderer can identify it.
            button.Tag = pin;
            menu.Add(button);
        }

        var built = menu.Build();
        TrackFocus(built, SelectedPin);
        return built;
    }

    private PinDefinition? SelectedPin(IPlayer player)
    {
        return DataService.PinDataService.TryGetPin(player.SteamID, out var index)
            ? EconService.Pins.Values.FirstOrDefault(pin => pin.Index == index)
            : null;
    }
}
