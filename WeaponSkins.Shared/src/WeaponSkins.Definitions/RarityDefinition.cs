namespace WeaponSkins.Shared;

public record RarityDefinition
{
    public required string Name { get; init; }
    public required int Id { get; init; }
    public required ColorDefinition Color { get; init; }
}