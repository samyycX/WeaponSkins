using FluentMigrator;

namespace WeaponSkins.Database;

[Migration(3)]
public class Migration003 : Migration
{
    public override void Up()
    {
        Alter.Table("wp_player_gloves")
            .AddColumn("weapon_paintkit").AsInt32().WithDefaultValue(0)
            .AddColumn("weapon_paintkitseed").AsInt32().WithDefaultValue(0)
            .AddColumn("weapon_paintkitwear").AsFloat().WithDefaultValue(0.0f);
    }

    public override void Down()
    {
        Delete.Column("weapon_paintkit").FromTable("wp_player_gloves");
        Delete.Column("weapon_paintkitseed").FromTable("wp_player_gloves");
        Delete.Column("weapon_paintkitwear").FromTable("wp_player_gloves");
    }
}
