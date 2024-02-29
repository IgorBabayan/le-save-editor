namespace LastEpochSaveEditor.Models.Characters;

public interface ICharacterInventory : IInventory
{
	ItemDataInfo Helm { get; set; }
	ItemDataInfo Body { get; set; }
	ItemDataInfo Weapon { get; set; }
	ItemDataInfo OffHand { get; set; }
	ItemDataInfo Gloves { get; set; }
	ItemDataInfo Belt { get; set; }
	ItemDataInfo Boots { get; set; }
	ItemDataInfo LeftRing { get; set; }
	ItemDataInfo RightRing { get; set; }
	ItemDataInfo Amulet { get; set; }
	ItemDataInfo Relic { get; set; }
}

public class CharacterInventory : ICharacterInventory
{
	private readonly ILogger<CharacterInventory> _logger;
	private readonly IDB _db;

	public ItemDataInfo Helm { get; set; }
	public ItemDataInfo Body { get; set; }
	public ItemDataInfo Weapon { get; set; }
	public ItemDataInfo OffHand { get; set; }
	public ItemDataInfo Gloves { get; set; }
	public ItemDataInfo Belt { get; set; }
	public ItemDataInfo Boots { get; set; }
	public ItemDataInfo LeftRing { get; set; }
	public ItemDataInfo RightRing { get; set; }
	public ItemDataInfo Amulet { get; set; }
	public ItemDataInfo Relic { get; set; }

	public CharacterInventory(ILogger<CharacterInventory> logger)
	{
		_logger = logger;
		_db = App.GetService<IDB>();
	}

	public void Parse(IDictionary<int, List<int>> data)
	{
		ParseHelm(data);
		ParseBody(data);
		ParseWeapon(data);
		ParseOffHand(data);
		ParseGloves(data);
		ParseBelt(data);
		ParseBoots(data);
		ParseLeftRing(data);
		ParseRightRing(data);
		ParseAmulet(data);
		ParseRelic(data);
	}

	private static BitmapImage CreateImage(string path, string defaultPath)
	{
		var icon = new BitmapImage
		{
			CacheOption = BitmapCacheOption.OnLoad,
			CreateOptions = BitmapCreateOptions.IgnoreImageCache,
			DecodePixelWidth = 200
		};
		icon.BeginInit();
		if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
			icon.UriSource = new(path);
		else
			icon.UriSource = new(defaultPath, UriKind.RelativeOrAbsolute);
		icon.EndInit();

		return icon;
	}

	private static string GetQualityFolderName(QualityType quality)
	{
		switch (quality)
		{
			case QualityType.Basic:
			case QualityType.Magic:
			case QualityType.Rare:
			case QualityType.Exalted:
				return Const.BASIC_FOLDER_NAME;

			case QualityType.Unique:
			case QualityType.Legendary:
				return Const.UNIQUE_FOLDER_NAME;

			default:
				return Const.SET_FOLDER_NAME;
		}
	}

	private static string GetIcon(Item item, ItemDataInfo itemInfo, string basePath, string? baseTypeName = null)
	{
		var id = itemInfo.Quality >= QualityType.Unique ? itemInfo.UniqueOrSetId : itemInfo.BaseId;
		var name = item.FindName(itemInfo.Quality, id);

		if (string.IsNullOrEmpty(name))
			return string.Empty;

		var quality = GetQualityFolderName(itemInfo.Quality);
		if (string.IsNullOrEmpty(basePath))
			return Path.GetFullPath($"{basePath}{quality}/{name.ToLowerInvariant().Replace(" ", "_")}.png");

		return Path.GetFullPath($"{basePath}{baseTypeName}/{quality}/{name.ToLowerInvariant().Replace(" ", "_")}.png");
	}

	private static string GetIcon(IEnumerable<Item> items, ItemDataInfo itemInfo, string basePath)
	{
		var path = string.Empty;
		foreach (var item in items)
		{
			path = GetIcon(item, itemInfo, basePath, item.Base.BaseTypeName);
			if (!string.IsNullOrWhiteSpace(path))
				break;
		}

		return path;
	}

	private void ParseHelm(IDictionary<int, List<int>> data)
	{
		Helm = Parse(data, 2, "Helm");
		Helm.Icon = CreateImage(GetIcon(_db.GetHelmets(), Helm, Const.HELMETS), Const.HELM_ICON);
		Helm.Width = 69;
		Helm.Height = 69;
	}

