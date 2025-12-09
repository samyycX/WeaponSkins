using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;

namespace WeaponSkins;

public partial class MenuService
{
    private ValueTask OnGloveSkinOptionClick(object? sender, MenuOptionClickEventArgs args)
    {
        Core.Scheduler.NextWorldUpdate(() =>
        {
            if (TryGetGloveDataInHand(args.Player, out var gloveInHand))
            {
                if (Utilities.IsGloveDefinitionIndex(gloveInHand.DefinitionIndex))
                {
                    var menu = Core.MenusAPI.GetCurrentMenu(args.Player);
                    menu.MoveToOption(args.Player,
                        menu.Options.FirstOrDefault(o =>
                            o.Tag is int tag &&
                            tag == gloveInHand.Paintkit));
                }
            }
        });
        return ValueTask.CompletedTask;
    }

    public IMenuAPI BuildGloveSkinMenu(IPlayer player)
    {
        var language = GetLanguage(player);
        var main = Core.MenusAPI.CreateBuilder();
        main.Design.SetMenuTitle(LocalizationService[player].MenuTitleGloves);

        foreach (var (glove, paintkits) in EconService.WeaponToPaintkits)
        {
            var item = EconService.Items[glove];
            if (!Utilities.IsGloveDefinitionIndex(item.Index))
            {
                continue;
            }

            var submenuOption = new SubmenuMenuOption(EconService.Items[glove].LocalizedNames[language], () =>
            {
                var skinMenu = Core.MenusAPI.CreateBuilder();
                skinMenu.Design.SetMenuTitleVisible(false);
                var sorted = paintkits.OrderByDescending(p => p.Rarity.Id).ToList();
                var resetOption = new ButtonMenuOption(LocalizationService[player].MenuReset);
                resetOption.Click += (_,
                    args) =>
                {
                    Api.ResetGloveSkin(args.Player.SteamID, args.Player.Controller.Team, true);
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
                        Api.UpdateGloveSkin(args.Player.SteamID, args.Player.Controller.Team, skin =>
                        {
                            skin.DefinitionIndex = (ushort)item.Index;
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
            submenuOption.Click += OnGloveSkinOptionClick;
            main.AddOption(submenuOption);
        }

        var menu = main.Build();
        return menu;
    }

    private ValueTask OnGloveMenuSkinOptionClick(object? sender, MenuOptionClickEventArgs args)
    {
        Core.Scheduler.NextWorldUpdate(() =>
        {
            if (TryGetGloveDataInHand(args.Player, out var gloveInHand))
            {
                Core.Scheduler.NextWorldUpdate(() =>
                {
                    var menu = Core.MenusAPI.GetCurrentMenu(args.Player);
                    menu.MoveToOption(args.Player,
                        menu.Options.FirstOrDefault(o =>
                            o.Tag is ushort tag &&
                            tag == gloveInHand.DefinitionIndex));
                });
            }

        });
        return ValueTask.CompletedTask;
    }

    public IMenuOption GetGloveSkinMenuSubmenuOption(IPlayer player)
    {
        if (!ItemPermissionService.CanUseGloveSkins(player.SteamID))
        {
            return CreateDisabledOption(LocalizationService[player].MenuTitleGloves);
        }

        var skinOption = new SubmenuMenuOption(LocalizationService[player].MenuTitleGloves,
            () => Task.FromResult(BuildGloveSkinMenu(player)));

        if (TryGetGloveDataInHand(player, out var gloveInHand))
        {
            skinOption.Click += OnGloveMenuSkinOptionClick;
        }

        return skinOption;
    }
}
