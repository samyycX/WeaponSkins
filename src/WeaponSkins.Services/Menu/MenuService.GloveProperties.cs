using SwiftlyS2.Shared.Menu;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins;

public partial class MenuService
{
    public IMenu BuildGlovePropertiesMenu(IPlayer player)
    {
        var loc = LocalizationService[player];
        var steamId = player.SteamID;
        var team = player.Controller.Team;
        var main = CreateMenu("weaponskins.gloveproperties")
            .WithTitle(loc.MenuTitleGloveProperties);

        if (!Api.TryGetGloveSkin(steamId, team, out var glove))
        {
            AddEmptyNotice(main, loc);
            return main.Build();
        }

        void Update(Action<GloveData> apply) =>
            Api.UpdateGloveSkin(steamId, team, apply, true);

        // Gloves take wear only: no name tag, seed, StatTrak, stickers or charms.
        main.Add(CreateHero(loc, player.PlayerLanguage.Value, glove.DefinitionIndex, glove.Paintkit, glove: true));
        AddWear(main, loc, glove.PaintkitWear, wear => Update(g => g.PaintkitWear = wear));

        return main.Build();
    }
}