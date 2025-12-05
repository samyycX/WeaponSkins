using FluentMigrator;

namespace WeaponSkins.Database;

[Migration(2)]
public class Migration002 : Migration
{
    public override void Up()
    {
        Create.UniqueConstraint("wp_player_skins_steamid_weapon_team_weapon_defindex_unique")
            .OnTable("wp_player_skins")
            .Columns("steamid", "weapon_team", "weapon_defindex");

        Create.UniqueConstraint("wp_player_knife_steamid_weapon_team_unique")
            .OnTable("wp_player_knife")
            .Columns("steamid", "weapon_team");

        Create.UniqueConstraint("wp_player_gloves_steamid_weapon_team_unique")
            .OnTable("wp_player_gloves")
            .Columns("steamid", "weapon_team");
    }

    public override void Down()
    {
        Delete.UniqueConstraint("wp_player_skins_steamid_weapon_team_weapon_defindex_unique");
        Delete.UniqueConstraint("wp_player_knife_steamid_weapon_team_unique");
        Delete.UniqueConstraint("wp_player_gloves_steamid_weapon_team_unique");
    }
}