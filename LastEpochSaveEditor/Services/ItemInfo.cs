namespace LastEpochSaveEditor.Services;

public interface IItemInfo<TEnum>
	where TEnum : struct, Enum
{
	string BaseIcon { get; }
	int Width { get; }
	int Height { get; }
	TEnum Type { get; }
	string GetIcon(IDatabaseSerive db, QualityType quality, int id);
}

public class HelmItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.Helmet;
	public string BaseIcon => Const.HELM_ICON;
	public int Width => 69;
	public int Height => 69;
	public string GetIcon(IDatabaseSerive db, QualityType quality, int id)
	{
		var pathBuilder = new StringBuilder(Const.HELMETS);
		pathBuilder.Append($"/{quality.GetQualityFolderName()}/");
		var items = db.Get(quality, ItemInfoTypeEnum.Helmet, id);
		
		return pathBuilder.ToString();
	}
}

public class BeltItemInfo : IItemInfo<ItemInfoTypeEnum>
{
	public ItemInfoTypeEnum Type => ItemInfoTypeEnum.Belt;
	public string BaseIcon => Const.BELTS_ICON;
	public int Width => 93;
	public int Height => 39;
	public string GetIcon(IDatabaseSerive db, QualityType quality, int id)
	{
		var pathBuilder = new StringBuilder(Const.BELTS_ICON);
		pathBuilder.Append($"/{quality.GetQualityFolderName()}/");

		return pathBuilder.ToString();
	}
}