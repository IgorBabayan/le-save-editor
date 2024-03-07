namespace LastEpochSaveEditor.Services.ItemInfo;

public class BodyItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.Body;
	public string BaseIcon => BODY_ARMOR_ICON;
	public int Width => 93;
	public int Height => 139;
	public async Task<string> GetIcon(IDatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType)
	{
		var item = await db.Get(quality, itemType, id);
		var path = $"{BODY_ARMOR}/{quality.GetQualityFolderName()}/{item.GetName()}.png";
		return Path.GetFullPath(path);
	}
}
