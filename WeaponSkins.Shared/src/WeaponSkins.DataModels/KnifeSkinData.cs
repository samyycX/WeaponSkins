using SwiftlyS2.Shared.Players;

namespace WeaponSkins.Shared;

public record KnifeSkinData
{
    public required ulong SteamID { get; set; }
    public required Team Team { get; init; }
    public required ushort DefinitionIndex { get; init; }

    public EconItemQuality Quality { get; set; } = EconItemQuality.Normal;
    public int Paintkit { get; set; } = 0;
    public int PaintkitSeed { get; set; } = 0;
    public float PaintkitWear { get; set; } = 0.0f;
}