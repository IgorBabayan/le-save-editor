namespace LastEpochSaveEditor.Services.ItemInfo;

public class HelmItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.Helmet;
	public string BaseIcon => HELM_ICON;
	public int Width => 69;
	public int Height => 69;
	public async Task<string> GetIcon(IDatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType)
	{
		var item = await db.Get(quality, itemType, id);
		var path = $"{HELMETS}/{quality.GetQualityFolderName()}/{item!.GetName()}.png";
		return Path.GetFullPath(path);
	}
}