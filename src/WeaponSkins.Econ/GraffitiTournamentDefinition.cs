namespace WeaponSkins.Econ;

// A tournament's team and player graffiti. Kept off WeaponSkins.Shared: only the menu groups graffiti
// this way. Names, not definitions, so its rows tag the same instances as EconService.Graffiti.
public record GraffitiTournamentDefinition
{
    public required int EventId { get; init; }
    public required Dictionary<string, string> LocalizedNames { get; init; }
    public required List<string> Graffiti { get; init; }
}
