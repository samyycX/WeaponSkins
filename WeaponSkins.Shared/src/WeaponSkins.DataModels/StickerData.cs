namespace WeaponSkins.Shared;

public record StickerData
{
    public required int Id { get; set; }
    public float Wear { get; set; } = 0f;
    public float Scale { get; set; } = 1f;
    public float Rotation { get; set; } = 0f;
    public float OffsetX { get; set; } = 0f;
    public float OffsetY { get; set; } = 0f;
    public int Schema { get; set; } = 0;
}