namespace LastEpochSaveEditor.Services.ItemInfo;

public class AmuletItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.Amulet;
	public string BaseIcon => AMULET_ICON;
	public int Width => 43;
	public int Height => 43;
	public async Task<string> GetIcon(IDatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType)
	{
		var item = await db.Get(quality, itemType, id);
		var path = $"{AMULET}/{quality.GetQualityFolderName()}/{item!.GetName()}.png";
		return Path.GetFullPath(path);
	}
}
