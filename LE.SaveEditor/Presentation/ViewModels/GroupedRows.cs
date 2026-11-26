namespace LE.SaveEditor.Presentation.ViewModels;

/// <summary>Title row of a grouped list: shown above the items of its group, neither selectable nor clickable.</summary>
public sealed record ListGroupHeader(string Name)
{
    public override string ToString() => Name;
}

/// <summary>
/// Flattens a list into rows for a grouped ListBox: a <see cref="ListGroupHeader"/>, then the items of that group.
/// Groups keep the order in which they first appear and collect every item of their name (like a grouped
/// collection view), and the whole list is built at once, so the ListBox gets a single change notification.
/// </summary>
public static class GroupedRows
{
    public static IReadOnlyList<object> Build<T>(IEnumerable<T> items, Func<T, string> groupOf) where T : notnull
    {
        var rows = new List<object>();
        foreach (var group in items.GroupBy(groupOf, StringComparer.Ordinal))
        {
            rows.Add(new ListGroupHeader(group.Key));
            foreach (var item in group)
                rows.Add(item);
        }

        return rows;
    }
}
