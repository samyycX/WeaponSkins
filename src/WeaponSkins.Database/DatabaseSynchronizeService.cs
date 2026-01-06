namespace WeaponSkins.Database;

public class DatabaseSynchronizeService
{
    private DatabaseService DatabaseService { get; init; }
    private DataService DataService { get; init; }

    public DatabaseSynchronizeService(DatabaseService databaseService, DataService dataService)
    {
        DatabaseService = databaseService;
        DataService = dataService;
    }

    public void Synchronize()
    {
        Task.Run(async () =>
        {
            var skins = await DatabaseService.GetAllSkinsAsync();
            skins.ToList().ForEach(skin => DataService.WeaponDataService.StoreSkin(skin));
            var knives = await DatabaseService.GetAllKnifesAsync();
            knives.ToList().ForEach(knife => DataService.KnifeDataService.StoreKnife(knife));
            var gloves = await DatabaseService.GetAllGlovesAsync();
            gloves.ToList().ForEach(glove => DataService.GloveDataService.StoreGlove(glove));
            var agents = await DatabaseService.GetAllAgentsAsync();
            agents.ToList().ForEach(agent => DataService.AgentDataService.SetAgent(agent.SteamID, agent.Team, agent.AgentIndex));
        });
    }
}