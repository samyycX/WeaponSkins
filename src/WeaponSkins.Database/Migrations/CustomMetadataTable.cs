using FluentMigrator.Runner.VersionTableInfo;

namespace WeaponSkins.Database;

public class CustomMetadataTable : IVersionTableMetaData
{
    public string TableName => "__weaponskins_version_info";
    public string ColumnName => "version";
    public string AppliedOnColumnName => "applied_on";
    public string DescriptionColumnName => "description";
    public string UniqueIndexName => "ix_version";
    public string SchemaName => string.Empty;
    public bool OwnsSchema => true;
    public bool CreateWithPrimaryKey => false;
}