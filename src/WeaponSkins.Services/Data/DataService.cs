using WeaponSkins.Services;

namespace WeaponSkins;

public class DataService
{
    public WeaponDataService WeaponDataService { get; init; }

    public KnifeDataService KnifeDataService { get; init; }

    public GloveDataService GloveDataService { get; init; }

    public AgentDataService AgentDataService { get; init; }

    public MusicKitDataService MusicKitDataService { get; init; }

    public GraffitiDataService GraffitiDataService { get; init; }

    public AgentPatchDataService AgentPatchDataService { get; init; }

    public PinDataService PinDataService { get; init; }

    public DataService(
        WeaponDataService weaponDataService,
        KnifeDataService knifeDataService,
        GloveDataService gloveDataService,
        AgentDataService agentDataService,
        MusicKitDataService musicKitDataService,
        GraffitiDataService graffitiDataService,
        AgentPatchDataService agentPatchDataService,
        PinDataService pinDataService
    )
    {
        WeaponDataService = weaponDataService;
        KnifeDataService = knifeDataService;
        GloveDataService = gloveDataService;
        AgentDataService = agentDataService;
        MusicKitDataService = musicKitDataService;
        GraffitiDataService = graffitiDataService;
        AgentPatchDataService = agentPatchDataService;
        PinDataService = pinDataService;
    }
}
