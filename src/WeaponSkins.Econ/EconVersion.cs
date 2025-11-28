namespace WeaponSkins.Econ;

public record EconVersion
{
    public required string EconDataVersion { get; set; }
    public required int SchemaVersion { get; set; }
}