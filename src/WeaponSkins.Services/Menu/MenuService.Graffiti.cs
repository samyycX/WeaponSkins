using SwiftlyS2.Shared.Menu;
using SwiftlyS2.Shared.Menu.Components;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins;

public partial class MenuService
{
    private IMenu BuildGraffitiMenu(IPlayer player)
    {
        var loc = LocalizationService[player];
        var language = player.PlayerLanguage.Value;
        var main = CreateMenu("weaponskins.graffiti")
            .WithTitle(loc.MenuTitleGraffiti);

        main.Add(CreateButton(loc.MenuReset, p => Api.ResetGraffiti(p.SteamID)));

        foreach (var tournament in EconService.GraffitiTournaments.Values.OrderByDescending(t => t.EventId))
        {
            var tournamentName = EconService.GetLocalizedName(tournament.LocalizedNames, language);
            main.Add(CreateSubmenu(tournamentName, _ =>
            {
                var tournamentMenu = CreateMenu("weaponskins.graffiti.tournament")
                    .WithTitle(tournamentName);
                AddGraffitiRows(tournamentMenu, player, tournament.Graffiti.Select(name => EconService.Graffiti[name]));

                var built = tournamentMenu.Build();
                TrackFocus(built, SelectedGraffiti);
                return built;
            }));
        }

        var inTournaments = EconService.GraffitiTournaments.Values
            .SelectMany(t => t.Graffiti)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        AddGraffitiRows(main, player, EconService.Graffiti.Values.Where(g => !inTournaments.Contains(g.Name)));

        var menu = main.Build();
        TrackFocus(menu, SelectedGraffiti);
        return menu;
    }

    private void AddGraffitiRows(IMenuBuilder menu, IPlayer player, IEnumerable<GraffitiDefinition> graffiti)
    {
        var language = player.PlayerLanguage.Value;
        var rows = graffiti
            .Select(g => (Graffiti: g, Name: EconService.GetLocalizedName(g.LocalizedNames, language)))
            .OrderByDescending(entry => entry.Graffiti.Rarity.Id)
            .ThenBy(entry => entry.Name);

        foreach (var (definition, name) in rows)
        {
            var color = definition.Rarity.Color.HexColor;
            TextComponent row = definition.Tintable
                ? CreateSubmenu(name, _ => BuildGraffitiTintMenu(player, definition, name), color)
                : CreateRarityButton(name, color, p => Api.SetGraffiti(p.SteamID, definition.Index, 0));
            row.Tag = definition;
            menu.Add(row);
        }
    }

    private IMenu BuildGraffitiTintMenu(IPlayer player, GraffitiDefinition graffiti, string title)
    {
        var language = player.PlayerLanguage.Value;
        var menu = CreateMenu("weaponskins.graffiti.tints")
            .WithTitle(title);

        foreach (var tint in EconService.GraffitiTints.Values.OrderBy(t => t.Index))
        {
            var button = CreateRarityButton(EconService.GetLocalizedName(tint.LocalizedNames, language), tint.HexColor,
                p => Api.SetGraffiti(p.SteamID, graffiti.Index, tint.Index));
            button.Tag = tint;
            menu.Add(button);
        }

        var built = menu.Build();
        TrackFocus(built, p =>
            DataService.GraffitiDataService.TryGetGraffiti(p.SteamID, out var index, out var selectedTint) && index == graffiti.Index
                ? EconService.GraffitiTints.Values.FirstOrDefault(t => t.Index == selectedTint)
                : null);
        return built;
    }

    private GraffitiDefinition? SelectedGraffiti(IPlayer player)
    {
        return DataService.GraffitiDataService.TryGetGraffiti(player.SteamID, out var index, out _)
            ? EconService.Graffiti.Values.FirstOrDefault(g => g.Index == index)
            : null;
    }
}
