namespace WeaponSkins.Shared;

public record StickerData
{
    public required int Id { get; set; }
    public float Wear { get; set; } = 0f;
    public float Scale { get; set; } = 1f;
    public float Rotation { get; set; } = 0f;
    public float OffsetX { get; set; } = 0f;
    public float OffsetY { get; set; } = 0f;
    public int Schema { get; set; } = 1337;

    public StickerData DeepClone()
    {
        return new StickerData
        {
            Id = Id,
            Wear = Wear,
            Scale = Scale,
            Rotation = Rotation,
            OffsetX = OffsetX,
            OffsetY = OffsetY,
            Schema = Schema,
        };
    }
}