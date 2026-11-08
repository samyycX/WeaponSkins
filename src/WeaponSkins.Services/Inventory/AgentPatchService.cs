using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Extensions;

namespace WeaponSkins.Services;

// The game draws an agent's patches from the pawn's m_vecPlayerPatchEconIndices, so they show on
// whatever agent the player wears, theirs or one picked in WeaponSkins.
public class AgentPatchService
{
    private ISwiftlyCore Core { get; init; }
    private DataService DataService { get; init; }
    private ItemPermissionService ItemPermissionService { get; init; }
    private PlayerService PlayerService { get; init; }

    public AgentPatchService(ISwiftlyCore core,
        DataService dataService,
        ItemPermissionService itemPermissionService,
        PlayerService playerService)
    {
        Core = core;
        DataService = dataService;
        ItemPermissionService = itemPermissionService;
        PlayerService = playerService;
    }

    // Without chosen patches the pawn keeps the ones from the player's own inventory, unless clear is set.
    public void Apply(IPlayer player, bool clear = false)
    {
        if (!player.IsAlive() || player.PlayerPawn is not { IsValid: true } pawn)
        {
            return;
        }

        IReadOnlyList<int>? patches = null;
        var chosen = ItemPermissionService.CanUsePatches(player.SteamID)
                     && DataService.AgentPatchDataService.TryGetPatches(player.SteamID, player.Controller.Team, out patches);
        if (!chosen && !clear)
        {
            return;
        }

        var indices = pawn.PlayerPatchEconIndices;
        for (var slot = 0; slot < AgentPatchDataService.Slots; slot++)
        {
            indices[slot] = chosen ? (uint)patches![slot] : 0;
        }

        pawn.PlayerPatchEconIndicesUpdated();
    }

    // After a change to a team's patches. Safe from any thread: applies on the next tick, if the
    // player is still here and on that team.
    public void Refresh(ulong steamId, Team team)
    {
        Core.Scheduler.NextTick(() =>
        {
            if (PlayerService.TryGetPlayer(steamId, out var player) && player.IsValid && player.Controller.Team == team)
            {
                Apply(player, clear: true);
            }
        });
    }
}
