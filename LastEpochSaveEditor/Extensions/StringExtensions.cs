namespace LastEpochSaveEditor.Extensions;

internal static class StringExtensions
{
	public static string GetItemName(this string self)
	{
		if (string.IsNullOrWhiteSpace(self))
			return self;

		return self.ToLowerInvariant().Replace(" ", "_");
	}

	public static string GetItemNameAsWebP(this string self) => $"{self.GetItemName()}.webp";
}
