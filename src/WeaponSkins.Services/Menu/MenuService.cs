using System;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Logging;

using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;
using SwiftlyS2.Shared.Translation;

using WeaponSkins.Econ;
using WeaponSkins.Services;
using WeaponSkins.Shared;

namespace WeaponSkins;

public partial class MenuService
{
    private ISwiftlyCore Core { get; init; }
    private ILogger<MenuService> Logger { get; init; }
    private WeaponSkinAPI Api { get; init; }
    private EconService EconService { get; init; }
    private LocalizationService LocalizationService { get; init; }
    private ItemPermissionService ItemPermissionService { get; init; }
    private DataService DataService { get; init; }

    public MenuService(ISwiftlyCore core,
        ILogger<MenuService> logger,
        WeaponSkinAPI api,
        EconService econService,
        LocalizationService localizationService,
        ItemPermissionService itemPermissionService,
        DataService dataService)
    {
        Core = core;
        Logger = logger;
        Api = api;
        EconService = econService;
        LocalizationService = localizationService;
        ItemPermissionService = itemPermissionService;
        DataService = dataService;
    }

    public void OpenMainMenu(IPlayer player)
    {
        var main = Core.MenusAPI.CreateBuilder();
        main.Design.SetMenuTitle(LocalizationService[player].MenuTitle);

        AddMenuOption(main, ItemPermissionService.CanUseWeaponSkins(player.SteamID),
            () => GetWeaponSkinMenuSubmenuOption(player), LocalizationService[player].MenuTitleSkins);
        AddMenuOption(main, ItemPermissionService.CanUseKnifeSkins(player.SteamID),
            () => GetKnifeSkinMenuSubmenuOption(player), LocalizationService[player].MenuTitleKnifes);
        AddMenuOption(main, ItemPermissionService.CanUseGloveSkins(player.SteamID),
            () => GetGloveSkinMenuSubmenuOption(player), LocalizationService[player].MenuTitleGloves);
        AddMenuOption(main, ItemPermissionService.CanUseStickers(player.SteamID),
            () => GetStickerMenuSubmenuOption(player), LocalizationService[player].MenuTitleStickers);
        AddMenuOption(main, ItemPermissionService.CanUseKeychains(player.SteamID),
            () => GetKeychainMenuSubmenuOption(player), LocalizationService[player].MenuTitleKeychains);
        AddMenuOption(main, ItemPermissionService.CanUseWeaponSkins(player.SteamID),
            () => GetSkinPropertiesMenuSubmenuOption(player), LocalizationService[player].MenuTitleSkinProperties);
        AddMenuOption(main, ItemPermissionService.CanUseKnifeSkins(player.SteamID),
            () => GetKnifePropertiesMenuSubmenuOption(player), LocalizationService[player].MenuTitleKnifeProperties);
        AddMenuOption(main, ItemPermissionService.CanUseGloveSkins(player.SteamID),
            () => GetGlovePropertiesMenuSubmenuOption(player), LocalizationService[player].MenuTitleGloveProperties);

        AddMenuOption(main, ItemPermissionService.CanUseAgents(player.SteamID),
            () => GetAgentMenuSubmenuOption(player), LocalizationService[player].MenuTitleAgents);

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


    public bool TryGetWeaponInHand(IPlayer player,
        [MaybeNullWhen(false)] out CBasePlayerWeapon weaponInHand)
    {
        weaponInHand = null;
        if (!player.IsValid)
        {
            return false;
        }

        if (!(player.PlayerPawn is { IsValid: true, LifeState: (byte)LifeState_t.LIFE_ALIVE }))
        {
            return false;
        }

        weaponInHand = player.PlayerPawn.WeaponServices!.ActiveWeapon.Value;
        if (weaponInHand == null)
        {
            return false;
        }

        return true;
    }


    public bool TryGetWeaponDataInHand(IPlayer player,
        [MaybeNullWhen(false)] out WeaponSkinData dataInHand)
    {
        dataInHand = null;
        if (!TryGetWeaponInHand(player, out var weaponInHand))
        {
            return false;
        }

        var defIndex = weaponInHand.AttributeManager.Item.ItemDefinitionIndex;
        if (!Api.TryGetWeaponSkin(player.SteamID, player.Controller.Team, defIndex, out dataInHand))
        {
            return false;
        }

        return true;
    }

    public bool TryGetKnifeDataInHand(IPlayer player,
        [MaybeNullWhen(false)] out KnifeSkinData dataInHand)
    {
        return Api.TryGetKnifeSkin(player.SteamID, player.Controller.Team, out dataInHand);
    }

    public bool TryGetGloveDataInHand(IPlayer player,
        [MaybeNullWhen(false)] out GloveData dataInHand)
    {
        return Api.TryGetGloveSkin(player.SteamID, player.Controller.Team, out dataInHand);
    }

    private string GetAgentName(AgentDefinition agent, string language)
    {
        return agent.LocalizedNames.TryGetValue(language, out var localized)
            ? localized
            : agent.LocalizedNames.GetValueOrDefault("english") ?? agent.Name;
    }

    private static void AddMenuOption(IMenuBuilderAPI builder,
        bool enabled,
        Func<IMenuOption> optionFactory,
        string title)
    {
        if (enabled)
        {
            builder.AddOption(optionFactory());
            return;
        }

        builder.AddOption(CreateDisabledOption(title));
    }

    private static TextMenuOption CreateDisabledOption(string title)
    {
        var option = new TextMenuOption(title);
        option.Enabled = false;
        return option;
    }
}