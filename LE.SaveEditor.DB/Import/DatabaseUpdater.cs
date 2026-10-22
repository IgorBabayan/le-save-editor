using System.Text.Json;
using LE.SaveEditor.DB.Data;
using LE.SaveEditor.DB.Data.Entities;
using LE.SaveEditor.DB.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LE.SaveEditor.DB.Import;

/// <summary>
/// Compares the new <c>data.json</c> with <c>base.db</c> row by row (key + content hash),
/// prepares a data migration from the differences and applies it in one transaction.
/// </summary>
internal sealed class DatabaseUpdater(ConfigPaths paths)
{
    public async Task<UpdateResult> UpdateAsync(ParsedGameData data, CancellationToken ct)
    {
        await using var db = new GameDbContext(paths.DatabaseFile);

        var migration = await PrepareMigrationAsync(db, data, ct);
        if (migration.IsEmpty)
            return new UpdateResult(migration, Applied: null);

        var applied = await ApplyMigrationAsync(db, data, migration, ct);
        return new UpdateResult(migration, applied);
    }

    private static async Task<MigrationPlan> PrepareMigrationAsync(GameDbContext db, ParsedGameData data, CancellationToken ct)
    {
        using var bar = new ConsoleProgressBar("Comparing data");
        var changes = new List<TableChanges>();

        for (var i = 0; i < GameNode.All.Count; i++)
        {
            var node = GameNode.All[i];
            var existing = await node.LoadHashesAsync(db, ct);
            var incoming = data.Records[node];

            var added = new List<GameEntity>();
            var updated = new List<GameEntity>();
            var incomingKeys = new HashSet<string>(StringComparer.Ordinal);

            foreach (var record in incoming)
            {
                incomingKeys.Add(record.Key);

                if (!existing.TryGetValue(record.Key, out var hash))
                    added.Add(record);
                else if (!string.Equals(hash, record.Hash, StringComparison.Ordinal))
                    updated.Add(record);
            }

            var removed = existing.Keys.Where(k => !incomingKeys.Contains(k)).ToList();
            changes.Add(new TableChanges(node, added, updated, removed));

            bar.Report(i + 1, GameNode.All.Count, node.TableName);
        }

        var plan = new MigrationPlan(changes);
        bar.Complete(plan.IsEmpty
            ? "no differences"
            : $"+{plan.Added} added, ~{plan.Updated} updated, -{plan.Removed} removed");

        return plan;
    }

    private static async Task<DataMigration> ApplyMigrationAsync(
        GameDbContext db, ParsedGameData data, MigrationPlan plan, CancellationToken ct)
    {
        var record = new DataMigration
        {
            Name = DatabaseBuilder.MigrationName("Update"),
            SourceSha256 = data.SourceSha256,
            Added = plan.Added,
            Updated = plan.Updated,
            Removed = plan.Removed,
            Changes = JsonSerializer.Serialize(plan.Tables
                .Where(t => t.Count > 0)
                .ToDictionary(t => t.Node.TableName, t => new
                {
                    added = t.Added.Select(e => e.Key),
                    updated = t.Updated.Select(e => e.Key),
                    removed = t.Removed
                }))
        };

        using var bar = new ConsoleProgressBar($"Applying migration {record.Name}");
        var total = plan.Added + plan.Updated + plan.Removed;
        var done = 0;

        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        foreach (var table in plan.Tables.Where(t => t.Count > 0))
        {
            var name = table.Node.TableName;

            foreach (var batch in table.Removed.Chunk(DatabaseBuilder.BatchSize))
            {
                await table.Node.DeleteAsync(db, batch, ct);
                done += batch.Length;
                bar.Report(done, total, $"{name}: removing");
            }

            foreach (var batch in table.Updated.Chunk(DatabaseBuilder.BatchSize))
            {
                await table.Node.UpdateAsync(db, batch, ct);
                done += batch.Length;
                bar.Report(done, total, $"{name}: updating");
            }

            foreach (var batch in table.Added.Chunk(DatabaseBuilder.BatchSize))
            {
                await table.Node.InsertAsync(db, batch, ct);
                done += batch.Length;
                bar.Report(done, total, $"{name}: adding");
            }
        }

        record.AppliedAtUtc = DateTime.UtcNow;
        db.DataMigrations.Add(record);
        await db.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);
        await db.SealAsync(compact: plan.Removed > 0, ct);
        bar.Complete($"{total} changes applied");

        return record;
    }

    /// <summary>The newest applied migration, or <c>null</c> for a database without history.</summary>
    public async Task<DataMigration?> GetLastMigrationAsync(CancellationToken ct)
    {
        await using var db = new GameDbContext(paths.DatabaseFile);
        return await db.DataMigrations.AsNoTracking().OrderByDescending(m => m.Id).FirstOrDefaultAsync(ct);
    }
}

internal sealed record TableChanges(
    GameNode Node,
    IReadOnlyList<GameEntity> Added,
    IReadOnlyList<GameEntity> Updated,
    IReadOnlyList<string> Removed)
{
    public int Count => Added.Count + Updated.Count + Removed.Count;
}

internal sealed record MigrationPlan(IReadOnlyList<TableChanges> Tables)
{
    public int Added => Tables.Sum(t => t.Added.Count);

    public int Updated => Tables.Sum(t => t.Updated.Count);

    public int Removed => Tables.Sum(t => t.Removed.Count);

    public bool IsEmpty => Added + Updated + Removed == 0;

    /// <summary>Keys of icons that are new or changed, so their images are downloaded again.</summary>
    public IReadOnlySet<string> ChangedIconKeys => Tables
        .Where(t => t.Node == GameNode.Icons)
        .SelectMany(t => t.Added.Concat(t.Updated))
        .Select(e => e.Key)
        .ToHashSet(StringComparer.Ordinal);
}

internal sealed record UpdateResult(MigrationPlan Plan, DataMigration? Applied);
