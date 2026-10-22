using LE.SaveEditor.DB.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LE.SaveEditor.DB.Data;

/// <summary>
/// A node of <c>data.json</c> that is stored as a table, plus the typed EF operations on that table
/// (so the builder/updater can treat all seven tables the same way).
/// </summary>
internal abstract class GameNode(string jsonName, string tableName)
{
    public static GameNode Classes { get; } = new GameNode<GameClass>("classes", "Classes");
    public static GameNode Affixes { get; } = new GameNode<Affix>("affixes", "Affixes");
    public static GameNode ItemTypes { get; } = new GameNode<ItemType>("itemTypes", "ItemTypes");
    public static GameNode ItemCategories { get; } = new GameNode<ItemCategory>("itemCategories", "ItemCategories");
    public static GameNode Uniques { get; } = new GameNode<Unique>("uniques", "Uniques");
    public static GameNode Stats { get; } = new GameNode<Stat>("stats", "Stats");
    public static GameNode Icons { get; } = new GameNode<Icon>("icons", "Icons");

    public static IReadOnlyList<GameNode> All { get; } =
        [Classes, Affixes, ItemTypes, ItemCategories, Uniques, Stats, Icons];

    /// <summary>Property name in <c>data.json</c>.</summary>
    public string JsonName { get; } = jsonName;

    /// <summary>Table name in <c>base.db</c>.</summary>
    public string TableName { get; } = tableName;

    public abstract Type EntityType { get; }

    public abstract GameEntity Create();

    public abstract Task<Dictionary<string, string>> LoadHashesAsync(GameDbContext db, CancellationToken ct);

    public abstract Task InsertAsync(GameDbContext db, IReadOnlyList<GameEntity> entities, CancellationToken ct);

    public abstract Task UpdateAsync(GameDbContext db, IReadOnlyList<GameEntity> entities, CancellationToken ct);

    public abstract Task<int> DeleteAsync(GameDbContext db, IReadOnlyList<string> keys, CancellationToken ct);

    public override string ToString() => JsonName;
}

internal sealed class GameNode<T>(string jsonName, string tableName) : GameNode(jsonName, tableName)
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
