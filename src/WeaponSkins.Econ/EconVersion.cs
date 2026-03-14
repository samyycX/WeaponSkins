namespace WeaponSkins.Econ;

public record EconVersion
{
    public List<string>? ItemLanguages { get; set; }
    public required string EconDataVersion { get; set; }
    public required int SchemaVersion { get; set; }
}