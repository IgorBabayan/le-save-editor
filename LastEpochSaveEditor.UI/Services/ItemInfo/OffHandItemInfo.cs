namespace LastEpochSaveEditor.Services.ItemInfo;

public class OffHandItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.OffHands;
	public string BaseIcon => OFF_HAND_ICON;
	public int Width => 69;
	public int Height => 139;
	public async Task<string> GetIcon(IDatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType)
	{
		var item = await db.Get(quality, itemType, id);
		var path = $"{OFF_HAND}/{itemType}/{quality.GetQualityFolderName()}/{item.GetName()}.png";
		return Path.GetFullPath(path);
	}
}
