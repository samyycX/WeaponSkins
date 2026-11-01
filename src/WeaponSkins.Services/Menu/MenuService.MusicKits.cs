using SwiftlyS2.Shared.Menu;
using SwiftlyS2.Shared.Menu.Components;
using SwiftlyS2.Shared.Players;

namespace WeaponSkins;

public partial class MenuService
{
    private IMenuComponent GetMusicKitMenuSubmenuOption(IPlayer player)
    {
        return new SubmenuComponent(LocalizationService[player].MenuTitleMusicKits, _ =>
        {
            var menu = Core.Menu.CreateMenu("weaponskins.musickits")
                .WithTitle(LocalizationService[player].MenuTitleMusicKits);

            menu.Add(CreateButton(LocalizationService[player].MenuReset, p => Api.ResetMusicKit(p.SteamID)));

            foreach (var musicKit in EconService.MusicKits.Values.OrderBy(mk => mk.Index))
            {
                var musicKitName = EconService.GetLocalizedName(musicKit.LocalizedNames, player.PlayerLanguage.Value);
                var truncatedName = musicKitName.Length > 30 ? musicKitName.Substring(0, 27) + "..." : musicKitName;
                var index = musicKit.Index;

                menu.Add(CreateButton(truncatedName, p => Api.SetMusicKit(p.SteamID, index)));
            }

            return menu.Build();
        });
    }
}