namespace LastEpochSaveEditor.Models.Characters;

public interface ICharacterInventory : IInventory
{
	ItemDataInfo Helm { get; }
	ItemDataInfo Body { get; }
	ItemDataInfo Weapon { get; }
	ItemDataInfo OffHand { get; }
	ItemDataInfo Gloves { get; }
	ItemDataInfo Belt { get; }
	ItemDataInfo Boots { get; }
	ItemDataInfo LeftRing { get; }
	ItemDataInfo RightRing { get; }
	ItemDataInfo Amulet { get; }
	ItemDataInfo Relic { get; }
}

public class CharacterInventory : ICharacterInventory
{
	private readonly ILogger<CharacterInventory> _logger;
	private readonly DatabaseService _db;

	public ItemDataInfo Helm => _items!.FirstOrDefault(x => x.Type == ItemInfoTypeEnum.Helmet)!;
	public ItemDataInfo Body => _items!.FirstOrDefault(x => x.Type == ItemInfoTypeEnum.Body)!;
	public ItemDataInfo Weapon => _items!.FirstOrDefault(x => x.Type.IsInWeapon())!;
	public ItemDataInfo OffHand => _items!.FirstOrDefault(x => x.Type.IsInOffHands())!;
	public ItemDataInfo Gloves => _items!.FirstOrDefault(x => x.Type == ItemInfoTypeEnum.Gloves)!;
	public ItemDataInfo Belt => _items!.FirstOrDefault(x => x.Type == ItemInfoTypeEnum.Belt)!;
	public ItemDataInfo Boots => _items!.FirstOrDefault(x => x.Type == ItemInfoTypeEnum.Boots)!;
	public ItemDataInfo LeftRing => _items!.FirstOrDefault(x => x.Type == ItemInfoTypeEnum.Ring)!;
	public ItemDataInfo RightRing => _items!.FirstOrDefault(x => x.Type == ItemInfoTypeEnum.Ring)!;
	public ItemDataInfo Amulet => _items!.FirstOrDefault(x => x.Type == ItemInfoTypeEnum.Amulet)!;
	public ItemDataInfo Relic => _items!.FirstOrDefault(x => x.Type == ItemInfoTypeEnum.Relic)!;
	
	private IEnumerable<ItemDataInfo>? _items;
	private readonly IItemInfoFactory _factory;

	public CharacterInventory(ILogger<CharacterInventory> logger, DatabaseService db, IItemInfoFactory factory)
	{
		_logger = logger;
		_db = db;
		_factory = factory;
	}

	public async Task Parse(IDictionary<int, List<int>> data)
	{
		var tasks = await Task.WhenAll(new[]
		{
			Parse(data, 2, "Helm"),
			Parse(data, 3, "Body"),
			Parse(data, 4, "Weapon"),
			Parse(data, 5, "Off-hand"),
			Parse(data, 6, "Gloves"),
			Parse(data, 7, "Belt"),
			Parse(data, 8, "Boots"),
			Parse(data, 9, "Left ring"),
			Parse(data, 10, "Right ring"),
			Parse(data, 11, "Amulet"),
			Parse(data, 12, "Relic")
		});
		_items = new List<ItemDataInfo>(tasks);
	}

	private async Task<BitmapImage> CreateImage(ItemDataInfo itemInfo, IItemInfo<ItemInfoTypeEnum> item)
	{
		var id = itemInfo.Quality >= QualityType.Unique ? itemInfo.UniqueOrSetId : itemInfo.BaseId;
		
		var imagePath = await item.GetIcon(_db, itemInfo.Quality, id, itemInfo.Type);
		var icon = new BitmapImage
		{
			CacheOption = BitmapCacheOption.OnLoad,
			CreateOptions = BitmapCreateOptions.IgnoreImageCache,
			DecodePixelWidth = 200
		};
		icon.BeginInit();
		if (string.IsNullOrWhiteSpace(imagePath) || File.Exists(imagePath))
			icon.UriSource = new(imagePath);
		else
			icon.UriSource = new(item.BaseIcon, UriKind.RelativeOrAbsolute);
		icon.EndInit();

		return icon;
	}

	private async Task<ItemDataInfo> Parse(IDictionary<int, List<int>> data, int index, string name)
	{
		if (!data.Any())
			return ItemDataInfo.Empty.Copy();

		var hasError = false;
		var result = ItemDataInfo.Empty;
		try
		{
			_logger.LogInformation($"Begin parse '{name}' data.");

			if (data.TryGetValue(index, out var value))
			{
				result = ItemDataParser.ParseData(value);
				var item = _factory.Create(result.Type);
				result.Icon = await CreateImage(result, item);
				result.Height = item.Height;
				result.Width = item.Width;
			}
		}
		catch (Exception exception)
		{
			hasError = true;
			_logger.LogError(exception, $"Can't parse '{name}' data.");
		}
		finally
		{
			if (hasError)
				_logger.LogWarning($"Parsing '{name}' data ended with error.");
			else
				_logger.LogInformation($"Parsing '{name}' data ended successfully.");
		}

		return result;
	}
}