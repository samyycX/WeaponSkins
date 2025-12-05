using FluentMigrator;

namespace WeaponSkins.Database;

[Migration(1)]
public class Migration001 : Migration
{
    public override void Up()
    {
        if (!Schema.Table("wp_player_skins").Exists())
        {
            Create.Table("wp_player_skins")
                .WithColumn("steamid").AsString(18).NotNullable()
                .WithColumn("weapon_team").AsInt16().NotNullable()
                .WithColumn("weapon_defindex").AsInt32().NotNullable()
                .WithColumn("weapon_paint_id").AsInt32().NotNullable()
                .WithColumn("weapon_wear").AsFloat().NotNullable().WithDefaultValue(0.000001f)
                .WithColumn("weapon_seed").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("weapon_nametag").AsString(128).Nullable()
                .WithColumn("weapon_stattrak").AsByte().NotNullable().WithDefaultValue(0)
                .WithColumn("weapon_stattrak_count").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("weapon_sticker_0").AsString(128).NotNullable().WithDefaultValue("0;0;0;0;0;0;0")
                .WithColumn("weapon_sticker_1").AsString(128).NotNullable().WithDefaultValue("0;0;0;0;0;0;0")
                .WithColumn("weapon_sticker_2").AsString(128).NotNullable().WithDefaultValue("0;0;0;0;0;0;0")
                .WithColumn("weapon_sticker_3").AsString(128).NotNullable().WithDefaultValue("0;0;0;0;0;0;0")
                .WithColumn("weapon_sticker_4").AsString(128).NotNullable().WithDefaultValue("0;0;0;0;0;0;0")
                .WithColumn("weapon_sticker_5").AsString(128).NotNullable().WithDefaultValue("0;0;0;0;0;0;0")
                .WithColumn("weapon_keychain").AsString(128).NotNullable().WithDefaultValue("0;0;0;0;0");
        }

        if (!Schema.Table("wp_player_knife").Exists())
        {
            Create.Table("wp_player_knife")
                .WithColumn("steamid").AsString(18).NotNullable()
                .WithColumn("weapon_team").AsInt16().NotNullable()
                .WithColumn("knife").AsString(64).NotNullable();

        }

        if (!Schema.Table("wp_player_gloves").Exists())
        {
            Create.Table("wp_player_gloves")
                .WithColumn("steamid").AsString(18).NotNullable()
                .WithColumn("weapon_team").AsInt16().NotNullable()
                .WithColumn("weapon_defindex").AsInt32().NotNullable();

        }
    }

    public override void Down()
    {
        Delete.Table("wp_player_skins");
        Delete.Table("wp_player_knife");
        Delete.Table("wp_player_gloves");
    }
}