	private void ParseBody(IDictionary<int, List<int>> data)
	{
		Body = Parse(data, 3, "Body");
		Body.Icon = CreateImage(GetIcon(_db.GetBodies(), Body, Const.BODY_ARMOR), Const.BODY_ICON);
		Body.Width = 93;
		Body.Height = 139;
	}

	private void ParseWeapon(IDictionary<int, List<int>> data)
	{
		Weapon = Parse(data, 4, "Weapon");

		var icon = GetIcon(_db.Get1HandWeapons(), Weapon, Const.ONE_HAND_WEAPONS);
		if (string.IsNullOrWhiteSpace(icon))
			icon = GetIcon(_db.Get2HandWeapons(), Weapon, Const.TWO_HAND_WEAPONS);

		Weapon.Icon = CreateImage(icon, Const.WEAPON_ICON);
		Weapon.Width = 69;
		Weapon.Height = 139;
	}

	private void ParseOffHand(IDictionary<int, List<int>> data)
	{
		OffHand = Parse(data, 5, "Off-hand");
		OffHand.Icon = CreateImage(GetIcon(_db.GetOffHands(), OffHand, Const.OFF_HAND), Const.OFF_HAND_ICON);
		OffHand.Width = 69;
		OffHand.Height = 139;
	}

	private void ParseGloves(IDictionary<int, List<int>> data)
	{
		Gloves = Parse(data, 6, "Gloves");
		Gloves.Icon = CreateImage(GetIcon(_db.GetGloves(), Gloves, Const.GLOVES), Const.GLOVES_ICON);
		Gloves.Width = 69;
		Gloves.Height = 69;
	}

	private void ParseBelt(IDictionary<int, List<int>> data)
	{
		Belt = Parse(data, 7, "Belt");
		Belt.Icon = CreateImage(GetIcon(_db.GetBelts(), Belt, Const.BELTS), Const.BELTS_ICON);
		Belt.Width = 93;
		Belt.Height = 39;
	}

	private void ParseBoots(IDictionary<int, List<int>> data)
	{
		Boots = Parse(data, 8, "Boots");
		Boots.Icon = CreateImage(GetIcon(_db.GetBoots(), Boots, Const.BOOTS), Const.BOOTS_ICON);
		Boots.Width = 69;
		Boots.Height = 69;
	}

	private void ParseLeftRing(IDictionary<int, List<int>> data)
	{
		LeftRing = Parse(data, 9, "Left ring");
		LeftRing.Icon = CreateImage(GetIcon(_db.GetRings(), LeftRing, Const.RING), Const.RING_ICON);
		LeftRing.Width = 39;
		LeftRing.Height = 39;
	}

	private void ParseRightRing(IDictionary<int, List<int>> data)
	{
		RightRing = Parse(data, 10, "Right ring");
		RightRing.Icon = CreateImage(GetIcon(_db.GetRings(), RightRing, Const.RING), Const.RING_ICON);
		RightRing.Width = 39;
		RightRing.Height = 39;
	}

	private void ParseAmulet(IDictionary<int, List<int>> data)
	{
		Amulet = Parse(data, 11, "Amulet");
		Amulet.Icon = CreateImage(GetIcon(_db.GetAmulets(), Amulet, Const.AMULET), Const.AMULET_ICON);
		Amulet.Width = 43;
		Amulet.Height = 43;
	}

	private void ParseRelic(IDictionary<int, List<int>> data)
	{
		Relic = Parse(data, 12, "Relic");
		Relic.Icon = CreateImage(GetIcon(_db.GetRelics(), Relic, Const.RELIC), Const.RELIC_ICON);
		Relic.Width = 69;
		Relic.Height = 69;
	}

	private ItemDataInfo Parse(IDictionary<int, List<int>> data, int index, string name)
	{
		if (!data.Any())
			return ItemDataInfo.Empty.Copy();

		var hasError = false;
		var result = ItemDataInfo.Empty;
		try
		{
			_logger.LogInformation($"Begin parse '{name}' data.");

			if (data.TryGetValue(index, out var value))
				result = ItemDataParser.ParseData(value);
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