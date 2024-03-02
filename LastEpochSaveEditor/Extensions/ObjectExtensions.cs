namespace LastEpochSaveEditor.Extensions;

internal static class ObjectExtensions
{
    public static string GetName(this object self)
    {
        if (self == null)
            throw new ArgumentNullException(nameof(self));

        return self.GetType() switch
        {
            { } type when type == typeof(SubItem) => !string.IsNullOrWhiteSpace(((SubItem)self).Name)
                ? ((SubItem)self).Name.GetItemName()
                : ((SubItem)self).DisplayName.GetItemName(),
            { } type when type == typeof(Unique) => !string.IsNullOrWhiteSpace(((Unique)self).Name)
                ? ((Unique)self).Name.GetItemName()
                : ((Unique)self).DisplayName.GetItemName(),
            _ => throw new ArgumentException("Type not supported")
        };
    }
}