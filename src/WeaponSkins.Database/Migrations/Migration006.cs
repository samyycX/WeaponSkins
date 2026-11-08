using FluentMigrator;

namespace WeaponSkins.Database;

[Migration(6)]
public class Migration006 : Migration
{
    public override void Up()
    {
        if (!Schema.Table("wp_player_agent_patches").Exists())
        {
            Create.Table("wp_player_agent_patches")
                .WithColumn("steamid").AsString(64).NotNullable()
                .WithColumn("weapon_team").AsInt32().NotNullable()
                .WithColumn("slot").AsInt32().NotNullable()
                .WithColumn("patch_id").AsInt32().NotNullable();

            Create.UniqueConstraint("UQ_wp_player_agent_patches_steamid_team_slot")
                .OnTable("wp_player_agent_patches")
                .Columns("steamid", "weapon_team", "slot");
        }
    }

    public override void Down()
    {
        Delete.Table("wp_player_agent_patches");
    }
}
