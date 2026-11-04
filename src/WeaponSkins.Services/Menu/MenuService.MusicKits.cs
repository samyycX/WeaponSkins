using SwiftlyS2.Shared.Menu;
using SwiftlyS2.Shared.Players;

namespace WeaponSkins;

public partial class MenuService
{
    private IMenu BuildMusicKitMenu(IPlayer player)
    {
        var menu = CreateMenu("weaponskins.musickits")
            .WithTitle(LocalizationService[player].MenuTitleMusicKits);

        menu.Add(CreateButton(LocalizationService[player].MenuReset, p => Api.ResetMusicKit(p.SteamID)));

        foreach (var musicKit in EconService.MusicKits.Values.OrderBy(mk => mk.Index))
        {
            var musicKitName = EconService.GetLocalizedName(musicKit.LocalizedNames, player.PlayerLanguage.Value);
            var truncatedName = musicKitName.Length > 30 ? musicKitName.Substring(0, 27) + "..." : musicKitName;
            var index = musicKit.Index;

            var button = CreateButton(truncatedName, p => Api.SetMusicKit(p.SteamID, index));
            // A row's Tag is the item it stands for, so any renderer can identify it.
            button.Tag = musicKit;
            menu.Add(button);
        }

        return menu.Build();
    }
}