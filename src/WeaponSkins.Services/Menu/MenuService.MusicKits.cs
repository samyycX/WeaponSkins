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

        var typeLabel = LocalizationService[player].MenuTypeMusicKit;
        foreach (var musicKit in EconService.MusicKits.Values.OrderBy(mk => mk.Index))
        {
            var musicKitName = EconService.GetLocalizedName(musicKit.LocalizedNames, player.PlayerLanguage.Value);
            var truncatedName = musicKitName.Length > 30 ? musicKitName.Substring(0, 27) + "..." : musicKitName;
            var index = musicKit.Index;

            menu.Add(CreateButton(truncatedName, p => Api.SetMusicKit(p.SteamID, index),
                EconService.GetImageKey("music", musicKit.Name), typeLabel,
                session => DataService.MusicKitDataService.TryGetMusicKit(session.Player.SteamID, out var current) && current == index));
        }

        return menu.Build();
    }
}