using Microsoft.Extensions.Options;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;
using SwiftlyS2.Shared.Sounds;
using SwiftlyS2.Shared.Trace;

using WeaponSkins.Configuration;

namespace WeaponSkins.Services;

public class GraffitiService
{
    // Source's classic spray reach.
    private const float SprayDistance = 128f;

    private readonly Dictionary<ulong, CHandle<CPlayerSprayDecal>> _sprays = new();
    private readonly Dictionary<ulong, long> _nextSprayAt = new();
    private readonly HashSet<int> _inMenuAtTick = new();
    private ISwiftlyCore Core { get; init; }
    private DataService DataService { get; init; }
    private ItemPermissionService ItemPermissionService { get; init; }
    private LocalizationService LocalizationService { get; init; }
    private IOptionsMonitor<MainConfigModel> Options { get; init; }

    public GraffitiService(ISwiftlyCore core,
        DataService dataService,
        ItemPermissionService itemPermissionService,
        LocalizationService localizationService,
        IOptionsMonitor<MainConfigModel> options)
    {
        Core = core;
        DataService = dataService;
        ItemPermissionService = itemPermissionService;
        LocalizationService = localizationService;
        Options = options;

        Core.Event.OnMapLoad += _ =>
        {
            _sprays.Clear();
            _nextSprayAt.Clear();
        };
        Core.Event.OnEntityCreated += OnEntityCreated;
        Core.Event.OnTick += OnTick;
        Core.Event.OnClientKeyStateChanged += OnClientKeyStateChanged;
    }

    public void Spray(IPlayer player)
    {
        Spray(player, player.SendChat);
    }

    private void Spray(IPlayer player,
        Action<string>? reply)
    {
        var loc = LocalizationService[player];
        var steamId = player.SteamID;
        if (!ItemPermissionService.CanUseGraffiti(steamId))
        {
            reply?.Invoke(loc.GraffitiNoPermission);
            return;
        }

        if (!DataService.GraffitiDataService.TryGetGraffiti(steamId, out var graffitiIndex, out var tintIndex))
        {
            reply?.Invoke(loc.GraffitiNotChosen);
            return;
        }

        if (!player.IsAlive || player.PlayerPawn is not { IsValid: true } pawn || pawn.EyePosition is not { } eye)
        {
            reply?.Invoke(loc.GraffitiNotAlive);
            return;
        }

        var now = Environment.TickCount64;
        if (_nextSprayAt.TryGetValue(steamId, out var nextSprayAt) && now < nextSprayAt)
        {
            reply?.Invoke(loc.GraffitiCooldown((int)Math.Ceiling((nextSprayAt - now) / 1000.0)));
            return;
        }

        var angles = pawn.EyeAngles;
        // World and static props only, passing through everything else: the decal stays where it was
        // put, so it would float off a moving entity. The world hits as an entity too ("worldent").
        var trace = Core.Trace.TraceShapeAngle(eye, angles, SprayDistance,
            TraceParams.Builder()
                .IgnoreEntity(pawn)
                .IgnoreOwner(pawn)
                .WithShouldHitEntity(entity => entity.DesignerName is "worldent" or "prop_static")
                .Build());
        if (!trace.DidHit || trace.StartInSolid)
        {
            reply?.Invoke(loc.GraffitiNoSurface);
            return;
        }

        angles.ToDirectionVectors(out _, out var right, out _);
        var normal = trace.HitNormal;
        // The decal's left edge has to lie on the surface, whatever angle the player sprays it from.
        var left = -right;
        left -= normal * left.Dot(normal);
        if (left.Length() < 0.1f)
        {
            reply?.Invoke(loc.GraffitiNoSurface);
            return;
        }

        if (Core.EntitySystem.CreateEntityByDesignerName<CPlayerSprayDecal>("player_spray_decal") is not { } decal)
        {
            return;
        }

        decal.EndPos = trace.EndPos;
        decal.Start = trace.EndPos;
        decal.Left = left.Normalized();
        decal.Normal = normal;
        decal.AccountID = (uint)steamId;
        // Despite its name, m_nPlayer carries the graffiti's sticker kit.
        decal.Player = graffitiIndex;
        decal.TintID = tintIndex;
        decal.DispatchSpawn();

        if (_sprays.Remove(steamId, out var previous) && previous.Value is { IsValid: true } old)
        {
            old.Despawn();
        }

        _sprays[steamId] = Core.EntitySystem.GetRefEHandle(decal);
        _nextSprayAt[steamId] = now + Math.Max(0, Options.CurrentValue.GraffitiCooldown) * 1000L;

        using var sound = new SoundEvent("SprayCan.Paint");
        sound.Recipients.AddRecipient(player.PlayerID);
        sound.Emit();
    }

    // SwiftlyS2's menus see USE first, so a press that closes one needs this pre-input snapshot.
    private void OnTick()
    {
        _inMenuAtTick.Clear();
        if (!Options.CurrentValue.SprayWithUseKey)
        {
            return;
        }

        var playerCap = Core.PlayerManager.PlayerCap;
        for (var playerId = 0; playerId < playerCap; playerId++)
        {
            if (Core.PlayerManager.GetPlayer(playerId) is { } player
                && Core.Menu.GetSession(player) is not null
                && !player.IsFakeClient)
            {
                _inMenuAtTick.Add(playerId);
            }
        }
    }

    private void OnClientKeyStateChanged(IOnClientKeyStateChangedEvent @event)
    {
        if (@event.Key != KeyKind.E || !@event.Pressed || !Options.CurrentValue.SprayWithUseKey)
        {
            return;
        }

        // With a menu open, USE selects its highlighted row instead.
        if (_inMenuAtTick.Contains(@event.PlayerId)
            || Core.PlayerManager.GetPlayer(@event.PlayerId) is not { IsValid: true, IsFakeClient: false } player
            || Core.Menu.GetSession(player) is not null)
        {
            return;
        }

        Spray(player, reply: null);
    }

    // The game's own graffiti key sprays a graffiti from the player's Steam inventory, so it only
    // exists for players who own and equip one. Their spray then shows what they chose here.
    private void OnEntityCreated(IOnEntityCreatedEvent @event)
    {
        if (!Options.CurrentValue.ReplaceNativeGraffiti || @event.Entity.DesignerName != "player_spray_decal")
        {
            return;
        }

        var handle = Core.EntitySystem.GetRefEHandle(@event.Entity.As<CPlayerSprayDecal>());
        // The game fills the decal in after creating it.
        Core.Scheduler.NextWorldUpdate(() => ReplaceNativeSpray(handle));
    }

    private void ReplaceNativeSpray(CHandle<CPlayerSprayDecal> handle)
    {
        if (handle.Value is not { IsValid: true } decal || decal.AccountID == 0)
        {
            return;
        }

        var accountId = decal.AccountID;
        var player = Core.PlayerManager.GetAllPlayers()
            .FirstOrDefault(p => p.IsValid && !p.IsFakeClient && (uint)p.SteamID == accountId);
        if (player is null
            || !ItemPermissionService.CanUseGraffiti(player.SteamID)
            || !DataService.GraffitiDataService.TryGetGraffiti(player.SteamID, out var graffitiIndex, out var tintIndex)
            || (decal.Player == graffitiIndex && decal.TintID == tintIndex))
        {
            return;
        }

        decal.Player = graffitiIndex;
        decal.PlayerUpdated();
        decal.TintID = tintIndex;
        decal.TintIDUpdated();
    }
}
