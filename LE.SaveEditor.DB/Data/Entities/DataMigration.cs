namespace LE.SaveEditor.DB.Data.Entities;

/// <summary>
/// History of data migrations applied to <c>base.db</c> (table <c>__DataMigrations</c>).
/// The newest row tells which version of <c>data.json</c> the database holds.
/// </summary>
public sealed class DataMigration
{
    public int Id { get; set; }

    /// <summary>e.g. <c>20261022181500_Build</c> or <c>20261023090000_Update</c>.</summary>
    public string Name { get; set; } = string.Empty;

    public DateTime AppliedAtUtc { get; set; }

    /// <summary>SHA-256 of the downloaded <c>data.json</c>.</summary>
    public string SourceSha256 { get; set; } = string.Empty;

    public int Added { get; set; }

    public int Updated { get; set; }

    public int Removed { get; set; }

    /// <summary>JSON: <c>{ "Affixes": { "added": [keys], "updated": [keys], "removed": [keys] }, ... }</c>.</summary>
    public string Changes { get; set; } = "{}";
}
