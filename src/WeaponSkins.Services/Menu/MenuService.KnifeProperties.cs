using SwiftlyS2.Shared.Menu;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins;

public partial class MenuService
{
    public IMenu BuildKnifePropertiesMenu(IPlayer player)
    {
        var loc = LocalizationService[player];
        var steamId = player.SteamID;
        var team = player.Controller.Team;
        var main = CreateMenu("weaponskins.knifeproperties")
            .WithTitle(loc.MenuTitleKnifeProperties);

        if (!Api.TryGetKnifeSkin(steamId, team, out var knife))
        {
            AddEmptyNotice(main, loc);
            return main.Build();
        }

        void Update(Action<KnifeSkinData> apply) =>
            Api.UpdateKnifeSkin(steamId, team, apply, true);

        main.Add(CreateHero(player.PlayerLanguage.Value, knife.DefinitionIndex, knife.Paintkit));
        AddNametag(main, loc, knife.Nametag, nametag => Update(k => k.Nametag = nametag));
        AddWear(main, loc, knife.PaintkitWear, wear => Update(k => k.PaintkitWear = wear));
        main.Add(CreateSeedInput(loc, knife.PaintkitSeed, seed => Update(k => k.PaintkitSeed = seed)));
        AddStattrak(main, loc, knife.StattrakCount, knife.Quality,
            count => Update(k => k.StattrakCount = count),
            quality => Update(k => k.Quality = quality),
            withSouvenir: false);

        return main.Build();
    }
}