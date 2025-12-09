using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;

namespace WeaponSkins;

public partial class MenuService
{
    private ValueTask OnWeaponSkinOptionClick(object? sender,
        MenuOptionClickEventArgs args)
    {
        Core.Scheduler.NextWorldUpdate(() =>
        {
            if (TryGetWeaponDataInHand(args.Player, out var weaponInHand))
            {
                var menu = Core.MenusAPI.GetCurrentMenu(args.Player);
                menu.MoveToOption(args.Player,
                    menu.Options.FirstOrDefault(o =>
                        o.Tag is int tag &&
                        tag == weaponInHand.Paintkit)!);
            }
        });
        return ValueTask.CompletedTask;
    }

    public IMenuAPI BuildWeaponSkinMenu(IPlayer player)
    {
        var language = GetLanguage(player);

        var main = Core.MenusAPI.CreateBuilder();
        main.Design.SetMenuTitle(LocalizationService[player].MenuTitleSkins);

        foreach (var (weapon, paintkits) in EconService.WeaponToPaintkits)
        {
            var item = EconService.Items[weapon];
            if (!Utilities.IsWeaponDefinitionIndex(item.Index))
            {
                continue;
            }

            var submenuOption = new SubmenuMenuOption(EconService.Items[weapon].LocalizedNames[language], () =>
            {
                var skinMenu = Core.MenusAPI.CreateBuilder();
                skinMenu.Design.SetMenuTitleVisible(false);
                var sorted = paintkits.OrderByDescending(p => p.Rarity.Id).ToList();
                var resetOption = new ButtonMenuOption(LocalizationService[player].MenuReset);
                
                resetOption.Click += (_,
                    args) =>
                {
                    Api.ResetWeaponSkin(args.Player.SteamID, args.Player.Controller.Team, (ushort)item.Index, true);
                    return ValueTask.CompletedTask;
                };

                skinMenu.AddOption(resetOption);
                foreach (var paintkit in sorted)
                {
                    var option = new ButtonMenuOption(HtmlGradient.GenerateGradientText(
                        paintkit.LocalizedNames[language],
                        paintkit.Rarity.Color.HexColor));

                    option.Click += (_,
                        args) =>
                    {
                        Api.UpdateWeaponSkin(
                            args.Player.SteamID, args.Player.Controller.Team, (ushort)item.Index, skin =>
                            {
                                skin.Paintkit = paintkit.Index;
                            }, true);

                        return ValueTask.CompletedTask;
                    };

                    option.Tag = paintkit.Index;

                    skinMenu.AddOption(option);
                }

                return Task.FromResult(skinMenu.Build());
            });

            submenuOption.Tag = (ushort)item.Index;
            submenuOption.Click += OnWeaponSkinOptionClick;
            main.AddOption(submenuOption);
        }

        var menu = main.Build();
        return menu;
    }

    private ValueTask OnWeaponMenuSkinOptionClick(object? sender,
        MenuOptionClickEventArgs args)
    {
        Core.Scheduler.NextWorldUpdate(() =>
        {
            if (TryGetWeaponInHand(args.Player, out var weaponInHand))
            {
                if (Utilities.IsWeaponDefinitionIndex(weaponInHand.AttributeManager.Item.ItemDefinitionIndex))
                {
                    var menu = Core.MenusAPI.GetCurrentMenu(args.Player);
                    menu.MoveToOption(args.Player,
                        menu.Options.FirstOrDefault(o =>
                            o.Tag is ushort tag &&
                            tag == weaponInHand.AttributeManager.Item.ItemDefinitionIndex));
                }
            }
        });
        return ValueTask.CompletedTask;
    }

    public IMenuOption GetWeaponSkinMenuSubmenuOption(IPlayer player)
    {
        if (!ItemPermissionService.CanUseWeaponSkins(player.SteamID))
        {
            return CreateDisabledOption(LocalizationService[player].MenuTitleSkins);
        }

        var skinOption = new SubmenuMenuOption(LocalizationService[player].MenuTitleSkins,
            () => Task.FromResult(BuildWeaponSkinMenu(player)));

        skinOption.Click += OnWeaponMenuSkinOptionClick;

        return skinOption;
    }
}
