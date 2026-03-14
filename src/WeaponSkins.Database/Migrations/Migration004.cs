using FluentMigrator;

namespace WeaponSkins.Database;

[Migration(4)]
public class Migration004 : Migration
{
    public override void Up()
    {
        Create.Table("wp_player_music")
            .WithColumn("steamid").AsString(64).NotNullable()
            .WithColumn("weapon_team").AsInt32().NotNullable()
            .WithColumn("music_id").AsInt32().NotNullable();

        Create.UniqueConstraint("UQ_wp_player_music_steamid_team")
            .OnTable("wp_player_music")
            .Columns("steamid", "weapon_team");
    }

    public override void Down()
    {
        Delete.Table("wp_player_music");
    }
}
