using System.Diagnostics.CodeAnalysis;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Players;

namespace WeaponSkins.Services;

public class PlayerService
{

  private Dictionary<ulong, IPlayer> Players = new();
  private ISwiftlyCore Core { get; init; }
  public PlayerService(ISwiftlyCore core)
  {
    Core = core;

    Core.Event.OnClientSteamAuthorize += (@event) => {
      var player = Core.PlayerManager.GetPlayer(@event.PlayerId);
      if (player == null)
      {
        return;
      }
      Players[player.SteamID] = player;
    };

    Core.Event.OnClientDisconnected += (@event) => {
      var player = Core.PlayerManager.GetPlayer(@event.PlayerId);
      if (player == null)
      {
        return;
      }
      Players.Remove(player.SteamID);
    };

    foreach(var player in Core.PlayerManager.GetAllPlayers())
    {
        Players[player.SteamID] = player;
    }
  }

  public bool TryGetPlayer(ulong steamID, [MaybeNullWhen(false)] out IPlayer player)
  {
    return Players.TryGetValue(steamID, out player);
  }
}