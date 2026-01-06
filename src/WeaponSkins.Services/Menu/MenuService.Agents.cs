using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Extensions;

namespace WeaponSkins;

public partial class MenuService
{
    public IMenuOption GetAgentMenuSubmenuOption(IPlayer player)
    {
        if (!ItemPermissionService.CanUseAgents(player.SteamID))
        {
            return CreateDisabledOption(LocalizationService[player].MenuTitleAgents);
        }

        var option = new SubmenuMenuOption(LocalizationService[player].MenuTitleAgents, () => Task.FromResult(BuildAgentMenu(player)));
        return option;
    }

    private IMenuAPI BuildAgentMenu(IPlayer player)
    {
        var main = Core.MenusAPI.CreateBuilder();
        main.Design.SetMenuTitle(LocalizationService[player].MenuTitleAgents);

        var reset = new ButtonMenuOption(LocalizationService[player].MenuReset);
        reset.Click += (_, args) =>
        {
            var team = args.Player.Controller.Team;
            DataService.AgentDataService.TryRemoveAgent(args.Player.SteamID, team);
            if (DataService.AgentDataService.TryGetDefaultModel(args.Player.SteamID, team, out var defaultModel))
            {
                ApplyAgentModel(args.Player, defaultModel);
            }

            Api.ResetAgentSkin(args.Player.SteamID, team, true);

            return ValueTask.CompletedTask;
        };
        main.AddOption(reset);

        var language = GetLanguage(player);
        var team = player.Controller.Team;
        
        // Filter agents by team: CT agents start with "ctm_", T agents start with "tm_"
        // Team 3 = CT, Team 2 = T
        var teamPrefix = (int)team == 3 ? "ctm_" : "tm_";
        var agents = EconService.Agents.Values
            .Where(a => a.ModelPath.Contains($"/{teamPrefix}", StringComparison.OrdinalIgnoreCase) || 
                       a.ModelPath.StartsWith(teamPrefix, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(a => a.Rarity.Id)
            .ThenBy(a => a.LocalizedNames.TryGetValue(language, out var ln) ? ln : a.Name)
            .ToList();

        foreach (var agent in agents)
        {
            var title = agent.LocalizedNames.TryGetValue(language, out var localized)
                ? localized
                : agent.Name;

            var coloredTitle = HtmlGradient.GenerateGradientText(title, agent.Rarity.Color.HexColor);
            var option = new ButtonMenuOption(coloredTitle);
            option.Click += (_, args) =>
            {
                var team = args.Player.Controller.Team;

                if (args.Player.IsAlive())
                {
                    var pawn = args.Player.PlayerPawn!;
                    var current = pawn.CBodyComponent!.SceneNode!.GetSkeletonInstance()
                        .ModelState
                        .ModelName;
                    DataService.AgentDataService.CaptureDefaultModel(args.Player.SteamID, team, current);
                }

                DataService.AgentDataService.SetAgent(args.Player.SteamID, team, agent.Index);
                ApplyAgentModel(args.Player, agent.ModelPath);

                Api.UpdateAgentSkin(args.Player.SteamID, team, agent.Index, true);

                Core.Scheduler.DelayBySeconds(0.1f, () =>
                {
                    ApplyAgentModel(args.Player, agent.ModelPath);
                });

                return ValueTask.CompletedTask;
            };

            main.AddOption(option);
        }

        return main.Build();
    }

    private string? GetRefreshModel(string currentModel,
        string targetModel)
    {
        foreach (var agent in EconService.Agents.Values)
        {
            var candidate = agent.ModelPath;
            if (string.IsNullOrWhiteSpace(candidate)) continue;
            if (string.Equals(candidate, currentModel, StringComparison.OrdinalIgnoreCase)) continue;
            if (string.Equals(candidate, targetModel, StringComparison.OrdinalIgnoreCase)) continue;
            return candidate;
        }

        return null;
    }

    private void ApplyAgentModel(IPlayer player,
        string modelPath)
    {
        if (!player.IsAlive()) return;

        Core.Scheduler.NextWorldUpdate(() =>
        {
            if (!player.IsAlive()) return;
            var pawn = player.PlayerPawn!;

            var current = pawn.CBodyComponent!.SceneNode!.GetSkeletonInstance()
                .ModelState
                .ModelName;

            var refreshModel = GetRefreshModel(current, modelPath);
            if (!string.IsNullOrWhiteSpace(refreshModel))
            {
                pawn.SetModel(refreshModel);
                pawn.SetModel(current);
            }

            Core.Scheduler.NextWorldUpdate(() =>
            {
                if (!player.IsAlive()) return;
                pawn.SetModel(modelPath);
            });
        });
    }
}
