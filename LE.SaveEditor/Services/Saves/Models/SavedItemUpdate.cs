namespace LE.SaveEditor.Services.Saves.Models;

/// <summary>One change to <c>savedItems</c> made in the editor.</summary>
/// <param name="Original">
/// The entry as it was read (<see cref="SavedItem.Index"/> says where it is in the file); <c>null</c> to add a new entry.
/// </param>
/// <param name="Replacement">What the entry becomes; <c>null</c> to remove <paramref name="Original"/>.</param>
public sealed record SavedItemUpdate(SavedItem? Original, SavedItem? Replacement)
{
    public static SavedItemUpdate Add(SavedItem item) => new(null, item);
    public static SavedItemUpdate Remove(SavedItem item) => new(item, null);
    public static SavedItemUpdate Replace(SavedItem original, SavedItem replacement) => new(original, replacement);
}
