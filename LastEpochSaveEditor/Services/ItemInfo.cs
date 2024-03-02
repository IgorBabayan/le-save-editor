namespace LastEpochSaveEditor.Services;

public interface IItemInfo<TEnum>
	where TEnum : struct, Enum
{
	string BaseIcon { get; }
	int Width { get; }
	int Height { get; }
	TEnum Type { get; }
	Task<string> GetIcon(IDatabaseService db, QualityType quality, int id);
}

public class HelmItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.Helmet;
	public string BaseIcon => Const.HELM_ICON;
	public int Width => 69;
	public int Height => 69;
	public async Task<string> GetIcon(IDatabaseService db, QualityType quality, int id)
	{
		var item = await db.Get(quality, ItemInfoTypeEnum.Helmet, id);
		var path = $"{Const.HELMETS}/{quality.GetQualityFolderName()}/{item!.GetName()}.png";
		return Path.GetFullPath(path);
	}
}

public class BeltItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.Belt;
	public string BaseIcon => Const.BELTS_ICON;
	public int Width => 93;
	public int Height => 39;
	public async Task<string> GetIcon(IDatabaseService db, QualityType quality, int id)
	{
		var item = await db.Get(quality, ItemInfoTypeEnum.Belt, id);
		var path = $"{Const.BELTS}/{quality.GetQualityFolderName()}/{item!.GetName()}.png";
		return Path.GetFullPath(path);
	}
}