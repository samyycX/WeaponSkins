using Microsoft.Extensions.Logging;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Translation;

namespace WeaponSkins.Services;

public class LocalizationService
{
    private ISwiftlyCore Core { get; init; }
    private ILogger Logger { get; init; }
    private ILocalizer Localizer { get; init;  }

    public LocalizationService(ISwiftlyCore core,
        ILogger<LocalizationService> logger)
    {
        Core = core;
        Logger = logger;
        Localizer = Core.Localizer;
    }

    public string MenuTitle => Localizer["menu.title"];
    public string MenuTitleSkins => Localizer["menu.skins.title"];
    public string MenuTitleKnifes => Localizer["menu.knifes.title"];
    public string MenuTitleGloves => Localizer["menu.gloves.title"];
    public string MenuTitleStickers => Localizer["menu.stickers.title"];
}