using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace WeaponSkins.Services;

// The scoreboard draws a player's pin from the controller's m_rank, which the game fills from their own
// inventory. A reconnect or map change replaces the controller, so the pin is re-applied each spawn.
public class PinService : IDisposable
{
    // MEDAL_CATEGORY_SEASON_COIN, the pin or coin a player displays.
    private const int PinSlot = 5;

    private readonly Dictionary<ulong, MedalRank_t> _originalPins = new();
    private ISwiftlyCore Core { get; init; }
    private WeaponSkinGetterAPI Api { get; init; }
    private PlayerService PlayerService { get; init; }

    public PinService(ISwiftlyCore core,
        WeaponSkinGetterAPI api,
        PlayerService playerService)
    {
        Core = core;
        Api = api;
        PlayerService = playerService;

        Core.Event.OnClientDisconnected += (@event) =>
        {
            var player = Core.PlayerManager.GetPlayer(@event.PlayerId);
            if (player != null) _originalPins.Remove(player.SessionId);
        };
    }

    // Without a chosen pin, or the permission for it, the player gets back the pin the game gave them.
    public void Apply(IPlayer player)
    {
        if (!player.IsValid || player.Controller.InventoryServices is not { IsValid: true } inventory) return;

        if (Api.TryGetPin(player.SteamID, out var pinIndex))
        {
            // Saved once per session, before the first write, so it is the player's own pin.
            _originalPins.TryAdd(player.SessionId, inventory.Rank[PinSlot]);
            Write(inventory, (MedalRank_t)pinIndex);
        }
        else if (_originalPins.Remove(player.SessionId, out var original))
        {
            Write(inventory, original);
        }
    }

    // After a change to a player's pin. Safe from any thread: applies on the next tick, if the player is still here.
    public void Refresh(ulong steamId)
    {
        Core.Scheduler.NextTick(() =>
        {
            if (PlayerService.TryGetPlayer(steamId, out var player)) Apply(player);
        });
    }

    // Every spawn re-applies the pin, so an unchanged slot is not networked again.
    private static void Write(CCSPlayerController_InventoryServices inventory,
        MedalRank_t pin)
    {
        var ranks = inventory.Rank;
        if (ranks[PinSlot] == pin) return;
        ranks[PinSlot] = pin;
        inventory.RankUpdated();
    }

    public void Dispose()
    {
        foreach (var (sessionId, original) in _originalPins)
        {
            var player = Core.PlayerManager.GetPlayerFromSessionId(sessionId);
            if (player is { IsValid: true } && player.Controller.InventoryServices is { IsValid: true } inventory)
            {
                Write(inventory, original);
            }
        }
        _originalPins.Clear();
    }
}
