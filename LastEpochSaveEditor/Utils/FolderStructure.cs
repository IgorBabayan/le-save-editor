namespace LastEpochSaveEditor.Utils;

internal class FolderStructure
{
	public static IDictionary<ItemInfoTypeEnum, IEnumerable<ItemInfoTypeEnum>> Folders = new Dictionary<ItemInfoTypeEnum, IEnumerable<ItemInfoTypeEnum>>
	{
		[ItemInfoTypeEnum.Accessories] = EnumExtensions.Accessories,
		[ItemInfoTypeEnum.Armours] = EnumExtensions.Armours,
		[ItemInfoTypeEnum.Idols] = EnumExtensions.Idols,
		[ItemInfoTypeEnum.OffHands] = EnumExtensions.OffHands,
		[ItemInfoTypeEnum.OneHandWeapons] = EnumExtensions.OneHands,
		[ItemInfoTypeEnum.TwoHandWeapons] = EnumExtensions.TwoHands,
		[ItemInfoTypeEnum.Lens] = EnumExtensions.Lens,
		[ItemInfoTypeEnum.Misc] = EnumExtensions.Misc,
		[ItemInfoTypeEnum.AffixShard] = new[] { ItemInfoTypeEnum.AffixShard }
	};
}