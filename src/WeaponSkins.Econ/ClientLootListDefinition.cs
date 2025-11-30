namespace WeaponSkins.Econ;

public record ClientLootListDefinition
{
    public required string Name { get; set; }
    public required List<ClientLootItemDefinition> Items { get; set; }
}

public record ClientLootItemDefinition
{
    public required string Name { get; set; }
    public required string BelongingItemName { get; set; }
}