using Microsoft.Extensions.Logging;

using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Econ;

namespace WeaponSkins.Services;

public class MenuService
{
    private ISwiftlyCore Core { get; init; }
    private ILogger<MenuService> Logger { get; init; }
    private WeaponSkinAPI Api { get; init; }
    private EconService EconService { get; init; }
    private Dictionary<ulong /* steamid */, long /* time */> Debounce = new();

    public MenuService(ISwiftlyCore core,
        ILogger<MenuService> logger,
        WeaponSkinAPI api,
        EconService econService)
    {
        Core = core;
        Logger = logger;
        Api = api;
        EconService = econService;
    }

    public void TestMenu(IPlayer player)
    {
        var main = Core.MenusAPI.CreateBuilder();
        main.Design.SetMenuTitle("Weapon Skins");
        foreach (var (weapon, paintkits) in EconService.WeaponToPaintkits)
        {
            var skinMenu = Core.MenusAPI.CreateBuilder();
            skinMenu.Design.SetMenuTitle(weapon);
            var sorted = paintkits.OrderByDescending(p => p.Rarity.Id).ToList();
            foreach (var paintkit in sorted)
            {
                if (!paintkit.LocalizedNames.ContainsKey("schinese"))
                {
                    throw new Exception($"Paintkit {paintkit} not found in languages schinese");
                    continue;
                }

                var option = new ButtonMenuOption(HtmlGradient.GenerateGradientText(paintkit.LocalizedNames["schinese"],
                    paintkit.Rarity.Color.HexColor));

                option.Click += async (sender,
                    args) =>
                {
                    if (Debounce.TryGetValue(args.Player.SteamID, out var time) &&
                        DateTime.UtcNow.Ticks - time < 1000000)
                    {
                        return;
                    }

                    var index = (ushort)EconService.Items[weapon].Index;
                    if (!Utilities.IsKnifeDefinitionIndex(index))
                    {
                        Api.UpdateWeaponSkins([
                            new()
                            {
                                SteamID = args.Player.SteamID,
                                Team = args.Player.Controller.Team,
                                DefinitionIndex = index,
                                Paintkit = paintkit.Index,
                            }
                        ]);
                    }
                    else
                    {
                        Api.UpdateKnifeSkins([
                            new()
                            {
                                SteamID = args.Player.SteamID,
                                Team = args.Player.Controller.Team,
                                DefinitionIndex = index,
                                Paintkit = paintkit.Index,
                            }
                        ]);
                    }

                    Debounce[args.Player.SteamID] = DateTime.UtcNow.Ticks;
                    await Task.CompletedTask;
                };

                skinMenu.AddOption(option);
            }

            main.AddOption(
                new SubmenuMenuOption(EconService.Items[weapon].LocalizedNames["schinese"], skinMenu.Build()));
        }


        Core.MenusAPI.OpenMenuForPlayer(player, main.Build());
    }
}