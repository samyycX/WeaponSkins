namespace WeaponSkins.Shared;

public record KeychainData
{
    public required int Id { get; set; }
    public float OffsetX { get; set; } = 0f;
    public float OffsetY { get; set; } = 0f;
    public float OffsetZ { get; set; } = 0f;
    public int Seed { get; set; } = 0;
}