using System.Numerics;

using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;

namespace WeaponSkins;

public partial class MenuService
{
    private ValueTask OnKnifeSkinOptionClick(object? sender, MenuOptionClickEventArgs args)
    {
        Core.Scheduler.NextWorldUpdate(() =>
        {
            if (TryGetKnifeDataInHand(args.Player, out var knifeInHand))
            {
                var menu = Core.MenusAPI.GetCurrentMenu(args.Player);
                menu.MoveToOption(args.Player,
                    menu.Options.FirstOrDefault(o =>
                        o.Tag is int tag &&
                        tag == knifeInHand.Paintkit));
            }
        });
        return ValueTask.CompletedTask;
    }

    public IMenuAPI BuildKnifeSkinMenu(IPlayer player)
    {
        var language = GetLanguage(player);
        var main = Core.MenusAPI.CreateBuilder();
        main.Design.SetMenuTitle(LocalizationService[player].MenuTitleKnifes);

        foreach (var (knife, paintkits) in EconService.WeaponToPaintkits)
        {
            var item = EconService.Items[knife];
            if (!Utilities.IsKnifeDefinitionIndex(item.Index))
            {
                continue;
            }

            var submenuOption = new SubmenuMenuOption(EconService.Items[knife].LocalizedNames[language], () =>
            {
                var skinMenu = Core.MenusAPI.CreateBuilder();
                skinMenu.Design.SetMenuTitleVisible(false);
                var sorted = paintkits.OrderByDescending(p => p.Rarity.Id).ToList();
                var resetOption = new ButtonMenuOption(LocalizationService[player].MenuReset);
                resetOption.Click += (_,
                    args) =>
                {
                    Api.ResetKnifeSkin(args.Player.SteamID, args.Player.Controller.Team, true);
                    return ValueTask.CompletedTask;
                };
                skinMenu.AddOption(resetOption);
                foreach (var paintkit in sorted)
                {
                    var option = new ButtonMenuOption(HtmlGradient.GenerateGradientText(paintkit.LocalizedNames[language],
                        paintkit.Rarity.Color.HexColor));

                    option.Click += (_,
                        args) =>
                    {
                        Api.UpdateKnifeSkin(args.Player.SteamID, args.Player.Controller.Team, (knife) =>
                        {
                            knife.DefinitionIndex = (ushort)item.Index;
                            knife.Paintkit = paintkit.Index;
                        }, true);

                        return ValueTask.CompletedTask;
                    };

                    option.Tag = paintkit.Index;

                    skinMenu.AddOption(option);
                }
                return Task.FromResult(skinMenu.Build());
            });

            submenuOption.Tag = (ushort)item.Index;
            submenuOption.Click += OnKnifeSkinOptionClick;
            main.AddOption(submenuOption);
        }

        var menu = main.Build();
        return menu;
    }

    private ValueTask OnKnifeMenuSkinOptionClick(object? sender, MenuOptionClickEventArgs args)
    {
        Core.Scheduler.NextWorldUpdate(() =>
        {
            if (TryGetKnifeDataInHand(args.Player, out var knifeInHand))
            {
                var menu = Core.MenusAPI.GetCurrentMenu(args.Player);
                menu.MoveToOption(args.Player,
                    menu.Options.FirstOrDefault(o =>
                        o.Tag is ushort tag &&
                        tag == knifeInHand.DefinitionIndex));
            }
        });
        return ValueTask.CompletedTask;
    }

    public IMenuOption GetKnifeSkinMenuSubmenuOption(IPlayer player)
    {
        if (!ItemPermissionService.CanUseKnifeSkins(player.SteamID))
        {
            return CreateDisabledOption(LocalizationService[player].MenuTitleKnifes);
        }

        var skinOption = new SubmenuMenuOption(LocalizationService[player].MenuTitleKnifes, 
            () => Task.FromResult(BuildKnifeSkinMenu(player)));
        
        skinOption.Click += OnKnifeMenuSkinOptionClick;

        return skinOption;
    }
}
