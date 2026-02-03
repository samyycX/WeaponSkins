using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;

namespace WeaponSkins;

public partial class MenuService
{
    private IMenuOption GetMusicKitMenuSubmenuOption(IPlayer player)
    {
        var language = GetLanguage(player);
        var option = new SubmenuMenuOption(LocalizationService[player].MenuTitleMusicKits, () =>
        {
            var menu = Core.MenusAPI.CreateBuilder();
            menu.Design.SetMenuTitle(LocalizationService[player].MenuTitleMusicKits);

            var resetOption = new ButtonMenuOption(LocalizationService[player].MenuReset);
            resetOption.Click += (_, args) =>
            {
                Api.ResetMusicKit(player.SteamID);
                return ValueTask.CompletedTask;
            };
            menu.AddOption(resetOption);

            foreach (var musicKit in EconService.MusicKits.Values.OrderBy(mk => mk.Index))
            {
                var musicKitName = musicKit.LocalizedNames.TryGetValue(language, out var localized)
                    ? localized
                    : musicKit.LocalizedNames.GetValueOrDefault("english") ?? musicKit.Name;

                var truncatedName = musicKitName.Length > 30 ? musicKitName.Substring(0, 27) + "..." : musicKitName;
                var index = musicKit.Index;
                
                var submenuOption = new SubmenuMenuOption(truncatedName, () =>
                {
                    var detailMenu = Core.MenusAPI.CreateBuilder();
                    detailMenu.Design.SetMenuTitle(musicKitName);
                    
                    var selectOption = new ButtonMenuOption(LocalizationService[player].MenuReset.Replace("Reset", "Select"));
                    selectOption.Click += (_, args) =>
                    {
                        Api.SetMusicKit(player.SteamID, index);
                        return ValueTask.CompletedTask;
                    };
                    detailMenu.AddOption(selectOption);
                    
                    return detailMenu.Build();
                });
                
                menu.AddOption(submenuOption);
            }

            return menu.Build();
        });

        return option;
    }
}
