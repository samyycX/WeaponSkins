using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;

namespace WeaponSkins;

public partial class MenuService
{
    private Dictionary<string, IMenuAPI> _cachedGloveSkinMenus = new();

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
        if (_cachedGloveSkinMenus.TryGetValue(language, out var cachedMenu))
        {
            return cachedMenu;
        }

        var main = Core.MenusAPI.CreateBuilder();
        main.Design.SetMenuTitle(LocalizationService[player].MenuTitleGloves);

        foreach (var (glove, paintkits) in EconService.WeaponToPaintkits)
        {
            var item = EconService.Items[glove];
            if (!Utilities.IsGloveDefinitionIndex(item.Index))
            {
                continue;
            }

            var skinMenu = Core.MenusAPI.CreateBuilder();
            skinMenu.Design.SetMenuTitleVisible(false);
            var sorted = paintkits.OrderByDescending(p => p.Rarity.Id).ToList();
            foreach (var paintkit in sorted)
            {
                var option = new ButtonMenuOption(HtmlGradient.GenerateGradientText(paintkit.LocalizedNames[language],
                    paintkit.Rarity.Color.HexColor));

                option.Click += (_,
                    args) =>
                {
                    Api.UpdateGloveSkin(args.Player.SteamID, args.Player.Controller.Team, skin =>
                    {
                        skin.Paintkit = paintkit.Index;
                    }, true);

                    return ValueTask.CompletedTask;
                };

                option.Tag = paintkit.Index;


                skinMenu.AddOption(option);
            }

            var submenuOption =
                new SubmenuMenuOption(EconService.Items[glove].LocalizedNames[language], skinMenu.Build());
            submenuOption.Tag = (ushort)item.Index;
            submenuOption.Click += OnGloveSkinOptionClick;
            main.AddOption(submenuOption);
        }

        var menu = main.Build();
        _cachedGloveSkinMenus[language] = menu;
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
        var skinOption = new SubmenuMenuOption(LocalizationService[player].MenuTitleGloves, BuildGloveSkinMenu(player));
        if (TryGetGloveDataInHand(player, out var gloveInHand))
        {
            skinOption.Click += OnGloveMenuSkinOptionClick;
        }

        return skinOption;
    }
}