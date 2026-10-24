using LE.SaveEditor.DB.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LE.SaveEditor.DB.Data;

/// <summary>
/// A node of <c>data.json</c> that is stored as a table, plus the typed EF operations on that table
/// (so the builder/updater can treat all tables the same way).
/// </summary>
/// <param name="keyField">
/// Field of array elements to key rows by, when the generic id detection would not find it
/// (e.g. <c>property</c> in <c>properties</c>); <c>null</c> for the generic detection.
/// </param>
internal abstract class GameNode(string jsonName, string tableName, string? keyField = null)
{
    public static GameNode Classes { get; } = new GameNode<GameClass>("classes", "Classes");
    public static GameNode Affixes { get; } = new GameNode<Affix>("affixes", "Affixes");
    public static GameNode ItemTypes { get; } = new GameNode<ItemType>("itemTypes", "ItemTypes");
    public static GameNode ItemCategories { get; } = new GameNode<ItemCategory>("itemCategories", "ItemCategories");
    public static GameNode Uniques { get; } = new GameNode<Unique>("uniques", "Uniques");
    public static GameNode Stats { get; } = new GameNode<Stat>("stats", "Stats");
    public static GameNode Properties { get; } = new GameNode<GameProperty>("properties", "Properties", "property");
    public static GameNode Ailments { get; } = new GameNode<Ailment>("ailments", "Ailments");
    public static GameNode AbilityProperties { get; } = new GameNode<AbilityProperty>("abilityProperties", "AbilityProperties", "abilityID");
    public static GameNode IdolAltarProperties { get; } = new GameNode<IdolAltarProperty>("idolAltarProperties", "IdolAltarProperties", IndexKey);

    /// <summary>
    /// <see cref="KeyField"/> value that keys array elements by their position: for lists that items refer to by index
    /// (<c>idolAltarProperties</c>), where an id-like field must not be picked instead.
    /// </summary>
    public const string IndexKey = "#index";
    public static GameNode Icons { get; } = new GameNode<Icon>("icons", "Icons");

    public static IReadOnlyList<GameNode> All { get; } =
        [Classes, Affixes, ItemTypes, ItemCategories, Uniques, Stats, Properties, Ailments, AbilityProperties, IdolAltarProperties, Icons];

    /// <summary>Property name in <c>data.json</c>.</summary>
    public string JsonName { get; } = jsonName;

    /// <summary>Table name in <c>base.db</c>.</summary>
    public string TableName { get; } = tableName;

    /// <summary>Preferred key field of array elements (see the constructor).</summary>
    public string? KeyField { get; } = keyField;

    public abstract Type EntityType { get; }

    public abstract GameEntity Create();

    public abstract Task<Dictionary<string, string>> LoadHashesAsync(GameDbContext db, CancellationToken ct);

    public abstract Task InsertAsync(GameDbContext db, IReadOnlyList<GameEntity> entities, CancellationToken ct);

    public abstract Task UpdateAsync(GameDbContext db, IReadOnlyList<GameEntity> entities, CancellationToken ct);

    public abstract Task<int> DeleteAsync(GameDbContext db, IReadOnlyList<string> keys, CancellationToken ct);

    public override string ToString() => JsonName;
}

internal sealed class GameNode<T>(string jsonName, string tableName, string? keyField = null) : GameNode(jsonName, tableName, keyField)
    where T : GameEntity, new()
{
    public override Type EntityType => typeof(T);

    public override GameEntity Create() => new T();

    public override Task<Dictionary<string, string>> LoadHashesAsync(GameDbContext db, CancellationToken ct) =>
        db.Set<T>()
            .AsNoTracking()
            .Select(e => new { e.Key, e.Hash })
            .ToDictionaryAsync(e => e.Key, e => e.Hash, StringComparer.Ordinal, ct);

    public override async Task InsertAsync(GameDbContext db, IReadOnlyList<GameEntity> entities, CancellationToken ct)
    {
        db.Set<T>().AddRange(entities.Cast<T>());
        await db.SaveChangesAsync(ct);
        db.ChangeTracker.Clear();
    }

    public override async Task UpdateAsync(GameDbContext db, IReadOnlyList<GameEntity> entities, CancellationToken ct)
    {
        // Key is set, so Update() marks every column as modified without loading the old row.
        db.Set<T>().UpdateRange(entities.Cast<T>());
        await db.SaveChangesAsync(ct);
        db.ChangeTracker.Clear();
    }

    public override Task<int> DeleteAsync(GameDbContext db, IReadOnlyList<string> keys, CancellationToken ct) =>
        db.Set<T>().Where(e => keys.Contains(e.Key)).ExecuteDeleteAsync(ct);
}
