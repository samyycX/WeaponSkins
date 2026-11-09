using FluentMigrator;

namespace WeaponSkins.Database;

// The same table WeaponPaints creates, so a database coming from it is used as it is.
[Migration(7)]
public class Migration007 : Migration
{
    public override void Up()
    {
        if (!Schema.Table("wp_player_pins").Exists())
        {
            Create.Table("wp_player_pins")
                .WithColumn("steamid").AsString(64).NotNullable()
                .WithColumn("weapon_team").AsInt32().NotNullable()
                .WithColumn("id").AsInt32().NotNullable();

            Create.UniqueConstraint("UQ_wp_player_pins_steamid_team")
                .OnTable("wp_player_pins")
                .Columns("steamid", "weapon_team");
        }
    }

    public override void Down()
    {
        Delete.Table("wp_player_pins");
    }
}
