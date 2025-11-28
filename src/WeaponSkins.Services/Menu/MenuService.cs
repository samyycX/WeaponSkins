using Microsoft.Extensions.Logging;

using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Translation;

using WeaponSkins.Econ;

namespace WeaponSkins.Services;

public class MenuService
{
    private ISwiftlyCore Core { get; init; }
    private ILogger<MenuService> Logger { get; init; }
    private WeaponSkinAPI Api { get; init; }
    private EconService EconService { get; init; }
    private LocalizationService LocalizationService { get; init; }

    public MenuService(ISwiftlyCore core,
        ILogger<MenuService> logger,
        WeaponSkinAPI api,
        EconService econService,
        LocalizationService localizationService)
    {
        Core = core;
        Logger = logger;
        Api = api;
        EconService = econService;
        LocalizationService = localizationService;
    }

    public void TestMenu(IPlayer player)
    {
        var main = Core.MenusAPI.CreateBuilder();
        main.Design.SetMenuTitle("Skins");

        main.AddOption(new SubmenuMenuOption(LocalizationService.MenuTitleSkins, BuildWeaponSkinMenu(player)));
        main.AddOption(new SubmenuMenuOption(LocalizationService.MenuTitleKnifes, BuildKnifeSkinMenu(player)));
        main.AddOption(new SubmenuMenuOption(LocalizationService.MenuTitleGloves, BuildGloveSkinMenu(player)));
        main.AddOption(new SubmenuMenuOption(LocalizationService.MenuTitleStickers, BuildStickerMenu(player)));

        Core.MenusAPI.OpenMenuForPlayer(player, main.Build());
    }

    public static readonly Dictionary<string, string> LanguageCodeToTranslationKey = new Dictionary<string, string>
    {
        { "ar", "arabic" },
        { "bg", "bulgarian" },
        { "zh-CN", "schinese" },
        { "zh-TW", "tchinese" },
        { "cs", "czech" },
        { "da", "danish" },
        { "nl", "dutch" },
        { "en", "english" },
        { "fi", "finnish" },
        { "fr", "french" },
        { "de", "german" },
        { "el", "greek" },
        { "hu", "hungarian" },
        { "id", "indonesian" },
        { "it", "italian" },
        { "ja", "japanese" },
        { "ko", "koreana" },
        { "no", "norwegian" },
        { "pl", "polish" },
        { "pt", "portuguese" },
        { "pt-BR", "brazilian" },
        { "ro", "romanian" },
        { "ru", "russian" },
        { "es", "spanish" },
        { "es-419", "latam" },
        { "sv", "swedish" },
        { "th", "thai" },
        { "tr", "turkish" },
        { "uk", "ukrainian" },
        { "vn", "vietnamese" }
    };

    private static string GetLanguage(IPlayer player)
    {
        return LanguageCodeToTranslationKey[player.PlayerLanguage.Value];
    }


    public IMenuAPI BuildWeaponSkinMenu(IPlayer player)
    {
        var language = GetLanguage(player);
        var main = Core.MenusAPI.CreateBuilder();
        main.Design.SetMenuTitle(LocalizationService.MenuTitleSkins);

        foreach (var (weapon, paintkits) in EconService.WeaponToPaintkits)
        {
            var item = EconService.Items[weapon];
            if (!Utilities.IsWeaponDefinitionIndex(item.Index))
            {
                continue;
            }

            var skinMenu = Core.MenusAPI.CreateBuilder();
            skinMenu.Design.SetMenuTitle(weapon);
            var sorted = paintkits.OrderByDescending(p => p.Rarity.Id).ToList();
            foreach (var paintkit in sorted)
            {
                var option = new ButtonMenuOption(HtmlGradient.GenerateGradientText(paintkit.LocalizedNames[language],
                    paintkit.Rarity.Color.HexColor));

                option.Click += (_,
                    args) =>
                {
                    Api.SetWeaponPaintsWithoutStattrakPermanently([
                        new()
                        {
                            SteamID = args.Player.SteamID,
                            Team = args.Player.Controller.Team,
                            DefinitionIndex = (ushort)item.Index,
                            Paintkit = paintkit.Index,
                            Quality = EconItemQuality.StatTrak,
                        }
                    ]);

                    return ValueTask.CompletedTask;
                };

                skinMenu.AddOption(option);
            }

            main.AddOption(
                new SubmenuMenuOption(EconService.Items[weapon].LocalizedNames[language], skinMenu.Build()));
        }

        return main.Build();
    }

