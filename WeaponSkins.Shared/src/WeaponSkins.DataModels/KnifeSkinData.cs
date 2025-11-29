using SwiftlyS2.Shared.Players;

namespace WeaponSkins.Shared;

public record KnifeSkinData
{
    public required ulong SteamID { get; set; }
    public required Team Team { get; set; }
    public required ushort DefinitionIndex { get; set; }

    public EconItemQuality Quality { get; set; } = EconItemQuality.Normal;
    public string? Nametag { get; set; } = null;
    public int StattrakCount { get; set; } = 0;

    public int Paintkit { get; set; } = 0;
    public int PaintkitSeed { get; set; } = 0;
    public float PaintkitWear { get; set; } = 0.0f;

    public KnifeSkinData DeepClone()
    {
        return new KnifeSkinData
        {
            SteamID = SteamID,
            Team = Team,
            DefinitionIndex = DefinitionIndex,
            Quality = Quality,
            Nametag = Nametag,
            StattrakCount = StattrakCount,
            Paintkit = Paintkit,
            PaintkitSeed = PaintkitSeed,
            PaintkitWear = PaintkitWear,
        };
    }
}