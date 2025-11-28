namespace WeaponSkins.Database;

public class DatabaseSynchronizeService
{
    private DatabaseService DatabaseService { get; init; }
    private DataService DataService { get; init; }

    public DatabaseSynchronizeService(DatabaseService databaseService, DataService dataService)
    {
        DatabaseService = databaseService;
        DataService = dataService;
        
        Synchronize();
    }

    private void Synchronize()
    {
        DatabaseService.GetAllSkins().ToList().ForEach(skin => DataService.WeaponDataService.StoreSkin(skin));
        DatabaseService.GetAllKnifes().ToList().ForEach(knife => DataService.KnifeDataService.StoreKnife(knife));
        DatabaseService.GetAllGloves().ToList().ForEach(glove => DataService.GloveDataService.StoreGlove(glove));
    }
}