using FluentMigrator;

namespace WeaponSkins.Database;

[Migration(3)]
public class Migration003 : Migration
{
    public override void Up()
    {
        if (!Schema.Table("wp_player_agents").Exists())
        {
            Create.Table("wp_player_agents")
                .WithColumn("steamid").AsString(18).NotNullable()
                .WithColumn("weapon_team").AsInt16().NotNullable()
                .WithColumn("agent_index").AsInt32().NotNullable();

            Create.UniqueConstraint("wp_player_agents_steamid_weapon_team_unique")
                .OnTable("wp_player_agents")
                .Columns("steamid", "weapon_team");
        }
    }

    public override void Down()
    {
        Delete.UniqueConstraint("wp_player_agents_steamid_weapon_team_unique");
        Delete.Table("wp_player_agents");
    }
}
