using Microsoft.Extensions.Options;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;
using SwiftlyS2.Shared.Sounds;
using SwiftlyS2.Shared.Trace;

using WeaponSkins.Configuration;

namespace WeaponSkins.Services;

// Sprays made with the game's own graffiti key are left alone.
public class GraffitiService
{
    // Source's classic spray reach.
    private const float SprayDistance = 128f;

    private readonly Dictionary<ulong, CHandle<CPlayerSprayDecal>> _sprays = new();
    private readonly Dictionary<ulong, long> _nextSprayAt = new();
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
    }

    public void Spray(IPlayer player)
    {
        var loc = LocalizationService[player];
        var steamId = player.SteamID;
        if (!ItemPermissionService.CanUseGraffiti(steamId))
        {
            player.SendCenter(loc.GraffitiNoPermission);
            return;
        }

        if (!DataService.GraffitiDataService.TryGetGraffiti(steamId, out var graffitiIndex, out var tintIndex))
        {
            player.SendCenter(loc.GraffitiNotChosen);
            return;
        }

        if (!player.IsAlive || player.PlayerPawn is not { IsValid: true } pawn || pawn.EyePosition is not { } eye)
        {
            player.SendCenter(loc.GraffitiNotAlive);
            return;
        }

        var now = Environment.TickCount64;
        if (_nextSprayAt.TryGetValue(steamId, out var nextSprayAt) && now < nextSprayAt)
        {
            player.SendCenter(loc.GraffitiCooldown((int)Math.Ceiling((nextSprayAt - now) / 1000.0)));
            return;
        }

        var angles = pawn.EyeAngles;
        var trace = Core.Trace.TraceShapeAngle(eye, angles, SprayDistance,
            TraceParams.Builder().IgnoreEntity(pawn).Build());
        // Static surfaces only: the decal stays where it was put, so it would float off a moving entity.
        if (!trace.DidHit || trace.StartInSolid || trace.Entity is { IsValid: true, Index: not 0 })
        {
            player.SendCenter(loc.GraffitiNoSurface);
            return;
        }

        angles.ToDirectionVectors(out _, out var right, out _);
        var normal = trace.HitNormal;
        // The decal's left edge has to lie on the surface, whatever angle the player sprays it from.
        var left = -right;
        left -= normal * left.Dot(normal);
        if (left.Length() < 0.1f)
        {
            player.SendCenter(loc.GraffitiNoSurface);
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
}