    public IMenuAPI BuildKnifeSkinMenu(IPlayer player)
    {
        var language = GetLanguage(player);
        var main = Core.MenusAPI.CreateBuilder();
        main.Design.SetMenuTitle(LocalizationService.MenuTitleKnifes);

        foreach (var (knife, paintkits) in EconService.WeaponToPaintkits)
        {
            var item = EconService.Items[knife];
            if (!Utilities.IsKnifeDefinitionIndex(item.Index))
            {
                continue;
            }

            var skinMenu = Core.MenusAPI.CreateBuilder();
            skinMenu.Design.SetMenuTitle(knife);
            var sorted = paintkits.OrderByDescending(p => p.Rarity.Id).ToList();
            foreach (var paintkit in sorted)
            {
                var option = new ButtonMenuOption(HtmlGradient.GenerateGradientText(paintkit.LocalizedNames[language],
                    paintkit.Rarity.Color.HexColor));

                option.Click += (_,
                    args) =>
                {
                    Api.SetKnifeSkinsPermanently([
                        new()
                        {
                            SteamID = args.Player.SteamID,
                            Team = args.Player.Controller.Team,
                            DefinitionIndex = (ushort)item.Index,
                            Paintkit = paintkit.Index,
                        }
                    ]);

                    return ValueTask.CompletedTask;
                };

                skinMenu.AddOption(option);
            }

            main.AddOption(
                new SubmenuMenuOption(EconService.Items[knife].LocalizedNames[language], skinMenu.Build()));
        }

        return main.Build();
    }

    public IMenuAPI BuildGloveSkinMenu(IPlayer player)
    {
        var language = GetLanguage(player);
        var main = Core.MenusAPI.CreateBuilder();
        main.Design.SetMenuTitle(LocalizationService.MenuTitleGloves);

        foreach (var (glove, paintkits) in EconService.WeaponToPaintkits)
        {
            var item = EconService.Items[glove];
            if (!Utilities.IsGloveDefinitionIndex(item.Index))
            {
                continue;
            }

            var skinMenu = Core.MenusAPI.CreateBuilder();
            skinMenu.Design.SetMenuTitle(glove);
            var sorted = paintkits.OrderByDescending(p => p.Rarity.Id).ToList();
            foreach (var paintkit in sorted)
            {
                var option = new ButtonMenuOption(HtmlGradient.GenerateGradientText(paintkit.LocalizedNames[language],
                    paintkit.Rarity.Color.HexColor));

                option.Click += (_,
                    args) =>
                {
                    Api.SetGloveSkins([
                        new()
                        {
                            SteamID = args.Player.SteamID,
                            Team = args.Player.Controller.Team,
                            DefinitionIndex = (ushort)item.Index,
                            Paintkit = paintkit.Index,
                        }
                    ]);

                    return ValueTask.CompletedTask;
                };

                skinMenu.AddOption(option);
            }

            main.AddOption(
                new SubmenuMenuOption(EconService.Items[glove].LocalizedNames[language], skinMenu.Build()));
        }

        return main.Build();
    }

    public IMenuAPI BuildStickerMenu(IPlayer player)
    {
        var language = GetLanguage(player);
        var main = Core.MenusAPI.CreateBuilder();
        main.Design.SetMenuTitle(LocalizationService.MenuTitleStickers);

        foreach (var (index, stickerCollection) in EconService.StickerCollections)
        {
            var stickerMenu = Core.MenusAPI.CreateBuilder();
            stickerMenu.Design.SetMenuTitle(stickerCollection.LocalizedNames[language]);

            foreach (var sticker in stickerCollection.Stickers)
            {
                var option = new ButtonMenuOption(HtmlGradient.GenerateGradientText(sticker.LocalizedNames[language],
                    sticker.Rarity.Color.HexColor));
                option.Click += (_,
                    args) =>
                {
                    return ValueTask.CompletedTask;
                };
                stickerMenu.AddOption(option);
            }

            main.AddOption(new SubmenuMenuOption(stickerCollection.LocalizedNames[language], stickerMenu.Build()));
        }

        return main.Build();
    }
}