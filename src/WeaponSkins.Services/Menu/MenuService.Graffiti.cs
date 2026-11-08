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
            var submenu = CreateSubmenu(tournamentName, _ =>
            {
                var tournamentMenu = CreateMenu("weaponskins.graffiti.tournament")
                    .WithTitle(tournamentName);
                AddGraffitiRows(tournamentMenu, player, tournament.Graffiti.Select(name => EconService.Graffiti[name]));

                var built = tournamentMenu.Build();
                TrackFocus(built, SelectedGraffiti);
                return built;
            });
            // The event id, not the definition: GraffitiTournamentDefinition is not part of WeaponSkins.Shared.
            submenu.Tag = tournament.EventId;
            main.Add(submenu);
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
                : CreateRarityButton(name, color, p => SelectGraffiti(p, definition.Index, 0, name));
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
            var tintName = EconService.GetLocalizedName(tint.LocalizedNames, language);
            var button = CreateRarityButton(tintName, tint.HexColor,
                p => SelectGraffiti(p, graffiti.Index, tint.Index, $"{title} ({tintName})"));
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

    // Choosing changes nothing the player can see until they spray, so say what was chosen.
    private void SelectGraffiti(IPlayer player, int graffitiIndex, int tintIndex, string name)
    {
        Api.SetGraffiti(player.SteamID, graffitiIndex, tintIndex);
        player.SendChat(LocalizationService[player].GraffitiSelected(name));
    }

    private GraffitiDefinition? SelectedGraffiti(IPlayer player)
    {
        return DataService.GraffitiDataService.TryGetGraffiti(player.SteamID, out var index, out _)
            ? EconService.Graffiti.Values.FirstOrDefault(g => g.Index == index)
            : null;
    }
}
