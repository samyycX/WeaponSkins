using SwiftlyS2.Shared.Players;

namespace WeaponSkins.Shared;

public record GloveData
{
    public required ulong SteamID { get; set; }
    public required Team Team { get; set; }
    public required ushort DefinitionIndex { get; set; }
    public int Paintkit { get; set; } = 0;
    public int PaintkitSeed { get; set; } = 0;
    public float PaintkitWear { get; set; } = 0.0f;

    public GloveData DeepClone()
    {
        return new GloveData
        {
            SteamID = SteamID,
            Team = Team,
            DefinitionIndex = DefinitionIndex,
            Paintkit = Paintkit,
            PaintkitSeed = PaintkitSeed,
            PaintkitWear = PaintkitWear,
        };
    }
}