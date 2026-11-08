using FluentMigrator;

namespace WeaponSkins.Database;

[Migration(5)]
public class Migration005 : Migration
{
    public override void Up()
    {
        if (!Schema.Table("wp_player_graffiti").Exists())
        {
            Create.Table("wp_player_graffiti")
                .WithColumn("steamid").AsString(64).NotNullable().PrimaryKey()
                .WithColumn("graffiti_id").AsInt32().NotNullable()
                .WithColumn("tint_id").AsInt32().NotNullable();
        }
    }

    public override void Down()
    {
        Delete.Table("wp_player_graffiti");
    }
}
