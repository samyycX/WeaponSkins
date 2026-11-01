using SwiftlyS2.Shared.Menu;
using SwiftlyS2.Shared.Menu.Components;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Extensions;

namespace WeaponSkins;

public partial class MenuService
{
    public IMenuComponent GetAgentMenuSubmenuOption(IPlayer player)
    {
        if (!ItemPermissionService.CanUseAgents(player.SteamID))
        {
            return CreateDisabledOption(LocalizationService[player].MenuTitleAgents);
        }

        return new SubmenuComponent(LocalizationService[player].MenuTitleAgents, _ => BuildAgentMenu(player));
    }

    private IMenu BuildAgentMenu(IPlayer player)
    {
        var main = Core.Menu.CreateMenu("weaponskins.agents")
            .WithTitle(LocalizationService[player].MenuTitleAgents);

        main.Add(CreateButton(LocalizationService[player].MenuReset, p =>
        {
            var team = p.Controller.Team;
            DataService.AgentDataService.TryRemoveAgent(p.SteamID, team);
            if (DataService.AgentDataService.TryGetDefaultModel(p.SteamID, team, out var defaultModel))
            {
                ApplyAgentModel(p, defaultModel);
            }

            Api.ResetAgentSkin(p.SteamID, team, true);
        }));

        var team = player.Controller.Team;

        // Team 3 = CT ("ctm_" models), team 2 = T ("tm_" models).
        var teamPrefix = (int)team == 3 ? "ctm_" : "tm_";
        var agents = EconService.Agents.Values
            .Where(a => a.ModelPath.Contains($"/{teamPrefix}", StringComparison.OrdinalIgnoreCase) ||
                       a.ModelPath.StartsWith(teamPrefix, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(a => a.Rarity.Id)
            .ThenBy(a => EconService.GetLocalizedName(a.LocalizedNames, player.PlayerLanguage.Value))
            .ToList();

        foreach (var agent in agents)
        {
            var title = EconService.GetLocalizedName(agent.LocalizedNames, player.PlayerLanguage.Value);
            main.Add(CreateRarityButton(title, agent.Rarity.Color.HexColor, p =>
            {
                var team = p.Controller.Team;

                if (p.IsAlive())
                {
                    var pawn = p.PlayerPawn!;
                    var current = pawn.CBodyComponent!.SceneNode!.GetSkeletonInstance()
                        .ModelState
                        .ModelName;
                    DataService.AgentDataService.CaptureDefaultModel(p.SteamID, team, current);
                }

                DataService.AgentDataService.SetAgent(p.SteamID, team, agent.Index);
                ApplyAgentModel(p, agent.ModelPath);

                Api.UpdateAgentSkin(p.SteamID, team, agent.Index, true);

                Core.Scheduler.DelayBySeconds(0.1f, () =>
                {
                    ApplyAgentModel(p, agent.ModelPath);
                });
            }));
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

        Core.Scheduler.NextTick(() =>
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

            Core.Scheduler.NextTick(() =>
            {
                if (!player.IsAlive()) return;
                pawn.SetModel(modelPath);
            });
        });
    }
}